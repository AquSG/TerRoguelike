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
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class CorruptVomit : ModProjectile, ILocalizedModType
    {
        public int setTimeLeft = 300;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = setTimeLeft;
            Projectile.penetrate = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Point spawnTile = Projectile.Center.ToTileCoordinates();
            if (TerRoguelikeUtils.ParanoidTileRetrieval(spawnTile.X, spawnTile.Y).IsTileSolidGround(true))
            {
                Projectile.active = false;
                return;
            }
            SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 1f }, Projectile.Center);
        }
        public override void AI()
        {
            if (Projectile.velocity.Y < 8)
                Projectile.velocity.Y += 0.1f;
            if (Projectile.velocity.Y > 8)
                Projectile.velocity.Y = 8;

            if (Main.rand.NextBool())
            {
                int d = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.CorruptGibs, 0, 0, Projectile.alpha, default(Color), 1f);
                Dust dust = Main.dust[d];
                dust.velocity *= 0.5f;
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }
            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, 0, 0, Projectile.alpha, Color.LimeGreen, 1.5f);
                Dust dust = Main.dust[d];
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }

            Vector2 velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * 1;
            velocity *= Main.rand.NextFloat(0.5f, 1f);
            velocity.Y -= 0.8f;
            if (Main.rand.NextBool(3))
                velocity *= 1.5f;
            Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
            int time = 30 + Main.rand.Next(20);
            Color color = Color.Lerp(Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0.7f)), Color.Black, 0.48f);
            
            float fadeIn = 10;
            if (Projectile.timeLeft > setTimeLeft - fadeIn)
            {
                float interpolant = 1f - ((Projectile.timeLeft - (setTimeLeft - fadeIn)) / fadeIn);
                color *= interpolant;
                scale *= interpolant;
            }
                

            ParticleManager.AddParticle(new Blood(Projectile.Center + Projectile.velocity, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
            ParticleManager.AddParticle(new Blood(Projectile.Center + Projectile.velocity, velocity, time, color * 0.65f, scale, velocity.ToRotation(), true));
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, default(Color), 0.8f);
                Dust dust = Main.dust[d];
                dust.noLightEmittence = true;
                dust.noLight = true;
                if (Main.rand.NextBool())
                {
                    d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), Projectile.alpha, Color.LimeGreen, 1.5f);
                    dust = Main.dust[d];
                    dust.noLightEmittence = true;
                    dust.noLight = true;
                }
            }
        }
    }
}
