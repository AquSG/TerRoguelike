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
using Steamworks;
using System.IO;

namespace TerRoguelike.Projectiles
{
    public class SoulBlast : ModProjectile, ILocalizedModType
    {
        public Texture2D glowTex;
        public int maxTimeLeft;
        public float startSpeed;
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
            glowTex = TexDict["CircularGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            startSpeed = Projectile.velocity.Length();
            Projectile.velocity *= 0.5f;

            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = Projectile.Center + Projectile.rotation.ToRotationVector2() * -12 + (Vector2.UnitY * Main.rand.NextFloat(-6, 6)).RotatedBy(Projectile.rotation);
                Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.75f));
                ParticleManager.AddParticle(new Ball(
                    pos, (Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f)).ToRotationVector2() * Main.rand.NextFloat(0.3f, 3f),
                    30, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.98f, 30));
            }

            if (Projectile.ai[0] == 0)
                Projectile.ai[0] = 1;
            Vector2 cacheCenter = Projectile.Center;
            Projectile.scale = Projectile.ai[0];
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);
            Projectile.Center = cacheCenter;
        }
        public override void AI()
        {
            int time = maxTimeLeft - Projectile.timeLeft;
            Projectile.frame = (Projectile.frameCounter / 6) % Main.projFrames[Type];
            Projectile.frameCounter++;
            if (Projectile.velocity.Length() < startSpeed)
            {
                Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitY) * 0.075f;
                if (Projectile.velocity.Length() > startSpeed)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * (startSpeed + 0.01f);
                }
            }

            float amplitude = (float)Math.Cos(time * 0.3f) * Projectile.scale * 8;
            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 pos = Projectile.Center + (Vector2.UnitY * i * amplitude).RotatedBy(Projectile.rotation);
                Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.5f));
                ParticleManager.AddParticle(new Ball(
                    pos, Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(0.2f, 0.2f) * Projectile.scale,
                    10, color, new Vector2(0.2f) * Main.rand.NextFloat(0.7f, 1f) * Projectile.scale, 0, 0.96f, 8));
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = Projectile.Center + Projectile.rotation.ToRotationVector2() * -12 + (Vector2.UnitY * Main.rand.NextFloat(-6, 6)).RotatedBy(Projectile.rotation);
                Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.75f));
                ParticleManager.AddParticle(new Ball(
                    pos, (Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f)).ToRotationVector2() * Main.rand.NextFloat(0.3f, 3f),
                    30, color, new Vector2(0.25f) * Main.rand.NextFloat(0.7f, 1f), 0, 0.98f, 30));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight - 2);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.66f, frameHeight * 0.5f), Projectile.scale, SpriteEffects.None, 0);

            TerRoguelikeUtils.StartAdditiveSpritebatch();

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Cyan * 0.65f, 0f, glowTex.Size() * 0.5f, 0.11f * Projectile.scale, SpriteEffects.None, 0);

            TerRoguelikeUtils.StartVanillaSpritebatch();

            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(startSpeed);
            writer.Write(Projectile.tileCollide);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            startSpeed = reader.ReadSingle();
            Projectile.tileCollide = reader.ReadBoolean();
        }
    }
}
