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
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class PhantasmalBoltShooter : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/NPCs/Enemy/Boss/TrueEyeOfCthulhu";
        public Texture2D deathrayTex;
        public Texture2D innerEyeTex;
        public int maxTimeLeft;
        public int LaserActivateTime = 40;
        public Entity target = null;
        public bool playShootSound = true;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ModProj().killOnRoomClear = true;
            if (TexturesLoaded)
                deathrayTex = TextureAssets.Projectile[ModContent.ProjectileType<PhantasmalDeathray>()].Value;
            innerEyeTex = TexDict["MoonLordInnerEye"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[0] == 1)
                playShootSound = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.ai[0] = Projectile.velocity.Length();
            Projectile.velocity = Vector2.Zero;
        }
        public override void AI()
        {
            var modProj = Projectile.ModProj();
            target = modProj.GetTarget(Projectile);
            Vector2 targetPos = target != null ? target.Center : Projectile.rotation.ToRotationVector2() * 5;
            Projectile.rotation = (targetPos - Projectile.Center).ToRotation();

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            int time = maxTimeLeft - Projectile.timeLeft;
            int shootingTime = time - LaserActivateTime;
            if (time == 1)
            {
                SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.12f, Pitch = -0.1f, MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
            }
            if (time >= 20 && shootingTime < 0)
            {
                if (playShootSound && time == 20)
                {
                    SoundEngine.PlaySound(SoundID.Item13 with { Volume = 1f, Pitch = 0.2f, PitchVariance = 0, MaxInstances = 4 }, Projectile.Center);
                }
                int maxLength = 32;
                float range = MathHelper.PiOver4 * 1.5f;
                Vector2 vectToTarget = (targetPos - Projectile.Center);
                if (vectToTarget.Length() > maxLength)
                    vectToTarget = vectToTarget.SafeNormalize(Vector2.UnitY) * maxLength;
                Vector2 particleSpawnPos = Projectile.Center + vectToTarget * new Vector2(0.35f, 1f);

                Vector2 offset = (Main.rand.NextFloat(-range, range) + vectToTarget.ToRotation()).ToRotationVector2() * Main.rand.NextFloat(32);

                ParticleManager.AddParticle(new Ball(
                    particleSpawnPos + offset, -offset * 0.1f,
                    20, Color.Lerp(Color.Teal, Color.Cyan, Main.rand.NextFloat(0.25f, 0.75f)), new Vector2(0.25f), 0, 0.96f, 10),
                    ParticleManager.ParticleLayer.AfterProjectiles);
            }
            if (Projectile.timeLeft == 20)
            {
                SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.08f, Pitch = 0.2f, MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
            }
            if (shootingTime >= 0 && shootingTime % 8 == 0 && Projectile.timeLeft > 20)
            {
                if (playShootSound && shootingTime / 8 % 2 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.6f , MaxInstances = 8, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
                }
                int maxLength = 15;
                Vector2 vectToTarget = (targetPos - Projectile.Center);
                if (vectToTarget.Length() > maxLength)
                    vectToTarget = vectToTarget.SafeNormalize(Vector2.UnitY) * maxLength;
                Vector2 projSpawnPos = Projectile.Center + vectToTarget;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), projSpawnPos, vectToTarget.SafeNormalize(Vector2.UnitY) * Projectile.ai[0], ModContent.ProjectileType<PhantasmalBolt>(), Projectile.damage, 0);
            }
        }
        public override bool? CanDamage() => maxTimeLeft - Projectile.timeLeft > 20 && Projectile.timeLeft > 20 ? null : false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight - 2);
            Vector2 origin = new Vector2(tex.Width * 0.5f);

            int time = maxTimeLeft - Projectile.timeLeft;
            Vector2 scale = new Vector2(Projectile.scale);
            float teleportInterpolant = 0;
            if (time < 20)
            {
                teleportInterpolant = 0.5f + (time / 20f) * 0.5f;
            }
            if (Projectile.timeLeft < 20)
            {
                teleportInterpolant = (1 - (Projectile.timeLeft / 20f)) * 0.5f;
            }
            
            if (teleportInterpolant > 0)
            {
                float interpolant = teleportInterpolant < 0.5f ? teleportInterpolant / 0.5f : 1f - ((teleportInterpolant - 0.5f) / 0.5f);
                float horizInterpolant = MathHelper.Lerp(1f, 2f, 0.5f + (0.5f * -(float)Math.Cos(interpolant * MathHelper.TwoPi)));
                float verticInterpolant = MathHelper.Lerp(0.5f + (0.5f * (float)Math.Cos(interpolant * MathHelper.TwoPi)), 8f, interpolant * interpolant);
                scale.X *= horizInterpolant;
                scale.Y *= verticInterpolant;

                scale *= 1f - interpolant;
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, 0, origin, scale, SpriteEffects.None);
            Main.EntitySpriteDraw(innerEyeTex, Projectile.Center + (Projectile.rotation.ToRotationVector2() * 15) * scale - Main.screenPosition, null, Color.White, 0, innerEyeTex.Size() * 0.5f, scale, SpriteEffects.None);

            return false;
        }
    }
}
