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

namespace TerRoguelike.Projectiles
{
    public class BouncingFire : ModProjectile, ILocalizedModType
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 1f }, Projectile.Center);
        }
        public override void AI()
        {
            Projectile.rotation += 0.4f;
            if (Projectile.timeLeft < 160)
                Projectile.velocity.Y += 0.24f;
            for (int i = 0; i < 1; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0, 0, Projectile.alpha, default(Color), 2.6f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 16; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, default(Color), 1.5f);
                Dust dust = Main.dust[d];
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.95f;
            }
            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.95f;
                Projectile.velocity.X *= 0.95f;
            }
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
