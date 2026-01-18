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

        private Texture2D texture;
        private Texture2D textureAtlas;
        private Texture2D itemTexture;

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

            texture = Content.Load<Texture2D>("characters (1)");
            textureAtlas = Content.Load<Texture2D>("Terrain (16x16)");
            itemTexture = Content.Load<Texture2D>("Apple");

            Debug.WriteLine($"textureAtlas size = {textureAtlas.Width} x {textureAtlas.Height}");

            hero = new Hero(texture, GraphicsDevice.Viewport.Bounds);

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
                ),

                ["gameover"] = new LevelDefinition(
                    csvPath: "../../../Data/GameOver.csv",
                    spawnPixels: new Vector2(256, 480),
                    exitTriggerPixels: Rectangle.Empty,
                    nextLevelKey: null
                ),
            };

            var solid = new HashSet<int> { 6, 7, 8, 28, 30, 35, 36, 39, 40, 41, 50, 51, 52, 57, 58, 100, 101, 102, 105, 106, 107, 127, 128, 129, 149, 150, 151, 188, 189, 190, 191, 233, 234 };
            var oneWay = new HashSet<int> { 61, 62, 63 };

            var gameOver = new HashSet<int> { 123, 124, 145, 146 };
            var startOver = new HashSet<int> { 216, 217, 238, 239 };

            levelManager = new LevelManager(DisplayTileSize, levels, solid, oneWay, gameOver, startOver);

            levelManager.Load("level1", hero, texture, GraphicsDevice.Viewport.Bounds);
            ApplyBackbufferForCurrentMap();
        }

        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                ks.IsKeyDown(Keys.Escape))
                Exit();

            hero.Update(gameTime);

            var current = levelManager.Current;

            hero.ResolveCollisions(current.TileCollider.SolidColliders, current.TileCollider.OneWayColliders);

            var heroRect = hero.GetCollisionBounds();
            if (heroRect != Rectangle.Empty)
            {
                
                foreach (var r in current.QuitTriggers)
                {
                    if (heroRect.Intersects(r))
                    {
                        Exit();
                        return;
                    }
                }

                foreach (var r in current.StartOverTriggers)
                {
                    if (heroRect.Intersects(r))
                    {
                        levelManager.Load("level1", hero, texture, GraphicsDevice.Viewport.Bounds);
                        ApplyBackbufferForCurrentMap();
                        return;
                    }
                }
            }

            foreach (var jb in current.JumpBoosts)
                jb.TryCollect(hero);

            if (levelManager.TryTransition(hero))
            {
                var nextKey = current.NextLevelKey;
                levelManager.Load(nextKey, hero, texture, GraphicsDevice.Viewport.Bounds);
                ApplyBackbufferForCurrentMap();
                current = levelManager.Current;
            }

            if (levelManager.CurrentKey != "gameover" &&
                levelManager.CurrentKey != "level2")
            {
                int gameOverY = 16 * DisplayTileSize;

                if (heroRect.Bottom >= gameOverY)
                {
                    levelManager.Load("gameover", hero, texture, GraphicsDevice.Viewport.Bounds);
                    ApplyBackbufferForCurrentMap();
                    return;
                }
            }

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

            foreach (var jb in current.JumpBoosts)
                jb.Draw(spriteBatch, itemTexture);

            hero.Draw(spriteBatch);
            foreach (var e in current.Enemies)
                e.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
