using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.EngagementBaiting
{
    internal class DeathScreen
    {
        private const float showDuration = 1.5f;
        private const float fadeInTime = 0.5f;

        private float showTime = 0.0f;
        private bool isShowing = false;

        public void Show() {
            Logger.Log(LogLevel.Verbose, "EngagementBaiting/DeathScreen", "Showing death screen");
            
            isShowing = true;
            showTime = 0.0f;
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

            Draw.SpriteBatch.Begin();

            float alpha = MathHelper.Clamp(showTime / fadeInTime, 0.0f, 1.0f);
            Color color = new Color(Color.White, alpha);

            MTexture feedback = GFX.Game["EngagementBaiting/negativeFeedback"];
            feedback.Draw(Vector2.Zero, Vector2.Zero, color);

            Draw.SpriteBatch.End();
        }
    }
}
