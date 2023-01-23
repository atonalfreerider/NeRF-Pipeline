using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Drawing;
using System.Numerics;
using GleamTech.VideoUltimate;
using OpenCvSharp;

public class Program
{
    class Args
    {
        public string InputPath { get; set; }
        public string OutputFolderPath { get; set; }

        public double TimeStampInSeconds { get; set; }
    }

    public static void Main(string[] args)
    {
        RootCommand rootCommand = new()
        {
            new Argument<string>("InputPath"),

            new Argument<string>("OutputFolderPath"),

            new Option<double>("--timeStampInSeconds", "The frame number to extract")
        };

        rootCommand.Description = "Video to image converter";

        // Note that the parameters of the handler method are matched according to the names of the options 
        rootCommand.Handler = CommandHandler.Create<Args>(ExtractFrames);

        //rootCommand.Invoke(args);

        Matrix4x4 camPose = CameraPoseFromImage(@"C:\Users\john\Desktop\Carlos-Aline\frame-2-28-09\carlos-aline_2.png");

        Environment.Exit(0);
    }

    /// <summary>
    /// Get the camera pose from the image using Cv2.solvePnPRansac
    /// </summary>
    static Matrix4x4 CameraPoseFromImage(string imagePath)
    {
        using Mat src = new Mat(imagePath, ImreadModes.Grayscale);

        // Get the keypoints and descriptors
        using ORB orb = ORB.Create();
        KeyPoint[] keypoints;
        using Mat descriptors = new();
        orb.DetectAndCompute(src, null, out keypoints, descriptors);

        // Get the 2D points
        Point2f[] points2D = new Point2f[keypoints.Length];
        for (int i = 0; i < keypoints.Length; i++)
        {
            points2D[i] = keypoints[i].Pt;
        }

        using InputArray point2DArray = InputArray.Create(points2D);

        // Get the 3D points
        Point3f[] points3D = new Point3f[keypoints.Length];
        for (int i = 0; i < keypoints.Length; i++)
        {
            points3D[i] = new Point3f(keypoints[i].Pt.X, keypoints[i].Pt.Y, 0);
        }

        using InputArray point3DArray = InputArray.Create(points3D);

        // Get the camera matrix
        double[,] camMatrix = new double[3, 3];
        {
            camMatrix[0, 0] = 1;
            camMatrix[1, 1] = 1;
            camMatrix[2, 2] = 1;
        }
        ;

        using InputArray camMatrixInputArray = InputArray.Create(camMatrix);
        using InputArray distCoeffs = InputArray.Create(new double[] { 0, 0, 0, 0, 0 });

        // Get the rotation vector
        OutputArray? rvecArray = new Mat(1, 3, MatType.CV_64F, new double[] { 0, 0, 0 });

        // Get the translation vector
        OutputArray? tvecArray = new Mat(1, 3, MatType.CV_64F, new double[] { 0, 0, 0 });
        OutputArray? inliers = new Mat(1, 5, MatType.CV_64F, new double[] { 0, 0, 0, 0, 0 });

        // Solve the pose
        Cv2.SolvePnPRansac(point3DArray, point2DArray, camMatrixInputArray, distCoeffs, rvecArray, tvecArray,
            false, 100, 8F, 0.99D, inliers, SolvePnPFlags.Iterative);

        // Get the rotation matrix
        using Mat rotationMatrix = new(3, 3, MatType.CV_64F, new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        using InputArray rvecArrayInputArray = InputArray.Create(rvecArray.GetMat());
        Cv2.Rodrigues(rvecArrayInputArray, rotationMatrix);

        using Mat tvecArrayInputArray = tvecArray.GetMat();

        // Get the camera pose
        Matrix4x4 cameraPose = new Matrix4x4
        {
            M11 = (float)rotationMatrix.At<double>(0, 0),
            M12 = (float)rotationMatrix.At<double>(0, 1),
            M13 = (float)rotationMatrix.At<double>(0, 2),
            M14 = (float)tvecArrayInputArray.At<double>(0, 0),
            M21 = (float)rotationMatrix.At<double>(1, 0),
            M22 = (float)rotationMatrix.At<double>(1, 1),
            M23 = (float)rotationMatrix.At<double>(1, 2),
            M24 = (float)tvecArrayInputArray.At<double>(1, 0),
            M31 = (float)rotationMatrix.At<double>(2, 0),
            M32 = (float)rotationMatrix.At<double>(2, 1),
            M33 = (float)rotationMatrix.At<double>(2, 2),
            M34 = (float)tvecArrayInputArray.At<double>(2, 0),
            M44 = 1
        };

        /*
        using Mat drawFromPoints = new(1, points2D.Length, MatType.CV_64F, points2D);
        using Window window = new("Camera pose", drawFromPoints);
        Cv2.WaitKey();
        */
        
        return cameraPose;
    }

    /// <summary>
    /// Iterate through all .mp4 files in the input folder and extract the frame number from each video and write each frame to the output folder
    /// </summary>
    /// <param name="args"></param>
    static void ExtractFrames(Args args)
    {
        string inputPath = args.InputPath;
        string outputFolderPath = args.OutputFolderPath;

        double timeStampInSeconds = args.TimeStampInSeconds;

        // iterate through all .mp4 files in the input folder
        foreach (string filePath in Directory.GetFiles(inputPath, "*.mp4"))
        {
            // extract the frame number from each video and write each frame to the output folder
            ExtractFrame(filePath, outputFolderPath, timeStampInSeconds);
        }
    }

    static void ExtractFrame(string filePath, string outputFolderPath, double timeStampInSeconds)
    {
        // extract the frame number from each video and write each frame to the output folder
        using VideoFrameReader reader = new VideoFrameReader(filePath);

        reader.Seek(timeStampInSeconds);
        reader.Read();
        Bitmap frame = reader.GetFrame();
        string fileName = $"{Path.GetFileNameWithoutExtension(filePath)}_{RoundDoubleToInt(timeStampInSeconds)}.png";
        string outputPath = Path.Combine(outputFolderPath, fileName);
        frame.Save(outputPath);
        Console.WriteLine($"Wrote frame {fileName}");
    }

    static int RoundDoubleToInt(double value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}