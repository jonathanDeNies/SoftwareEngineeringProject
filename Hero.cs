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

        // visual position and physical velocity
        private Vector2 positie = new Vector2(0, 0);
        private Vector2 velocity = Vector2.Zero;

        // movement/physics tuning
        private const float MoveSpeed = 120f;
        private const float Gravity = 700f;
        private const float JumpVelocity = 360f;
        private const float TerminalVelocity = 900f;

        // hitbox insets
        private const int HitboxHorizontalInset = 6;
        private const int HitboxTopInset = 3;
        private const int HitboxBottomInset = 0;

        private KeyboardState previousKeyboardState;
        private bool isGrounded;

        // Optional: Jump buffering (improves "feel")
        private float jumpBufferTime = 0f;
        private const float MaxJumpBuffer = 0.15f; // 150ms window

        public Hero(Texture2D texture)
            : this(texture, new Rectangle(0, 0, 700, 700))
        { }

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
            var kb = Keyboard.GetState();

            // 1. Handle Horizontal Movement
            int dirX = 0;
            if (kb.IsKeyDown(Keys.Left)) dirX -= 1;
            if (kb.IsKeyDown(Keys.Right)) dirX += 1;
            velocity.X = dirX * MoveSpeed;

            // 2. Handle Jump Input & Buffering
            if (kb.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
            {
                jumpBufferTime = MaxJumpBuffer;
            }

            if (jumpBufferTime > 0)
            {
                jumpBufferTime -= dt;
                if (isGrounded)
                {
                    velocity.Y = -JumpVelocity;
                    isGrounded = false;
                    jumpBufferTime = 0;
                }
            }

            // 3. Apply Gravity
            velocity.Y += Gravity * dt;
            if (velocity.Y > TerminalVelocity) velocity.Y = TerminalVelocity;

            // 4. Integrate Position
            positie += velocity * dt;

            // 5. World Bounds Clamping
            ClampToWorld();

            animation.Update(gameTime);
            previousKeyboardState = kb;
        }

        private void ClampToWorld()
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            int maxX = worldBounds.Right - frame.Width;
            int maxY = worldBounds.Bottom - frame.Height;

            if (positie.X < worldBounds.X) positie.X = worldBounds.X;
            else if (positie.X > maxX) positie.X = maxX;

            if (positie.Y < worldBounds.Y) positie.Y = worldBounds.Y;
            else if (positie.Y > maxY)
            {
                positie.Y = maxY;
                velocity.Y = 0;
                isGrounded = true; // Touching the bottom of the world counts as grounded
            }
        }

        public Rectangle GetCollisionBounds()
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            return new Rectangle(
                (int)positie.X + HitboxHorizontalInset,
                (int)positie.Y + HitboxTopInset,
                Math.Max(0, frame.Width - HitboxHorizontalInset * 2),
                Math.Max(0, frame.Height - HitboxTopInset - HitboxBottomInset));
        }

        public void ResolveCollisions(IEnumerable<Rectangle> colliders)
        {
            if (colliders == null) return;

            // We use a local variable to track grounding during the loop
            bool foundGroundThisFrame = false;
            var bounds = GetCollisionBounds();

            foreach (var c in colliders)
            {
                if (!bounds.Intersects(c)) continue;

                var inter = Rectangle.Intersect(bounds, c);
                if (inter.Width == 0 || inter.Height == 0) continue;

                if (inter.Width < inter.Height)
                {
                    if (bounds.Center.X < c.Center.X) positie.X -= inter.Width;
                    else positie.X += inter.Width;
                    velocity.X = 0f;
                }
                else
                {
                    if (bounds.Center.Y < c.Center.Y) // Landed on top
                    {
                        positie.Y -= inter.Height;
                        velocity.Y = 0f;
                        foundGroundThisFrame = true;
                    }
                    else // Hit head
                    {
                        positie.Y += inter.Height;
                        velocity.Y = 0f;
                    }
                }
                bounds = GetCollisionBounds();
            }

            isGrounded = foundGroundThisFrame;
            ClampToWorld();
        }
    }
}