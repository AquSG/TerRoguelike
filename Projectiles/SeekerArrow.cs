using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class SeekerArrow : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 2;
            glowTex = TexDict["CircularGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[2] = -1;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void AI()
        {
            if (Projectile.ai[2] == -1)
                GetTarget();

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
                if (Math.Abs(RadianSizeBetween(Projectile.velocity.ToRotation(), direction)) < MathHelper.PiOver2)
                {
                    float newRot = Projectile.velocity.ToRotation().AngleTowards(direction, 0.03f);
                    Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(newRot); 
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = -1; i < 2; i += 2)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + (Vector2.UnitX * 26).RotatedBy(Projectile.rotation), DustID.YellowTorch, (-Projectile.velocity * 0.56f).RotatedBy(0.5f * i), 0, default, 1.1f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.LightYellow, 0.5f);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 offset = (Main.rand.Next(2, 6) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.YellowTorch, offset.SafeNormalize(Vector2.UnitX) + Projectile.oldVelocity, Projectile.alpha, default(Color), 1.5f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 1f }, Projectile.Center);
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector3 colorHSL = Main.rgbToHsl(Color.Yellow);

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

            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition + new Vector2(0, 0), null, Projectile.GetAlpha(lightColor), Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
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
                        if (Math.Abs(RadianSizeBetween(Projectile.rotation, (player.Center - Projectile.Center).ToRotation())) < MathHelper.PiOver2)
                        {
                            closestTarget = distance;
                            Projectile.ai[2] = i;
                        }
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
                        if (Math.Abs(RadianSizeBetween(Projectile.rotation, (npc.Center - Projectile.Center).ToRotation())) < MathHelper.PiOver2)
                        {
                            closestTarget = distance;
                            Projectile.ai[2] = i;
                        }
                    }
                }
            }
        }
    }
}
