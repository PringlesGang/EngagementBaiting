using System;
using System.IO;
using System.Collections.Generic;

namespace Celeste.Mod.EngagementBaiting;

internal static class FileManager
{
    private static List<string> SourcePaths = new()
    {
        "./Mods/EngagementBaiting/Logs/*.log",
        "./Mods/PlayerPositions.csv"
    };
    private static string DestPath = "./Mods/EngagementBaiting/Logs/Archived";

    public static void BackupFiles()
    {
        string destDir = Path.Combine(DestPath, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        CreateDirectory(destDir);

        foreach (string sourcePath in SourcePaths)
        {
            string directoryPath = Path.GetDirectoryName(sourcePath);
            string filePattern = Path.GetFileName(sourcePath);

            string[] files = Directory.GetFiles(directoryPath, filePattern);

            if (files.Length == 0) {
                Logger.Log(LogLevel.Warn, "EngagementBaiting/FileManager", $"No files found matching pattern {sourcePath}");
                continue;
            }

            MoveFiles(files, destDir);
        }

        if (Directory.GetFiles(destDir).Length == 0) DeleteDirectory(destDir);
    }

    private static void CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/FileManager", $"Failed to create directory \"{path}\": {e}");
            return;
        }
    }

    private static void DeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/FileManager", $"Failed to delete directory \"{path}\": {e}");
        }
    }

    private static void MoveFile(string source, string dest)
    {
        try
        {
            File.Move(source, dest);
            Logger.Log(LogLevel.Verbose, "EngagementBaiting/FileManager", $"Moved file \"{source}\" to \"{dest}\"");
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/FileManager", $"Failed to move file \"{source}\" to \"{dest}\": {e}");
        }
    }

    private static void MoveFiles(string[] sourceFiles, string destDir)
    {
        foreach (string file in sourceFiles)
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            MoveFile(file, destFile);
        }
    }
}
