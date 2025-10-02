using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.EngagementBaiting
{
    internal class DeathScreen
    {
        private const float showDuration = 1.5f;
        private const float fadeInTime = 0.5f;

        private float showTime = 0.0f;
        private bool isShowing = false;

        private List<string> messages = new List<string>();
        private int currentMessageId = 0;

        private static Random rng = new Random();

        public DeathScreen() {
            const string messagePath = "./Mods/EngagementBaiting/Assets/negative_feedback.txt";
            if (System.IO.File.Exists(messagePath)) {
                messages.AddRange(System.IO.File.ReadAllLines(messagePath));
            } else {
                Logger.Log(LogLevel.Error, "EngagementBaiting/DeathScreen", $"Message file not found: {messagePath}");
                messages.Add("Failed to load messages file"); // Fallback string
            }
        }

        public void Show() {
            isShowing = true;
            showTime = 0.0f;

            currentMessageId = rng.Next(messages.Count);

            Logger.Log(LogLevel.Verbose, "EngagementBaiting/DeathScreen", $"Showing death screen message \"{messages[currentMessageId]}\"");
        }

        public void Update() {
            if (!isShowing) return;

            showTime += Engine.RawDeltaTime;

            if (showTime >= showDuration) {
                isShowing = false;
                showTime = 0.0f;
            }
        }

        public void Render() {
            if (!isShowing) return;

            float alpha = MathHelper.Clamp(showTime / fadeInTime, 0.0f, 1.0f);
            Rectangle viewport = Draw.SpriteBatch.GraphicsDevice.Viewport.Bounds;

            Draw.SpriteBatch.Begin();

            Texture2D background = new Texture2D(Draw.SpriteBatch.GraphicsDevice, 1, 1);
            background.SetData(new Color[1] { Color.Black });
            Draw.SpriteBatch.Draw(background, viewport, new Color(Color.White, alpha));

            ActiveFont.Draw(messages[currentMessageId], viewport.Center.ToVector2(),
                            new Vector2(0.5f, 0.5f), Vector2.One, new Color(Color.White, alpha));

            Draw.SpriteBatch.End();
        }
    }
}
