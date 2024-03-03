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
    public class StormDiver : BaseRoguelikeNPC
    {
        public Texture2D lightTex;
        public override int modNPCID => ModContent.NPCType<StormDiver>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => 2;
        public int attackTelegraph = 40;
        public int attackExtend = 20;
        public int attackCooldown = 91;
        public int jetpackDuration = 60;
        int currentFrame;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 13;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 47;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.4f;
            NPC.noGravity = true;
            modNPC.drawCenter = new Vector2(0, -7);
            lightTex = TexDict["StormDiverGlow"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = -attackCooldown + 1;
        }
        public override void AI()
        {
            int jetpackTelegraph = 40;
            NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.1d;
            if (NPC.ai[2] < 0)
            {
                bool flying = NPC.ai[2] >= -jetpackTelegraph;
                if (flying || (NPC.ai[2] < -jetpackTelegraph && (int)NPC.ai[2] % 4 == 0))
                {
                    Dust d = Dust.NewDustPerfect(NPC.Center + new Vector2(-13 * NPC.direction, -3), DustID.Vortex, new Vector2(-2f * NPC.direction * (flying ? 1.2f : 1f), 2f).RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) + NPC.velocity, 0, default, flying ? 1f : 0.8f);
                    d.noGravity = true;
                    d.noLight = true;
                    d.noLightEmittence = true;
                }
            }
            modNPC.RogueStormDiverAI(NPC, 3f, -7.9f, 480f, attackTelegraph, attackCooldown, attackExtend, jetpackTelegraph, jetpackDuration, 300, 160f, 10f, 0f, MathHelper.Pi * 0.16f, ModContent.ProjectileType<VortexLaser>(), 10f, new Vector2(26f * NPC.spriteDirection, -6), NPC.damage, true, false, 5, MathHelper.Pi * 0.04f, 1.5f);
            if (NPC.ai[1] == attackTelegraph)
                SoundEngine.PlaySound(SoundID.Item36 with { Volume = 1f }, NPC.Center);
            else if (NPC.ai[2] == -(jetpackDuration + jetpackTelegraph) + 1)
                SoundEngine.PlaySound(SoundID.Item13 with { Volume = 1f }, NPC.Center);
            else if (NPC.ai[2] == -jetpackDuration)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 0.8f, Pitch = -0.7f }, NPC.Center);
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(8 * NPC.direction, 0), DustID.Vortex, Vector2.UnitY * i + (Vector2.UnitX * -NPC.velocity.X * 0.13f * (i + 1)), 0, default, 0.8f);
                    d.noGravity = true;
                    d.noLight = true;
                    d.noLightEmittence = true;
                    d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(-12 * NPC.direction, 0), DustID.Vortex, Vector2.UnitY * i + (Vector2.UnitX * -NPC.velocity.X * 0.13f * (i + 1)), 0, default, 0.8f);
                    d.noGravity = true;
                    d.noLight = true;
                    d.noLightEmittence = true;
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
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 796, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, 797, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 798, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 809, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 809, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 810, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 810, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (NPC.ai[2] < 0)
            {
                currentFrame = NPC.ai[2] >= -jetpackDuration ? ((int)-NPC.ai[2] % 3) + 10 : 9;
            }
            else if (NPC.ai[1] > 0)
            {
                currentFrame = 0;
            }
            else if (NPC.velocity.Y == 0)
            {
                currentFrame = ((int)NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 5)) + 2;
            }
            else
            {
                currentFrame = 1;
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition + modNPC.drawCenter + (Vector2.UnitY * NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}
