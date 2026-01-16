using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoftwareEngineeringProject.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    internal class Hero: IGameObject
    {
        private Texture2D texture;
        Animation animation;
        
        private int schuifOp_X = 0;

        private Vector2 positie = new Vector2(0, 0);
        private Vector2 snelheid = new Vector2(2,2);
        private Vector2 versnelling = new Vector2(0.1f, 0.1f);

        private const int HitboxHorizontalInset = 6;

        public Hero(Texture2D texture)
        {
            this.texture = texture;
            animation = new Animation();
            animation.AddFrame(new AnimationFrame(new Rectangle(0, 0, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(32, 0, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(64, 0, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(96, 0, 32, 32)));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, positie, animation.CurrentFrame.SourceRectangle, Color.White);
        }

        public void Update(GameTime gameTime)
        {
            animation.Update(gameTime);
            MoveWithKeyboard(gameTime);
        }

        private Vector2 Limit(Vector2 v, float max)
        {
            if(v.Length() > max)
            {
                var ratio = max /v.Length();
                v.X *= ratio;
                v.Y *= ratio;
            }
            return v;
        }

        private void MoveWithKeyboard(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            var direction = Vector2.Zero;
            if (state.IsKeyDown(Keys.Left))
            {
                direction.X -= 1;
            }
            if (state.IsKeyDown(Keys.Right))
            {
                direction.X += 1;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                direction.Y -= 1;
            }
            if (state.IsKeyDown(Keys.Down))
            {
                direction.Y += 1;
            }

            direction *= snelheid;
            positie += direction;

            // de collision with screen bounds (kept as final clamp, collisions with tiles handled separately)
            var frame = animation.CurrentFrame.SourceRectangle;
            int minX = 0;
            int minY = 0;
            int maxX = 700 - frame.Width;
            int maxY = 700 - frame.Height;
            if (positie.X < minX) positie.X = minX;
            else if (positie.X > maxX) positie.X = maxX;
            if (positie.Y < minY) positie.Y = minY;
            else if (positie.Y > maxY) positie.Y = maxY;

            animation.Update(gameTime);
        }

        /// <summary>
        /// Resolve overlaps with a set of collider rectangles. Uses an inset horizontal hitbox so the player
        /// can get closer to tiles horizontally. Call after movement in Game1.Update.
        /// </summary>
        public void ResolveCollisions(IEnumerable<Rectangle> colliders)
        {
            if (colliders == null) return;

            var frame = animation.CurrentFrame.SourceRectangle;

            // Use an inset rectangle for collisions so horizontal hitbox is thinner than the sprite.
            var bounds = new Rectangle(
                (int)positie.X + HitboxHorizontalInset,
                (int)positie.Y,
                Math.Max(0, frame.Width - HitboxHorizontalInset * 2),
                frame.Height);

            foreach (var c in colliders)
            {
                if (!bounds.Intersects(c)) continue;

                var inter = Rectangle.Intersect(bounds, c);
                if (inter.Width == 0 || inter.Height == 0) continue;

                // push out on the smaller penetration axis
                if (inter.Width < inter.Height)
                {
                    // horizontal push
                    if (bounds.Center.X < c.Center.X)
                    {
                        // move hero left by penetration amount (adjusting by inset keeps sprite flush with tile)
                        positie.X -= inter.Width;
                    }
                    else
                    {
                        positie.X += inter.Width;
                    }
                }
                else
                {
                    // vertical push
                    if (bounds.Center.Y < c.Center.Y)
                    {
                        positie.Y -= inter.Height;
                    }
                    else
                    {
                        positie.Y += inter.Height;
                    }
                }

                // update bounds after moving
                bounds = new Rectangle(
                    (int)positie.X + HitboxHorizontalInset,
                    (int)positie.Y,
                    Math.Max(0, frame.Width - HitboxHorizontalInset * 2),
                    frame.Height);
            }
        }
    }
}
