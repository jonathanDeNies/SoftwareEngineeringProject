using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    internal sealed class LevelState
    {
        public Dictionary<Vector2, int> Map { get; }
        public TileCollider TileCollider { get; }
        public List<Enemy> Enemies { get; }
        public Rectangle ExitTrigger { get; }
        public string NextLevelKey { get; }

        public int MapWidthTiles { get; }
        public int MapHeightTiles { get; }

        public LevelState(
            Dictionary<Vector2, int> map,
            int mapWidthTiles,
            int mapHeightTiles,
            TileCollider tileCollider,
            List<Enemy> enemies,
            Rectangle exitTrigger,
            string nextLevelKey)
        {
            Map = map;
            MapWidthTiles = mapWidthTiles;
            MapHeightTiles = mapHeightTiles;
            TileCollider = tileCollider;
            Enemies = enemies;
            ExitTrigger = exitTrigger;
            NextLevelKey = nextLevelKey;
        }
    }
}
