using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Celeste.Mod.EngagementBaiting;

internal static class EBLogger
{
    public static String LogPath { get; private set; } = null;

    private static FileStream fileStream;
    private static StreamWriter fileWriter;
    private static Mutex fileMutex = new();
    private static HashSet<Thread> logThreads = new();

    public static void NewFile()
    {
        CloseFile();

        // Construct file name
        String now = DateTime.Now.ToString("yyyyMMddHHmmss");
        LogPath = $"./Mods/EngagementBaiting/Logs/EngagementBaiting-{now}.log";

        // Ensure no overwrite
        int suffixNum = 0;
        while (File.Exists(LogPath))
        {
            suffixNum++;
            LogPath = $"./Mods/EngagementBaiting/EngagementBaiting-{now}-{suffixNum}.log";
        }

        // Open file stream
        try
        {
            fileStream = File.OpenWrite(LogPath);
        } catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "EngagementBaiting/Logger", $"Failed to open log file {LogPath}: {e}");
            fileStream = null;
        }

        fileWriter = (fileStream == null) ? null : new StreamWriter(fileStream);
    }

    public static void CloseFile()
    {
        if (LogPath == null) return;

        // Wait for all log threads to finish
        foreach (Thread thread in logThreads)
        {
            thread.Join();
        }
        logThreads.Clear();

        fileMutex.WaitOne();

        fileWriter?.Flush();
        fileWriter?.Dispose();
        fileWriter = null;

        fileStream?.Dispose();
        fileStream = null;

        // Automatically delete empty log files
        FileInfo fileInfo = new FileInfo(LogPath);
        if (fileInfo.Exists && fileInfo.Length == 0)
        {
            try
            {
                File.Delete(LogPath);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "EngagementBaiting/Logger", $"Failed to delete empty log file {LogPath}: {e}");
            }
        }

        fileMutex.ReleaseMutex();

        LogPath = null;
    }

    public static void Log(String message)
    {
        String logLine = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {message}";

        if (fileWriter == null)
        {
            Logger.Log(LogLevel.Warn, "EngagementBaiting/Logger", $"EB log file not open, cannot log message \"{logLine}\"");
            return;
        }

        Thread thread = new Thread(() => WriteLine(logLine));
        thread.Start();
        logThreads.Add(thread);
    }

    private static void WriteLine(String logLine)
    {
        fileMutex.WaitOne();
        fileWriter.WriteLine(logLine);
        fileMutex.ReleaseMutex();

        logThreads.Remove(Thread.CurrentThread);
    }
}
