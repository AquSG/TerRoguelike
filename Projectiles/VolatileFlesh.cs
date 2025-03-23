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
    public class VolatileFlesh : ModProjectile, ILocalizedModType
    {
        public List<float> spikes = [];
        public Texture2D spikeTex;
        public Texture2D lineTex;
        public Texture2D glowTex;
        public int maxTimeLeft;
        public int direction;
        public bool initialized = false;
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 90;
            Projectile.penetrate = -1;
            spikeTex = TexDict["CurvedSpike"];
            lineTex = TexDict["LerpLineGradient"];
            glowTex = TexDict["CircularGlow"];
            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public void Initialize()
        {
            initialized = true;

            Vector2 lineStart = Projectile.Center + new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Vector2 lineVector = Projectile.Center - lineStart;
            float distance = lineVector.Length();
            float rot = lineVector.ToRotation();
            bool spawn = false;
            for (int i = 16; i < distance; i += 8)
            {
                spawn = !spawn;
                float completion = i / distance;
                Vector2 pos = lineStart + lineVector * completion;
                ParticleManager.AddParticle(new Spark(
                    pos, Vector2.Zero, 30, Color.Black, new Vector2(0.5f, 0.5f), rot, false, SpriteEffects.None, true, false));
                if (!spawn)
                    continue;
                ParticleManager.AddParticle(new ThinSpark(
                    pos, Vector2.Zero, 30, Color.Red, new Vector2(0.1f, 0.1f), rot, true, false));
            }
            for (int t = 0; t < 20; t++)
            {
                Rectangle rect = Projectile.getRect();
                for (int i = 0; i < 6; i++)
                {
                    if (i == 4)
                        rect.Inflate(-4, -4);
                    Color color = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                    Vector2 vel = Main.rand.NextVector2Circular(1.3f, 1.3f);
                    Vector2 offset = vel * (1f - (t / 20f)) * 5;
                    ParticleManager.AddParticle(new Ball(
                        Main.rand.NextVector2FromRectangle(rect) + offset, vel + Projectile.velocity,
                        t, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 15));
                }
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            direction = Main.rand.NextBool() ? -1 : 1;
            Initialize();
            float startingRot = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 5; i++)
            {
                float completion = i / 5f;
                spikes.Add(startingRot + (completion * MathHelper.TwoPi) + Main.rand.NextFloat(-0.4f, 0.4f));
            }
        }
        public override void AI()
        {
            if (!initialized)
                Initialize();
            Rectangle rect = Projectile.getRect();
            for (int i = 0; i < 6; i++)
            {
                if (i == 4)
                    rect.Inflate(-4, -4);
                Color color = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                ParticleManager.AddParticle(new Ball(
                    Main.rand.NextVector2FromRectangle(rect), Main.rand.NextVector2Circular(1.3f, 1.3f) + Projectile.velocity,
                    20, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 15));
            }
            float completion = Projectile.timeLeft / (float)maxTimeLeft;
            for (int i = 0; i < spikes.Count; i++)
            {
                float speed = i / (float)spikes.Count * 0.5f + 0.5f;
                spikes[i] += speed * 0.05f * completion * direction;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath19 with { Volume = 0.8f, Pitch = -0.15f, PitchVariance = 0.05f, MaxInstances = 6 }, Projectile.Center);
            for (int i = 0; i < spikes.Count; i++)
            {
                Vector2 rot = spikes[i].ToRotationVector2();
                if (!TerRoguelike.mpClient)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + rot * 8, rot * 7, ModContent.ProjectileType<BloodClot>(), Projectile.damage, 0);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float completion = 1f - (Projectile.timeLeft / (float)maxTimeLeft);
            Color spikeColor = Color.Lerp(Color.Red, Color.DarkRed, 0.25f);
            Vector2 spikeOrigin = new Vector2(0, spikeTex.Height * 0.5f);

            TerRoguelikeUtils.StartNonPremultipliedSpritebatch();

            Color lineColor = Color.Red;
            lineColor.A = (byte)(lineColor.A * completion);
            Vector2 lineOrigin = new Vector2(0, lineTex.Height * 0.5f);

            float glowScale = 1f;
            if (completion < 0.05f)
                glowScale *= completion / 0.05f;
            else if (completion > 0.95f)
                glowScale *= 1f - ((completion - 0.95f) / 0.05f);
            glowScale = MathHelper.SmoothStep(0, 1, glowScale);

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Black * 0.75f * glowScale, 0, glowTex.Size() * 0.5f, 0.4f * glowScale, SpriteEffects.None);
            for (int i = 0; i < spikes.Count; i++)
            {
                float rot = spikes[i];
                Main.EntitySpriteDraw(lineTex, Projectile.Center - Main.screenPosition + rot.ToRotationVector2() * 8, null, lineColor, rot, lineOrigin, new Vector2(0.7f, 0.7f), SpriteEffects.None);
            }

            TerRoguelikeUtils.StartVanillaSpritebatch();

            for (int i = 0; i < spikes.Count; i++)
            {
                float rot = spikes[i];
                Main.EntitySpriteDraw(spikeTex, Projectile.Center - Main.screenPosition + rot.ToRotationVector2() * 8, null, spikeColor, rot, spikeOrigin, new Vector2(0.5f, 0.3f), SpriteEffects.None);
            }

            
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(direction);
            writer.Write(spikes.Count);
            for (int i = 0; i < spikes.Count; i++)
            {
                writer.Write(spikes[i]);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            direction = reader.ReadInt32();
            int count = reader.ReadInt32();
            spikes.Clear();
            for (int i = 0; i < count; i++)
            {
                spikes.Add(reader.ReadSingle());
            }
        }
    }
}
