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
using Terraria.Graphics.Effects;
using static TerRoguelike.Managers.TextureManager;
using Terraria.DataStructures;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class ShiningSand : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public Texture2D noiseTex;
        public Texture2D glowTex;
        public override string Texture => "TerRoguelike/ExtraTextures/CircularGlow";
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            noiseTex = TexDict["Crust"];
            glowTex = TexDict["CircularGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(9))
            {
                float crossHeight = 12;
                Vector2 randPos = new Vector2(Main.rand.NextFloat(-crossHeight, crossHeight), Main.rand.NextFloat(-crossHeight, crossHeight));
                ParticleManager.AddParticle(new Square(Projectile.Center + randPos, Vector2.Zero, 25, Color.Goldenrod, new Vector2(1f), 0, 0.96f, 25));
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int x = -10; x <= 10; x += 4)
            {
                for (int d = 0; d <= 1; d++)
                {
                    Vector2 offset = new Vector2(x, 0).RotatedBy(d * MathHelper.PiOver2);
                    int timeOff = Math.Abs(x);
                    ParticleManager.AddParticle(new Square(Projectile.Center + (offset * 0.5f), offset * 0.1f, 25 - timeOff, Color.Goldenrod * 0.9f, new Vector2(1f), 0, 0.96f, 25 - timeOff));
                }
            }
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.4f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft;
            float scale = 1f;
            
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            Main.spriteBatch.End();
            Effect maskEffect = Filters.Scene["TerRoguelike:MaskOverlay"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);

            Vector2 screenOff = new Vector2(Projectile.frameCounter / 60f, 0);
            Color tint = Color.Goldenrod * 0.95f;
            if (time < 10)
            {
                float completion = time / 10f;
                tint *= completion;
                scale *= completion;
            }
                

            maskEffect.Parameters["screenOffset"].SetValue(screenOff);
            maskEffect.Parameters["stretch"].SetValue(new Vector2(0.25f, 0.25f));
            maskEffect.Parameters["replacementTexture"].SetValue(noiseTex);
            maskEffect.Parameters["tint"].SetValue(tint.ToVector4());

            for (int i = 0; i < 2; i++)
            {
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0, tex.Size() * 0.5f, Projectile.scale * 0.06f * new Vector2(1f, 0.25f) * scale, SpriteEffects.None);
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0, tex.Size() * 0.5f, Projectile.scale * 0.06f * new Vector2(0.25f, 1f) * scale, SpriteEffects.None);
            }
            

            TerRoguelikeUtils.StartVanillaSpritebatch();

            return false;
        }
    }
}
