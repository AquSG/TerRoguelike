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
using Terraria.DataStructures;

namespace TerRoguelike.NPCs.Enemy
{
    public class Corruptor : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Corruptor>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Corrupt"] };
        public override int CombatStyle => 2;
        public int attackCooldown = 120;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 44;
            NPC.height = 44;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -1);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            int attackTelegraph = 30;
            modNPC.RogueCorruptorAI(NPC, 4f, 0.03f, attackTelegraph, attackCooldown, ModContent.ProjectileType<VileSpit>(), 8f, NPC.damage);
            if (NPC.ai[0] > 0 && NPC.ai[0] <= attackTelegraph && NPC.ai[1] % 2 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(NPC.width * 0.4f, NPC.width * 0.4f);
                Dust dust = Dust.NewDustPerfect(NPC.Center + offset + NPC.rotation.ToRotationVector2() * 10, DustID.GreenTorch, -offset * 0.01f + NPC.velocity, 0, Color.LimeGreen, 1.4f);
                dust.noGravity = true;
                dust.noLightEmittence = true;
                dust.noLight = true;
            }

            NPC.frameCounter += 0.2d;
            float direction = NPC.velocity.ToRotation();
            if (modNPC.targetNPC != -1)
            {
                direction = (Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation();
            }
            else if (modNPC.targetPlayer != -1)
            {
                direction = (Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation();
            }
            NPC.rotation = NPC.rotation.AngleLerp(direction, 0.1f);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 60.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -1f, NPC.alpha, NPC.color, NPC.scale);
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -2f, NPC.alpha, NPC.color, NPC.scale);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 108, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 108, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 109, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 110, NPC.scale);
            }
            

        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            NPC.ai[0] = -attackCooldown + 1;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[modNPCID]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
