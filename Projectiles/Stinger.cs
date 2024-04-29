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

namespace TerRoguelike.Projectiles
{
    public class Stinger : ModProjectile, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[0] == 1)
            {
                Projectile.timeLeft = 300;
            }
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 18, 0f, 0f, 0, default(Color), 0.9f);
            Main.dust[d].noGravity = true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            int dustID = Projectile.ai[0] == 0 ? DustID.GreenTorch : DustID.YellowTorch;
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(Projectile.Center, 1, 1, dustID, 0f, 0f, 0, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector3 colorHSL = Main.rgbToHsl(Projectile.ai[0] == 0 ? Color.LimeGreen : Color.Lerp(Color.Goldenrod, Color.White, 0.3f));

            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (Vector2.UnitX * 1).RotatedBy(Projectile.rotation + (i * MathHelper.PiOver4));
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition + offset, null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
