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
    public class PaladinHammer : ModProjectile, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity.Y -= 2;
        }
        public override void AI()
        {
            Projectile.direction = Math.Sign(Projectile.velocity.X);
            if (Projectile.direction == 0)
                Projectile.direction = 1;

            Projectile.rotation += 0.3f * Projectile.direction;
            if (Projectile.velocity.Y < 9)
                Projectile.velocity.Y += MathHelper.Clamp(MathHelper.Lerp(0, 0.24f, -(Projectile.timeLeft - 300) / 27f), 0, 0.24f);
            else if (Projectile.velocity.Y > 0)
                Projectile.velocity.Y = 9;
        }
    
        public override Color? GetAlpha(Color lightColor)
        {
            return lightColor;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            /*
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector3 colorHSL = Main.rgbToHsl(Color.Goldenrod);

            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (Vector2.UnitX * 1).RotatedBy(Projectile.rotation + (i * MathHelper.PiOver4));
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + offset, null, Color.Goldenrod, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, Projectile.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            */
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, Projectile.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
