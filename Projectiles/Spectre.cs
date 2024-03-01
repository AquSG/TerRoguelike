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
using static TerRoguelike.Managers.TextureManager;
using Terraria.GameContent;

namespace TerRoguelike.Projectiles
{
    public class Spectre : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public Texture2D spectreTex;
        public Entity target = null;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 320;
            glowTex = TexDict["CircularGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.4f }, Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation();
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SpectreStaff, 0, 0, 0, default, 1.65f);
            }

            if (Projectile.ai[0] != -1)
            {
                if (Projectile.ai[1] == 1)
                {
                    target = Main.player[(int)Projectile.ai[0]];
                }
                else if (Projectile.ai[1] == 2)
                {
                    target = Main.npc[(int)Projectile.ai[0]];
                }
            }
        }
        public override void AI()
        {
            Projectile.frame = (Projectile.frameCounter / 6) % Main.projFrames[Type];
            Projectile.frameCounter++;

            if (Projectile.velocity.Length() < 10)
                Projectile.velocity *= 1.01f;

            if (Main.rand.NextBool(3))
            {
                int i = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SpectreStaff, 0, 0, 0, default, 1.8f);
                Main.dust[i].noGravity = true;
                Main.dust[i].velocity *= 0;
            }

            if (target != null)
            {
                if (!target.active)
                {
                    target = null;
                        return;
                }

                Projectile.velocity = Projectile.rotation.AngleTowards((target.Center - Projectile.Center).ToRotation(), 0.01f).ToRotationVector2() * Projectile.velocity.Length();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SpectreStaff, 0, 0, 0, default, 1.5f);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            spectreTex = TextureAssets.Npc[Type].Value;
            int frameHeight = spectreTex.Height / Main.projFrames[Type];

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.White * 0.6f, 0f, glowTex.Size() * 0.5f, 0.085f, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(spectreTex, Projectile.Center - Main.screenPosition + (Vector2.UnitY * -6).RotatedBy(Projectile.rotation - MathHelper.PiOver2), new Rectangle(0, frameHeight * Projectile.frame, spectreTex.Width, frameHeight), Color.White * 0.75f, Projectile.rotation - MathHelper.PiOver2, new Vector2(spectreTex.Size().X * 0.5f, spectreTex.Size().Y / (Main.projFrames[Type] * 2)), 1f, SpriteEffects.None, 0);

            return false;
        }
    }
}
