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
    public class PhantasmalLaser : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/NPCs/Enemy/Boss/TrueEyeOfCthulhu";
        public Texture2D deathrayTex;
        public Texture2D innerEyeTex;
        public int maxTimeLeft;
        public int LaserActivateTime = 40;
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
            deathrayTex = TextureAssets.Projectile[ModContent.ProjectileType<PhantasmalDeathray>()].Value;
            innerEyeTex = TexDict["MoonLordInnerEye"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.ai[0] = Projectile.velocity.Length();
            Projectile.velocity = Vector2.Zero;
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            int time = maxTimeLeft - Projectile.timeLeft;
            if (time == 1)
            {
                SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.12f, Pitch = -0.1f, MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
            }
            if (Projectile.timeLeft == 20)
            {
                SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.08f, Pitch = 0.2f, MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
            }
            if (Projectile.timeLeft > 20 && time >= LaserActivateTime)
            {
                Vector2 basePos = Projectile.Center;
                float particleRot = Projectile.rotation;
                float randRot = Main.rand.NextFloat(-0.7f, 0.7f);
                Vector2 particleVel = (Vector2.UnitX * Main.rand.NextFloat(1, 2)).RotatedBy(randRot + Math.Sign(randRot) * 1.2f + particleRot);
                ParticleManager.AddParticle(new BallOutlined(
                    basePos + particleRot.ToRotationVector2() * 24, particleVel + Projectile.velocity, 
                    30, Color.Lerp(Color.Teal, Color.Cyan, 0.5f), Color.White * 0.5f, new Vector2(0.2f), 4, 0, 0.96f, 15),
                    ParticleManager.ParticleLayer.AfterProjectiles);
            }
            if (time == LaserActivateTime)
            {
                ExtraSoundSystem.ExtraSounds.Add(new ExtraSound(SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.15f, Pitch = 0.1f, PitchVariance = 0f, MaxInstances = 8, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest}, Projectile.Center), 1, 30, 30));
            }
        }
        public override bool? CanDamage() => maxTimeLeft - Projectile.timeLeft > 20 && Projectile.timeLeft > 10 ? null : false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            if (maxTimeLeft - Projectile.timeLeft < LaserActivateTime)
                return false;

            int radius = 18;
            float length = Projectile.ai[0];
            Vector2 anchor = Projectile.Center;
            Vector2 rotVect = Projectile.rotation.ToRotationVector2();
            for (int i = 24; i < length; i += radius)
            {
                Vector2 checkPos = anchor + rotVect * i;
                if (targetHitbox.ClosestPointInRect(checkPos).Distance(checkPos) <= radius)
                    return true;
            }
            return false;
        }
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


            int timeToEnd = Projectile.timeLeft - 10;

            float deathrayLength = Projectile.ai[0];
            Vector2 deathrayNormalVect = Projectile.rotation.ToRotationVector2();
            float deathrayRot = Projectile.rotation;
            Vector2 deathrayStartPos = Projectile.Center + deathrayNormalVect * 24;
            Vector2 deathrayEndPoint = Projectile.Center + deathrayNormalVect * (deathrayLength + 10);

            float verticalScale = 1f;
            if (timeToEnd < 10)
            {
                verticalScale *= timeToEnd / 10f;
            }
            else if (time < LaserActivateTime)
            {
                if (time < LaserActivateTime - 5)
                    verticalScale *= MathHelper.Clamp(time / 10f, 0, 1) * 0.2f;
                else
                    verticalScale *= (time - (LaserActivateTime - 5)) / 5f;
            }

            Main.EntitySpriteDraw(deathrayTex, deathrayStartPos - Main.screenPosition, null, Color.White, deathrayRot, new Vector2(0, deathrayTex.Height * 0.5f), new Vector2(1f, verticalScale), SpriteEffects.None);

            float middleScale = deathrayLength - deathrayTex.Width * 2;
            if (middleScale >= 1)
                Main.EntitySpriteDraw(deathrayTex, deathrayStartPos + deathrayNormalVect * deathrayTex.Width - Main.screenPosition, new Rectangle(deathrayTex.Width - 1, 0, 1, deathrayTex.Height), Color.White, deathrayRot, new Vector2(0, deathrayTex.Height * 0.5f), new Vector2(middleScale, verticalScale), SpriteEffects.None);

            Main.EntitySpriteDraw(deathrayTex, deathrayEndPoint - Main.screenPosition, null, Color.White, deathrayRot, new Vector2(1, deathrayTex.Height * 0.5f), new Vector2(1f, verticalScale), SpriteEffects.FlipHorizontally);
            return false;
        }
    }
}
