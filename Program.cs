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
        rootCommand.Handler = CommandHandler.Create<Args>(ExtractFrames);
        
        rootCommand.Invoke(args);
        
        Environment.Exit(0);
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