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
    public class VileSpit : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 1f }, Projectile.Center);
        }
        public override void AI()
        {
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                int d = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.Clentaminator_Cyan : DustID.CorruptGibs, 0, 0, Projectile.alpha, default(Color), 1.6f);
                Dust dust = Main.dust[d];
                dust.velocity *= 0.5f;
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
                if (Projectile.ModProj().hostileTurnedAlly)
                    dust.color = Color.Cyan;
            }
            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ModProj().hostileTurnedAlly ? DustID.CoralTorch : DustID.GreenTorch, 0, 0, Projectile.alpha, Color.LimeGreen, 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
            
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 24; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.noLightEmittence = true;
                dust.noLight = true;
                if (Main.rand.NextBool())
                {
                    int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, Color.LimeGreen, 1.5f);
                    Dust dust2 = Main.dust[d2];
                    dust2.noLightEmittence = true;
                    dust2.noLight = true;
                }
            }
        }
    }
}
