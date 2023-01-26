using System.Numerics;

public class NerfSerializer
{
    readonly string jsonPath;

    [Serializable]
    public class Container
    {
        public int aabb_scale = 16; // powers of 2 between 1 and 128, defines the bounding box size 
        public List<NerfFrame> frames = new();
    }

    /// <summary>
    /// from: https://github.com/NVlabs/instant-ngp/pull/1147#issuecomment-1374127391
    /// </summary>
    [Serializable]
    public class NerfFrame
    {
        public string file_path;
        public float sharpness = 1000f; // 0 to 1000?
        public float[][] transform_matrix;

        public float camera_angle_x = 0f;
        public float camera_angle_y = 0f;

        // focal lengths for rectangular pixels
        public float fl_x;

        public float fl_y;

        // these values are used by OPENCV
        public float k1 = 0f;
        public float k2 = 0f;
        public float k3 = 0f;

        public float k4 = 0f;

        // these values are used by OPENCV for distortion
        public float p1 = 0f;
        public float p2 = 0f;
        public bool is_fisheye = false;

        // center of image
        public float cx;
        public float cy;

        // dimensions
        public float w;
        public float h;

        public NerfFrame(
            string file_path,
            float[][] transform_matrix,
            float w, float h,
            float fl_x)
        {
            this.file_path = file_path;
            this.transform_matrix = transform_matrix;

            cx = w / 2f;
            cy = h / 2f;
            this.w = w;
            this.h = h;
            this.fl_x = fl_x;
            fl_y = fl_x * h / w;
        }
    }

    public NerfSerializer(string path)
    {
        jsonPath = path;
    }
    
    public static float[][] Matrix4X4toFloatArray(Matrix4x4 matrix4X4)
    {
        float[][] array = new float[4][];
        for (int i = 0; i < 4; i++)
        {
            array[i] = new float[4];
            for (int j = 0; j < 4; j++)
            {
                array[i][j] = matrix4X4[i, j];
            }
        }

        return array;
    }
}