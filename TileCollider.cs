using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    /// <summary>
    /// Encapsulates which tile indices are solid and produces collider rectangles
    /// from a map. Game1 no longer needs to build or own the collider list.
    /// </summary>
    internal sealed class TileCollider
    {
        private readonly List<Rectangle> colliders = new();

        public IReadOnlyList<Rectangle> Colliders => colliders;

        private Dictionary<Vector2, int> map;
        private readonly int tileSize;
        private readonly HashSet<int> solidTiles;

        public TileCollider(Dictionary<Vector2, int> map, int tileSize, IEnumerable<int> solidTiles)
        {
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            if (tileSize <= 0) throw new ArgumentOutOfRangeException(nameof(tileSize));
            this.tileSize = tileSize;
            this.solidTiles = new HashSet<int>(solidTiles ?? throw new ArgumentNullException(nameof(solidTiles)));

            BuildColliders();
        }

        public void UpdateMap(Dictionary<Vector2, int> newMap)
        {
            map = newMap ?? throw new ArgumentNullException(nameof(newMap));
            BuildColliders();
        }

        public void BuildColliders()
        {
            colliders.Clear();

            foreach (var kv in map)
            {
                if (solidTiles.Contains(kv.Value))
                {
                    var rect = new Rectangle(
                        (int)kv.Key.X * tileSize,
                        (int)kv.Key.Y * tileSize,
                        tileSize,
                        tileSize);
                    colliders.Add(rect);
                }
            }
        }

        // optional: debug draw helper (call from Game1.Draw while spriteBatch.Begin)
        public void DrawDebug(SpriteBatch spriteBatch, Texture2D pixel, Color color)
        {
            if (spriteBatch is null) throw new ArgumentNullException(nameof(spriteBatch));
            if (pixel is null) throw new ArgumentNullException(nameof(pixel));

            foreach (var r in colliders)
            {
                spriteBatch.Draw(pixel, r, color);
            }
        }
    }
}
