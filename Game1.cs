using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System;

namespace SoftwareEngineeringProject
{
    public class Game1 : Game
    {

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Dictionary<Vector2, int> map;
        private Texture2D textureAtlas;

        // new: precomputed collision rectangles for tiles that act like walls
        private TileCollider tileCollider;
        private const int DisplayTileSize = 32;

        // actual level dimensions (tiles)
        private int mapWidthTiles;
        private int mapHeightTiles;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // load map first so we can determine level dimensions and size window/camera accordingly
            map = LoadMap("../../../Data/level1.csv", out mapWidthTiles, out mapHeightTiles);

            // compute desired backbuffer to fit the full level (optional - clamps to reasonable max)
            int requiredWidth = mapWidthTiles * DisplayTileSize;
            int requiredHeight = mapHeightTiles * DisplayTileSize;

            const int MaxWidth = 1600;
            const int MaxHeight = 900;

            graphics.PreferredBackBufferWidth = Math.Min(requiredWidth, MaxWidth);
            graphics.PreferredBackBufferHeight = Math.Min(requiredHeight, MaxHeight);
            graphics.ApplyChanges();
        }
        Hero hero;
        List<Enemy> enemies;

        /// <summary>
        /// Loads CSV into a dictionary of tile indices and returns the tile grid width/height (in tiles).
        /// Empty tiles (-1) are not stored in the dictionary, but width/height reflect the full CSV grid.
        /// </summary>
        private Dictionary<Vector2, int> LoadMap(string filepath, out int widthTiles, out int heightTiles)
        {
            var result = new Dictionary<Vector2, int>();
            widthTiles = 0;
            heightTiles = 0;

            using var reader = new StreamReader(filepath);

            int y = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var items = line.Split(',');

                // record width as maximum number of columns encountered
                if (items.Length > widthTiles) widthTiles = items.Length;

                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value))
                    {
                        if (value != -1)
                        {
                            result[new Vector2(x, y)] = value;
                        }
                    }
                }
                y++;
            }

            heightTiles = y;
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

            // pass actual viewport bounds into the hero so clamping uses the real window size
            hero = new Hero(texture, GraphicsDevice.Viewport.Bounds);

            textureAtlas = Content.Load<Texture2D>("Terrain (16x16)");

            Debug.WriteLine($"textureAtlas size = {textureAtlas.Width} x {textureAtlas.Height}");

            var solid = new HashSet<int> { 6, 7, 8, 28, 30, 39, 40, 41, 50, 51, 52, 61, 62, 63 };
            tileCollider = new TileCollider(map, DisplayTileSize, solid);

            // create enemy list and spawn only the enemies you want
            enemies = new List<Enemy>();

            // example: spawn only the third enemy (snake) at tile (0, 10) -> convert tile coords to pixels
            var snakeTile = new Vector2(19, 10);
            var snakePosition = new Vector2(snakeTile.X * DisplayTileSize, snakeTile.Y * DisplayTileSize);

            // Option A: direct create
            enemies.Add(EnemyFactory.Create(EnemyFactory.EnemyKind.Snake, texture, snakePosition));

            // Option B: use SpawnMany for multiple controlled spawns
            // EnemyFactory.SpawnMany(enemies, texture,
            //     new EnemyFactory.EnemySpec(EnemyFactory.EnemyKind.Snake, snakePosition)
            // );
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            hero.Update(gameTime);

            // resolve hero collisions against colliders owned by TileCollider
            if (tileCollider != null)
            {
                hero.ResolveCollisions(tileCollider.Colliders);
            }

            if (enemies != null)
            {
                int screenWidth = GraphicsDevice.Viewport.Width;
                // pass tile collider colliders (if tileCollider is null we pass null)
                var colliders = tileCollider?.Colliders;
                foreach (var e in enemies)
                    e.Update(gameTime, screenWidth, colliders);
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

            int displayTileSize = DisplayTileSize;
            int pixelTileSize = 16; // your atlas tiles are 16x16 px
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

            // optional debug: draw colliders (requires a 1x1 white pixel texture if enabled)
            // tileCollider?.DrawDebug(spriteBatch, debugPixel, Color.Red * 0.25f);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}