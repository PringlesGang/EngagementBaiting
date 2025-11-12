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
        FileManager.Init();
        FileManager.BackupFiles();

        On.Celeste.HudRenderer.RenderContent += OnHudRenderHook;

        On.Celeste.Player.Update += OnPlayerUpdateHook;
        On.Celeste.Player.Die += OnDeathHook;
        On.Celeste.Level.DoScreenWipe += ScreenWipeHook;

        On.Celeste.Level.LoadLevel += OnLoadScreenHook;
        On.Celeste.LevelExit.ctor += OnLevelExitHook;

        On.Celeste.Celeste.OnExiting += OnGameExitHook;
    }

    public override void Unload() {
        On.Celeste.HudRenderer.RenderContent -= OnHudRenderHook;

        On.Celeste.Player.Update -= OnPlayerUpdateHook;
        On.Celeste.Player.Die -= OnDeathHook;
        On.Celeste.Level.DoScreenWipe -= ScreenWipeHook;

        On.Celeste.Level.LoadLevel -= OnLoadScreenHook;
        On.Celeste.LevelExit.ctor -= OnLevelExitHook;

        On.Celeste.Celeste.OnExiting -= OnGameExitHook;

        FileManager.BackupFiles();
    }

    private void OnPlayerUpdateHook(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        PositionLogger.Log(self);
    }

    private PlayerDeadBody OnDeathHook(On.Celeste.Player.orig_Die orig, Player self,
                                              Vector2 direction, bool evenIfInvinsible,
                                              bool registerDeathInStats)
    {
        deathScreen.Show();
        EBLogger.Log($"The player died at {self.Position.ToString()}");

        return orig(self, direction, evenIfInvinsible, registerDeathInStats);
    }

    private void ScreenWipeHook(On.Celeste.Level.orig_DoScreenWipe orig, Level self,
                                bool wipeIn, Action onComplete = null, bool hiresSnow = false)
    {
        orig(self, wipeIn, onComplete, hiresSnow);

        if (Settings.Enabled && deathScreen.IsShowing && self.Wipe != null)
        {
            self.Wipe.Cancel();
            deathScreen.onComplete = onComplete;
        }
    }

    private void OnHudRenderHook(On.Celeste.HudRenderer.orig_RenderContent orig, HudRenderer self, Monocle.Scene scene) {
        orig(self, scene);

        deathScreen.Update();
        deathScreen.Render();

        FileManager.Update();
        FileManager.Render();
    }

    private void OnLoadScreenHook(On.Celeste.Level.orig_LoadLevel orig, Level level, Player.IntroTypes playerIntroType, bool isFromLoader)
    {
        EBLogger.Log($"Entering screen \"{level.Session.Level}\"");
        FileManager.AreasPlayed.Add(level.Session.Area.ID);

        orig(level, playerIntroType, isFromLoader);
    }

    private void OnLevelExitHook(On.Celeste.LevelExit.orig_ctor orig, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
    {
        EBLogger.Log("Ending level");
        EBLogger.CloseFile();
        FileManager.AreasPlayed.Add(session.Area.ID);

        orig(exit, mode, session, snow);
    }

    private void OnGameExitHook(On.Celeste.Celeste.orig_OnExiting orig, Celeste self, object sender, System.EventArgs args)
    {
        EBLogger.CloseFile();
        PositionLogger.CloseFile();
        FileManager.BackupFiles();

        orig(self, sender, args);
    }
}
