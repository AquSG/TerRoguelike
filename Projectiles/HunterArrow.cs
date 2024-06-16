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
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class HunterArrow : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public Texture2D lineTex;

        public int RedirectTelegraph = 60;
        public Vector2 origVelocity;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 2;
            glowTex = TexDict["CircularGlow"];
            lineTex = TexDict["LerpLineGradient"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[2] = -1;
            origVelocity = Projectile.velocity;
        }
        public override void AI()
        {
            if (Projectile.ai[0] != 0 && Projectile.ai[0] <= RedirectTelegraph)
            {
                if (Projectile.ai[2] != -1)
                {
                    float direction = 0;
                    if (Projectile.ai[1] == 0)
                    {
                        direction = (Main.player[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
                    }
                    else if (Projectile.ai[1] == 1)
                    {
                        direction = (Main.npc[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
                    }
                    Projectile.rotation = Projectile.rotation.AngleLerp(direction, 0.05f);
                }
                if (Projectile.ai[0] < RedirectTelegraph)
                {
                    Vector2 offset = (Main.rand.Next(13, 19) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                    Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GreenTorch, -offset.SafeNormalize(Vector2.UnitX) + Projectile.velocity, Projectile.alpha, default(Color), 1.5f);
                    d.noGravity = true;
                    d.noLight = true;
                    d.noLightEmittence = true;
                }
                else
                {
                    Projectile.velocity = (Vector2.UnitX * origVelocity.Length()).RotatedBy(Projectile.rotation);
                    SoundEngine.PlaySound(SoundID.Item102 with { Volume = 0.7f }, Projectile.Center);
                }
                Projectile.ai[0]++;
            }
            else
            {
                if (Projectile.ai[0] == 0)
                    Projectile.velocity.Y += 0.1f;

                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 0, default(Color), 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.LightGreen, MathHelper.Clamp(Projectile.ai[0] / RedirectTelegraph, 0.4f, 1f));
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 offset = (Main.rand.Next(2, 6) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GreenTorch, offset.SafeNormalize(Vector2.UnitX) + Projectile.oldVelocity, Projectile.alpha, default(Color), 1.5f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.Kill();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.9f }, Projectile.Center);
                Projectile.ai[0]++;
                GetTarget();
                Projectile.velocity = Vector2.Zero;
                if (Projectile.ai[0] == -1)
                    return true;
            }
                

            if (Projectile.ai[0] < RedirectTelegraph)
                return false;

            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.9f }, Projectile.Center);
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Green * MathHelper.Clamp(Projectile.ai[0] / RedirectTelegraph * 0.8f, 0f, 0.8f), 0f, glowTex.Size() * 0.5f, 0.1f, SpriteEffects.None, 0);

            if (Projectile.ai[0] < RedirectTelegraph && Projectile.ai[0] > 0)
                Main.EntitySpriteDraw(lineTex, Projectile.Center - Main.screenPosition, null, Color.Green * MathHelper.Clamp(Projectile.ai[0] / RedirectTelegraph, 0f, 1f), Projectile.rotation, new Vector2(0, lineTex.Height * 0.5f), 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector3 colorHSL = Main.rgbToHsl(Color.LimeGreen);

            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (Vector2.UnitX * 1).RotatedBy(Projectile.rotation + (i * MathHelper.PiOver4));
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition + offset + new Vector2(0, 0), null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
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
                        Projectile.ai[2] = i;
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
                        Projectile.ai[2] = i;
                    }
                }
            }
        }
    }
}
