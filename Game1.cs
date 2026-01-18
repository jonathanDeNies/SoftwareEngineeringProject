using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace SoftwareEngineeringProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private const int DisplayTileSize = 32;

        private Texture2D texture;        // hero/enemy spritesheet
        private Texture2D textureAtlas;   // tileset
        private Texture2D debugPixel;

        private Hero hero;

        private LevelManager levelManager;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        private void ApplyBackbufferForCurrentMap()
        {
            var current = levelManager.Current;

            int requiredWidth = current.MapWidthTiles * DisplayTileSize;
            int requiredHeight = current.MapHeightTiles * DisplayTileSize;

            const int MaxWidth = 1600;
            const int MaxHeight = 900;

            graphics.PreferredBackBufferWidth = Math.Min(requiredWidth, MaxWidth);
            graphics.PreferredBackBufferHeight = Math.Min(requiredHeight, MaxHeight);
            graphics.ApplyChanges();
            hero.SetWorldBounds(GraphicsDevice.Viewport.Bounds);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            debugPixel = new Texture2D(GraphicsDevice, 1, 1);
            debugPixel.SetData(new[] { Color.White });

            texture = Content.Load<Texture2D>("characters (1)");
            textureAtlas = Content.Load<Texture2D>("Terrain (16x16)");

            Debug.WriteLine($"textureAtlas size = {textureAtlas.Width} x {textureAtlas.Height}");

            // Create hero once
            hero = new Hero(texture, GraphicsDevice.Viewport.Bounds);

            // Define your levels
            var levels = new Dictionary<string, LevelDefinition>
            {
                ["level1"] = new LevelDefinition(
                    csvPath: "../../../Data/level1.csv",
                    spawnPixels: new Vector2(32, 32),
                    exitTriggerPixels: new Rectangle(
                        31 * DisplayTileSize,
                        6 * DisplayTileSize,
                        DisplayTileSize * 2,
                        DisplayTileSize * 3
                    ),
                    nextLevelKey: "level2"
                ),

                ["level2"] = new LevelDefinition(
                    csvPath: "../../../Data/level2_Terrain.csv",
                    spawnPixels: new Vector2(0, 710),
                    exitTriggerPixels: Rectangle.Empty,
                    nextLevelKey: null
                )
            };

            // Solid / one-way sets
            var solid = new HashSet<int> { 6, 7, 8, 28, 30, 35,36, 39, 40, 41, 50, 51, 52, 57,58, 105, 106, 107, 127, 128, 129, 149, 150, 151 };
            var oneWay = new HashSet<int> { 61, 62, 63 };

            // Create level manager
            levelManager = new LevelManager(DisplayTileSize, levels, solid, oneWay);

            // Load first level
            levelManager.Load("level1", hero, texture, GraphicsDevice.Viewport.Bounds);

            // Resize window based on level size (optional)
            ApplyBackbufferForCurrentMap();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            hero.Update(gameTime);

            // Collisions against current level
            var current = levelManager.Current;
            hero.ResolveCollisions(current.TileCollider.SolidColliders, current.TileCollider.OneWayColliders);

            // Transition check
            if (levelManager.TryTransition(hero))
            {
                var nextKey = current.NextLevelKey;
                levelManager.Load(nextKey, hero, texture, GraphicsDevice.Viewport.Bounds);
                ApplyBackbufferForCurrentMap();
            }

            // Enemies update
            int screenWidth = GraphicsDevice.Viewport.Width;
            var solidColliders = current.TileCollider.SolidColliders;
            foreach (var e in current.Enemies)
                e.Update(gameTime, screenWidth, solidColliders);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            var current = levelManager.Current;

            // Draw map
            int displayTileSize = DisplayTileSize;
            int pixelTileSize = 16;
            int tilesPerRow = textureAtlas.Width / pixelTileSize;

            foreach (var item in current.Map)
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

            // Draw hero + enemies
            hero.Draw(spriteBatch);
            foreach (var e in current.Enemies)
                e.Draw(spriteBatch);

            // Debug exit trigger
            if (current.ExitTrigger != Rectangle.Empty)
            {
                spriteBatch.Draw(debugPixel, current.ExitTrigger, Color.Red * 0.35f);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
