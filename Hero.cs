using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SoftwareEngineeringProject.Interfaces;
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
            Move();
        }

        private void Move()
        {
            positie += snelheid;
            if (positie.X > 768
                || positie.X < 0)
            {
                snelheid.X *= -1;
            }
            if (positie.Y > 448
                || positie.Y < 0)
            {
                snelheid.Y *= -1;
            }


        }
    }
}
