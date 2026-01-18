using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SoftwareEngineeringProject
{
    internal sealed class LevelDefinition
    {
        public string CsvPath { get; }
        public Vector2 SpawnPixels { get; }
        public Rectangle ExitTriggerPixels { get; }   // where touching loads next level
        public string? NextLevelKey { get; }

        public LevelDefinition(string csvPath, Vector2 spawnPixels, Rectangle exitTriggerPixels, string? nextLevelKey)
        {
            CsvPath = csvPath;
            SpawnPixels = spawnPixels;
            ExitTriggerPixels = exitTriggerPixels;
            NextLevelKey = nextLevelKey;
        }
    }
}
