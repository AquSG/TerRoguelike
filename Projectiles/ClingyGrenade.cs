using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Player;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Projectiles
{
    public class ClingyGrenade : ModProjectile, ILocalizedModType
    {
        public int target = -1;
        public int stuckNPC = -1;
        public Vector2 stuckPosition = Vector2.Zero;
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 28;
            Projectile.timeLeft = 120;
            Projectile.rotation = Main.rand.NextFloatDirection();
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.scale = 0.85f;
            Projectile.spriteDirection = Main.rand.NextBool() ? -1 : 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] != 1)
            {
                stuckNPC = (int)Projectile.ai[0];
                stuckPosition = Projectile.Center - Main.npc[stuckNPC].Center;
                Projectile.localAI[0] = 1;
            }

            float fallSpeedCap = 25f;
            float downwardsAccel = 0.3f;

            if (stuckNPC != -1)
            {
                if (!Main.npc[stuckNPC].active || Main.npc[stuckNPC].life <= 0)
                {
                    stuckNPC = -1;
                    stuckPosition = Vector2.Zero;
                }
            }

            
            if (stuckNPC == -1)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].life > 0 && Projectile.getRect().Intersects(Main.npc[i].getRect()))
                    {
                        stuckNPC = i;
                        stuckPosition = Projectile.Center - Main.npc[i].Center;
                        break;
                    }
                }
            }
            
            if (stuckNPC != -1)
            {
                Projectile.Center = Main.npc[stuckNPC].Center + stuckPosition;
                return;
            }

            if (Projectile.velocity.Y < fallSpeedCap)
                Projectile.velocity.Y += downwardsAccel;
            if (Projectile.velocity.Y > fallSpeedCap)
                Projectile.velocity.Y = fallSpeedCap;
        }

        public override bool PreKill(int timeLeft)
        {
            int spawnedProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), Projectile.damage, 0f, Projectile.owner);
            Main.projectile[spawnedProjectile].scale = 1f;
            Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>().procChainBools = Projectile.GetGlobalProjectile<TerRoguelikeGlobalProjectile>().procChainBools;

            SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (stuckNPC == -1)
                Projectile.velocity = Vector2.Zero;

            return false;
        }
        public override bool? CanDamage() => false;
    }
}
