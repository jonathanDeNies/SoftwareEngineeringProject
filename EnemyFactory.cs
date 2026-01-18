using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoftwareEngineeringProject
{
    internal static partial class EnemyFactory
    {

        public readonly struct EnemySpec
        {
            public EnemyKind Kind { get; }
            public Vector2 StartPosition { get; }

            public EnemySpec(EnemyKind kind, Vector2 startPosition)
            {
                Kind = kind;
                StartPosition = startPosition;
            }
        }

        public static Enemy Create(EnemyKind kind, Texture2D texture, Vector2 startPosition)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            return kind switch
            {
                EnemyKind.Basic => new Enemy(texture, startPosition, 1.5f, 0, 33, wrap: false),
                EnemyKind.Fast  => new Enemy(texture, startPosition, 2.5f, 0, 65, wrap: false),
                EnemyKind.Snake => new Enemy(texture, startPosition, 1.0f, 0, 97, wrap: false),
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }

        public static void SpawnMany(List<Enemy> target, Texture2D texture, params EnemySpec[] specs)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            if (specs == null) return;

            foreach (var s in specs)
                target.Add(Create(s.Kind, texture, s.StartPosition));
        }

        public static Enemy CreateCustom(Texture2D texture, Vector2 startPosition, float speed, int spriteStartX, int spriteStartY, bool wrap = false)
            => new Enemy(texture, startPosition, speed, spriteStartX, spriteStartY, wrap);
    }
}