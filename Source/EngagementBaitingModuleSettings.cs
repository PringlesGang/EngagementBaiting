namespace Celeste.Mod.EngagementBaiting;

public class EngagementBaitingModuleSettings : EverestModuleSettings {
    public bool Enabled { get; set; } = true;

    public DeathScreenFeedbackType FeedbackType { get; set; } = DeathScreenFeedbackType.Neutral;

    [SettingNumberInput(allowNegatives: false, maxLength: 3)]
    public float Duration { get; set; } = 1.5f;

    [SettingNumberInput(allowNegatives: false, maxLength: 3)]
    public float FadeInTime { get; set; } = 0.5f;
}