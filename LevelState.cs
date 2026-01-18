using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SoftwareEngineeringProject
{
    internal sealed class LevelState
    {
        public Dictionary<Vector2, int> Map { get; }
        public TileCollider TileCollider { get; }
        public List<Enemy> Enemies { get; }
        public List<JumpBoost> JumpBoosts { get; }

        public Rectangle ExitTrigger { get; }
        public string NextLevelKey { get; }

        // trigger tiles (for your interactive gameover level)
        public List<Rectangle> QuitTriggers { get; }
        public List<Rectangle> StartOverTriggers { get; }

        public int MapWidthTiles { get; }
        public int MapHeightTiles { get; }

        public LevelState(
            Dictionary<Vector2, int> map,
            int mapWidthTiles,
            int mapHeightTiles,
            TileCollider tileCollider,
            List<Enemy> enemies,
            List<JumpBoost> jumpBoosts,
            List<Rectangle> quitTriggers,
            List<Rectangle> startOverTriggers,
            Rectangle exitTrigger,
            string nextLevelKey)
        {
            Map = map;
            MapWidthTiles = mapWidthTiles;
            MapHeightTiles = mapHeightTiles;

            TileCollider = tileCollider;
            Enemies = enemies;
            JumpBoosts = jumpBoosts;

            QuitTriggers = quitTriggers ?? new List<Rectangle>();
            StartOverTriggers = startOverTriggers ?? new List<Rectangle>();

            ExitTrigger = exitTrigger;
            NextLevelKey = nextLevelKey;
        }
    }
}
