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
using Microsoft.CodeAnalysis;

namespace TerRoguelike.Projectiles
{
    public class Daybreak : ModProjectile, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.MaxUpdates = 1;
            Projectile.timeLeft = 180 * Projectile.MaxUpdates;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.velocity /= Projectile.MaxUpdates;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.position += (Vector2.UnitX * 63).RotatedBy(Projectile.rotation);
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.SolarFlare, Projectile.velocity.X * 0.5f,Projectile.velocity.Y * 0.5f, 0, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }
        public override void OnKill(int timeLeft)
        {
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), Projectile.damage, 0);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = SoundID.Item41.Volume * 0.7f }, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X - 10, Projectile.position.Y - 10), Projectile.width + 20, Projectile.height + 20, DustID.Smoke, 0f, 0f, 0, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 offset = new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float completion = 1f - ((float)i / Projectile.oldPos.Length);
                Vector2 pos = Projectile.oldPos[i] + offset;
                float scale = Projectile.scale;
                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, null, Color.White * completion, Projectile.oldRot[i], new Vector2(tex.Size().X - Projectile.width * 0.5f, tex.Size().Y * 0.5f), scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(tex.Size().X - Projectile.width * 0.5f, tex.Size().Y * 0.5f), Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}