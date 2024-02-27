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
    public class LihzahrdLaser : ModProjectile, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.MaxUpdates = 10;
            Projectile.timeLeft = 180 * Projectile.MaxUpdates;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity /= Projectile.MaxUpdates;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.YellowTorch, 0f, 0f, 0, default(Color), 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLight = true;
                dust.noLightEmittence = true;

            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 offset = new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float completion = 1f - ((float)i / Projectile.oldPos.Length);
                Vector2 pos = Projectile.oldPos[i] + offset;
                float scale = Projectile.scale * 0.5f;
                Texture2D tex = TextureAssets.Projectile[Type].Value;
                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, null, Color.White, Projectile.oldRot[i], tex.Size() * 0.5f, scale, SpriteEffects.None);
            }
            return false;
        }
    }
}