using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoftwareEngineeringProject.Interfaces;
using System;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    internal class Hero : IGameObject
    {
        private readonly Texture2D texture;
        private readonly Animation animation;

        // visual position is owned by PhysicsBody now
        private readonly PhysicsBody physics;

        // sprite / hitbox tuning (kept here so Hero still controls sprite-related values)
        private const int HitboxHorizontalInset = 6;
        private const int HitboxTopInset = 3;
        private const int HitboxBottomInset = 0;

        // Backwards-compatible ctor
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

            // create the physics body and move physics responsibilities there
            // physics tuning (gravity, move speed, jump velocity, etc.) is now centralized in PhysicsBody defaults.
            physics = new PhysicsBody(
                startPosition: Vector2.Zero,
                worldBounds: worldBounds,
                // supply hitbox insets so Hero's visual tuning remains authoritative
                hitboxHorizontalInset: HitboxHorizontalInset,
                hitboxTopInset: HitboxTopInset,
                hitboxBottomInset: HitboxBottomInset);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw sprite at physics-controlled position
            spriteBatch.Draw(texture, physics.Position, animation.CurrentFrame.SourceRectangle, Color.White);
        }

        public void Update(GameTime gameTime)
        {
            // delegate input & physics integration to PhysicsBody
            physics.Update(gameTime, Microsoft.Xna.Framework.Input.Keyboard.GetState());

            // update animation separately
            animation.Update(gameTime);
        }

        // expose collision rectangle for debug/tuning (delegates to physics)
        public Rectangle GetCollisionBounds()
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            return physics.GetCollisionBounds(frame.Width, frame.Height);
        }

        // delegate collision resolution to physics body (call after Update)
        public void ResolveCollisions(IEnumerable<Rectangle> colliders)
        {
            var frame = animation.CurrentFrame.SourceRectangle;
            physics.ResolveCollisions(colliders, frame.Width, frame.Height);
        }
    }
}