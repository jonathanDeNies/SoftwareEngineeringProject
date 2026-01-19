using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        private bool wrap;

        public bool IsActive { get; set; } = true;
        
        public Enemy(Texture2D texture, Vector2 startPosition, float speed, int spriteStartX, int spriteStartY, bool wrap = false)
        {
            this.texture = texture ?? throw new ArgumentNullException(nameof(texture));
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
        public void Update(GameTime gameTime, int screenWidth, IEnumerable<Rectangle>? colliders = null)
        {
           
            if (!IsActive)
            {
                animation.Update(gameTime, false);
                return;
            }

            var frame = animation.CurrentFrame.SourceRectangle;
            int frameWidth = frame.Width;
            int frameHeight = frame.Height;

            float nextX = position.X + speed;
            var nextBounds = new Rectangle((int)nextX, (int)position.Y, frameWidth, frameHeight);

            var collided = false;

            if (colliders != null)
            {
                foreach (var c in colliders)
                {
                    if (nextBounds.Intersects(c))
                    {
                        if (speed > 0)
                        {
                            position.X = c.Left - frameWidth;
                        }
                        else
                        {
                            position.X = c.Right;
                        }

                        speed = -speed;
                        collided = true;
                        break;
                    }
                }
            }

            if (!collided)
            {
                position.X = nextX;

                if (wrap)
                {
                    if (position.X > screenWidth)
                    {
                        position.X = -frameWidth;
                    }
                }
                else
                {
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
            }

            bool isMoving = speed != 0;
            animation.Update(gameTime, isMoving);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var effects = speed < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, position, animation.CurrentFrame.SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, effects, 0f);
        }

        public Rectangle GetCollisionBounds()
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            return new Rectangle((int)position.X, (int)position.Y, frame.Width, frame.Height);
        }
    }
}
