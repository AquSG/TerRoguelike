using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerRoguelike;
using TerRoguelike.TerPlayer;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.GameContent;
using TerRoguelike.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using TerRoguelike.Projectiles;
using static TerRoguelike.Schematics.SchematicManager;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using static TerRoguelike.Managers.TextureManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class VortexWatcher : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public Texture2D ballTex;
        public Texture2D vortexTex;
        public override int modNPCID => ModContent.NPCType<VortexWatcher>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 300;
        public int attackExtendTime = 120;
        public int attackCooldown = 240;
        public Vector2 blackVortexPosition = new Vector2(0, -96);
        int currentFrame;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 12;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 22;
            NPC.height = 47;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 1800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0.3f;
            modNPC.drawCenter = new Vector2(0, -5);
            lightTex = TexDict["VortexWatcherGlow"];
            ballTex = TexDict["CircularGlow"];
            vortexTex = TexDict["BlackVortex"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = -attackCooldown + 1;
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.08d;

            modNPC.RogueFighterShooterAI(NPC, 2.4f, -7.9f, 1000f, attackTelegraph, attackCooldown, 0f, ModContent.ProjectileType<BlackVortex>(), 1f, blackVortexPosition, NPC.damage, false, false, null, attackExtendTime, 1, 0, 0, 6, false);
            if (NPC.ai[1] == attackTelegraph)
            {
                SoundEngine.PlaySound(SoundID.Item105 with { Volume = 1f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen with { Volume = 0.7f, Pitch = 0 }, NPC.Center);
            }
            if (NPC.ai[1] > 10 && NPC.ai[1] < attackTelegraph - 15)
            {
                Vector2 pos = new Vector2(-24 * NPC.direction, -2);
                Vector2 velocity = (blackVortexPosition - pos) * 0.1f * Main.rand.NextFloat(0.9f, 1f);
                Dust d = Dust.NewDustPerfect(pos + NPC.Center + (Vector2.UnitY * NPC.gfxOffY), DustID.Dirt, velocity + NPC.velocity, 0, Color.Black,  1.6f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
                d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                pos = new Vector2(24 * NPC.direction, -2);
                velocity = (blackVortexPosition - pos) * 0.1f * Main.rand.NextFloat(0.9f, 1f);
                d = Dust.NewDustPerfect(pos + NPC.Center + (Vector2.UnitY * NPC.gfxOffY), DustID.Dirt, velocity + NPC.velocity, 0, Color.Black,  1.6f);
                d.noGravity = true;
                d.noLight = true;
                d.noLightEmittence = true;
                d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            if (NPC.ai[1] > 30)
            {
                float scale = NPC.ai[1] < attackTelegraph - 15 ? MathHelper.Lerp(0.1f, 2.5f, (NPC.ai[1] - 20) / (attackTelegraph - 35)) : MathHelper.SmoothStep(2.5f, 0.1f, (NPC.ai[1] - attackTelegraph + 15) / 30);
                for (int i = 0; i < 2; i++)
                {
                    if (i == 1 && Main.rand.NextBool())
                        continue;

                    bool vortexroll = Main.rand.NextBool(3);

                    Vector2 offset = Main.rand.NextVector2CircularEdge(30f, 30f) * scale;
                    offset *= Main.rand.NextFloat(0.3f, 1f);
                    Dust d = Dust.NewDustPerfect(NPC.Center + blackVortexPosition + offset + (Vector2.UnitY * NPC.gfxOffY), vortexroll ? DustID.Vortex : DustID.Stone, offset.RotatedBy(-MathHelper.PiOver2 * 1.2f) * 0.1f + NPC.velocity, 0, Color.Black, vortexroll ? 0.27f * scale : 0.66f * scale);
                    d.noGravity = true;
                    d.noLightEmittence = true;
                    d.noLight = true;
                }
            }

        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / 20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 241, hit.HitDirection, -1f);
                    if (Main.rand.NextBool(4))
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 229)];
                        d.noGravity = true;
                        d.scale = 1.5f;
                        d.fadeIn = 1f;
                        d.velocity *= 3f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 241, hit.HitDirection, -1f);
                    if (Main.rand.NextBool(3))
                    {
                        Dust d = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 229)];
                        d.noGravity = true;
                        d.scale = 1.5f;
                        d.fadeIn = 1f;
                        d.velocity *= 3f;
                    }
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 808, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 809, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 809, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 810, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 810, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[modNPCID];
            if (NPC.ai[1] > 0)
            {
                NPC.frameCounter = 0;
                currentFrame = NPC.ai[1] < attackTelegraph ? frameCount - 2 : frameCount - 1;
            }
            else
            {
                currentFrame = NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (frameCount - 4)) + 2 : 1;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            if (NPC.ai[1] > 20)
            {
                float scale = NPC.ai[1] < attackTelegraph - 15 ? MathHelper.Lerp(0, 1f, (NPC.ai[1] - 20) / (attackTelegraph - 35)) : MathHelper.SmoothStep(1f, 0, (NPC.ai[1] - attackTelegraph + 15) / 30);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(ballTex, NPC.Center + blackVortexPosition - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY), null, Color.Lerp(Color.Green, Color.Blue, 0.2f) * 0.8f, 0f, ballTex.Size() * 0.5f, scale * 0.46f, SpriteEffects.None);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(ballTex, NPC.Center + blackVortexPosition - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY), null, Color.Black * 0.9f, 0f, ballTex.Size() * 0.5f, 0.39f * scale, SpriteEffects.None);
                Main.EntitySpriteDraw(vortexTex, NPC.Center + blackVortexPosition - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY), null, Color.Black * 0.5f, (Main.GlobalTimeWrappedHourly * 5f) % MathHelper.Pi, vortexTex.Size() * 0.5f, 3.4f * scale, SpriteEffects.None);
                Main.EntitySpriteDraw(vortexTex, NPC.Center + blackVortexPosition - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY), null, Color.Black * 0.5f, (Main.GlobalTimeWrappedHourly * -4f) % MathHelper.Pi, vortexTex.Size() * 0.5f, 3.4f * scale, SpriteEffects.FlipHorizontally);

                
            }
            
        }
    }
}
