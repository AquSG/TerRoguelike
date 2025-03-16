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
using TerRoguelike.Particles;
using System.IO;

namespace TerRoguelike.Projectiles
{
    public class PhantasmalSphere : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public float timeOffset;
        public Entity target;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 480;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ModProj().killOnRoomClear = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.ai[1] <= 0)
                Projectile.ai[1] = Projectile.velocity.Length();
            Projectile.velocity = Vector2.Zero;
            if (Projectile.ai[0] >= 0)
            {
                NPC npc = Main.npc[(int)Projectile.ai[0]];
                if (!npc.active)
                    Projectile.ai[0] = -1;
            }
            Projectile.timeLeft -= (int)Projectile.ai[2];
        }
        public override void AI()
        {
            Projectile.netSpam = 0;
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 10 % Main.projFrames[Type];
            var modProj = Projectile.ModProj();


            int time = maxTimeLeft - Projectile.timeLeft;
            float circleEdge = 30;
            if (time < 30)
                circleEdge *= (time + 1) / 30f;
            ParticleManager.AddParticle(new Ball(
                Projectile.Center + Main.rand.NextVector2CircularEdge(circleEdge, circleEdge), Main.rand.NextVector2Circular(1.5f, 1.5f) + Projectile.velocity,
                15, Color.Lerp(Color.Teal, Color.LightCyan, Main.rand.NextFloat()), new Vector2(0.15f), 0, 0.96f, 15, false));

            if (time % 10 == 0)
                Projectile.netUpdate = true;
            if (time < 30)
            {
                if (Projectile.ai[0] >= 0)
                {
                    NPC npc = Main.npc[(int)Projectile.ai[0]];
                    if (npc.active)
                    {
                        Projectile.Center = npc.Center;
                    }
                }
            }
            else if (time < 150)
            {
                if (time == 30)
                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * Projectile.ai[1];
                else
                    Projectile.velocity *= 0.97f;
                
            }
            else if (time < 180)
            {
                if (time % 5 == 0)
                {
                    Vector2 randOff = Main.rand.NextVector2CircularEdge(2, 2);
                    Projectile.localAI[0] = randOff.X;
                    Projectile.localAI[1] = randOff.Y;
                }
            }
            else if (time < 210)
            {
                if (time % 5 == 0)
                {
                    Vector2 randOff = Main.rand.NextVector2CircularEdge(2, 2);
                    Projectile.localAI[0] = randOff.X;
                    Projectile.localAI[1] = randOff.Y;
                }

                target = modProj.GetTarget(Projectile);

                Vector2 targetPos = target != null ? target.Center : Projectile.Center + Vector2.UnitY;
                Projectile.rotation = (targetPos - Projectile.Center).ToRotation();
                float windupCompletion = 1 - ((time - 150) / 30f);

                Projectile.Center += Projectile.rotation.ToRotationVector2() * 3 * windupCompletion;
                if (time == 195)
                    SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaivePierce with { Volume = 0.3f, Pitch = -1f, MaxInstances = 10 }, Projectile.Center);
            }
            else
            {
                target = modProj.GetTarget(Projectile);

                Projectile.localAI[0] = 0;
                Projectile.localAI[1] = 0;
                if (time == 210)
                {
                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * 15;
                    SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.12f, Pitch = 0.1f, MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
                }
                if (time % 2 == 0)
                {
                    for (int i = -1; i <= 1; i += 2)
                    {
                        ParticleManager.AddParticle(new ThinSpark(
                            Projectile.Center + (Vector2.UnitY * 28 * i).RotatedBy(Projectile.rotation), (-Vector2.UnitY * i * 0.7f).RotatedBy(Projectile.rotation),
                            30, Color.Lerp(Color.Teal, Color.Cyan, 0.4f), new Vector2(0.1f, 0.05f), Projectile.rotation, true, false));
                    }
                }
                if (target != null)
                {
                    float targetRot = (target.Center - Projectile.Center).ToRotation();
                    if (Math.Abs(TerRoguelikeUtils.AngleSizeBetween(Projectile.rotation, targetRot)) < MathHelper.PiOver4)
                    {
                        float velLength = Projectile.velocity.Length();
                        Projectile.rotation = Projectile.rotation.AngleTowards(targetRot, 0.0002f * velLength);
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * velLength;
                        Projectile.netUpdate = true;
                    }
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Color outlineColor = Color.Lerp(Color.Teal, Color.Cyan, 0.4f);
                Vector2 offset = Main.rand.NextVector2Circular(3.5f, 3.5f);
                ParticleManager.AddParticle(new BallOutlined(
                    Projectile.Center - offset, offset,
                    21, outlineColor, Color.White * 0.75f, new Vector2(0.3f), 5, 0, 0.96f, 15));
            }
        }
        public override bool? CanDamage() => maxTimeLeft - (Projectile.timeLeft + (int)(Projectile.ai[2])) >= 30 ? null : false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Vector2 offset = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);

            int time = maxTimeLeft - (Projectile.timeLeft + (int)(Projectile.ai[2]));
            float spawnCompletion = 1f;
            if (time < 30)
            {
                spawnCompletion = time / 30f;
            }

            Main.EntitySpriteDraw(tex, Projectile.Center + offset - Main.screenPosition, frame, Color.White * spawnCompletion, 0, frame.Size() * 0.5f, Projectile.scale * spawnCompletion, SpriteEffects.None);
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.timeLeft);
            writer.Write(timeOffset);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.timeLeft = reader.ReadInt32();
            timeOffset = reader.ReadSingle();
        }
    }
}
