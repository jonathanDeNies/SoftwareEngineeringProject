using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    public class Game1 : Game
    {

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 700;
            graphics.PreferredBackBufferHeight = 700;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        


        private Rectangle deelRectangle;
        private int schuifOp_X = 0;
        Hero hero;
        List<Enemy> enemies;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            base.Initialize();
        }


        private Texture2D texture;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            texture = Content.Load<Texture2D>("characters (1)");
            hero = new Hero(texture);

            // create some enemies that use other characters from the same sprite sheet
            enemies = new List<Enemy>();

            // Move the enemies.Add calls into the factory so spawn logic is centralized
            //enemies.Add(new Enemy(texture, new Vector2(0, 100), 1.5f, 0, 33));
            //enemies.Add(new Enemy(texture, new Vector2(0, 200), 2.0f, 0, 65));
            //enemies.Add(new Enemy(texture, new Vector2(0, 300), 1.0f, 0, 97));
            EnemyFactory.PopulateDefaultEnemies(enemies, texture);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            hero.Update(gameTime);
            if (enemies != null)
            {
                foreach (var e in enemies)
                    e.Update(gameTime);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            hero.Draw(spriteBatch);
            if (enemies != null)
            {
                foreach (var e in enemies)
                    e.Draw(spriteBatch);
            }
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
