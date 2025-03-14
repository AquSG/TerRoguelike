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
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class FlyingSnake : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<FlyingSnake>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Temple"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 34;
            NPC.height = 50;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit23;
            NPC.DeathSound = SoundID.NPCDeath26;
            NPC.knockBackResist = 0.3f;
            modNPC.drawCenter = new Vector2(0, -2);
            NPC.lavaImmune = true;
            NPC.noGravity = true;
        }
        public override void AI()
        {
            NPC.frameCounter += 0.15d;
            NPC.rotation = MathHelper.Pi * NPC.velocity.Length() * 0.02f * NPC.direction;
            modNPC.RogueFlierAI(NPC, 5f, 5f, 0.09f, true);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 30.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 317);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 318);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 318);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 319);
            }

        }
        public override void FindFrame(int frameHeight)
        {
            NPC.gfxOffY = 0;
            int currentFrame = (int)(NPC.frameCounter % Main.npcFrameCount[Type]);
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
    }
}
