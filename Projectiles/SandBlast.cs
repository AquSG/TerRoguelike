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
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0, 0, 0, default, 1f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0, 0, 0, default, 0.9f);
            } 
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.4f }, Projectile.Center);
            return true;
        }
    }
}
