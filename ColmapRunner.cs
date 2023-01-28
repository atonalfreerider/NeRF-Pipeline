using System.Diagnostics;

public class ColmapRunner
{
    readonly string outputFolderPath;
    readonly string imagesPath;
    readonly string dbPath;
    readonly string sparsePath;
    readonly string sparsePath0;

    public ColmapRunner(string outputFolderPath, string frameFolderName)
    {
        this.outputFolderPath = outputFolderPath;
        dbPath = Path.Combine(outputFolderPath, "database.db");

        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }

        imagesPath = Path.Combine(outputFolderPath, frameFolderName);

        sparsePath = Path.Combine(this.outputFolderPath, "sparse");
        sparsePath0 = Path.Combine(sparsePath, "0");
        if (Directory.Exists(sparsePath))
        {
            Directory.Delete(sparsePath, true);
        }

        Directory.CreateDirectory(sparsePath);
        
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

    public void FeatureExtractor()
    {
        string cmd = $"feature_extractor --ImageReader.camera_model OPENCV --SiftExtraction.estimate_affine_shape=true --SiftExtraction.domain_size_pooling=true --ImageReader.single_camera 1 --database_path {dbPath} --image_path {imagesPath}";

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

    public void Matcher()
    {
        string cmd = $"sequential_matcher --SiftMatching.guided_matching=true --database_path {dbPath}";

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
    
    public void Mapper()
    {
        string cmd = $"mapper --database_path {dbPath} --image_path {imagesPath} --output_path {sparsePath}";

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
    
    public void BundleAdjuster()
    {
        string cmd = $"bundle_adjuster --input_path {sparsePath0} --output_path {sparsePath0} --BundleAdjustment.refine_principal_point 1";

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
            $"model_converter --input_path={sparsePath0} --output_path={outputFolderPath} --output_type=TXT";

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