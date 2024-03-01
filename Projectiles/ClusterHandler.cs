using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace TerRoguelike.Projectiles
{
    public class ClusterHandler : ModProjectile, ILocalizedModType
    {
        //almost everything in this is just visuals. the hitbox is active for 1/4 of a second after 30 frames pass, and is a big square
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public float MaxScale = -1f;
        public List<Vector2> bombPositions = new List<Vector2>(); // purely visual 
        public List<Vector2> bombVelocities = new List<Vector2>(); // purely visual
        public List<float> bombRotations = new List<float>(); // purely visual
        public List<int> bombDetonateOffset = new List<int>(); // purely visual
        public float randomSmokeRotation = -100f; // purely visual
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
        }

        public override void AI()
        {
            if (MaxScale == -1f)
            {
                //scale support
                MaxScale = Projectile.scale * 2.56f;
                Projectile.position = Projectile.Center + new Vector2(-50 * MaxScale, -50 * MaxScale);
                Projectile.width = (int)(100 * MaxScale);
                Projectile.height = (int)(100 * MaxScale);
                for (int i = 0; i < (int)(10 * MaxScale); i++)
                {
                    Vector2 bombVector = Main.rand.NextVector2Circular(2.5f, 2.5f) * MaxScale;
                    bombPositions.Add(bombVector);
                    bombVelocities.Add(bombVector);
                    bombRotations.Add(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi + float.Epsilon));
                    bombDetonateOffset.Add(Main.rand.Next(12));
                }
                SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite with { Volume = 0.8f }, Projectile.Center);
            }

            if (Projectile.timeLeft == 60)
            {
                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Volume = 0.625f }, Projectile.Center);
            }
            if (Projectile.timeLeft <= 60)
            {
                Projectile.frameCounter++;
            }

        }

        public override bool? CanDamage() => Projectile.timeLeft <= 60 && Projectile.timeLeft >= 45 ? (bool?)null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bombTex = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/SingularClusterBomb").Value;
            Texture2D explosionTex = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/Explosion").Value;
            Texture2D smokeTex = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/Smoke").Value;

            if (randomSmokeRotation == -100f)
            {
                randomSmokeRotation = Main.rand.NextFloatDirection();
            }
            if (Projectile.timeLeft > 48)
            {
                for (int i = 0; i < bombPositions.Count; i++)
                {
                    if (Projectile.frameCounter - bombDetonateOffset[i] > 0 || Main.gamePaused)
                        continue;

                    if (bombRotations[i] > 0)
                        bombRotations[i] += bombVelocities[i].Length() / 5f;
                    else
                        bombRotations[i] -= bombVelocities[i].Length() / 5f;


                    Main.EntitySpriteDraw(bombTex, bombPositions[i] + Projectile.Center - Main.screenPosition, null, lightColor.MultiplyRGB(Color.Orange * 2f), bombRotations[i], bombTex.Size() * 0.5f, 1f, SpriteEffects.None);

                    bombPositions[i] += bombVelocities[i];
                    bombVelocities[i] *= 0.975f;

                    if (Main.GlobalTimeWrappedHourly % 0.05f > 0.025f)
                    {
                        Dust dust = Dust.NewDustPerfect(bombPositions[i] + Projectile.Center + (-Vector2.UnitY * 6f).RotatedBy(bombRotations[i]), DustID.YellowTorch);
                        dust.noLight = true;
                        dust.noLightEmittence = true;
                        dust.noGravity = true;
                    }
                }
            }
            if (Projectile.timeLeft <= 60)
            {
                Vector2 smokeOffset = new Vector2(0, MathHelper.Lerp(-72, 0, Projectile.timeLeft / 60f));
                Color smokeColor = Color.Black * MathHelper.Lerp(0f, 0.4f, Projectile.timeLeft / 60f);

                Main.EntitySpriteDraw(smokeTex, Projectile.Center - Main.screenPosition + smokeOffset, null, smokeColor, 0f, smokeTex.Size() * 0.5f, MaxScale, SpriteEffects.None);
                Main.EntitySpriteDraw(smokeTex, Projectile.Center - Main.screenPosition + smokeOffset + new Vector2(0, 16), null, smokeColor, randomSmokeRotation, smokeTex.Size() * 0.5f, MaxScale, SpriteEffects.None);
            }
            if (Projectile.frameCounter > 0)
            {
                int frameHeight = explosionTex.Height / Main.projFrames[Projectile.type];
                for (int i = 0; i < bombPositions.Count; i++)
                {
                    if (Projectile.frameCounter - bombDetonateOffset[i] <= 0 || Projectile.frameCounter - bombDetonateOffset[i] >= 20)
                        continue;

                    Projectile.frame = (Projectile.frameCounter - bombDetonateOffset[i]) / 4;
                    Main.EntitySpriteDraw(explosionTex, bombPositions[i] + Projectile.Center - Main.screenPosition, new Rectangle(0, frameHeight * Projectile.frame, explosionTex.Width, frameHeight), Color.White * 0.8f, 0, new Vector2(explosionTex.Width * 0.5f, frameHeight * 0.5f), 0.5f, SpriteEffects.None);
                }
            }
            return false;
        }
    }
}
