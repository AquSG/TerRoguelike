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

namespace TerRoguelike.Projectiles
{
    public class WoodSliver : ModProjectile, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.velocity.Y > 7)
                Projectile.velocity.Y = 7;
            else
                Projectile.velocity.Y += MathHelper.Clamp(MathHelper.Lerp(0.2f, 0, (Projectile.timeLeft - 170) / 10f), 0, 0.2f);
            Projectile.velocity.X *= 0.995f;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.6f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.WoodFurniture, 0f, 0f, 0, default(Color), 1.5f);
                Main.dust[d].noGravity = true;
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            Vector3 colorHSL = Main.rgbToHsl(Color.SandyBrown);

            GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
            GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
            GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (Vector2.UnitX * 1).RotatedBy(Projectile.rotation + (i * MathHelper.PiOver4));
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition + offset + new Vector2(6, 0), null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Value.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return true;
        }
    }
}
