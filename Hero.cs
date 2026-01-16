using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoftwareEngineeringProject.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;


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

            // de collision
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
    }
}
