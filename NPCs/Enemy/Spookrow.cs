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
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy
{
    public class Spookrow : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Spookrow>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.2f;
            modNPC.drawCenter = new Vector2(0, -11);
        }
        public override void AI()
        {
            NPC.rotation = MathHelper.Clamp(MathHelper.Lerp(0.2f, 0f, (NPC.ai[0] - 30) / 60f), 0f, 0.2f) * NPC.spriteDirection;
            modNPC.RogueSpookrowAI(NPC, 6f, -6.7f);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 100.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Scarecrow, hit.HitDirection, -1f, 0, default(Color), 1.1f);
                }
            }
            else
            {
                for (int i = 0; i < 60; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Scarecrow, 2 * hit.HitDirection, -2f, 0, default(Color), 1.1f);
                }
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, NPC.velocity, 441, NPC.scale);
                Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 442, NPC.scale);
                Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 443, NPC.scale);
                Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 444, NPC.scale);
                Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 445, NPC.scale);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            int completion = (int)MathHelper.Clamp(NPC.ai[0] - 30, 0, 60);
            int currentFrame = completion < 30 ? (completion <= 14 ? 0 : 1) : (int)MathHelper.Lerp(2f, Main.npcFrameCount[Type] - 1, (completion - 30) / 30f);
            if (currentFrame >= Main.npcFrameCount[Type])
                currentFrame = Main.npcFrameCount[Type];
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
    }
}
