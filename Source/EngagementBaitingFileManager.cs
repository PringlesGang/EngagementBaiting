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

    private static string CurrentId = null;
    private const uint IdLength = 5;

    private static Random rng = new();

    public static void Init()
    {
        GenerateId();
    }

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
        float saveMsgAlpha = 1.0f;
        if (DisplayTime < FadeDuration)
        {
            saveMsgAlpha = DisplayTime / FadeDuration;
        }
        else if (DisplayTime > DisplayDuration - FadeDuration)
        {
            saveMsgAlpha = (DisplayDuration - DisplayTime) / FadeDuration;
        }

        Vector2 saveMsgJustify = new Vector2(1.0f + 15.0f / Engine.Width, 15.0f / Engine.Height);

        Draw.SpriteBatch.Begin();
        try
        {
            ActiveFont.Draw(CurrentId, new Vector2(5.0f, 5.0f), Vector2.Zero, Vector2.One * 0.3f, Color.Gray * 0.7f);
            ActiveFont.Draw(DisplayMessage, new Vector2(Engine.Width, 0.0f), saveMsgJustify,
                            new Vector2(0.5f, 0.5f), Color.White * saveMsgAlpha);
        }
        catch (Exception)
        {
        }
        Draw.SpriteBatch.End();
    }

    private static void CloseFiles()
    {
        EBLogger.CloseFile();
        PositionLogger.CloseFile();
    }

    public static void BackupFiles()
    {
        CloseFiles();

        bool success = true;

        string nowString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string destDir = Path.Combine(DestPath, $"{nowString}_{CurrentId}");
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
            // Create a text file with the experiment ID, just to be safe
            success &= CreateFile(Path.Combine(destDir, $"{CurrentId}.txt"), $"Experiment ID: {CurrentId}");

            DisplayMessage = success ? $"Saved logs for {CurrentId}" : "Failed to save logs";

            if (success) GenerateId();
        }
        DisplayTime = 0.0f;
    }

    private static void GenerateId()
    {
        /*
        Chances of collision after n trials is
            1 - (64^IdLength)! / ((64^IdLength - n)! * (64^IdLength)^n)
        With IdLength=5, chances of collision are:
            n= 100 -> ~4.39e-6
            n=1000 -> ~4.61e-4
            n=5000 -> ~0.012
        I think we're good
        */

        // Not to-spec base64, but it has to be filename-safe
        const string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+=";

        CurrentId = "";
        for (uint i = 0; i < IdLength; i++)
        {
            int curVal = rng.Next(0, 64);
            CurrentId += base64Chars[curVal];
        }
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

    private static bool CreateFile(string path, string contents = null)
    {
        try
        {
            FileStream fs = File.Create(path);

            if (contents != null)
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(contents);
                }
            }

            fs.Close();
            return true;
        } catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/FileManager", $"Failed to create file \"{path}\": {e}");
            return false;
        }
    }
}
