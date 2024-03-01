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
using TerRoguelike.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.NPCs.Enemy
{
    public class LihzahrdSentry : BaseRoguelikeNPC
    {
        public Texture2D lightTex = ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/LihzahrdSentryGlow").Value;
        public override int modNPCID => ModContent.NPCType<LihzahrdSentry>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Temple"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 60;
        public int attackCooldown = 180;
        public int attackDuration = 120;
        public int attackTimeBetween = 7;
        public int currentFrame;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 22;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 20;
            NPC.height = 32;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -10);
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.25d;
            modNPC.RogueTurretAI(NPC, attackTelegraph, attackCooldown, 2000f, ModContent.ProjectileType<LihzahrdLaser>(), NPC.damage, 10f, Vector2.Zero, true, NPC.ai[1], null, attackDuration, attackTimeBetween);
            float direction = NPC.ai[1];
            if (modNPC.targetPlayer != -1)
            {
                direction = (Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation();
            }
            else if (modNPC.targetNPC != -1)
            {
                direction = (Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation();
            }
            NPC.ai[1] = NPC.ai[1].AngleLerp(direction, 0.2f);
            if (NPC.ai[0] >= attackTelegraph && NPC.ai[0] < attackTelegraph + attackDuration && (NPC.ai[0] - attackTelegraph) % attackTimeBetween == 0)
            {
                SoundEngine.PlaySound(SoundID.Item91 with { Volume = 1f }, NPC.Center);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(24f, 24f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center + offset, DustID.YellowTorch, -offset * 0.1f, 0, default, 1.5f);
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                }
            }
            else if (NPC.ai[0] == 15)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 1f }, NPC.Center);
            }
            else if (NPC.ai[0] < attackTelegraph && NPC.ai[0] > 0)
            {
                if (Main.rand.NextBool(8))
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(24f, 24f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center + offset, DustID.YellowTorch, -offset * 0.1f, 0, default, 1.5f);
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Lihzahrd, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Lihzahrd, 2.5f * (float)hit.HitDirection, -2.5f);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.gfxOffY = 10;
            int currentFrame;
            if (NPC.ai[0] > 0 && NPC.ai[0] < attackTelegraph)
            {
                currentFrame = NPC.ai[0] < attackTelegraph - 11 ? (Main.npcFrameCount[modNPCID] - 9) + (((int)(NPC.ai[0]) * 9) / (attackTelegraph - 11)) : (Main.npcFrameCount[modNPCID] - 8) - ((int)(NPC.ai[0] - (attackTelegraph - 11)) / 6);
            }
            else if (NPC.ai[0] < attackTelegraph + 3 && NPC.ai[0] > 0)
            {
                NPC.frameCounter = 1;
                currentFrame = (Main.npcFrameCount[modNPCID] - 10);
            }
            else
            {
                currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 10));
            }
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!modNPC.ignitedStacks.Any())
                Main.EntitySpriteDraw(lightTex, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
}
