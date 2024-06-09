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
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class DemonLaser : ModProjectile, ILocalizedModType
    {
        public TerRoguelikeGlobalProjectile modProj;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 1;
            modProj = Projectile.ModProj();
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 6; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, Color.Purple, 1.2f);
                Dust dust = Main.dust[d];
                dust.velocity *= 2f;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
        }
        public override void AI()
        {
            float rot = Projectile.velocity.ToRotation();
            Vector2 step = -Projectile.velocity / 5;
            Vector2 off = Projectile.velocity;
            for (int i = 0; i < 5; i++)
            {
                if (Main.rand.NextBool(3))
                    continue;

                Vector2 position = Main.rand.NextVector2FromRectangle(Projectile.getRect());
                position += step * i + off;
                ParticleManager.AddParticle(new Square(
                    position, -Projectile.velocity.SafeNormalize(Vector2.UnitY), 15, Color.Lerp(Color.MediumPurple, Color.Magenta, 0.4f), new Vector2(2.5f, 1.5f), rot, 1f, 15, false));
                ParticleManager.AddParticle(new Square(
                    position, -Projectile.velocity.SafeNormalize(Vector2.UnitY), 15, Color.White, new Vector2(0.9f, 0.6f), rot, 1f, 15, true));
            }
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Vector2 off = Projectile.velocity;
            float rot = Projectile.velocity.ToRotation();
            Vector2 step = -Projectile.velocity / 30;
            for (int i = 0; i < 30; i++)
            {
                if (Main.rand.NextBool(3))
                    continue;

                Vector2 position = Main.rand.NextVector2FromRectangle(Projectile.getRect());
                position += step * i + off;
                ParticleManager.AddParticle(new Square(
                    position, Vector2.Zero, 15, Color.Lerp(Color.MediumPurple, Color.Magenta, 0.4f), new Vector2(1.5f), rot, 1f, 15, false));
                ParticleManager.AddParticle(new Square(
                    position, Vector2.Zero, 15, Color.White, new Vector2(0.4f), rot, 1f, 15, true));
            }

            for (int i = 0; i < 12; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, Color.Purple, 1.2f);
                Dust dust = Main.dust[d];
                dust.velocity *= 3f;
                dust.noLight = true;
                dust.noLightEmittence = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit3 with { Volume = 0.4f }, Projectile.Center);
            return true;
        }
    }
}
