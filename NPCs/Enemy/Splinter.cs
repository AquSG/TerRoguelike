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

namespace TerRoguelike.NPCs.Enemy
{
    public class Splinter : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Splinter>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 14;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.2f;
            modNPC.drawCenter = new Vector2(0, -4);
        }
        public override void AI()
        {
            NPC.frameCounter += NPC.velocity.Length() * 0.25d;
            modNPC.RogueFighterAI(NPC, 2f, -7.9f);
            
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.ai[0] = 0;
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage/ (double)NPC.lifeMax * 100.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, hit.HitDirection, -1f, 0, default(Color), 1.1f);
                }
            }
            else
            {
                for (int i = 0; i < 60; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.WoodFurniture, 2 * hit.HitDirection, -2f, 0, default(Color), 1.1f);
                }
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = NPC.velocity.Y == 0 ? (int)(NPC.frameCounter % (Main.npcFrameCount[modNPCID] - 1)) + 1 : 0;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, TextureAssets.Npc[modNPCID].Value.Width, frameHeight);
        }
    }
}
