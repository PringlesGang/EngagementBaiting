using System;

namespace Celeste.Mod.EngagementBaiting;

public class EngagementBaitingModule : EverestModule {
    public static EngagementBaitingModule Instance { get; private set; }

    public override Type SettingsType => typeof(EngagementBaitingModuleSettings);
    public static EngagementBaitingModuleSettings Settings => (EngagementBaitingModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(EngagementBaitingModuleSession);
    public static EngagementBaitingModuleSession Session => (EngagementBaitingModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(EngagementBaitingModuleSaveData);
    public static EngagementBaitingModuleSaveData SaveData => (EngagementBaitingModuleSaveData) Instance._SaveData;

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
        // TODO: apply any hooks that should always be active
    }

    public override void Unload() {
        // TODO: unapply any hooks applied in Load()
    }
}