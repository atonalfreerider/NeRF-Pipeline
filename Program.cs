using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
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
            /*
            if (!Path.GetFileNameWithoutExtension(filePath).EndsWith("03") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("05") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("12") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("15") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("18") &&
                !Path.GetFileNameWithoutExtension(filePath).EndsWith("20")) continue;
*/

            // extract the frame number from each video and write each frame to the output folder
            ExtractFrame(filePath, outputFolderPath, timeStampInSeconds);
        }

        RunColmap(args, frameFolderName);

        Colmap2Nerf colmap2Nerf = new Colmap2Nerf(outputFolderPath);
        colmap2Nerf.Convert(frameFolderName);
    }

    static void RunColmap(Args args, string frameFolderName)
    {
        string dbPath = Path.Combine(args.OutputFolderPath, "database.db");

        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }

        string imagesPath = Path.Combine(args.OutputFolderPath, frameFolderName);
        string outPath = Path.Combine(args.OutputFolderPath, "out");
        if(Directory.Exists(outPath))
        {
            Directory.Delete(outPath, true);
        }

        Directory.CreateDirectory(outPath);

        // works but is slow
        string cmd0 = $"automatic_reconstructor --image_path {imagesPath} --workspace_path {args.OutputFolderPath}";

        using Process colmapProcess0 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd0
            }
        };

        colmapProcess0.Start();
        colmapProcess0.WaitForExit();

        string cmd1 = $"mapper --database_path={dbPath} --image_path={imagesPath} --output_path={outPath}";

        using Process colmapProcess1 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd1
            }
        };

        colmapProcess1.Start();
        colmapProcess1.WaitForExit();

        string cmd2 =
            $"model_converter --input_path={Path.Combine(outPath, "0")} --output_path={args.OutputFolderPath} --output_type=TXT";

        using Process colmapProcess2 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd2
            }
        };

        colmapProcess2.Start();
        colmapProcess2.WaitForExit();
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

    /// <summary>
    /// Doesn't converge, even though it's the same process as automatic
    /// </summary>
    static void Efficient(string imagesPath, string dbPath, string outputFolderPath)
    {
        // works but is slow
        string cmd0 = $"feature_extractor --image_path {imagesPath} --database_path {dbPath}";

        using Process colmapProcess0 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd0
            }
        };

        colmapProcess0.Start();
        colmapProcess0.WaitForExit();

        string cmd1 = $"exhaustive_matcher --database_path {dbPath}";

        using Process colmapProcess1 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd1
            }
        };

        colmapProcess1.Start();
        colmapProcess1.WaitForExit();

        string cmd2 = $"mapper --image_path {imagesPath} --database_path {dbPath} --output_path {outputFolderPath}";


        using Process colmapProcess2 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd2
            }
        };

        colmapProcess2.Start();
        colmapProcess2.WaitForExit();
    }
}