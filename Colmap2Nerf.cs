using System.Numerics;
using Newtonsoft.Json;
using OpenCvSharp;

public class Colmap2Nerf
{
    public string ColmapFolder;

    public Colmap2Nerf(string colmapFolder)
    {
        ColmapFolder = colmapFolder;
    }

    class CamData
    {
        public string camName;
        public int width;
        public int height;
        public float fx;
        public float fy;
        public int cx;
        public int cy;
        public Vector4 k;
        public Vector2 distortion;

        public CamData(string camName, int width, int height, float fx, float fy, int cx, int cy, Vector4 k,
            Vector2 distortion)
        {
            this.camName = camName;
            this.width = width;
            this.height = height;
            this.fx = fx;
            this.fy = fy;
            this.cx = cx;
            this.cy = cy;
            this.k = k;
            this.distortion = distortion;
        }
    }

    public void Convert(string imageDir)
    {
        // read cameras.txt
        string camerasTxt = File.ReadAllText(ColmapFolder + "/cameras.txt");
        Dictionary<string, CamData> cameras = new Dictionary<string, CamData>();

        string[] cameraLines = camerasTxt.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string cameraLine in cameraLines)
        {
            if (cameraLine.StartsWith('#')) continue;

            string[] cameraLineSplit = cameraLine.Split(' ');
            string cameraName = cameraLineSplit[0];
            string cameraType = cameraLineSplit[1];

            int width = int.Parse(cameraLineSplit[2]);
            int height = int.Parse(cameraLineSplit[3]);
            float fx = float.Parse(cameraLineSplit[4]);
            float fy = float.Parse(cameraLineSplit[5]);
            int cx = width / 2;
            int cy = height / 2;
            Vector4 k = new Vector4(
                float.Parse(cameraLineSplit[8]),
                float.Parse(cameraLineSplit[9]),
                0f,
                0f);

            Vector2 distortion = new Vector2(
                float.Parse(cameraLineSplit[10]),
                float.Parse(cameraLineSplit[11]));

            cameras.Add(cameraName, new CamData(cameraName, width, height, fx, fy, cx, cy, k, distortion));
        }

        // read images.txt
        string imagesTxt = File.ReadAllText(ColmapFolder + "/images.txt");

        string[] lines = imagesTxt.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        List<NerfSerializer.NerfFrame> frames = new();
        foreach (string line in lines)
        {
            if (line.EndsWith(".png") || line.EndsWith(".jpg"))
            {
                string[] parts = line.Split(' ');

                string cameraName = parts[8];
                CamData camData = cameras[cameraName];

                string imageName = Path.Combine(ColmapFolder, imageDir, parts[9]);

                Quaternion rot = new Quaternion(
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3]),
                    float.Parse(parts[4]));

                Vector3 pos = new Vector3(float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]));

                Matrix4x4 viewMatrix = Matrix4x4.CreateFromQuaternion(rot);
                viewMatrix.Translation = pos;
                viewMatrix = Flip(viewMatrix);

                float sharpness = Sharpness(imageName);

                NerfSerializer.NerfFrame nerfFrame = new NerfSerializer.NerfFrame(
                    imageName,
                    NerfSerializer.Matrix4X4toFloatArray(viewMatrix),
                    camData.width,
                    camData.height,
                    camData.fx,
                    camData.fy,
                    camData.k,
                    camData.distortion,
                    sharpness);

                frames.Add(nerfFrame);
            }
        }

        NerfSerializer.Container container = new NerfSerializer.Container
        {
            frames = frames
        };

        string outPath = Path.Combine(ColmapFolder, "transforms.json");
        if (File.Exists(outPath))
        {
            File.Delete(outPath);
        }

        string cameraJsonString = JsonConvert.SerializeObject(container, Formatting.Indented);
        File.WriteAllText(outPath, cameraJsonString);
    }

    static Matrix4x4 Flip(Matrix4x4 matrix4X4)
    {
        Matrix4x4 flipMat = Matrix4x4.Identity;
        flipMat.M11 = -1;
        flipMat.M22 = -1;

        return Matrix4x4.Multiply(matrix4X4, flipMat);
    }

    static float Sharpness(string imagePath)
    {
        Mat image = Cv2.ImRead(imagePath);

        using Mat laplacian = new Mat();
        int kernel_size = 3;
        int scale = 1;
        int delta = 0;
        int ddepth = image.Type().Depth;
        Cv2.Laplacian(image, laplacian, ddepth, kernel_size, scale, delta);
        Cv2.MeanStdDev(laplacian, out Scalar mean, out Scalar stddev);
        double sharpness = stddev.Val0 * stddev.Val0;

        return (float)sharpness;
    }

    void Reorient()
    {
        double[] up = new double[3];
// normalize the up vector
        up = up.Select(x => x / Math.Sqrt(up.Sum(y => y * y))).ToArray();
        Console.WriteLine("up vector was {0}", string.Join(", ", up));
        double[,] R = rotmat(up, new double[] { 0, 0, 1 });
        R = PadMatrix(R, 0, 1);
        R[R.GetLength(0) - 1, R.GetLength(1) - 1] = 1;
        for (int i = 0; i < out [
        "frames"].Length;
        i++)
        {
            double[,] f = out[
            "frames"][i]
            [

            "transform_matrix"];
            out[
            "frames"][i]
            [

            "transform_matrix"] = MultiplyMatrices(R, f);
        }

        Console.WriteLine("computing center of attention...");
        double totw = 0;
        double[] totp = new double[] { 0, 0, 0 };
        for (int i = 0; i < out [
        "frames"].Length;
        i++)
        {
            double[,] mf = CopyMatrix(out["frames"][i]["transform_matrix"],  0, 3, 0, 3);
            for (int j = 0; j < out [
            "frames"].Length;
            j++)
            {
                double[,] mg = CopyMatrix(out["frames"][j]["transform_matrix"],  0, 3, 0, 3);
                double[] p;
                double w = closest_point_2_lines(mf[0, 3], mf[1, 3], mf[2, 3], mg[0, 3], mg[1, 3], mg[2, 3], out p);
                if (w > 0.00001)
                {
                    totp = totp.Select((x, k) => x + p[k] * w).ToArray();
                    totw += w;
                }
            }
        }
        if (totw > 0)
        {
            totp = totp.Select(x => x / totw).ToArray();
        }

        Console.WriteLine(string.Join(", ", totp));
        for (int i = 0; i < out [
        "frames"].Length;
        i++)
        {
            double[,] f = out[
            "frames"][i]
            [

            "transform_matrix"];
            for (int j = 0; j < 3; j++)
            {
                f[j, 3] -= totp[j];
            }
            out[
            "frames"][i]
            [

            "transform_matrix"] = f;
        }

        double avglen = out[
        "frames"].Sum(f => Math.Sqrt(f["transform_matrix"].Cast<double>().Take(3).Sum(x => x * x))) / nframes;
        Console.WriteLine("avg camera distance from origin {0}", avglen);
        for (int i = 0; i < out [
        "frames"].Length;
        i++)
        {
            f["transform_matrix"][0:3,3] *= 4.0 / avglen # scale to "nerf sized"
        }
    }

    public static object qvec2rotmat(object qvec)
    {
        return np.array(new List<object>
        {
            new List<object>
            {
                1 - 2 * Math.Pow(qvec[2], 2) - 2 * Math.Pow(qvec[3], 2),
                2 * qvec[1] * qvec[2] - 2 * qvec[0] * qvec[3],
                2 * qvec[3] * qvec[1] + 2 * qvec[0] * qvec[2]
            },
            new List<object>
            {
                2 * qvec[1] * qvec[2] + 2 * qvec[0] * qvec[3],
                1 - 2 * Math.Pow(qvec[1], 2) - 2 * Math.Pow(qvec[3], 2),
                2 * qvec[2] * qvec[3] - 2 * qvec[0] * qvec[1]
            },
            new List<object>
            {
                2 * qvec[3] * qvec[1] - 2 * qvec[0] * qvec[2],
                2 * qvec[2] * qvec[3] + 2 * qvec[0] * qvec[1],
                1 - 2 * Math.Pow(qvec[1], 2) - 2 * Math.Pow(qvec[2], 2)
            }
        });
    }

    public static object rotmat(object a, object b)
    {
        a = a / np.linalg.norm(a);
        b = b / np.linalg.norm(b);
        var v = np.cross(a, b);
        var c = np.dot(a, b);
        // handle exception for the opposite direction input
        if (c < -1 + 1E-10)
        {
            return rotmat(a + np.random.uniform(-0.01, 0.01, 3), b);
        }

        var s = np.linalg.norm(v);
        var kmat = np.array(new List<object>
        {
            new List<object>
            {
                0,
                -v[2],
                v[1]
            },
            new List<object>
            {
                v[2],
                0,
                -v[0]
            },
            new List<object>
            {
                -v[1],
                v[0],
                0
            }
        });
        return np.eye(3) + kmat + kmat.dot(kmat) * ((1 - c) / (Math.Pow(s, 2) + 1E-10));
    }

    public static object closest_point_2_lines(object oa, object da, object ob, object db)
    {
        // returns point closest to both rays of form o+t*d, and a weight factor that goes to 0 if the lines are parallel
        da = da / np.linalg.norm(da);
        db = db / np.linalg.norm(db);
        var c = np.cross(da, db);
        var denom = Math.Pow(np.linalg.norm(c), 2);
        var t = ob - oa;
        var ta = np.linalg.det(new List<object>
        {
            t,
            db,
            c
        }) / (denom + 1E-10);
        var tb = np.linalg.det(new List<object>
        {
            t,
            da,
            c
        }) / (denom + 1E-10);
        if (ta > 0)
        {
            ta = 0;
        }

        if (tb > 0)
        {
            tb = 0;
        }

        return Tuple.Create((oa + ta * da + ob + tb * db) * 0.5, denom);
    }
}