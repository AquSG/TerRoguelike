using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class TrueServant : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<TrueServant>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => -1;
        public int currentFrame = 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 24;
            NPC.height = 24;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = NPC.DeathSound = SoundID.NPCDeath6 with { Volume = 0.8f, Pitch = 0.1f, PitchVariance = 0.05f };
            NPC.knockBackResist = 0.6f;
            modNPC.drawCenter = new Vector2(0, 6);
            NPC.noTileCollide = false;
            NPC.noGravity = true;
            NPC.hide = true;
            modNPC.IgniteCentered = true;
        }
        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
        public override void OnSpawn(IEntitySource source)
        {

        }
        
        public override void AI()
        {
            NPC.localAI[0]++;
            if (NPC.localAI[0] < 50)
            {
                ParticleManager.AddParticle(new Glow(
                    NPC.Center, NPC.velocity, 30, Color.Teal * 0.23f, new Vector2(0.15f), 0, 0.96f, 30, true));
            }

            NPC.knockBackResist = 0.6f;
            Entity target = modNPC.GetTarget(NPC);
            float velCap = 5;

            bool canHit = target != null && CanHitInLine(NPC.Center, target.Center);
            if (target != null)
            {
                if (NPC.ai[1] == 0 && NPC.Center.Distance(target.Center) < 500 && canHit) // start dash
                {
                    NPC.ai[1] = 1;
                    SoundEngine.PlaySound(SoundID.NPCHit45 with { Volume = 0.25f, Pitch = 0.5f, MaxInstances = 3, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                    for (int i = 0; i < 32; i++)
                    {
                        float completion = i / 32f;
                        float rot = MathHelper.TwoPi * completion + Main.rand.NextFloat(-0.04f, 0.04f);
                        Color color = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.5f));
                        ParticleManager.AddParticle(new ThinSpark(
                            NPC.Center + rot.ToRotationVector2() * 10, rot.ToRotationVector2() * 1 + NPC.velocity,
                            20, color * 0.9f, new Vector2(0.13f, 0.27f) * Main.rand.NextFloat(0.7f, 1f) * 0.6f, rot, true, false));
                    }
                }
                if (NPC.ai[1] != 0) // timer outside of default movement
                    NPC.ai[1]++;
            }
            else if (NPC.ai[1] > 0 && NPC.ai[1] <= 60)
                NPC.ai[1] = 0;

            if (NPC.ai[1] >= 0 && NPC.ai[1] < 60 && canHit) // default movement while LoS
            {
                if (NPC.ai[1] != 0) // Charge windup
                    NPC.velocity *= 0.92f;
                NPC.velocity += (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 0.2f;
                if (NPC.velocity.Length() > velCap)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * velCap;
            }   
            else if (NPC.ai[1] == 60) // charge
            {
                SoundEngine.PlaySound(SoundID.Zombie102 with { Volume = 0.16f, Pitch = 0.4f, MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);
                NPC.velocity = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 12f;
            }
            else if (NPC.ai[1] <= 0 && !canHit) //default movement no LoS
            {
                NPC.velocity *= 0.98f;
                NPC.velocity += (NPC.velocity.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f))).SafeNormalize(Vector2.UnitX) * 0.1f;
                if (NPC.velocity.Length() > velCap)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * velCap;
            }
            if (NPC.ai[1] < 0) // bonk stun slowdown
                NPC.velocity *= 0.97f;

            NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
            NPC.frameCounter += 0.2d;
            if (NPC.collideX || NPC.collideY)
            {
                if (NPC.collideX)
                {
                    NPC.velocity.X = -NPC.oldVelocity.X * 0.5f;
                }
                if (NPC.collideY)
                {
                    NPC.velocity.Y = -NPC.oldVelocity.Y * 0.5f;
                }
                NPC.velocity *= 0.8f;
                if (NPC.ai[1] > 60) // bonk while dashing
                {
                    NPC.ai[1] = -90;
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    NPC.velocity *= 0.6f;
                    SoundEngine.PlaySound(SoundID.Zombie103 with { Volume = 0.15f, Pitch = 0.8f, MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, NPC.Center);

                    for (int i = 0; i < 6; i++)
                    {
                        Color outlineColor = Color.Lerp(Color.Teal, Color.Cyan, 0.4f);
                        Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
                        ParticleManager.AddParticle(new BallOutlined(
                            NPC.Center - offset, offset + NPC.oldVelocity * 0.15f,
                            21, outlineColor, Color.White * 0.75f, new Vector2(0.3f), 5, 0, 0.96f, 15));
                    }
                }
            }

            if (NPC.ai[1] >= 60) // dashing
            {
                NPC.knockBackResist = 0;
                if (NPC.ai[1] % 2 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(2, 2);
                    ParticleManager.AddParticle(new Ball(
                        NPC.Center + offset, offset,
                        20, Color.Teal, new Vector2(0.25f), 0, 0.96f, 10));
                }
                if (NPC.ai[1] % 2 == 0)
                {
                    float realRot = NPC.rotation - MathHelper.PiOver2;
                    Color particleColor = Color.Lerp(Color.Teal, Color.White, 0.2f);
                    ParticleManager.AddParticle(new Wriggler(
                        NPC.Center - realRot.ToRotationVector2() * 18 + (Vector2.UnitY * Main.rand.NextFloat(-5, 5)).RotatedBy(realRot), NPC.velocity * 0.25f,
                        14, particleColor, new Vector2(0.5f), Main.rand.Next(4), realRot, 0.98f, 9,
                        Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically));
                }
            }

            modNPC.drawCenter = new Vector2(0, 6).RotatedBy(NPC.rotation);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    Color outlineColor = Color.Lerp(Color.Teal, Color.Cyan, 0.4f);
                    Vector2 offset = Main.rand.NextVector2CircularEdge(2f, 2f) * Main.rand.NextFloat(0.7f, 0.1f);
                    ParticleManager.AddParticle(new BallOutlined(
                        NPC.Center + offset * 3, offset,
                        21, outlineColor, Color.White * 0.75f, new Vector2(Main.rand.NextFloat(0.19f, 0.24f)), 5, 0, 0.96f, 15));
                }
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    Color outlineColor = Color.Lerp(Color.Teal, Color.Cyan, 0.4f);
                    Vector2 offset = Main.rand.NextVector2CircularEdge(2f, 2f) * Main.rand.NextFloat(0.85f, 0.1f);
                    ParticleManager.AddParticle(new BallOutlined(
                        NPC.Center + offset * 2, offset + NPC.velocity * Main.rand.NextFloat(0.1f, 0.18f),
                        21, outlineColor, Color.White * 0.75f, new Vector2(Main.rand.NextFloat(0.25f, 0.3f)), 5, 0, 0.98f, 15));
                }
            }
        }
        public override void OnKill()
        {

        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            currentFrame = (int)NPC.frameCounter % Main.npcFrameCount[Type];

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool? CanFallThroughPlatforms() => true;
        public override bool CanHitNPC(NPC target) => NPC.localAI[0] >= 60;
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.localAI[0] >= 60;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            float opacity = 1;
            if (NPC.localAI[0] < 60)
            {
                opacity *= NPC.localAI[0] / 60f;
            }
            Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition, NPC.frame, Color.White * opacity, NPC.rotation, NPC.frame.Size() * new Vector2(0.5f, 0.35f), NPC.scale, SpriteEffects.None);
            return false;
        }
    }
}
