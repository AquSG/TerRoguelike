using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using System.Collections.Generic;

namespace TerRoguelike.Projectiles
{
    public class DisposableTurretMinion : ModProjectile, ILocalizedModType
    {
        public Texture2D headTex;
        public int maxTimeLeft;
        public static readonly SoundStyle TurretFire = new SoundStyle("TerRoguelike/Sounds/TurretFire");
        public static readonly SoundStyle TurretSpawn = new SoundStyle("TerRoguelike/Sounds/TurretSpawn");
        public static readonly SoundStyle TurretEnd = new SoundStyle("TerRoguelike/Sounds/TurretEnd");
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 1320;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            headTex = TexDict["DisposableTurretMinionHead"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = -1;
            Projectile.rotation = -MathHelper.PiOver2;
        }
        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            int time = maxTimeLeft - Projectile.timeLeft;
            if (Projectile.localAI[1] > 0)
                Projectile.localAI[1]--;

            if (time <= 40 || Projectile.timeLeft <= 40)
            {
                if (time == 0)
                    SoundEngine.PlaySound(TurretSpawn with { Volume = 0.16f, PitchVariance = 0 }, Projectile.Center);
                else if (Projectile.timeLeft == 40)
                    SoundEngine.PlaySound(TurretEnd with { Volume = 0.1f, PitchVariance = 0 }, Projectile.Center);

                Projectile.frameCounter = 0;
                if (time <= 40)
                {
                    if (time >= 25)
                    {
                        float targetRot = Projectile.localAI[0] == -1 ? MathHelper.Pi : 0;
                        float lookAtCompletion = (time - 25) / 15f;
                        Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, lookAtCompletion);
                    }
                }
                else
                {
                    if (Projectile.timeLeft <= 40)
                    {
                        float targetRot = -MathHelper.PiOver2;
                        float lookAtCompletion = 1 - ((Projectile.timeLeft - 25) / 15f);
                        Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, lookAtCompletion);
                    }
                }
                return;
            }

            if (Projectile.ai[1] != 0)
            {
                Projectile.ai[1] -= Math.Sign(Projectile.ai[1]);
            }
            if (Projectile.ai[0] >= 0)
            {
                NPC checkNPC = Main.npc[(int)Projectile.ai[0]];
                if (!checkNPC.active || checkNPC.immortal || checkNPC.dontTakeDamage || checkNPC.life <= 0 || checkNPC.friendly || !CanHitInLine(Projectile.Top, checkNPC.Center, 2501))
                {
                    Projectile.ai[0] = -1;
                    Projectile.ai[1] = -40;
                }
            }
            if (Projectile.ai[0] < 0 && Projectile.ai[1] >= 0)
            {
                Vector2 losStart = Projectile.Top;
                float distanceToBeat = 2500;
                int closest = -1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.immortal || npc.dontTakeDamage || npc.life <= 0 || npc.friendly)
                        continue;
                    float distance = losStart.Distance(npc.Center);
                    if (distance >= distanceToBeat || !CanHitInLine(losStart, npc.Center, 2500))
                        continue;

                    distanceToBeat = distance;
                    closest = i;
                }
                if (closest >= 0)
                {
                    Projectile.ai[0] = closest;
                    Projectile.ai[1] = -40;
                }
            }

            if (Projectile.ai[0] >= 0)
            {
                Projectile.frameCounter++;

                NPC npc = Main.npc[(int)Projectile.ai[0]];
                Vector2 targetVect = npc.Center - Projectile.Top;
                float targetRot = targetVect.ToRotation();
                if (Projectile.ai[1] < 0)
                {
                    float lookAtCompletion = MathHelper.Clamp(1 - (-Projectile.ai[1] / 40f), 0, 1);
                    Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, lookAtCompletion);
                }
                else
                {
                    Projectile.rotation = targetRot;
                }
                

                Projectile.localAI[0] = Math.Abs(Projectile.rotation) > MathHelper.PiOver2 ? -1 : 1;

                if (Projectile.ai[1] == 0)
                {
                    Projectile.ai[1] = 40;
                    Projectile.localAI[1] = 40;
                    Vector2 targetRotVect = targetRot.ToRotationVector2();
                    Vector2 projSpawnPos = Projectile.Top + targetRotVect * 14;
                    int spawnedProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), projSpawnPos, targetRotVect * 11.25f, ModContent.ProjectileType<AdaptiveGunBullet>(), 100, 1f, Projectile.owner);
                    ParticleManager.AddParticle(new Square(
                        Projectile.Top + new Vector2(14, 4 * Projectile.localAI[0]).RotatedBy(Projectile.rotation), (Projectile.rotation + (MathHelper.PiOver2 + Main.rand.NextFloat(0.16f, 0.25f)) * Projectile.localAI[0]).ToRotationVector2() * 2, 
                        20, Color.Yellow * 0.75f, new Vector2(3, 1) * 0.6f, Projectile.rotation, 0.86f, 20, false));

                    SoundEngine.PlaySound(TurretFire with { Volume = 0.3f, PitchVariance = 0.3f, Pitch = 0.4f }, projSpawnPos);

                    if (Projectile.owner >= 0)
                    {
                        var modOwner = Main.player[Projectile.owner].ModPlayer();
                        if (modOwner != null)
                            Main.projectile[spawnedProj].scale = modOwner.scaleMultiplier;
                    }
                }
            }
            else
            {
                Projectile.frameCounter = 0;
                float targetRot = Projectile.localAI[0] == -1 ? MathHelper.Pi : 0;
                float lookAtCompletion = MathHelper.Clamp(1 - (-Projectile.ai[1] / 40f), 0, 1);
                Projectile.rotation = Projectile.rotation.AngleLerp(targetRot, lookAtCompletion);
            }
        }
        public override bool? CanDamage() => false;
        public override bool PreDraw(ref Color lightColor)
        {
            int bodyGrownTimestamp = 10;
            int nearDeadTime = Projectile.timeLeft > 40 ? 100 : Math.Abs(40 - Projectile.timeLeft);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int time = maxTimeLeft - Projectile.timeLeft;
            float baseScale = 1;
            if (time < bodyGrownTimestamp || Projectile.timeLeft < bodyGrownTimestamp)
            {
                if (time < bodyGrownTimestamp)
                {
                    baseScale *= time / (float)bodyGrownTimestamp;
                }
                else
                {
                    baseScale *= (Projectile.timeLeft / (float)bodyGrownTimestamp);
                }
            }

            Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            Main.EntitySpriteDraw(tex, Projectile.Bottom - Main.screenPosition, null, color, 0, tex.Size() * new Vector2(0.5f, 1f), Projectile.scale * baseScale, SpriteEffects.None);

            if (time < bodyGrownTimestamp || Projectile.timeLeft < bodyGrownTimestamp)
                return false;
            Vector2 headOff = Vector2.Zero;
            Vector2 headScale = Vector2.One;
            int headGrowTime = 15;
            if (time < bodyGrownTimestamp + headGrowTime || Projectile.timeLeft < bodyGrownTimestamp + headGrowTime)
            {
                if (time < bodyGrownTimestamp + headGrowTime)
                {
                    float interpolant = (time - bodyGrownTimestamp) / (float)headGrowTime;
                    headOff += Vector2.UnitY * MathHelper.Lerp(12, 0, interpolant);
                    headScale *= interpolant;
                }
                else
                {
                    float interpolant = (Projectile.timeLeft - bodyGrownTimestamp) / (float)headGrowTime;
                    headOff += Vector2.UnitY * MathHelper.Lerp(12, 0, interpolant);
                    headScale *= interpolant;
                }
            }
            if (Projectile.localAI[1] > 0)
            {
                headOff += Projectile.rotation.ToRotationVector2() * -2.5f * MathHelper.Clamp((float)Math.Pow((Projectile.localAI[1]) / 40f, 2), 0, 1);
            }

            int headFrameHeight = headTex.Height / 2;
            Rectangle headFrame = new Rectangle(0, 0, headTex.Width, headFrameHeight - 2);
            Main.EntitySpriteDraw(headTex, Projectile.Top - Main.screenPosition + headOff, headFrame, color, Projectile.rotation, headFrame.Size() * new Vector2(0.37f, 0.47f), Projectile.scale * headScale, Projectile.localAI[0] == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);

            headFrame.Y += headFrameHeight;
            Color headLightColor = Projectile.ai[0] < 0 ? new Color(14, 209, 69) : (Projectile.frameCounter % 10 < 5 ? new Color(236, 28, 36) : Color.White);
            Main.EntitySpriteDraw(headTex, Projectile.Top - Main.screenPosition + headOff, headFrame, headLightColor, Projectile.rotation, headFrame.Size() * new Vector2(0.37f, 0.47f), Projectile.scale * headScale, Projectile.localAI[0] == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);

            return false;
        }
    }
}
