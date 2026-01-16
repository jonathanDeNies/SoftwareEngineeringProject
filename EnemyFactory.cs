using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftwareEngineeringProject
{
    internal static class EnemyFactory
    {
        /// <summary>
        /// Adds the project's default enemies to the provided list.
        /// The factory calls `enemies.Add(...)` so callers do not need to construct or add enemies directly.
        /// </summary>
        public static void PopulateDefaultEnemies(List<Enemy> enemies, Texture2D texture)
        {
            if (enemies is null) throw new ArgumentNullException(nameof(enemies));
            if (texture is null) throw new ArgumentNullException(nameof(texture));

            // These match the original placements/speeds from Game1.LoadContent
            enemies.Add(new Enemy(texture, new Vector2(0, 100), 1.5f, 0, 33));
            enemies.Add(new Enemy(texture, new Vector2(0, 200), 2.0f, 0, 65));
            enemies.Add(new Enemy(texture, new Vector2(0, 300), 1.0f, 0, 97));
        }

        /// <summary>
        /// Helper to add a custom enemy directly to the list.
        /// </summary>
        public static void AddCustom(List<Enemy> enemies, Texture2D texture, Vector2 startPosition, float speed, int spriteStartX, int spriteStartY, bool wrap = true)
        {
            if (enemies is null) throw new ArgumentNullException(nameof(enemies));
            if (texture is null) throw new ArgumentNullException(nameof(texture));

            enemies.Add(new Enemy(texture, startPosition, speed, spriteStartX, spriteStartY, wrap));
        }
    }
}