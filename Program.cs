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
        rootCommand.Handler = CommandHandler.Create<Args>(NerfVideos);

        rootCommand.Invoke(args);

        Environment.Exit(0);
    }

    /// <summary>
    /// Iterate through all .mp4 files in the input folder and extract the frame number from each video and write each frame to the output folder
    /// </summary>
    /// <param name="args"></param>
    static void NerfVideos(Args args)
    {
        string inputPath = args.InputPath;
        string outputFolderPath = args.OutputFolderPath;

        double timeStampInSeconds = args.TimeStampInSeconds;

        string frameFolderName = RoundDoubleToInt(timeStampInSeconds).ToString();
        string outputDirectoryFramePath = Path.Combine(outputFolderPath, frameFolderName);
        if (Directory.Exists(outputDirectoryFramePath))
        {
            Directory.Delete(outputDirectoryFramePath, true);
        }

        Directory.CreateDirectory(outputDirectoryFramePath);

        // iterate through all .mp4 files in the input folder
        foreach (string filePath in Directory.GetFiles(inputPath, "*.mp4"))
        {
            if (!Path.GetFileNameWithoutExtension(filePath).EndsWith("03") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("05") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("12") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("15") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("18") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("20")) continue;

            // extract the frame number from each video and write each frame to the output folder
            ExtractFrame(filePath, outputFolderPath, timeStampInSeconds);
        }

        ColmapRunner colmapRunner = new ColmapRunner(args.OutputFolderPath, frameFolderName);

        colmapRunner.RunColmapAutomatic();
        //colmapRunner.Efficient();
        colmapRunner.MapAndConvert();

        Colmap2Nerf colmap2Nerf = new Colmap2Nerf(outputFolderPath);
        colmap2Nerf.Convert(frameFolderName);
        
        
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