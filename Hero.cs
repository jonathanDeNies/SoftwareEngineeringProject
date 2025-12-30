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


        //basis movement
        private void Move()
        {
            positie += snelheid;
            snelheid += versnelling;
            float maximaleSnelheid = 40;
            snelheid = Limit(snelheid, maximaleSnelheid);
            if (positie.X > 768
                || positie.X < 0)
            {
                snelheid.X *= -1;
                versnelling.X *= -1;
            }
            if (positie.Y > 448
                || positie.Y < 0)
            {
                snelheid.Y *= -1;
                versnelling.Y *= -1;
            }
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
            // vertical movement
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

            animation.Update(gameTime);
        }
    }
}
