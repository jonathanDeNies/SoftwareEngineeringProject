using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SoftwareEngineeringProject
{
    internal class Enemy
    {
        private Texture2D texture;
        private Animation animation;
        private Vector2 position;
        private float speed;
        private int spriteStartX;
        private int spriteStartY;
        private bool wrap; // true = wrap around, false = bounce between borders

        // wrap defaults to true for backward compatibility
        public Enemy(Texture2D texture, Vector2 startPosition, float speed, int spriteStartX, int spriteStartY, bool wrap = true)
        {
            this.texture = texture;
            this.speed = speed;
            this.position = startPosition;
            this.spriteStartX = spriteStartX;
            this.spriteStartY = spriteStartY;
            this.wrap = wrap;

            animation = new Animation();
            animation.AddFrame(new AnimationFrame(new Rectangle(spriteStartX, spriteStartY, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(spriteStartX + 32, spriteStartY, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(spriteStartX + 64, spriteStartY, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(spriteStartX + 96, spriteStartY, 32, 32)));
        }

        public void Update(GameTime gameTime)
        {
            position.X += speed;
            var frameWidth = animation.CurrentFrame.SourceRectangle.Width;
            int screenWidth = 700; // matches Game1 preferred back buffer

            if (wrap)
            {
                // wrap around when reaching the right edge
                if (position.X > screenWidth)
                {
                    position.X = -frameWidth;
                }
            }
            else
            {
                // bounce between left and right borders
                int maxX = screenWidth - frameWidth;
                if (position.X > maxX)
                {
                    position.X = maxX;
                    speed = -Math.Abs(speed);
                }
                else if (position.X < 0)
                {
                    position.X = 0;
                    speed = Math.Abs(speed);
                }
            }

            animation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, animation.CurrentFrame.SourceRectangle, Color.White);
        }
    }
}
