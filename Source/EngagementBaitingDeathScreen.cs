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
    private readonly Dictionary<DeathScreenFeedbackType, List<string>> messages = [];
    private readonly Stack<string> messageStack = new();
    private string currentMessage = null;
    private DeathScreenFeedbackType currentFeedbackType = DeathScreenFeedbackType.Neutral;

    private float showTime = 0.0f;
    private bool isShowing = false;

    private readonly static Random rng = new();

    public DeathScreen() {
        MInput.Disabled = false;

        void AddMessages(DeathScreenFeedbackType type, string filePath) {
            if (File.Exists(filePath)) {
                messages[type] = [.. File.ReadAllLines(filePath)];
            } else {
                Logger.Log(LogLevel.Error, "EngagementBaiting/DeathScreen", $"Message file not found: {filePath}");
                messages[type] = ["Failed to load messages file"]; // Fallback string
            }
        }

        const string basePath = "./Mods/EngagementBaiting/Assets";
        AddMessages(DeathScreenFeedbackType.Negative, Path.Combine(basePath, "negative_feedback.txt"));
        AddMessages(DeathScreenFeedbackType.Positive, Path.Combine(basePath, "positive_feedback.txt"));
    }

    private void SelectMessage(DeathScreenFeedbackType type) {
        if (type == DeathScreenFeedbackType.Neutral) {
            currentMessage = null;
            currentFeedbackType = DeathScreenFeedbackType.Neutral;
            EBLogger.Log("Showing neutral death screen");
            return;
        }

        if (type != currentFeedbackType || messageStack.Count == 0)
        {
            currentFeedbackType = type;

            // Refill stack with shuffled messages
            List<string> permutation = [.. messages[type]];
            permutation.Shuffle(rng);

            messageStack.Clear();
            foreach (string msg in permutation) {
                messageStack.Push(msg);
            }
        }

        currentMessage = messageStack.Pop();
        string typeString = type == DeathScreenFeedbackType.Negative ? "negative" : "positive";
        EBLogger.Log($"Showing {typeString} death screen message \"{currentMessage}\"");
    }

    public void Show() {
        if (!EngagementBaitingModule.Settings.Enabled) return;

        isShowing = true;
        showTime = 0.0f;
        DisableInput();

        SelectMessage(EngagementBaitingModule.Settings.FeedbackType);
    }

    public void Update() {
        if (!isShowing) return;

        showTime += Engine.RawDeltaTime;

        if (showTime >= EngagementBaitingModule.Settings.Duration)
        {
            isShowing = false;
            showTime = 0.0f;
            MInput.Disabled = false;
        }
    }

    public void Render()
    {
        if (!isShowing || !EngagementBaitingModule.Settings.Enabled) return;

        float alpha = MathHelper.Clamp(showTime / EngagementBaitingModule.Settings.FadeInTime, 0.0f, 1.0f);
        Rectangle viewport = Draw.SpriteBatch.GraphicsDevice.Viewport.Bounds;

        Draw.SpriteBatch.Begin();

        Texture2D background = new(Draw.SpriteBatch.GraphicsDevice, 1, 1);
        background.SetData([Color.Black]);
        try
        {
            Draw.SpriteBatch.Draw(background, viewport, Color.White * alpha);

            if (currentMessage != null)
            {
                ActiveFont.Draw(currentMessage, viewport.Center.ToVector2(),
                                new Vector2(0.5f, 0.5f), Vector2.One, Color.White * alpha);
            }
        }
        catch (Exception)
        {
        }
        Draw.SpriteBatch.End();
    }

    private static void DisableInput()
    {
        MInput.Disabled = true;

        // Reset all input
        // (Buttons are handled by `MInput.Disabled = true`)
        Input.MoveX.Value = 0;
        Input.MoveY.Value = 0;
        Input.GliderMoveY.Value = 0;

        Input.Aim.Value = Vector2.Zero;
        Input.Feather.Value = Vector2.Zero;
        Input.MountainAim.Value = Vector2.Zero;
    }
}
