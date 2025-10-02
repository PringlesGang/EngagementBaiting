using System;
using System.IO;
using System.Text;
using Celeste;
using Celeste.Mod;
using Monocle;
using Microsoft.Xna.Framework;

namespace PositionLogger {
    public class PositionLoggerModule : EverestModule {
        private StreamWriter csv;
        private string outPath;

        public override void Load() {
            // Output CSV file in Mods folder
            outPath = Path.Combine(Everest.PathGame, "Mods", "PlayerPositions.csv");
            csv = new StreamWriter(new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
            csv.AutoFlush = true;
            csv.WriteLine("Timestamp,Level,X,Y,SessionTime,Deaths");

            // Hook into player update
            On.Celeste.Player.Update += Player_Update;
        }

        public override void Unload() {
            On.Celeste.Player.Update -= Player_Update;
            csv?.Dispose();
        }

        private void Player_Update(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self); // run original

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
                Logger.Log("PositionLogger", "Error: " + e);
            }
        }
    }
}
