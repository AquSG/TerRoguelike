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
using Terraria.DataStructures;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class RoyalSnowflake : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/Snowflake";
        public Texture2D glowTex;
        public int maxTimeLeft;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = maxTimeLeft = 600;
            Projectile.penetrate = -1;
            glowTex = TexDict["CircularGlow"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.direction = Math.Sign(Projectile.velocity.X);
        }

        public override void AI()
        {
            Projectile.rotation += MathHelper.Pi * 0.04f * Projectile.direction;
            Projectile.velocity.X *= 0.96f;
            if (Projectile.timeLeft % 20 == 0 && Main.rand.NextBool(8))
            {
                ParticleManager.AddParticle(new Snow(
                        Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.25f,
                        600, Color.White * 0.66f, new Vector2(Main.rand.NextFloat(0.03f, 0.04f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 180, 0));
            }
            if (maxTimeLeft - Projectile.timeLeft < 30)
            {
                
            }
            else
            {
                if (Projectile.velocity.Y < 8)
                    Projectile.velocity.Y += 0.04f;
                if (Projectile.velocity.Y > 8)
                    Projectile.velocity.Y = 8;
            }
            
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item48 with { Volume = 0.4f, MaxInstances = 2, Pitch = -0.1f, PitchVariance = 0.05f, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew}, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                ParticleManager.AddParticle(new Snow(
                        Projectile.Center, Main.rand.NextVector2CircularEdge(2, 1) * Main.rand.NextFloat(0.66f, 1f),
                        300, Color.White * 0.66f, new Vector2(Main.rand.NextFloat(0.035f, 0.045f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 0.04f, 180, 0));
            }
            return true;
        }
        public override bool? CanDamage() => Projectile.timeLeft > 45 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D flakeTex = TextureAssets.Projectile[Type].Value;


            float opacity = (maxTimeLeft - Projectile.timeLeft) < 60 ? 
                MathHelper.Clamp(MathHelper.Lerp(0, 1f, (maxTimeLeft - Projectile.timeLeft) / 20f), 0, 1f) : 
                MathHelper.Clamp(MathHelper.Lerp(0, 1f, Projectile.timeLeft / 60f), 0, 1f);

            float scale = 0.2f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null ,Color.Cyan * opacity * 0.8f, 0f, glowTex.Size() * 0.5f, 0.45f * scale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(flakeTex, Projectile.Center - Main.screenPosition, null ,Color.White, Projectile.rotation, flakeTex.Size() * 0.5f, scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
}
