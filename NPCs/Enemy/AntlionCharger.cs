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

namespace TerRoguelike.NPCs.Enemy
{
    public class AntlionCharger : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<AntlionCharger>();
        public override List<int> associatedFloors => new List<int>() { 5 };
        public override Vector2 DrawCenterOffset => new Vector2(0, 2);
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 6;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 68;
            NPC.height = 36;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 1200;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0.4f;
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.03d + 0.03d;
            modNPC.RogueFighterAI(NPC, 5f, -7.9f, 0.055f);
            
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
			if (NPC.life > 0)
			{
				for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * (double)100; i++)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, 250, hit.HitDirection, -1f);
				}
			}
            else
            {
                for (int i = 0; (float)i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 250, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 811);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 812);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 813);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 814);
            }
		}
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 1)) + 1 : 0;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
