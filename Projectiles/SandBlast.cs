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
using Terraria.GameContent.UI.Chat;

namespace TerRoguelike.Projectiles
{
    public class SandBlast : ModProjectile, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 600;
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            Projectile.rotation += MathHelper.Pi * 0.02f * Projectile.velocity.Length() * Projectile.direction;
            Projectile.velocity.X *= 0.99f;
            Projectile.velocity.Y += 0.25f;
            if (Main.rand.NextBool(4))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.DungeonSpirit : DustID.Sand, 0, 0, 0, default, 1f);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0, 0, 0, default, 0.9f);
            } 
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.8f }, Projectile.Center);
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            if (!Projectile.ModProj().hostileTurnedAlly)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Vector3 colorHSL = Main.rgbToHsl(Color.Gold);

                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();
                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = (Vector2.UnitX * 1).RotatedBy(Projectile.rotation + (i * MathHelper.PiOver4));
                    Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + offset, null, Color.White, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
                TerRoguelikeUtils.StartVanillaSpritebatch();
            }
            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
