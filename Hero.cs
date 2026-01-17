using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoftwareEngineeringProject.Interfaces;
using System;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    internal class Hero : IGameObject
    {
        private readonly Texture2D texture;
        private readonly Rectangle worldBounds;

        private Animation animation;

        // visual position and physical velocity (pixels)
        private Vector2 positie = new Vector2(0, 0);
        private Vector2 velocity = Vector2.Zero;

        // movement/physics tuning (pixels / second, pixels / second^2)
        private const float MoveSpeed = 120f;
        // gravity reduced from 900f -> 700f for a slightly floatier jump
        private const float Gravity = 700f;
        private const float JumpVelocity = 360f;
        private const float TerminalVelocity = 900f;

        // hitbox insets to align visual sprite with physics
        private const int HitboxHorizontalInset = 6;
        private const int HitboxTopInset = 3;
        private const int HitboxBottomInset = 0;

        // input tracking to detect single-press jump
        private KeyboardState previousKeyboardState;

        // state
        private bool isGrounded;

        // Backwards-compatible ctor
        public Hero(Texture2D texture)
            : this(texture, new Rectangle(0, 0, 700, 700))
        { }

        // Preferred ctor: supply the world bounds so clamping uses current viewport/camera
        public Hero(Texture2D texture, Rectangle worldBounds)
        {
            this.texture = texture ?? throw new ArgumentNullException(nameof(texture));
            this.worldBounds = worldBounds;

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
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // handle input
            var kb = Keyboard.GetState();
            int dirX = 0;
            if (kb.IsKeyDown(Keys.Left)) dirX -= 1;
            if (kb.IsKeyDown(Keys.Right)) dirX += 1;

            // horizontal velocity is immediate (no acceleration model here)
            velocity.X = dirX * MoveSpeed;

            // jump: on key press and only when grounded
            bool jumpPressed = kb.IsKeyDown(Keys.Space);
            bool jumpJustPressed = jumpPressed && previousKeyboardState.IsKeyUp(Keys.Space);
            if (jumpJustPressed && isGrounded)
            {
                // preserve horizontal velocity — jumping while moving is allowed
                velocity.Y = -JumpVelocity;
                isGrounded = false;
            }

            // apply (reduced) gravity
            velocity.Y += Gravity * dt;
            if (velocity.Y > TerminalVelocity) velocity.Y = TerminalVelocity;

            // integrate position
            positie += velocity * dt;

            // clamp to world bounds using sprite frame size
            var frame = animation.CurrentFrame.SourceRectangle;
            int maxX = worldBounds.Right - frame.Width;
            int maxY = worldBounds.Bottom - frame.Height;
            if (positie.X < worldBounds.X) positie.X = worldBounds.X;
            else if (positie.X > maxX) positie.X = maxX;
            if (positie.Y < worldBounds.Y) positie.Y = worldBounds.Y;
            else if (positie.Y > maxY) positie.Y = maxY;

            animation.Update(gameTime);

            // store keyboard state for next frame
            previousKeyboardState = kb;
        }

        // expose collision rectangle for debugging / drawing
        public Rectangle GetCollisionBounds()
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            return new Rectangle(
                (int)positie.X + HitboxHorizontalInset,
                (int)positie.Y + HitboxTopInset,
                Math.Max(0, frame.Width - HitboxHorizontalInset * 2),
                Math.Max(0, frame.Height - HitboxTopInset - HitboxBottomInset));
        }

        /// <summary>
        /// Resolve overlaps with tile colliders. Should be called after Update().
        /// This resolves axis-penetration and updates velocity/isGrounded accordingly.
        /// </summary>
        public void ResolveCollisions(IEnumerable<Rectangle> colliders)
        {
            if (colliders == null) return;

            // start assuming not grounded; landing detection will set this true
            isGrounded = false;

            var frame = animation.CurrentFrame.SourceRectangle;
            var bounds = GetCollisionBounds();

            foreach (var c in colliders)
            {
                if (!bounds.Intersects(c)) continue;

                var inter = Rectangle.Intersect(bounds, c);
                if (inter.Width == 0 || inter.Height == 0) continue;

                if (inter.Width < inter.Height)
                {
                    // horizontal penetration: push out on X and zero horizontal velocity
                    if (bounds.Center.X < c.Center.X)
                    {
                        // push left
                        positie.X -= inter.Width;
                    }
                    else
                    {
                        // push right
                        positie.X += inter.Width;
                    }

                    velocity.X = 0f;
                }
                else
                {
                    // vertical penetration: push out on Y and zero vertical velocity
                    if (bounds.Center.Y < c.Center.Y)
                    {
                        // hero is above collider -> landed on top
                        positie.Y -= inter.Height;
                        velocity.Y = 0f;
                        isGrounded = true;
                    }
                    else
                    {
                        // hero is below collider -> hit head
                        positie.Y += inter.Height;
                        velocity.Y = 0f;
                    }
                }

                // update collision bounds after adjustment
                bounds = GetCollisionBounds();
            }

            // final clamp to world bounds
            int maxX = worldBounds.Right - frame.Width;
            int maxY = worldBounds.Bottom - frame.Height;
            if (positie.X < worldBounds.X) positie.X = worldBounds.X;
            else if (positie.X > maxX) positie.X = maxX;
            if (positie.Y < worldBounds.Y) positie.Y = worldBounds.Y;
            else if (positie.Y > maxY) positie.Y = maxY;
        }
    }
}
