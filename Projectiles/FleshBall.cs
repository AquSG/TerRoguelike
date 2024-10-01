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
using Terraria.DataStructures;
using Terraria.Audio;
using ReLogic.Utilities;
using TerRoguelike.Particles;
using TerRoguelike.Utilities;
using TerRoguelike.NPCs.Enemy.Boss;
using Terraria.Graphics.Effects;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.Projectiles
{
    public class FleshBall : ModProjectile, ILocalizedModType
    {
        public override string Texture => "TerRoguelike/Projectiles/SandTurret";
        public float MaxScale = -1f;
        public Texture2D texture;
        public int maxTimeLeft;
        public Texture2D noiseTex;
        public Texture2D glowTex;
        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 54;
            Projectile.timeLeft = maxTimeLeft = 2100;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            noiseTex = TexDict["BlobbyNoiseSmall"];
            glowTex = TexDict["CircularGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            float rot = Projectile.velocity.ToRotation();
            for (int i = -10; i <= 10; i++)
            {
                if (i == 0)
                    continue;

                Vector2 pos = Projectile.Center + rot.ToRotationVector2() * Projectile.width * 0.5f;
                pos += new Vector2(0, Math.Sign(i) * 1 + i).RotatedBy(rot); 
                Vector2 velocity = (rot + (i * MathHelper.PiOver4 * 0.1f) + (Math.Sign(i) * MathHelper.PiOver4 * 0.5f)).ToRotationVector2() * Main.rand.NextFloat(1.9f, 2.3f);

                if (Main.rand.NextBool(3))
                    velocity *= 1.5f;

                Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
                int time = 110 + Main.rand.Next(70);
                Color color = Color.Lerp(Color.Red * 0.65f, Color.Purple, Main.rand.NextFloat());
                ParticleManager.AddParticle(new Blood(pos, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                ParticleManager.AddParticle(new Blood(pos, velocity, time, color, scale, velocity.ToRotation(), true));
            }
        }
        public override void AI()
        {
            Projectile.rotation += 0.06f * Projectile.direction;
            Projectile.frameCounter -= 1 * Projectile.direction;

            var modProj = Projectile.ModProj();
            if (modProj.npcOwner >= 0)
            {
                NPC npc = Main.npc[modProj.npcOwner];
                var modNPC = npc.ModNPC();
                if (modNPC.isRoomNPC)
                {
                    if (RoomSystem.RoomList[modNPC.sourceRoomListID].bossDead)
                    {
                        if (Projectile.ai[0] < 0)
                            Projectile.ai[0] = 0;
                        OnTileCollide(Projectile.velocity);
                    }
                }
            }

            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;

        }
        public void SpawnBlood()
        {
            Vector2 baseParticlePos = Projectile.Center;
            for (int i = -5; i <= 5; i++)
            {
                Vector2 velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * 2;
                velocity *= Main.rand.NextFloat(0.5f, 1f);
                velocity.X += Math.Sign(Projectile.velocity.X) * Math.Sign(Projectile.oldVelocity.X) * -0.5f;
                velocity.Y -= 0.8f;
                Vector2 offset = (Vector2.UnitX).RotatedBy((i * 0.2f) + velocity.ToRotation()) * 40;
                if (Main.rand.NextBool(3))
                    velocity *= 1.5f;
                Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
                int time = 110 + Main.rand.Next(70);
                ParticleManager.AddParticle(new Blood(baseParticlePos + offset, velocity, time, Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                ParticleManager.AddParticle(new Blood(baseParticlePos + offset, velocity, time, Color.Red * 0.65f, scale, velocity.ToRotation(), true));
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 80; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width * 0.5f, Projectile.width * 0.5f);
                Vector2 pos = Projectile.Center + offset;
                Vector2 velocity = (pos - (Projectile.Center + Vector2.UnitY * 50)).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.3f, 1.6f) + Projectile.oldVelocity * 0.1f;

                if (Main.rand.NextBool(3))
                    velocity *= 1.5f;
                velocity.X += Math.Sign(velocity.X) * Main.rand.NextFloat(0.25f, 1f);
                Vector2 scale = new Vector2(0.25f, 0.4f) * 0.75f;
                int time = 110 + Main.rand.Next(70);
                Color color = Color.Lerp(Color.Red * 0.65f, Color.Purple, Main.rand.NextFloat());
                bool switchup = Main.rand.NextBool(6);
                ParticleManager.AddParticle(new Blood(pos, velocity, time, switchup ? Color.Black : Color.Black * 0.65f, scale, velocity.ToRotation(), false));
                if (!switchup)
                    ParticleManager.AddParticle(new Blood(pos, velocity, time, color, scale, velocity.ToRotation(), true));
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            SpawnProjectiles();
            if (Projectile.ai[0] >= 2)
            {
                Projectile.Kill();
                SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.4f }, Projectile.Center);
            }

            return Projectile.ai[0] >= 2;
        }
        public void SpawnProjectiles()
        {
            if (Projectile.localAI[0] > 0)
                return;

            SpawnBlood();
            Projectile.ai[0]++;
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 1f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.NPCHit19 with { Volume = 0.4f, Pitch = -0.6f, PitchVariance = 0.05f }, Projectile.Center);
            int projCount = 8;
            for (int i = 0; i < projCount; i++)
            {
                float rot = (float)i / projCount * MathHelper.TwoPi;
                Vector2 offset = rot.ToRotationVector2() * Projectile.width * 0.25f;
                Vector2 velocity = rot.ToRotationVector2() * 8f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + offset, velocity, ModContent.ProjectileType<BloodOrb>(), Projectile.damage, 0f, -1, 1);
            }

            Projectile.localAI[0] = 12;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int time = maxTimeLeft - Projectile.timeLeft;

            float scaleInterpolant = Projectile.timeLeft < 60 ? Projectile.timeLeft / 60f : (maxTimeLeft - Projectile.timeLeft) / 12f;
            scaleInterpolant = MathHelper.SmoothStep(0, 1, MathHelper.Clamp(scaleInterpolant, 0, 1));

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float scale = 0.974f + (float)Math.Cos(Projectile.frameCounter / 10f) * 0.026f;
            float opacity = 1f;
            if ((maxTimeLeft - Projectile.timeLeft) < 12)
                opacity *= (maxTimeLeft - Projectile.timeLeft) / 12f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Black * opacity, 0, glowTex.Size() * 0.5f, 0.27f * scale * scaleInterpolant, SpriteEffects.None);

            Main.spriteBatch.End();
            Effect maskEffect = Filters.Scene["TerRoguelike:MaskOverlay"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);

            Vector2 screenOff = new Vector2(Projectile.frameCounter / 180f, Projectile.frameCounter / 60f);
            Color tint = Color.Red;
            tint *= opacity;

            maskEffect.Parameters["screenOffset"].SetValue(screenOff);
            maskEffect.Parameters["stretch"].SetValue(new Vector2(0.75f));
            maskEffect.Parameters["replacementTexture"].SetValue(noiseTex);
            maskEffect.Parameters["tint"].SetValue(tint.ToVector4());

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale * 0.05f * scale * scaleInterpolant, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);

            maskEffect.Parameters["screenOffset"].SetValue(screenOff);
            maskEffect.Parameters["stretch"].SetValue(new Vector2(0.35f));
            maskEffect.Parameters["replacementTexture"].SetValue(noiseTex);
            maskEffect.Parameters["tint"].SetValue((Color.Magenta).ToVector4());
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation + 0.5f, tex.Size() * 0.5f, Projectile.scale * 0.05f * scale * scaleInterpolant, SpriteEffects.None);

            TerRoguelikeUtils.StartVanillaSpritebatch();
            return false;
        }
    }
}
