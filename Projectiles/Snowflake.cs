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

namespace TerRoguelike.Projectiles
{
    public class Snowflake : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.direction = Main.rand.NextBool() ? -1 : 1;
            Projectile.penetrate = -1;
            glowTex = TexDict["CircularGlow"].Value;
        }

        public override void AI()
        {
            Projectile.rotation += MathHelper.Pi * 0.02f * Projectile.velocity.Length() * Projectile.direction;
            Projectile.velocity *= 0.98f;
            if (Projectile.timeLeft > 60 && Main.rand.NextBool(5))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SnowflakeIce, 0, 0, 0, default, 0.65f);
        }
        public override bool? CanDamage() => Projectile.timeLeft > 45 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D flakeTex = TextureAssets.Projectile[Type].Value;


            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, Projectile.timeLeft / 60f), 0, 1f);
            float scale = 0.2f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null ,Color.Cyan * opacity * 0.8f, 0f, glowTex.Size() * 0.5f, 0.45f * scale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(flakeTex, Projectile.Center - Main.screenPosition, null ,Color.White * opacity, Projectile.rotation, flakeTex.Size() * 0.5f, scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
}
