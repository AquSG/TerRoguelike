using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;
using Terraria.DataStructures;

namespace TerRoguelike.Projectiles
{
    public class GreenPetal : ModProjectile, ILocalizedModType
    {
        public float startingSpeed;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            startingSpeed = Projectile.velocity.Length();
            SoundEngine.PlaySound(SoundID.Grass with { Volume = 1f }, Projectile.Center);
            Projectile.ai[0] = -1;
        }
        public override void AI()
        {
            if (Projectile.timeLeft % 4 == 0 && Main.rand.NextBool())
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass, 0, 0, 0, default(Color), 0.9f);
                Main.dust[d].velocity *= 0.4f;
            }
                
            Projectile.frame = (Projectile.timeLeft / 4) % 2;
            Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.3f);
            if (Projectile.timeLeft > 260)
            {
                Projectile.velocity *= 0.924f;
                return;
            }

            if (Projectile.timeLeft == 260)
            {
                GetTarget();
                float direction = MathHelper.PiOver2;
                if (Projectile.ai[0] != -1)
                {
                    if (Projectile.ai[1] == 0)
                    {
                        direction = (Main.player[(int)Projectile.ai[0]].Center - Projectile.Center).ToRotation();
                    }
                    else if (Projectile.ai[1] == 1)
                    {
                        direction = (Main.npc[(int)Projectile.ai[0]].Center - Projectile.Center).ToRotation();
                    }
                }
                Projectile.velocity = Vector2.UnitX.RotatedBy(direction) * startingSpeed;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass);
            }
        }
        public void GetTarget()
        {
            float closestTarget = 3200f;
            if (Projectile.hostile)
            {
                Projectile.ai[1] = 0;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player == null)
                        continue;
                    if (!player.active)
                        continue;
                    if (player.dead)
                        continue;

                    float distance = (Projectile.Center - player.Center).Length();
                    if (distance <= closestTarget)
                    {
                        closestTarget = distance;
                        Projectile.ai[0] = i;
                    }
                }
            }
            else
            {
                Projectile.ai[1] = 1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc == null)
                        continue;
                    if (!npc.active)
                        continue;
                    if (npc.life <= 0)
                        continue;
                    if (npc.immortal)
                        continue;

                    float distance = (Projectile.Center - npc.Center).Length();
                    if (distance <= closestTarget)
                    {
                        closestTarget = distance;
                        Projectile.ai[0] = i;
                    }
                }
            }
        }
    }
}
