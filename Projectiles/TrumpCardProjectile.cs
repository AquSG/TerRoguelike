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
using Terraria.GameContent;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using TerRoguelike.Particles;

namespace TerRoguelike.Projectiles
{
    public class TrumpCardProjectile : ModProjectile, ILocalizedModType
    {
        //This is basically the same as missile but changed for different visuals and allowing wiggling in all directions.
        //also gets killed on room clear
        public float rotationOffset = 0;
        public bool ableToHit = true;
        public TerRoguelikeGlobalProjectile modProj;
        TerRoguelikePlayer modPlayer;
        public int setTimeLeft = 1200;
        public Vector2 originalDirection;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = setTimeLeft;
            Projectile.MaxUpdates = 2;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            modProj = Projectile.ModProj();
            modProj.killOnRoomClear = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1] += Main.rand.Next(30);
                Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
                originalDirection = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
            }
            if (Projectile.numUpdates == -1)
                Projectile.localAI[1]++;

            if (modPlayer == null)
                modPlayer = Main.player[Projectile.owner].GetModPlayer<TerRoguelikePlayer>();

            if (modProj.homingTarget == -1 && Projectile.ai[0] != -1)
                modProj.homingTarget = (int)Projectile.ai[0];

            float homingStrength = 8f;
            homingStrength *= MathHelper.Lerp(1f, 3f, (setTimeLeft - Projectile.timeLeft) / (float)setTimeLeft);
            if (Projectile.localAI[1] > 60)
            {
                float maxVel = 7;
                if (Projectile.velocity.Length() < maxVel)
                    Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitY) * 0.3f;
                float finalVelLength = Projectile.velocity.Length();
                if (finalVelLength > maxVel)
                {
                    finalVelLength = maxVel;
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * maxVel;
                }

                homingStrength *= finalVelLength / maxVel;
                modProj.HomingAI(Projectile, 0.001128f * homingStrength, true);

                Vector2 baseParticlePos = Projectile.Center;
                var color = GetParticleColor();
                color = Color.Lerp(color, Color.White, 0.25f);
                for (int i = 0; i < 4; i++)
                {
                    ParticleManager.AddParticle(new Square(baseParticlePos + Projectile.velocity.SafeNormalize(Vector2.UnitY) * i * 2, Vector2.Zero, 10, color, new Vector2(1), Projectile.velocity.ToRotation(), 0.96f, 10, false));
                }
            }
            else
            {
                Projectile.velocity *= 0.99f;
                var color2 = GetParticleColor();
                color2 = Color.Lerp(color2, Color.White, 0.8f);
                if (Projectile.numUpdates == -1 && !Main.rand.NextBool(3))
                    ParticleManager.AddParticle(new Square(Projectile.Center, Main.rand.NextVector2CircularEdge(2, 2), 20, color2 * 0.9f, new Vector2(0.8f), Projectile.velocity.ToRotation(), 0.96f, 10, false));
            }

            

            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact with { Volume = 1f, Pitch = 0.5f }, Projectile.Center);
            Color baseColor = GetParticleColor();
            for (int i = 0; i < 20; i++)
            {
                ParticleManager.AddParticle(new Square(Projectile.Center, Main.rand.NextVector2Circular(2, 2) + Projectile.velocity * 0.25f, 
                    45, Color.Lerp(baseColor, Color.White, Main.rand.NextFloat(0.25f)) * 0.5f, new Vector2(1f), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 30, false));
            }
        }
        public Color GetParticleColor()
        {
            return Projectile.frame switch
            {
                0 => Color.Red,
                1 => Color.Yellow,
                2 => Color.Green,
                _ => Color.Blue,
            };
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight - 2);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation + MathHelper.PiOver4, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

            return false;
        }
    }
}
