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
using Terraria.Graphics.Renderers;
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Projectiles
{
    public class ShadowBlast : ModProjectile, ILocalizedModType
    {
        public TerRoguelikeGlobalProjectile modProj;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 15;
            Projectile.timeLeft = 1800;
            Projectile.penetrate = 1;
            modProj = Projectile.ModProj();
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item60 with { Volume = 1f }, Projectile.Center);
        }
        public override void AI()
        { 
            if (Projectile.MaxUpdates < 30 && Projectile.timeLeft % 40 == 0)
            {
                Projectile.extraUpdates++;
            }
            if (Main.rand.NextBool(3))
                return;

            int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, Color.Purple, 1.2f);
            Dust dust = Main.dust[d];
            dust.velocity *= 0f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.noLightEmittence = true;
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }
    }
}
