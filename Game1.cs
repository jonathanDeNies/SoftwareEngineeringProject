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

            //enemies = new List<Enemy>();
            //EnemyFactory.PopulateDefaultEnemies(enemies, texture);
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