using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace TerRoguelike.Managers
{
    public class SpawnManager
    {
        public static void SpawnEnemy(int npcType, Vector2 position, int telegraphDuration, float telegraphSize = 1f)
        {
            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), position, Vector2.Zero, ModContent.ProjectileType<EnemySpawningProjectile>(), 0, 0f, -1, telegraphDuration, telegraphSize, npcType);
        }
    }
}
