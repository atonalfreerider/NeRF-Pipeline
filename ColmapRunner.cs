using System.Diagnostics;

public class ColmapRunner
{
    readonly string outputFolderPath;
    readonly string imagesPath;
    readonly string dbPath;

    public ColmapRunner(string outputFolderPath, string frameFolderName)
    {
        this.outputFolderPath = outputFolderPath;
        dbPath = Path.Combine(outputFolderPath, "database.db");

        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }

        imagesPath = Path.Combine(outputFolderPath, frameFolderName);

        if (Directory.Exists(Path.Combine(this.outputFolderPath, "sparse")))
        {
            Directory.Delete(Path.Combine(this.outputFolderPath, "sparse"), true);
        }

        if (Directory.Exists(Path.Combine(this.outputFolderPath, "dense")))
        {
            Directory.Delete(Path.Combine(this.outputFolderPath, "dense"), true);
        }

        foreach (string sFile in Directory.GetFiles(this.outputFolderPath, "*.txt"))
        {
            File.Delete(sFile);
        }
    }

    public void RunColmapAutomatic()
    {
        string cmd =
            $"automatic_reconstructor --image_path {imagesPath} --workspace_path {outputFolderPath} --sparse=1 --dense=0";

        using Process colmapProcess0 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd
            }
        };

        colmapProcess0.Start();
        colmapProcess0.WaitForExit();
    }

    public void Convert()
    {
        string cmd =
            $"model_converter --input_path={Path.Combine(outputFolderPath, "sparse/0")} --output_path={outputFolderPath} --output_type=TXT";

        using Process colmapProcess2 = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = "colmap",
                Arguments = cmd
            }
        };

        colmapProcess2.Start();
        colmapProcess2.WaitForExit();
    }
}