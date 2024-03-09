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
using ReLogic.Utilities;
using Terraria.DataStructures;
using TerRoguelike.Particles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Projectiles
{
    public class Shockwave : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public bool ableToHit = true;
        public Vector2 spawnVelocity;
        public SlotId crumblingSoundSlot;
        public static readonly SoundStyle crumblingLoop = new SoundStyle("TerRoguelike/Sounds/Shockwave", 3);
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            spawnVelocity = Projectile.velocity;
            Projectile.ai[1] = 1;
            crumblingSoundSlot = SoundEngine.PlaySound(crumblingLoop with { Volume = 0.25f, MaxInstances = 4 }, Projectile.Center);
        }
        public override void AI()
        {
            if (SoundEngine.TryGetActiveSound(crumblingSoundSlot, out var crumbleSound) && crumbleSound.IsPlaying)
            {
                crumbleSound.Position = Projectile.Center;
                if (Projectile.timeLeft < 240)
                {
                    crumbleSound.Volume -= 0.0055f;
                }
                crumbleSound.Update();
            }

            if (Projectile.ai[1] == 0)
                Projectile.velocity.X = spawnVelocity.X;

            ableToHit = Projectile.ai[1] == 0 && Projectile.ai[0] == 0;
            Projectile.velocity.Y = 16;
            Projectile.localAI[0]++;
            if ((int)Projectile.localAI[0] % 4 == 0 && Math.Abs(Projectile.velocity.X) > 0 && Projectile.ai[1] == 0)
            {
                Vector2 particleVelocity = new Vector2(-8f * Math.Sign(Projectile.velocity.X), 0.2f);
                Vector2 particleScale = new Vector2(0.45f, 0.34f);
                particleScale *= MathHelper.Clamp(MathHelper.Lerp(1f, 0.5f, (Projectile.timeLeft - 260) / 40f), 0, 1f);

                ParticleManager.AddParticle(new Spark(Projectile.Bottom + new Vector2(Projectile.width * 0.5f * Math.Sign(Projectile.velocity.X), 0), particleVelocity, 30, Color.DarkGray * 0.75f, particleScale, particleVelocity.ToRotation()));

                particleVelocity = new Vector2(-8f * Math.Sign(Projectile.velocity.X), 0).RotatedBy(Math.Sign(Projectile.velocity.X) * MathHelper.Pi * Main.rand.NextFloat(0.10f, 0.12f));
                ParticleManager.AddParticle(new Spark(Projectile.Bottom + new Vector2(Projectile.width * 0.5f * Math.Sign(Projectile.velocity.X), - 7), particleVelocity, 30, Color.DarkGray * 0.75f, particleScale, particleVelocity.ToRotation()));
            }
            Projectile.ai[1]++;
        }
        public override bool? CanDamage() => ableToHit ? null : false;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] >= 3 || Projectile.ai[1] >= 3)
                return true;

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Point point = (Projectile.Top + new Vector2(Math.Sign(Projectile.velocity.X) * (Projectile.width * 0.5f + 8), 0)).ToTileCoordinates();
                if (ParanoidTileRetrieval(point.X, point.Y).IsTileSolidGround(true))
                    return true;
                Projectile.velocity.X = 0;
                Projectile.position.Y -= 16f;
                Projectile.ai[0]++;
            }
            else
            {
                Projectile.ai[0] = 0;
            }
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.ai[1] = 0;
            }
            else
            {
                Projectile.velocity.X = 0;
            }
            return false;
        }
        public override bool PreKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(crumblingSoundSlot, out var crumbleSound) && crumbleSound.IsPlaying)
            {
                crumbleSound.Stop();
            }
            return true;
        }
    }
}
