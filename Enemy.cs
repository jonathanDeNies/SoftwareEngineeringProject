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
        private bool wrap; // true = wrap around, false = bounce between borders

        // Add an activation flag so movement can be toggled externally
        public bool IsActive { get; set; } = true;

        // wrap defaults to false for bounce behaviour
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

        /// <summary>
        /// Update enemy position and handle bounce/wrap.
        /// If <paramref name="colliders"/> is provided, the enemy will treat them as solid walls and reverse direction on horizontal collision.
        /// Otherwise behavior falls back to screen-edge bounce/wrap (using <paramref name="screenWidth"/>).
        /// </summary>
        public void Update(GameTime gameTime, int screenWidth, IEnumerable<Rectangle>? colliders = null)
        {
            // If the enemy is not active, do not move; still update animation with isMoving=false
            if (!IsActive)
            {
                animation.Update(gameTime, false);
                return;
            }

            var frame = animation.CurrentFrame.SourceRectangle;
            int frameWidth = frame.Width;
            int frameHeight = frame.Height;

            // proposed new X after movement
            float nextX = position.X + speed;
            var nextBounds = new Rectangle((int)nextX, (int)position.Y, frameWidth, frameHeight);

            var collided = false;

            if (colliders != null)
            {
                foreach (var c in colliders)
                {
                    if (nextBounds.Intersects(c))
                    {
                        // horizontal collision with a tile: place enemy outside and reverse direction
                        if (speed > 0)
                        {
                            // moving right -> align right edge to tile's left
                            position.X = c.Left - frameWidth;
                        }
                        else
                        {
                            // moving left -> align left edge to tile's right
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
                // no tile collision — apply movement and fall back to screen bounce/wrap logic
                position.X = nextX;

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
            }

            // Pass isMoving argument to Animation.Update
            bool isMoving = speed != 0;
            animation.Update(gameTime, isMoving);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // flip horizontally when moving left (speed < 0)
            var effects = speed < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, position, animation.CurrentFrame.SourceRectangle, Color.White, 0f, Vector2.Zero, 1f, effects, 0f);
        }
    }
}
