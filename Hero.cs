using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoftwareEngineeringProject.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace SoftwareEngineeringProject
{
    internal class Hero : IGameObject
    {
        private readonly Texture2D texture;
        private readonly Animation animation;

        private readonly PhysicsBody physics;

        private const int HitboxHorizontalInset = 6;
        private const int HitboxTopInset = 12;
        private const int HitboxBottomInset = 0;

        private SpriteEffects facing = SpriteEffects.None;

        public Hero(Texture2D texture)
            : this(texture, new Rectangle(0, 0, 700, 700))
        { }

        public Hero(Texture2D texture, Rectangle worldBounds)
        {
            this.texture = texture ?? throw new ArgumentNullException(nameof(texture));
            animation = new Animation();
            animation.AddFrame(new AnimationFrame(new Rectangle(0, 0, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(32, 0, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(64, 0, 32, 32)));
            animation.AddFrame(new AnimationFrame(new Rectangle(96, 0, 32, 32)));

            physics = new PhysicsBody(
                startPosition: Vector2.Zero,
                worldBounds: worldBounds,
                hitboxHorizontalInset: HitboxHorizontalInset,
                hitboxTopInset: HitboxTopInset,
                hitboxBottomInset: HitboxBottomInset);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture,
                physics.Position,
                animation.CurrentFrame.SourceRectangle,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                facing,
                0f
            );
        }


        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            physics.Update(gameTime, keyboardState);

            if (physics.Velocity.X > 0.05f)
                facing = SpriteEffects.None;
            else if (physics.Velocity.X < -0.05f)
                facing = SpriteEffects.FlipHorizontally; 

            bool isMoving = Math.Abs(physics.Velocity.X) > 0.05f;
            animation.Update(gameTime, isMoving);
        }

        public Rectangle GetCollisionBounds()
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            return physics.GetCollisionBounds(frame.Width, frame.Height);
        }

        public void ResolveCollisions(IEnumerable<Rectangle> solidColliders, IEnumerable<Rectangle> oneWayColliders)
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            physics.ResolveCollisions(solidColliders, oneWayColliders, frame.Width, frame.Height);
        }

        public void Respawn(Vector2 spawnPixels)
        {
            physics.Reset(spawnPixels);
        }

        public void SetWorldBounds(Rectangle bounds)
        {
            physics.SetWorldBounds(bounds);
        }
        public void SetJumpMultiplier(float multiplier)
        {
            physics.SetJumpMultiplier(multiplier);
        }
    }
}
