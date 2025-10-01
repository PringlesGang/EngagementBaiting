using Microsoft.Xna.Framework;

namespace Celeste.Mod.EngagementBaiting
{
    internal class EngagementBaitingDeathScreen
    {
        public static PlayerDeadBody ShowDeathScreen(On.Celeste.Player.orig_Die orig, Player self,
            Vector2 direction, bool evenIfInvinsible, bool registerDeathInStats) {

            Logger.Log(LogLevel.Verbose, "EngagementBaiting/DeathScreen", "Showing death screen");

            return orig(self, direction, evenIfInvinsible, registerDeathInStats);
        }
    }
}
