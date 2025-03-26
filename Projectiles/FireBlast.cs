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
using System.IO;
using TerRoguelike.Packets;

namespace TerRoguelike.Projectiles
{
    public class FireBlast : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 1f }, Projectile.Center);
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 1)
            {
                int newWidth = 64;
                int newHeight = 64;
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1f }, Projectile.Center);
                Projectile.velocity = Vector2.Zero;
                Projectile.localAI[0] = 2;
                Projectile.position = Projectile.Center + new Vector2(-(newWidth * 0.5f), -(newHeight * 0.5f));
                Projectile.width = newWidth;
                Projectile.height = newHeight;
                Projectile.timeLeft = 120;
                Projectile.penetrate = -1;
            }

            if (Projectile.localAI[0] == 0)
            {
                int d = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.Clentaminator_Cyan : DustID.InfernoFork, 0f, 0f, 100, default(Color), 1.2f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.velocity *= 0.5f;
                dust.velocity += Projectile.velocity * 0.3f;
                if (Projectile.ModProj().hostileTurnedAlly)
                    dust.color = Color.Cyan;
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    float rot = MathHelper.TwoPi / 20f * i;
                    Vector2 vel = (-Vector2.UnitY * 4f).RotatedBy(rot);
                    Dust dust = Dust.NewDustDirect(Projectile.Center + new Vector2(-4), 0, 0, Projectile.ModProj().hostileTurnedAlly ? DustID.Clentaminator_Cyan : DustID.InfernoFork, vel.X, vel.Y, 0, default(Color), 0.8f);
                    if (Projectile.ModProj().hostileTurnedAlly)
                        dust.color = Color.Cyan;

                    dust.noGravity = true;
                }
            }
            
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.localAI[0] >= 1)
                return false;

            Projectile.localAI[0] = 1;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.localAI[0] >= 1)
                return;

            OnHitServerProj.Send(Projectile, target.whoAmI, false);

            Projectile.localAI[0] = 1;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.localAI[0] >= 1)
                return;

            OnHitServerProj.Send(Projectile, target.whoAmI, false);

            Projectile.localAI[0] = 1;
        }
    }
}
