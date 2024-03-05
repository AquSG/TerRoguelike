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
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class FlowSpawn : ModProjectile, ILocalizedModType
    {
        public Texture2D fireTex;
        public Texture2D lightTex;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.MaxUpdates = 1;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
            fireTex = TexDict["Comet"];
            lightTex = TexDict["FlowSpawnGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.frameCounter++;
            Projectile.frame = (Projectile.frameCounter / 5) % Main.projFrames[Type];

            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y) + (Vector2.UnitX * 24).RotatedBy(Projectile.rotation + MathHelper.Pi), Projectile.width, Projectile.height, DustID.Clentaminator_Cyan, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Zombie53 with { Volume = 0.17f, PitchVariance = 0.2f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X - 10, Projectile.position.Y - 10), Projectile.width + 20, Projectile.height + 20, DustID.Clentaminator_Cyan, 0f, 0f, 0, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;
                dust.velocity *= 0.7f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, Projectile.frameCounter / 10f), 0, 1f);

            Main.EntitySpriteDraw(fireTex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation, new Vector2(fireTex.Width * 0.75f, fireTex.Height * 0.5f), Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation + MathHelper.PiOver2, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(lightTex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation + MathHelper.PiOver2, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}