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
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace TerRoguelike.NPCs.Enemy
{
    public class CrawlingSludge : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<CrawlingSludge>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Base"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 28;
            NPC.height = 20;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -3);
            NPC.alpha = 60;
        }
        public override void AI()
        {
            modNPC.RogueCrawlerAI(NPC, 1.2f, 0.04f, 90);
            if (NPC.ai[0] == 0)
            {
                NPC.frameCounter += 0.1d;
            }
            else
            {
                NPC.frameCounter += 0.025d;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100.0; i++)
{
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 4, hit.HitDirection, -1f, NPC.alpha, Color.Purple;
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
{
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 4, 2 * hit.HitDirection, -2f, NPC.alpha, Color.Purple);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID]));
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
