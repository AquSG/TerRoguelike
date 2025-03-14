﻿using System;
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
using TerRoguelike.Projectiles;
using TerRoguelike.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using static TerRoguelike.Schematics.SchematicManager;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class DesertSpirit : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<DesertSpirit>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Desert"] };
        public override int CombatStyle => 1;
        public int attackTelegraph = 60;
        public int attackCooldown = 30;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 16;
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 28;
            NPC.height = 62;
            NPC.aiStyle = -1;
            NPC.damage = 25;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.2f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            modNPC.drawCenter = new Vector2(0, 3);
        }
        public override void AI()
        {
            NPC.frameCounter += 0.2d;
            modNPC.RogueTeleportingShooterAI(NPC, 96f, 240f, 360, attackTelegraph, attackCooldown, ModContent.ProjectileType<DesertSpiritCurse>(), 1f, Main.rand.NextVector2CircularEdge(48f, 48f), NPC.damage, true);
            NPC.velocity *= 0.95f;
            if (NPC.ai[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 1f }, NPC.Center);
                Color outlineColor = Color.Purple;
                Color fillColor = Color.Lerp(outlineColor, Color.Black, 0.6f);
                for (int i = 0; i < 25; i++)
                {
                    Rectangle rect = NPC.getRect();
                    rect.Inflate(-4, -4);
                    Vector2 particlePos = Main.rand.NextVector2FromRectangle(rect);
                    ParticleManager.AddParticle(new BallOutlined(
                        particlePos, Main.rand.NextVector2Circular(2, 2),
                        60, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.25f, 0.5f)), 4, 0, 0.92f, 50));
                }
            }
            
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50.0; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 27, 0f, 0f, 50, default(Color), 1.5f);
                    Dust dust = Main.dust[d];
                    dust.velocity *= 2f;
                    dust.noGravity = true;
                }
            }
            else
            {
                for (int i = 0; i < 40; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 27, 0f, 0f, 50, default(Color), 1.5f);
                    Dust dust = Main.dust[d];
                    dust.velocity *= 2f;
                    dust.noGravity = true;
                    dust.fadeIn = 1f;
                }
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, Color.White, 0.5f);
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[Type] / 2)) + (NPC.ai[0] % (attackCooldown + attackTelegraph) <= attackTelegraph ? 8 : 0);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
        public override bool CanHitNPC(NPC target) => false;
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
    }
}
