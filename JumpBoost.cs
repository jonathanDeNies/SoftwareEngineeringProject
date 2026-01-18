using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringProject
{
    internal sealed class JumpBoost
    {
        public Rectangle Bounds { get; }
        public bool Collected { get; private set; }

        private readonly float jumpMultiplier;
        private readonly Rectangle sourceRect;

        public JumpBoost(Vector2 positionPixels, int sizePixels, float jumpMultiplier, Rectangle sourceRect)
        {
            this.jumpMultiplier = jumpMultiplier;
            this.sourceRect = sourceRect;

            Bounds = new Rectangle(
                (int)positionPixels.X,
                (int)positionPixels.Y,
                sizePixels,
                sizePixels
            );
        }

        public void TryCollect(Hero hero)
        {
            if (Collected) return;

            var heroRect = hero.GetCollisionBounds();
            if (heroRect != Rectangle.Empty && heroRect.Intersects(Bounds))
            {
                hero.SetJumpMultiplier(jumpMultiplier);
                Collected = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (Collected) return;

            spriteBatch.Draw(
                texture,
                Bounds,
                sourceRect,
                Color.White
            );
        }
    }
}