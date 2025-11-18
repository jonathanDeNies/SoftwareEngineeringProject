using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SoftwareEngineeringProject.Interfaces;


namespace SoftwareEngineeringProject
{
    internal class Hero: IGameObject
    {
        private Texture2D texture;
        private Rectangle deelRectangle;

        private int schuifOp_X = 0;

        public Hero(Texture2D texture)
        {
            this.texture = texture;
            deelRectangle = new Rectangle(schuifOp_X, 0, 34, 32);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(0, 0), deelRectangle, Color.White);
        }

        public void Update()
        {
            schuifOp_X += 32;
            if(schuifOp_X > 136)
            {
                schuifOp_X = 0;
            }
            deelRectangle.X = schuifOp_X;
        }
    }
}
