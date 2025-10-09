using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.EngagementBaiting;

public class EngagementBaitingModule : EverestModule {
    public static EngagementBaitingModule Instance { get; private set; }

    public override Type SettingsType => typeof(EngagementBaitingModuleSettings);
    public static EngagementBaitingModuleSettings Settings => (EngagementBaitingModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(EngagementBaitingModuleSession);
    public static EngagementBaitingModuleSession Session => (EngagementBaitingModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(EngagementBaitingModuleSaveData);
    public static EngagementBaitingModuleSaveData SaveData => (EngagementBaitingModuleSaveData) Instance._SaveData;

    private DeathScreen deathScreen = new();

    public EngagementBaitingModule() {
        Instance = this;

#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(EngagementBaitingModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(EngagementBaitingModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        EBLogger.NewFile();

        // TODO: apply any hooks that should always be active
        On.Celeste.HudRenderer.RenderContent += OnHudRenderHook;
        On.Celeste.Player.Die += OnDeathHook;
    }

    public override void Unload() {
        EBLogger.CloseFile();

        // TODO: unapply any hooks applied in Load()
        On.Celeste.HudRenderer.RenderContent -= OnHudRenderHook;
        On.Celeste.Player.Die -= OnDeathHook;
    }

    private PlayerDeadBody OnDeathHook(On.Celeste.Player.orig_Die orig, Player self,
                                              Vector2 direction, bool evenIfInvinsible,
                                              bool registerDeathInStats) {
        deathScreen.Show();
        EBLogger.Log($"The player died at {self.Position.ToString()}");

        return orig(self, direction, evenIfInvinsible, registerDeathInStats);
    }

    private void OnHudRenderHook(On.Celeste.HudRenderer.orig_RenderContent orig, HudRenderer self, Monocle.Scene scene) {
        orig(self, scene);

        deathScreen.Update();
        deathScreen.Render();
    }
}
