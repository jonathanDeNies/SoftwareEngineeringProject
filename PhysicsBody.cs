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
        private Vector2 previousPosition;
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
            float jumpVelocity = 400f,
            float terminalVelocity = 900f,
            int hitboxHorizontalInset = 6,
            int hitboxTopInset = 3,
            int hitboxBottomInset = 0,
            float maxJumpBuffer = 0.15f)
        {
            this.position = startPosition;
            this.previousPosition = startPosition;
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

            // store previous position for one-way platform checks
            previousPosition = position;

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

        private Rectangle GetCollisionBoundsAt(Vector2 pos, int frameWidth, int frameHeight)
        {
            return new Rectangle(
                (int)pos.X + hitboxHorizontalInset,
                (int)pos.Y + hitboxTopInset,
                Math.Max(0, frameWidth - hitboxHorizontalInset * 2),
                Math.Max(0, frameHeight - hitboxTopInset - hitboxBottomInset));
        }

        /// <summary>
        /// Resolve axis-penetration against tile colliders. Call after Update().
        /// Accepts full-solid colliders and one-way platform colliders (passable from below).
        /// </summary>
        public void ResolveCollisions(IEnumerable<Rectangle> solidColliders, IEnumerable<Rectangle> oneWayColliders, int frameWidth, int frameHeight)
        {
            if (solidColliders == null && oneWayColliders == null) return;

            bool foundGroundThisFrame = false;
            var bounds = GetCollisionBounds(frameWidth, frameHeight);
            var prevBounds = GetCollisionBoundsAt(previousPosition, frameWidth, frameHeight);

            // 1) resolve against full solid tiles (same behavior as before)
            if (solidColliders != null)
            {
                foreach (var c in solidColliders)
                {
                    if (!bounds.Intersects(c)) continue;

                    var inter = Rectangle.Intersect(bounds, c);
                    if (inter.Width == 0 || inter.Height == 0) continue;

                    if (inter.Width < inter.Height)
                    {
                        // horizontal penetration
                        if (bounds.Center.X < c.Center.X)
                            position.X -= inter.Width;
                        else
                            position.X += inter.Width;

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

                    // update bounds after correction
                    bounds = GetCollisionBounds(frameWidth, frameHeight);
                    prevBounds = GetCollisionBoundsAt(previousPosition, frameWidth, frameHeight);
                }
            }

            // 2) resolve one-way platforms: only when coming from above (previous bottom <= platform top) and falling
            if (oneWayColliders != null)
            {
                foreach (var p in oneWayColliders)
                {
                    // only consider if we are overlapping now
                    if (!bounds.Intersects(p)) continue;

                    // only block if we were above the platform in the previous frame and are moving down (or stationary)
                    if (prevBounds.Bottom <= p.Top && velocity.Y >= 0f)
                    {
                        // compute collision height (intersection)
                        var inter = Rectangle.Intersect(bounds, p);
                        if (inter.Height == 0) continue;

                        // push out vertically so collision box sits on top of platform
                        // place position so that collision bounds bottom equals platform top:
                        int boundsHeight = frameHeight - hitboxTopInset - hitboxBottomInset;
                        position.Y = p.Top - boundsHeight - hitboxTopInset;

                        velocity.Y = 0f;
                        foundGroundThisFrame = true;

                        // refresh bounds after change
                        bounds = GetCollisionBounds(frameWidth, frameHeight);
                        prevBounds = GetCollisionBoundsAt(previousPosition, frameWidth, frameHeight);
                    }
                }
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

        public void StopHorizontalMovement()
        {
            velocity.X = 0f;
        }

        public void Reset(Vector2 startPosition)
        {
            position = startPosition;
            previousPosition = startPosition;
            velocity = Vector2.Zero;
            isGrounded = false;
            jumpBufferTime = 0f;
            previousKeyboardState = new KeyboardState();
        }

        public void SetWorldBounds(Rectangle bounds) => worldBounds = bounds;
    }
}
