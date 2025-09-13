namespace EmployeeSalaryProcessor.Core.Utilities;

public static class FileSystemHelper
{
    public static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
    
    public static void EnsureOutputDirectoryExists(string baseDirectory)
    {
        var outputPath = Path.Combine(baseDirectory, "Output");
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
    }
}