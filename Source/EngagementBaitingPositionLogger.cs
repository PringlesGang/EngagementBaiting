using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text;

namespace Celeste.Mod.EngagementBaiting;

internal static class PositionLogger {

    private static StreamWriter csv;
    public static string outPath { get; private set; } = null;

    public static void NewFile() {
        CloseFile();

        // Output CSV file in Mods folder
        outPath = Path.Combine(Everest.PathGame, "Mods", "PlayerPositions.csv");
        csv = new StreamWriter(new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
        csv.AutoFlush = true;
        csv.WriteLine("Timestamp,Level,X,Y,SessionTime,Deaths");
    }

    public static void CloseFile()
    {
        if (outPath == null) return;

        csv?.Flush();
        csv?.Dispose();
        csv = null;

        outPath = null;
    }

    public static void Log(Player self) {
        try {
            Vector2 pos = self.Position;
            Level level = self.Scene as Level;
            float sessionTime = 0f;
            int deaths = 0;
            string levelName = "unknown";

            if (level != null && level.Session != null) {
                levelName = level.Session.Level;
                sessionTime = level.Session.Time; // Everest 5846 exposes Session.Time
                deaths = level.Session.Deaths;
            }

            string row = $"{DateTime.UtcNow:o},{levelName},{pos.X:F3},{pos.Y:F3},{sessionTime:F3},{deaths}";
            csv.WriteLine(row);
        } catch (Exception e) {
            Logger.Log(LogLevel.Warn, "PositionLogger", "Error: " + e);
        }
    }
}
