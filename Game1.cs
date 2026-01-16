using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SoftwareEngineeringProject
{
    public class Game1 : Game
    {

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Dictionary<Vector2, int> map;
        private Texture2D textureAtlas;

        // new: precomputed collision rectangles for tiles that act like walls
        private List<Rectangle> colliders;
        private const int DisplayTileSize = 32;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            map = LoadMap("../../../Data/level1.csv");
        }
        Hero hero;
        List<Enemy> enemies;

        private Dictionary<Vector2, int> LoadMap(string filepath)
        {
            Dictionary<Vector2, int> result = new();

            StreamReader reader = new(filepath);

            int y = 0;
            string line;
            while((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');

                for(int x = 0; x < items.Length; x++)
                {
                    if(int.TryParse(items[x], out int value))
                    {
                        if(value != -1)
                        {
                            result[new Vector2(x,y)] = value;
                        }
                    }
                }
                y++;
            }
            return result;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            base.Initialize();
        }


        private Texture2D texture;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            texture = Content.Load<Texture2D>("characters (1)");
            hero = new Hero(texture);

            textureAtlas = Content.Load<Texture2D>("Terrain (16x16)");

            // debug: verify atlas size at runtime
            Debug.WriteLine($"textureAtlas size = {textureAtlas.Width} x {textureAtlas.Height}");

            // build colliders for tiles that should block the player
            colliders = new List<Rectangle>();
            if (map != null)
            {
                var solid = new HashSet<int> { 6, 7, 8, 28, 30, 39, 40, 41, 50, 51, 52,61,62,63  };

                foreach (var item in map)
                {
                    if (solid.Contains(item.Value))
                    {
                        var rect = new Rectangle(
                            (int)item.Key.X * DisplayTileSize,
                            (int)item.Key.Y * DisplayTileSize,
                            DisplayTileSize,
                            DisplayTileSize);
                        colliders.Add(rect);
                    }
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            hero.Update(gameTime);

            // resolve hero collisions against the tile colliders
            if (colliders != null)
            {
                hero.ResolveCollisions(colliders);
            }

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

            // Corrected tile drawing
            int displayTileSize = DisplayTileSize;
            int pixelTileSize = 16; // your atlas is 16x16 tiles
            int tilesPerRow = textureAtlas.Width / pixelTileSize;
            if (textureAtlas.Width % pixelTileSize != 0)
            {
                Debug.WriteLine("Warning: textureAtlas.Width is not a multiple of pixelTileSize.");
            }

            foreach (var item in map)
            {
                Rectangle drect = new(
                    (int)item.Key.X * displayTileSize,
                    (int)item.Key.Y * displayTileSize,
                    displayTileSize,
                    displayTileSize
                );

                int tx = item.Value % tilesPerRow;
                int ty = item.Value / tilesPerRow;

                Rectangle src = new(
                    tx * pixelTileSize,
                    ty * pixelTileSize,
                    pixelTileSize,
                    pixelTileSize
                );

                spriteBatch.Draw(textureAtlas, drect, src, Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}