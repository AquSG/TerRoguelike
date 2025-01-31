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
    public class IchorBlob : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.velocity.Y += -2.5f;
                Projectile.ai[0]++;
            }

            Projectile.velocity.X *= 0.998f;
            Projectile.velocity.Y += 0.1f;
            
            for (int i = 0; i < 2; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.Clentaminator_Cyan : DustID.Ichor, 0, 0, 0, default, 1.2f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.velocity *= 0;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 20; i++)
            {
                int d = Dust.NewDust(Projectile.Center, 1, 1, DustID.Ichor, 0, 0, 0, default, 0.8f);
            } 
            //SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.4f }, Projectile.Center);
            return true;
        }
    }
}
