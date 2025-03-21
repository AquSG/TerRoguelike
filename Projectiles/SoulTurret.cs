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
using static TerRoguelike.Managers.TextureManager;
using System.IO;

namespace TerRoguelike.Projectiles
{
    public class SoulTurret : ModProjectile, ILocalizedModType
    {
        public Entity target = null;
        public Vector2 aimingDirection;
        public Texture2D glowTex;
        public Texture2D crossGlowTex;
        public int maxTimeLeft;
        public int startup = 160;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            glowTex = TexDict["CircularGlow"];
            crossGlowTex = TexDict["CrossSpark"];
            Projectile.hide = true;
            Projectile.ModProj().killOnRoomClear = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[2] = -21;
            Vector2 spawnOffset = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Projectile.velocity = spawnOffset * -0.02f;
            Projectile.Center += spawnOffset;
            aimingDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        }
        public override void AI()
        {
            Projectile.netSpam = 0;
            int time = maxTimeLeft - Projectile.timeLeft;

            if (Projectile.ai[2] >= -20)
                Projectile.ai[2]++;
            if (target != null)
            {
                aimingDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);

                if (time >= 160 && Projectile.timeLeft > 60 && Projectile.ai[2] == -21 && Projectile.Center.Distance(target.Center) < 320 && TerRoguelikeUtils.CanHitInLine(Projectile.Center, target.Center))
                {
                    Projectile.ai[2] = -20;
                    Projectile.timeLeft = 120;
                    SoundEngine.PlaySound(SoundID.Item176 with { Volume = 0.5f, MaxInstances = 1, Pitch = 0.5f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item82 with { Volume = 0.8f, MaxInstances = 1, Pitch = 0.7f, PitchVariance = 0, }, Projectile.Center);
                    for (int i = 0; i < 32; i++)
                    {
                        float completion = i / 32f;
                        float rot = MathHelper.TwoPi * completion + Main.rand.NextFloat(-0.04f, 0.04f);
                        Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.5f));
                        ParticleManager.AddParticle(new ThinSpark(
                            Projectile.Center + rot.ToRotationVector2() * 10, rot.ToRotationVector2() * 5 + Projectile.velocity,
                            20, color, new Vector2(0.25f, 0.4f) * Main.rand.NextFloat(0.7f, 1f) * 0.6f, rot, true, false));
                    }
                }
            }
                

            Projectile.velocity *= 0.98f;

            if (time >= 130)
            {
                var modProj = Projectile.ModProj();
                if (Projectile.ai[2] == -21 && time % 10 == 0)
                {
                    modProj.targetPlayer = -1;
                    modProj.targetNPC = -1;
                }
                target = modProj.GetTarget(Projectile);
            }
            else
            {
                var modProj = Projectile.ModProj();
                modProj.targetNPC = -1;
                modProj.targetPlayer = -1;
            }
            int count = 6;
            if (time < 160)
            {
                count = time / 90 + 1;
                if (time == 130)
                    SoundEngine.PlaySound(SoundID.Zombie53 with { Volume = 0.085f, MaxInstances = 30 }, Projectile.Center);
            }   
            if (time == 160)
            {
                for (int i = 0; i < 16; i++)
                {
                    float completion = i / 16f;
                    float rot = MathHelper.TwoPi * completion + Main.rand.NextFloat(-0.1f, 0.1f);
                    Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat());
                    ParticleManager.AddParticle(new Ball(
                        Projectile.Center + rot.ToRotationVector2() * 10, rot.ToRotationVector2() * Main.rand.NextFloat(0.5f, 2f) + Projectile.velocity,
                        17, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 15));
                }
            }
            for (int i = 0; i < count; i++)
            {
                float radius = count <= 2 ? 8 : 12;
                Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat());
                ParticleManager.AddParticle(new Ball(
                    Main.rand.NextVector2Circular(radius, radius) + Projectile.Center, Main.rand.NextVector2Circular(1.3f, 1.3f) + Projectile.velocity * 0.5f,
                    20, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 15));
            }

            if (Projectile.ai[2] >= 0 && Projectile.timeLeft >= 60 && Projectile.ai[2] % 8 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.2f, Pitch = 0.5f, PitchVariance = 0.08f }, Projectile.Center);
                Projectile.localAI[0] = 5;

                if (!TerRoguelike.mpClient)
                {
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + aimingDirection * 10, aimingDirection * 5, ModContent.ProjectileType<SoulBlast>(), Projectile.damage, 0, -1, 0.65f);
                    Main.projectile[proj].tileCollide = true;
                }
            }
        }
        public override bool? CanDamage() => (maxTimeLeft - Projectile.timeLeft) >= startup + 20 ? null : false;
        public override void OnKill(int timeLeft)
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft - startup;
            float completion = time / (maxTimeLeft - (float)startup);
            if (time < 0)
                return false;

            TerRoguelikeUtils.StartAdditiveSpritebatch();

            float glowScale = 1f;
            if (completion < 0.1f)
                glowScale *= completion / 0.1f;
            else if (completion > 0.9f)
                glowScale *= 1f - ((completion - 0.9f) / 0.1f);
            glowScale = (float)Math.Pow(glowScale, 0.1f);

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Cyan * 0.75f * glowScale, 0, glowTex.Size() * 0.5f, 0.14f * glowScale, SpriteEffects.None);

            if (time >= 0)
            {
                float crossOpacity = time < 20 ? time / 20f : (Projectile.timeLeft) / 60f;
                crossOpacity = MathHelper.Clamp(crossOpacity, 0, 1);
                Color crossColor = Color.Lerp(Color.Cyan * 0.92f, Color.White, Projectile.localAI[0] / 5) * 0.97f;
                Main.EntitySpriteDraw(crossGlowTex, Projectile.Center - Main.screenPosition + aimingDirection * 10, null, crossColor * crossOpacity, aimingDirection.ToRotation(), new Vector2(0, crossGlowTex.Size().Y * 0.5f), Projectile.scale * 0.14f * new Vector2(1f, 2f), SpriteEffects.FlipHorizontally);
            }


            TerRoguelikeUtils.StartVanillaSpritebatch();


            
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {

        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            
        }
    }
}
