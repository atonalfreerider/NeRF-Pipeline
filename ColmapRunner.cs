using System.Diagnostics;

public class ColmapRunner
{
    readonly string outputFolderPath;
    readonly string imagesPath;
    readonly string outPath;
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
        outPath = Path.Combine(outputFolderPath, "out");
        if (Directory.Exists(outPath))
        {
            Directory.Delete(outPath, true);
        }

        Directory.CreateDirectory(outPath);

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
        // works but is slow
        string cmd0 =
            $"automatic_reconstructor --image_path {imagesPath} --workspace_path {outputFolderPath} --sparse=0 --dense=0";

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
    }

    public void MapAndConvert()
    {
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
            $"model_converter --input_path={Path.Combine(outPath, "0")} --output_path={outputFolderPath} --output_type=TXT";

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