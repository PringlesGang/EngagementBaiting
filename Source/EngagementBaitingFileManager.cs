using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using Monocle;

namespace Celeste.Mod.EngagementBaiting;

internal static class FileManager
{
    private static List<string> SourcePaths = new()
    {
        "./Mods/EngagementBaiting/Logs/*.log",
        "./Mods/PlayerPositions.csv"
    };
    private static string DestPath = "./Mods/EngagementBaiting/Logs/Archived";

    private static bool SavePressed = false;
    private static List<Keys> SaveKeys = new()
    {
        Keys.LeftControl,
        Keys.LeftShift,
        Keys.L,
    };

    private const float DisplayDuration = 3.0f;
    private const float FadeDuration = 0.2f;
    private static float DisplayTime = 0.0f;
    private static string DisplayMessage = null;

    private static void UpdateInput()
    {
        KeyboardState keyboardState = Keyboard.GetState();

        bool allPressed = SaveKeys.All(key => keyboardState.IsKeyDown(key));
        if (allPressed && !SavePressed)
        {
            BackupFiles();
            SavePressed = true;
        }
        else if (!allPressed)
        {
            SavePressed = false;
        }
    }

    public static void Update()
    {
        if (DisplayMessage != null)
        {
            DisplayTime += Engine.DeltaTime;
            if (DisplayTime > DisplayDuration)
            {
                DisplayMessage = null;
                DisplayTime = 0.0f;
            }
        }

        UpdateInput();
    }

    public static void Render()
    {
        if (DisplayMessage == null) return;

        float alpha = 1.0f;
        if (DisplayTime < FadeDuration)
        {
            alpha = DisplayTime / FadeDuration;
        }
        else if (DisplayTime > DisplayDuration - FadeDuration)
        {
            alpha = (DisplayDuration - DisplayTime) / FadeDuration;
        }

        Vector2 justify = new Vector2(1.0f + 15.0f / Engine.Width, 15.0f / Engine.Height);

        try
        {
            Draw.SpriteBatch.Begin();
            ActiveFont.Draw(DisplayMessage, new Vector2(Engine.Width, 0.0f), justify,
                            new Vector2(0.5f, 0.5f), Color.White * alpha);
            Draw.SpriteBatch.End();
        }
        catch (Exception e)
        {
        }
    }

    private static void CloseFiles()
    {
        EBLogger.CloseFile();
        PositionLogger.CloseFile();
    }

    private static void NewFiles()
    {
        EBLogger.NewFile();
        PositionLogger.NewFile();
    }

    public static void BackupFiles()
    {
        CloseFiles();

        bool success = true;

        string nowString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string destDir = Path.Combine(DestPath, nowString);
        success &= CreateDirectory(destDir);

        foreach (string sourcePath in SourcePaths)
        {
            string directoryPath = Path.GetDirectoryName(sourcePath);
            string filePattern = Path.GetFileName(sourcePath);

            string[] files = Directory.GetFiles(directoryPath, filePattern);

            if (files.Length == 0) {
                Logger.Log(LogLevel.Warn, "EngagementBaiting/FileManager", $"No files found matching pattern {sourcePath}");
                continue;
            }

            success &= MoveFiles(files, destDir);
        }

        if (Directory.GetFiles(destDir).Length == 0)
        {
            DisplayMessage = "No logs to save";
            DeleteDirectory(destDir);
        }
        else
        {
            DisplayMessage = success ? $"Saved logs to {nowString}" : "Failed to save logs";
        }
        DisplayTime = 0.0f;

        NewFiles();
    }

    private static bool CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/FileManager", $"Failed to create directory \"{path}\": {e}");
            return false;
        }
    }

    private static bool DeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
            return true;
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/FileManager", $"Failed to delete directory \"{path}\": {e}");
            return false;
        }
    }

    private static bool MoveFile(string source, string dest)
    {
        try
        {
            File.Move(source, dest);
            Logger.Log(LogLevel.Verbose, "EngagementBaiting/FileManager", $"Moved file \"{source}\" to \"{dest}\"");
            return true;
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/FileManager", $"Failed to move file \"{source}\" to \"{dest}\": {e}");
            return false;
        }
    }

    private static bool MoveFiles(string[] sourceFiles, string destDir)
    {
        bool success = true;
        foreach (string file in sourceFiles)
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            success &= MoveFile(file, destFile);
        }
        return success;
    }
}
