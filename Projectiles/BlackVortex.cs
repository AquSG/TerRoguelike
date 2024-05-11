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
using Terraria.Graphics.Renderers;
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Projectiles
{
    public class BlackVortex : ModProjectile, ILocalizedModType
    {
        public TerRoguelikeGlobalProjectile modProj;
        public Texture2D texture;
        public Texture2D glowTex;
        public int maxTimeLeft;
        public float maxScale;
        public float maxVelocity;
        public int origWidth;
        public override void SetDefaults()
        {
            Projectile.scale = 3f;
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            modProj = Projectile.ModProj();
            maxTimeLeft = Projectile.timeLeft;
            maxScale = Projectile.scale;
            glowTex = TexDict["CircularGlow"];
            Projectile.hide = true;
            origWidth = Projectile.width;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            maxVelocity = Projectile.velocity.Length();
            Projectile.ai[2] = -1;
            Projectile.ai[1] = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 25; i++)
            {
                int randdust = Utils.SelectRandom<int>(Main.rand, 229, 229, 161);
                Dust d = Main.dust[Dust.NewDust(Projectile.Center + new Vector2(-20, -20), 40, 40, randdust)];
                d.noGravity = true;
                d.scale = 1.75f + Main.rand.NextFloat() * 1.25f;
                d.fadeIn = 0.25f;
                d.velocity *= 3.5f + Main.rand.NextFloat() * 0.5f;
                d.noLight = true;
            }
        }

        public override void AI()
        {
            int halfTime = (int)(maxTimeLeft * 0.5f);
            Projectile.scale = maxScale * MathHelper.Clamp(MathHelper.SmoothStep(0f, 1f, (halfTime - (Math.Abs(Projectile.timeLeft - halfTime))) / 120f), 0f, 1f);
            Projectile.rotation += 0.05f;
            SpawnDust();

            if (Projectile.ai[2] == -1)
                GetTarget();

            if (Projectile.ai[2] != -1)
            {
                float direction = 0;
                if (Projectile.ai[0] == 0)
                {
                    direction = (Main.player[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
                }
                else if (Projectile.ai[0] == 1)
                {
                    direction = (Main.npc[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
                }
                Projectile.velocity = (Vector2.UnitX * MathHelper.Clamp(MathHelper.SmoothStep(0, maxVelocity, (Math.Abs(Projectile.timeLeft - maxTimeLeft) - 60) / 180f), 0, maxVelocity)).RotatedBy(direction);
            }
        }
        public override bool? CanDamage() => Projectile.timeLeft < maxTimeLeft - 60 && Projectile.timeLeft > 60 ? null : false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, targetHitbox, origWidth * 0.35f * Projectile.scale);
        public override bool PreDraw(ref Color lightColor)
        {
            if (texture == null)
                texture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Black, Color.DarkGreen, 0.5f), Projectile.rotation + MathHelper.PiOver4, texture.Size() * 0.5f, Projectile.scale * 1.3f, SpriteEffects.None);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Black, Color.DarkGreen, 0.1f), -Projectile.rotation + MathHelper.PiOver4 + 0.2f, texture.Size() * 0.5f, Projectile.scale * 1.4f, SpriteEffects.None);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, -Projectile.rotation * 1.1f, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Green, Color.Blue, 0.25f) * 0.75f, 0f, glowTex.Size() * 0.5f, Projectile.scale * 0.2f, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public void GetTarget()
        {
            float closestTarget = 3200f;
            if (Projectile.hostile)
            {
                Projectile.ai[0] = 0;
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
                        Projectile.ai[2] = i;
                    }
                }
            }
            else
            {
                Projectile.ai[0] = 1;
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
                        Projectile.ai[2] = i;
                    }
                }
            }
        }
        public void SpawnDust()
        {
            if (Projectile.timeLeft > maxTimeLeft - 40)
            {
                if (Main.rand.NextBool())
                {
                    Vector2 rot = Projectile.ai[1].ToRotationVector2();
                    Vector2 displace = rot.RotatedBy(1.5707963705062866, default) * (float)(Main.rand.NextBool()).ToDirectionInt() * (float)Main.rand.Next(10, 21);
                    Vector2 randSpeed = rot * (float)Main.rand.Next(-160, 81);
                    Vector2 velocity = randSpeed - displace;
                    velocity /= 10f;
                    int dustid = 229;
                    Dust dust = Main.dust[Dust.NewDust(Projectile.Center, 0, 0, dustid)];
                    dust.noGravity = true;
                    dust.position = Projectile.Center + displace;
                    dust.velocity = velocity;
                    dust.scale = 0.5f + Main.rand.NextFloat();
                    dust.fadeIn = 0.5f;
                    randSpeed = rot * (float)Main.rand.Next(40, 121);
                    velocity = randSpeed - displace / 2f;
                    velocity /= 10f;
                    dust = Main.dust[Dust.NewDust(Projectile.Center, 0, 0, dustid)];
                    dust.noGravity = true;
                    dust.position = Projectile.Center + displace / 2f;
                    dust.velocity = velocity + Projectile.velocity;
                    dust.scale = 1f + Main.rand.NextFloat();
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                }
            }

            Vector2 offset = Main.rand.NextVector2CircularEdge(16f, 16f) * Projectile.scale;
            Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Vortex, offset.RotatedBy(MathHelper.PiOver2) * 0.1f + Projectile.velocity, 0, default, 1f);
            d.noGravity = true;
            d.noLightEmittence = true;
            d.noLight = true;

            for (int i = 0; i < 2; i++)
            {

                offset = Main.rand.NextVector2CircularEdge(30f, 30f) * Projectile.scale;
                offset *= Main.rand.NextFloat(0.85f, 1f);
                d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Stone, offset.RotatedBy(-MathHelper.PiOver2 * 1.2f) * 0.1f + Projectile.velocity, 0, Color.Black, 0.66f * Projectile.scale);
                d.noGravity = true;
                d.noLightEmittence = true;
                d.noLight = true;
            }


            
        }
    }
}
