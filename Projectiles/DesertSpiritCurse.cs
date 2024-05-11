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
    public class DesertSpiritCurse : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public Texture2D fireTex;
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
            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 1f }, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0, 0, 0, default, 1f);
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
                int i = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0, 0, 0, default, 1f);
                Main.dust[i].noGravity = true;
                Main.dust[i].velocity *= 0;
            } 
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0, 0, 0, default, 1f);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            fireTex = TextureAssets.Projectile[Type].Value;
            int frameHeight = fireTex.Height / Main.projFrames[Type];

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null ,Color.Purple * 0.8f, 0f, glowTex.Size() * 0.5f, 0.1f, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(fireTex, Projectile.Center - Main.screenPosition + (Vector2.UnitY * -6), new Rectangle(0, frameHeight * Projectile.frame, fireTex.Width, frameHeight), Color.White, 0f, new Vector2(fireTex.Size().X * 0.5f, fireTex.Size().Y / (Main.projFrames[Type] * 2)), 1f, SpriteEffects.None, 0);

            return false;
        }
    }
}
