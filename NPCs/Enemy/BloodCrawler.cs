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
    public class BloodCrawler : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<BloodCrawler>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Crimson"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 36;
            NPC.height = 36;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit20;
            NPC.DeathSound = SoundID.NPCDeath23;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -12);
            NPC.noGravity = true;
        }
        public override void AI()
        {
            modNPC.RogueSpiderAI(NPC, 1.5f, 0.08f, 120, 45, 120, 120f);

            if (NPC.ai[1] == 2)
            {
                NPC.frameCounter += 0.125d;
                float direction = 0;
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
            else if (NPC.velocity.Length() > 0.5f)
            {
                NPC.frameCounter += 0.125d;
                NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.ToRotation(), 0.1f);
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2.5f * (float)hit.HitDirection, -2.5f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 351);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 352);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 352);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 353);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 353);
            }
            
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
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
