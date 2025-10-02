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

            float alpha = MathHelper.Clamp(showTime / fadeInTime, 0.0f, 1.0f);
            Rectangle viewport = Draw.SpriteBatch.GraphicsDevice.Viewport.Bounds;

            Draw.SpriteBatch.Begin();

            Texture2D background = new Texture2D(Draw.SpriteBatch.GraphicsDevice, 1, 1);
            background.SetData(new Color[1] { Color.Black });
            Draw.SpriteBatch.Draw(background, viewport, new Color(Color.White, alpha));

            ActiveFont.Draw("Haha get fucked", viewport.Center.ToVector2(), new Vector2(0.5f, 0.5f),
                            Vector2.One, new Color(Color.White, alpha));

            Draw.SpriteBatch.End();
        }
    }
}
