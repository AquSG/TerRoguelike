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
    public class CursedFlame : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 120;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 1f }, Projectile.Center);
        }
        public override void AI()
        {
            int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, 0, 0, Projectile.alpha, Color.LimeGreen, 2f);
            Dust dust = Main.dust[d];
            dust.noGravity = true;
            dust.noLightEmittence = true;
            dust.noLight = true;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 16; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, Color.LimeGreen, 1.5f);
                Dust dust = Main.dust[d];
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
        }
    }
}
