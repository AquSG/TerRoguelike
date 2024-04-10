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
using static TerRoguelike.Managers.TextureManager;
using Terraria.GameContent;
using Terraria.DataStructures;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class IceBomb : ModProjectile, ILocalizedModType
    {
        public int direction;
        public Texture2D lineTex;
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            lineTex = TexDict["LerpLineGradient"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item89 with { Volume = 0.7f, Pitch = -0.25f, PitchVariance = 0.2f }, Projectile.Center);
            Projectile.ai[0] = -1;
            direction = Math.Sign(Projectile.velocity.X);
            if (direction == 0)
                direction = 1;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == -1)
            {
                GetTarget();
            }
            Projectile.rotation += MathHelper.Pi * 0.03f * direction;
            Projectile.velocity *= 0.98f;
            if (Projectile.timeLeft > 60 && Main.rand.NextBool(5))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SnowflakeIce, 0, 0, 0, default, 0.65f);

            if (Projectile.ai[0] >= 0)
            {
                Entity target;
                if (Projectile.ai[1] == 0)
                {
                    Player player = Main.player[(int)Projectile.ai[0]];
                    target = player;
                    if (!player.active || player.dead)
                    {
                        Projectile.ai[0] = -1;
                        return;
                    }
                }
                else
                {
                    NPC npc = Main.npc[(int)Projectile.ai[0]];
                    target = npc;
                    if (!npc.CanBeChasedBy())
                    {
                        Projectile.ai[0] = -1;
                        return;
                    }
                }
                Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 0.1f;
                if (Projectile.velocity.Length() > 8)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 8;
            }

            if (Projectile.timeLeft == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.9f, Pitch = 1f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item48 with { Volume = 0.9f, Pitch = 0 }, Projectile.Center);

                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(i * MathHelper.PiOver4) * 2f, ModContent.ProjectileType<Iceflake>(), Projectile.damage, 0);
                }
                if (!TerRoguelikeUtils.ParanoidTileRetrieval(Projectile.Center.ToTileCoordinates()).IsTileSolidGround(true))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        ParticleManager.AddParticle(new Snow(
                            Projectile.Center, Main.rand.NextVector2CircularEdge(2, 2) * Main.rand.NextFloat(0.66f, 1f),
                            300, Color.Cyan * 0.8f, new Vector2(Main.rand.NextFloat(0.03f, 0.04f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 180, 0));
                    }
                }   
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bombTex = TextureAssets.Projectile[Type].Value;
            float scale = 1f;
            float opacity = 1f - (Projectile.timeLeft / 60f);
            float horizScale = MathHelper.Clamp((Projectile.timeLeft / 10f), 0, 1);
            TerRoguelikeUtils.StartAdditiveSpritebatch();
            for (int i = 0; i < 8; i++)
            {
                Main.EntitySpriteDraw(lineTex, Projectile.Center - Main.screenPosition, null, Color.Cyan * 0.85f * opacity, i * MathHelper.PiOver4, new Vector2(0, lineTex.Height * 0.5f), new Vector2(0.8f * horizScale, 1f), SpriteEffects.None);
            }
            TerRoguelikeUtils.StartVanillaSpritebatch();
            Main.EntitySpriteDraw(bombTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, bombTex.Size() * 0.5f, scale, SpriteEffects.None, 0);
            
            return false;
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
                    if (!npc.CanBeChasedBy())
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
