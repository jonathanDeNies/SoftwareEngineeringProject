using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftwareEngineeringProject
{
    internal sealed class LevelManager
    {
        private readonly int displayTileSize;

        private readonly HashSet<int> solidTiles;
        private readonly HashSet<int> oneWayTiles;

        public IReadOnlyDictionary<string, LevelDefinition> Levels => levels;
        private readonly Dictionary<string, LevelDefinition> levels;

        public LevelState Current { get; private set; }
        public string CurrentKey { get; private set; }

        public LevelManager(
            int displayTileSize,
            Dictionary<string, LevelDefinition> levels,
            HashSet<int> solidTiles,
            HashSet<int> oneWayTiles)
        {
            this.displayTileSize = displayTileSize;
            this.levels = levels;
            this.solidTiles = solidTiles;
            this.oneWayTiles = oneWayTiles;
        }

        public void Load(string levelKey, Hero hero, Texture2D heroTexture, Rectangle worldBounds)
        {
            if (!levels.TryGetValue(levelKey, out var def))
                throw new ArgumentException($"Unknown level key: {levelKey}", nameof(levelKey));

            CurrentKey = levelKey;

            // Load map
            var map = LoadMap(def.CsvPath, out int widthTiles, out int heightTiles);

            // Build collider
            var collider = new TileCollider(map, displayTileSize, solidTiles, oneWayTiles);

            // Enemies per level (keep your existing behavior, but moved here)
            var enemies = new List<Enemy>();
            if (levelKey == "level1")
            {
                var snakeTile = new Vector2(19, 10);
                var snakePosition = new Vector2(snakeTile.X * displayTileSize, snakeTile.Y * displayTileSize);
                enemies.Add(EnemyFactory.Create(EnemyFactory.EnemyKind.Snake, heroTexture, snakePosition));
            }

            // Setup current state
            var exit = def.ExitTriggerPixels;
            var nextKey = def.NextLevelKey ?? "";

            Current = new LevelState(map, widthTiles, heightTiles, collider, enemies, exit, nextKey);

            // Respawn hero at spawn
            hero.Respawn(def.SpawnPixels);

            // Update world bounds (if your window/viewport changes later you can call this again)
            // (Hero ctor already got world bounds; physics body has SetWorldBounds too if needed.)
        }

        public bool TryTransition(Hero hero)
        {
            if (Current == null) return false;
            if (string.IsNullOrEmpty(Current.NextLevelKey)) return false;
            if (Current.ExitTrigger == Rectangle.Empty) return false;

            var heroRect = hero.GetCollisionBounds();
            if (heroRect == Rectangle.Empty) return false;

            if (heroRect.Intersects(Current.ExitTrigger))
                return true;

            return false;
        }

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

                if (items.Length > widthTiles) widthTiles = items.Length;

                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value))
                    {
                        if (value != -1)
                            result[new Vector2(x, y)] = value;
                    }
                }
                y++;
            }

            heightTiles = y;
            return result;
        }
    }
}
