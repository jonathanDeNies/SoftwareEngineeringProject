using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoftwareEngineeringProject
{
    internal sealed class TileCollider
    {
        private readonly List<Rectangle> solidColliders = new();
        private readonly List<Rectangle> oneWayColliders = new();
        private Dictionary<Vector2, int> map;
        private readonly int tileSize;
        private readonly HashSet<int> solidTiles;
        private readonly HashSet<int> oneWayTiles;

        public TileCollider(Dictionary<Vector2, int> map, int tileSize, IEnumerable<int> solidTiles, IEnumerable<int> oneWayTiles)
        {
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            if (tileSize <= 0) throw new ArgumentOutOfRangeException(nameof(tileSize));
            this.tileSize = tileSize;
            this.solidTiles = new HashSet<int>(solidTiles ?? throw new ArgumentNullException(nameof(solidTiles)));
            this.oneWayTiles = new HashSet<int>(oneWayTiles ?? throw new ArgumentNullException(nameof(oneWayTiles)));

            BuildColliders();
        }

        public IReadOnlyList<Rectangle> SolidColliders => solidColliders;
        public IReadOnlyList<Rectangle> OneWayColliders => oneWayColliders;

        public void UpdateMap(Dictionary<Vector2, int> newMap)
        {
            map = newMap ?? throw new ArgumentNullException(nameof(newMap));
            BuildColliders();
        }

        public void BuildColliders()
        {
            solidColliders.Clear();
            oneWayColliders.Clear();

            foreach (var kv in map)
            {
                var v = kv.Value;
                var rect = new Rectangle(
                    (int)kv.Key.X * tileSize,
                    (int)kv.Key.Y * tileSize,
                    tileSize,
                    tileSize);

                if (solidTiles.Contains(v))
                    solidColliders.Add(rect);
                else if (oneWayTiles.Contains(v))
                    oneWayColliders.Add(rect);
            }
        }
    }
}
