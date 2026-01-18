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
        private readonly HashSet<int> quitTiles;
        private readonly HashSet<int> startOverTiles;

        public IReadOnlyDictionary<string, LevelDefinition> Levels => levels;
        private readonly Dictionary<string, LevelDefinition> levels;

        public LevelState Current { get; private set; }
        public string CurrentKey { get; private set; }

        public LevelManager(
            int displayTileSize,
            Dictionary<string, LevelDefinition> levels,
            HashSet<int> solidTiles,
            HashSet<int> oneWayTiles,
            HashSet<int> quitTiles,
            HashSet<int> startOverTiles)

        {
            this.displayTileSize = displayTileSize;
            this.levels = levels;
            this.solidTiles = solidTiles;
            this.oneWayTiles = oneWayTiles;
            this.quitTiles = quitTiles;
            this.startOverTiles = startOverTiles;
        }

        public void Load(string levelKey, Hero hero, Texture2D heroTexture, Rectangle worldBounds)
        {
            if (!levels.TryGetValue(levelKey, out var def))
                throw new ArgumentException($"Unknown level key: {levelKey}", nameof(levelKey));

            CurrentKey = levelKey;

            var map = LoadMap(def.CsvPath, out int widthTiles, out int heightTiles);
            var collider = new TileCollider(map, displayTileSize, solidTiles, oneWayTiles);

            var enemies = new List<Enemy>();
            if (levelKey == "level1")
            {
                var snakeTile = new Vector2(19, 10);
                var snakePosition = new Vector2(snakeTile.X * displayTileSize, snakeTile.Y * displayTileSize);
                enemies.Add(EnemyFactory.Create(EnemyFactory.EnemyKind.Snake, heroTexture, snakePosition));
            }

            var exit = def.ExitTriggerPixels;
            var nextKey = def.NextLevelKey ?? "";

            // Jump boosts (level2 only)
            var jumpBoosts = new List<JumpBoost>();
            if (levelKey == "level2")
            {
                var itemsLayer = LoadCsvLayer("../../../Data/level2_Items.csv");
                var appleSrc = new Rectangle(0, 0, 16, 16);

                foreach (var kv in itemsLayer)
                {
                    var posPixels = new Vector2(kv.Key.X * displayTileSize, kv.Key.Y * displayTileSize);
                    jumpBoosts.Add(new JumpBoost(posPixels, displayTileSize, 1.3f, appleSrc));
                }
            }
            
            // Quit / StartOver triggers (scan the map for matching tile IDs)
            var quitTriggers = new List<Rectangle>();
            var startOverTriggers = new List<Rectangle>();

            foreach (var kv in map)
            {
                int tileId = kv.Value;

                bool isQuit = quitTiles.Contains(tileId);
                bool isStartOver = startOverTiles.Contains(tileId);

                if (!isQuit && !isStartOver)
                    continue;

                var rect = new Rectangle(
                    (int)kv.Key.X * displayTileSize,
                    (int)kv.Key.Y * displayTileSize,
                    displayTileSize,
                    displayTileSize
                );

                if (isQuit) quitTriggers.Add(rect);
                if (isStartOver) startOverTriggers.Add(rect);
            }

            Current = new LevelState(
                map, widthTiles, heightTiles,
                collider, enemies,
                jumpBoosts,
                quitTriggers, startOverTriggers,
                exit, nextKey
            );

            hero.Respawn(def.SpawnPixels);
        }


        public bool TryTransition(Hero hero)
        {
            if (Current == null) return false;
            if (string.IsNullOrEmpty(Current.NextLevelKey)) return false;
            if (Current.ExitTrigger == Rectangle.Empty) return false;

            var heroRect = hero.GetCollisionBounds();
            if (heroRect == Rectangle.Empty) return false;

            return heroRect.Intersects(Current.ExitTrigger);
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
                    if (int.TryParse(items[x], out int value) && value != -1)
                        result[new Vector2(x, y)] = value;
                }
                y++;
            }

            heightTiles = y;
            return result;
        }

        private Dictionary<Vector2, int> LoadCsvLayer(string filepath)
        {
            var result = new Dictionary<Vector2, int>();
            using var reader = new StreamReader(filepath);

            int y = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var items = line.Split(',');
                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value) && value != -1)
                        result[new Vector2(x, y)] = value;
                }
                y++;
            }
            return result;
        }
    }
}
