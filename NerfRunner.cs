using System.Diagnostics;

public class NerfRunner
{
    readonly string outputFolderPath;
    readonly string nerfExePath;
    
    public NerfRunner(string nerfExePath, string outputFolderPath)
    {
        this.outputFolderPath = outputFolderPath;
        this.nerfExePath = nerfExePath;
    }

    public void RunNerf()
    {
        // works but is slow
        string cmd0 = $"--scene {outputFolderPath}";

        using Process nerfProcess = new()
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                FileName = nerfExePath,
                Arguments = cmd0
            }
        };

        nerfProcess.Start();
        nerfProcess.WaitForExit();
    }
    
}