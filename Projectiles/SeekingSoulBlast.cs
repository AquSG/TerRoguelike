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
using static TerRoguelike.Managers.TextureManager;
using Terraria.GameContent;
using TerRoguelike.Particles;
using ReLogic.Utilities;

namespace TerRoguelike.Projectiles
{
    public class SeekingSoulBlast : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public Entity target = null;
        public float startSpeed;
        public int maxTimeLeft;
        public SlotId dashSlot;
        public override string Texture => "TerRoguelike/Projectiles/SoulBlast";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = maxTimeLeft = 400;
            glowTex = TexDict["CircularGlow"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            startSpeed = Projectile.velocity.Length();
            Projectile.velocity *= 1f;

            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = Projectile.Center + Projectile.rotation.ToRotationVector2() * -12 + (Vector2.UnitY * Main.rand.NextFloat(-6, 6)).RotatedBy(Projectile.rotation);
                Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.75f));
                ParticleManager.AddParticle(new Ball(
                    pos, (Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f)).ToRotationVector2() * Main.rand.NextFloat(0.3f, 3f),
                    30, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.98f, 30));
            }
            
        }
        public override void AI()
        {
            var modProj = Projectile.ModProj();
            target = modProj.GetTarget(Projectile);

            int time = maxTimeLeft - Projectile.timeLeft;
            Projectile.frame = (Projectile.frameCounter / 6) % Main.projFrames[Type];
            Projectile.frameCounter++;


            float magnitude = startSpeed;
            Vector2 targetPos = target != null ? target.Center : new Vector2(Projectile.ai[0], Projectile.ai[1]);
            float rotToTarget = (targetPos - Projectile.Center).ToRotation();
            if (time < 30)
            {
                if (time > 3)
                {
                    Projectile.rotation = Projectile.rotation.AngleLerp(rotToTarget, 0.14f);
                }

                magnitude *= 1f - (float)Math.Pow((time / 30f), 1.1d);

                if (time == 29)
                    dashSlot = SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.4f, MaxInstances = 4, Pitch = 0.14f }, Projectile.Center);
            }
            else if (time < 45 && Math.Abs(TerRoguelikeUtils.AngleSizeBetween(Projectile.rotation, rotToTarget)) < MathHelper.PiOver2)
            {
                Projectile.rotation = Projectile.rotation.AngleTowards(rotToTarget, 0.01f);
            }
                
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * magnitude;

            float amplitude = (float)Math.Cos(time * 0.3f);
            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 pos = Projectile.Center + (Vector2.UnitY * i * amplitude * 8).RotatedBy(Projectile.rotation);
                Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.5f));
                ParticleManager.AddParticle(new Ball(
                    pos, Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(0.2f, 0.2f),
                    10, color, new Vector2(0.2f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.96f, 8));
            }
            
            if (SoundEngine.TryGetActiveSound(dashSlot, out var sound) && sound.IsPlaying)
            {
                sound.Position = Projectile.Center;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, 0, 0, 0, default, 1.5f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight - 2);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.66f, frameHeight * 0.5f), 1f, SpriteEffects.None, 0);

            TerRoguelikeUtils.StartAdditiveSpritebatch();

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Cyan * 0.65f, 0f, glowTex.Size() * 0.5f, 0.11f, SpriteEffects.None, 0);

            TerRoguelikeUtils.StartVanillaSpritebatch();

            return false;
        }
    }
}
