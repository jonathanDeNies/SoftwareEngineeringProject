using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    /// <summary>
    /// Encapsulates movement, gravity, jumping and collision resolution for a single body.
    /// Hero will compose this instead of owning physics logic itself.
    /// </summary>
    internal sealed class PhysicsBody
    {
        private Vector2 position;
        private Vector2 velocity;
        private Rectangle worldBounds;

        private readonly float moveSpeed;
        private readonly float gravity;
        private readonly float jumpVelocity;
        private readonly float terminalVelocity;

        private readonly int hitboxHorizontalInset;
        private readonly int hitboxTopInset;
        private readonly int hitboxBottomInset;

        // input/behavior state
        private KeyboardState previousKeyboardState;
        private float jumpBufferTime;
        private readonly float maxJumpBuffer;

        private bool isGrounded;

        public PhysicsBody(
            Vector2 startPosition,
            Rectangle worldBounds,
            float moveSpeed = 175f,
            float gravity = 1150f,
            float jumpVelocity = 360f,
            float terminalVelocity = 900f,
            int hitboxHorizontalInset = 6,
            int hitboxTopInset = 3,
            int hitboxBottomInset = 0,
            float maxJumpBuffer = 0.15f)
        {
            this.position = startPosition;
            this.worldBounds = worldBounds;
            this.moveSpeed = moveSpeed;
            this.gravity = gravity;
            this.jumpVelocity = jumpVelocity;
            this.terminalVelocity = terminalVelocity;
            this.hitboxHorizontalInset = hitboxHorizontalInset;
            this.hitboxTopInset = hitboxTopInset;
            this.hitboxBottomInset = hitboxBottomInset;
            this.maxJumpBuffer = maxJumpBuffer;
            this.previousKeyboardState = new KeyboardState();
            this.jumpBufferTime = 0f;
            this.velocity = Vector2.Zero;
            this.isGrounded = false;
        }

        public Vector2 Position => position;
        public Vector2 Velocity => velocity;
        public bool IsGrounded => isGrounded;

        /// <summary>
        /// Handles input, gravity and integrates position. Call each frame before resolving collisions.
        /// </summary>
        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Horizontal input
            int dirX = 0;
            if (keyboard.IsKeyDown(Keys.Left)) dirX -= 1;
            if (keyboard.IsKeyDown(Keys.Right)) dirX += 1;
            velocity.X = dirX * moveSpeed;

            // Jump buffering: set buffer when space is pressed this frame
            if (keyboard.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
            {
                jumpBufferTime = maxJumpBuffer;
            }

            // If buffered jump and grounded -> jump
            if (jumpBufferTime > 0f)
            {
                jumpBufferTime -= dt;
                if (isGrounded)
                {
                    velocity.Y = -jumpVelocity;
                    isGrounded = false;
                    jumpBufferTime = 0f;
                }
            }

            // Gravity
            velocity.Y += gravity * dt;
            if (velocity.Y > terminalVelocity) velocity.Y = terminalVelocity;

            // Integrate
            position += velocity * dt;

            // Clamp to world rectangle (sprite size needed by caller if clamping must use sprite dims)
            // We keep general clamping using worldBounds origin; caller can re-clamp after supplying sprite size.
            if (position.X < worldBounds.X) position.X = worldBounds.X;
            if (position.Y < worldBounds.Y) position.Y = worldBounds.Y;

            previousKeyboardState = keyboard;
        }

        /// <summary>
        /// Returns the collision rectangle based on the sprite frame size and configured insets.
        /// </summary>
        public Rectangle GetCollisionBounds(int frameWidth, int frameHeight)
        {
            return new Rectangle(
                (int)position.X + hitboxHorizontalInset,
                (int)position.Y + hitboxTopInset,
                Math.Max(0, frameWidth - hitboxHorizontalInset * 2),
                Math.Max(0, frameHeight - hitboxTopInset - hitboxBottomInset));
        }

        /// <summary>
        /// Resolve axis-penetration against tile colliders. Call after Update().
        /// frameWidth/frameHeight are required to compute the collision rectangle used for resolution.
        /// Updates internal position, velocity and grounded state.
        /// </summary>
        public void ResolveCollisions(IEnumerable<Rectangle> colliders, int frameWidth, int frameHeight)
        {
            if (colliders == null) return;

            bool foundGroundThisFrame = false;
            var bounds = GetCollisionBounds(frameWidth, frameHeight);

            foreach (var c in colliders)
            {
                if (!bounds.Intersects(c)) continue;

                var inter = Rectangle.Intersect(bounds, c);
                if (inter.Width == 0 || inter.Height == 0) continue;

                if (inter.Width < inter.Height)
                {
                    // horizontal penetration
                    if (bounds.Center.X < c.Center.X)
                    {
                        // push left
                        position.X -= inter.Width;
                    }
                    else
                    {
                        // push right
                        position.X += inter.Width;
                    }

                    // stop horizontal movement
                    velocity.X = 0f;
                }
                else
                {
                    // vertical penetration
                    if (bounds.Center.Y < c.Center.Y)
                    {
                        // landed on top
                        position.Y -= inter.Height;
                        velocity.Y = 0f;
                        foundGroundThisFrame = true;
                    }
                    else
                    {
                        // hit head
                        position.Y += inter.Height;
                        velocity.Y = 0f;
                    }
                }

                // update bounds after correction (important when multiple colliders overlap)
                bounds = GetCollisionBounds(frameWidth, frameHeight);
            }

            isGrounded = foundGroundThisFrame;

            // final clamp to world bounds using sprite size
            int maxX = worldBounds.Right - frameWidth;
            int maxY = worldBounds.Bottom - frameHeight;
            if (position.X < worldBounds.X) position.X = worldBounds.X;
            else if (position.X > maxX) position.X = maxX;
            if (position.Y < worldBounds.Y) position.Y = worldBounds.Y;
            else if (position.Y > maxY)
            {
                position.Y = maxY;
                velocity.Y = 0f;
                isGrounded = true;
            }
        }

        /// <summary>
        /// Allows external code to set world bounds (useful if viewport changes).
        /// </summary>
        public void SetWorldBounds(Rectangle bounds) => worldBounds = bounds;
    }
}
