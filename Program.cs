using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Drawing;
using GleamTech.VideoUltimate;

public class Program
{
    class Args
    {
        public string InputPath { get; set; }
        public string OutputFolderPath { get; set; }

        public double TimeStampInSeconds { get; set; }

        public string NerfExePath { get; set; }
    }

    public static void Main(string[] args)
    {
        RootCommand rootCommand = new()
        {
            new Argument<string>("InputPath"),

            new Argument<string>("OutputFolderPath"),

            new Option<double>("--timeStampInSeconds", "The frame number to extract"),

            new Option<string>("--nerfExePath", "The path to the nerf executable")
        };

        rootCommand.Description = "Video to NeRF converter, or image to NeRF converter";

        // Note that the parameters of the handler method are matched according to the names of the options 
        rootCommand.Handler = CommandHandler.Create<Args>(NerfMedia);

        rootCommand.Invoke(args);

        Environment.Exit(0);
    }

    /// <summary>
    /// If a folder with videos: iterate through all .mp4 files in the input folder and extract the frame number from each video and write each frame to the output folder
    /// If a folder with images: copy and run Colmap and NeRF
    /// </summary>
    /// <param name="args"></param>
    static void NerfMedia(Args args)
    {
        string inputPath = args.InputPath;
        string outputFolderPath = args.OutputFolderPath;

        string imagesPath;

        if (Directory.GetFiles(inputPath, "*.mp4").Any())
        {
            double timeStampInSeconds = args.TimeStampInSeconds;

            imagesPath = RoundDoubleToInt(timeStampInSeconds).ToString();
            string outputDirectoryFramePath = Path.Combine(outputFolderPath, imagesPath);
            if (Directory.Exists(outputDirectoryFramePath))
            {
                Directory.Delete(outputDirectoryFramePath, true);
            }

            Directory.CreateDirectory(outputDirectoryFramePath);

            // iterate through all .mp4 files in the input folder
            foreach (string filePath in Directory.GetFiles(inputPath, "*.mp4"))
            {
                // extract the frame number from each video and write each frame to the output folder
                ExtractFrame(filePath, outputFolderPath, timeStampInSeconds);
            }
        }
        else
        {
            imagesPath = Path.Combine(outputFolderPath, "images");
            if (Directory.Exists(imagesPath))
            {
                Directory.Delete(imagesPath, true);
            }

            Directory.CreateDirectory(imagesPath);

            foreach (string filePath in Directory.GetFiles(inputPath, "*.jpg"))
            {
                string fileName = Path.Combine(imagesPath, Path.GetFileName(filePath));
                File.Copy(filePath, fileName);
            }

            foreach (string filePath in Directory.GetFiles(inputPath, "*.png"))
            {
                string fileName = Path.Combine(imagesPath, Path.GetFileName(filePath));
                File.Copy(filePath, fileName);
            }
        }

        ColmapRunner colmapRunner = new ColmapRunner(args.OutputFolderPath, imagesPath);

        colmapRunner.RunColmapAutomatic();
        colmapRunner.Convert();

        Colmap2Nerf colmap2Nerf = new Colmap2Nerf(outputFolderPath);
        colmap2Nerf.Convert(imagesPath);

        NerfRunner nerfRunner = new NerfRunner(args.NerfExePath, outputFolderPath);
        nerfRunner.RunNerf();
    }

    static void ExtractFrame(string filePath, string outputFolderPath, double timeStampInSeconds)
    {
        // extract the frame number from each video and write each frame to the output folder
        using VideoFrameReader reader = new VideoFrameReader(filePath);

        reader.Seek(timeStampInSeconds);
        reader.Read();
        Bitmap frame = reader.GetFrame();
        string fileName = $"{Path.GetFileNameWithoutExtension(filePath)}_{RoundDoubleToInt(timeStampInSeconds)}.png";
        string folderName = RoundDoubleToInt(timeStampInSeconds).ToString();
        string outputPath = Path.Combine(outputFolderPath, folderName, fileName);
        frame.Save(outputPath);
        Console.WriteLine($"Wrote frame {fileName}");
    }

    static int RoundDoubleToInt(double value)
    {
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}