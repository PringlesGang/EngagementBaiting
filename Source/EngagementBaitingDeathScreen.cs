using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.EngagementBaiting;

public enum DeathScreenFeedbackType
{
    Negative,
    Neutral,
    Positive
}

internal class DeathScreen
{
    private Dictionary<DeathScreenFeedbackType, List<String>> messages = new();
    private String currentMessage = null;

    private float showTime = 0.0f;
    private bool isShowing = false;

    private static Random rng = new Random();

    public DeathScreen() {
        void AddMessages(DeathScreenFeedbackType type, String filePath) {
            if (System.IO.File.Exists(filePath)) {
                messages[type] = new List<String>(System.IO.File.ReadAllLines(filePath));
            } else {
                Logger.Log(LogLevel.Error, "EngagementBaiting/DeathScreen", $"Message file not found: {filePath}");
                messages[type] = new List<String> { "Failed to load messages file" }; // Fallback string
            }
        }

        const String basePath = "./mods/EngagementBaiting/Assets";
        AddMessages(DeathScreenFeedbackType.Negative, Path.Combine(basePath, "negative_feedback.txt"));
        AddMessages(DeathScreenFeedbackType.Positive, Path.Combine(basePath, "positive_feedback.txt"));
    }

    public void Show() {
        if (!EngagementBaitingModule.Settings.Enabled) return;

        DeathScreenFeedbackType feedbackType = EngagementBaitingModule.Settings.FeedbackType;

        isShowing = true;
        showTime = 0.0f;

        if (feedbackType != DeathScreenFeedbackType.Neutral) {
            currentMessage = messages[feedbackType][rng.Next(messages[feedbackType].Count)];
            Logger.Log(LogLevel.Verbose, "EngagementBaiting/DeathScreen", $"Showing death screen message \"{currentMessage}\"");
        } else {
            currentMessage = null;
            Logger.Log(LogLevel.Verbose, "EngagementBaiting/DeathScreen", "Showing neutral death screen");
        }
    }

    public void Update() {
        if (!isShowing) return;

        showTime += Engine.RawDeltaTime;

        if (showTime >= EngagementBaitingModule.Settings.Duration) {
            isShowing = false;
            showTime = 0.0f;
        }
    }

    public void Render() {
        if (!isShowing || !EngagementBaitingModule.Settings.Enabled) return;

        float alpha = MathHelper.Clamp(showTime / EngagementBaitingModule.Settings.FadeInTime, 0.0f, 1.0f);
        Rectangle viewport = Draw.SpriteBatch.GraphicsDevice.Viewport.Bounds;

        Draw.SpriteBatch.Begin();

        Texture2D background = new Texture2D(Draw.SpriteBatch.GraphicsDevice, 1, 1);
        background.SetData(new Color[1] { Color.Black });
        Draw.SpriteBatch.Draw(background, viewport, new Color(Color.White, alpha));

        if (currentMessage != null) {
            ActiveFont.Draw(currentMessage, viewport.Center.ToVector2(),
                            new Vector2(0.5f, 0.5f), Vector2.One, new Color(Color.White, alpha));
        }

        Draw.SpriteBatch.End();
    }
}
