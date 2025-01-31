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
    public class Bone : ModProjectile, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;

        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 600;
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f * Projectile.direction;
            if (Projectile.velocity.Y < 9)
                Projectile.velocity.Y += MathHelper.Clamp(MathHelper.Lerp(0, 0.24f, -(Projectile.timeLeft - 600) / 27f), 0, 0.24f);
            else if (Projectile.velocity.Y > 0)
                Projectile.velocity.Y = 9;
        }
    
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bone, 0, 0, 0, default, 0.9f);
            } 
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.8f }, Projectile.Center);
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            if (!Projectile.ModProj().hostileTurnedAlly)
            {
                TerRoguelikeUtils.StartAlphaBlendSpritebatch();

                Vector3 colorHSL = Main.rgbToHsl(Color.LightGray);

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
