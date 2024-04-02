using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerRoguelike.Utilities;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class ParasiticEgg : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<ParasiticEgg>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Corrupt"] };
        public override int CombatStyle => -1;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 28;
            NPC.height = 28;
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.lifeMax = 500;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = false;
        }
        public override void AI()
        {
            NPC.GravityMultiplier *= 0.66f;
            NPC.rotation += NPC.velocity.X * 0.03f;
            if (NPC.collideX || NPC.collideY)
            {
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                NPC.velocity *= 0;
            }
            NPC.ai[0]++;
            if (NPC.ai[0] >= 300)
            {
                int whoAmI = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Clinger>());
                NPC npc = Main.npc[whoAmI];
                npc.ModNPC().isRoomNPC = modNPC.isRoomNPC;
                npc.ModNPC().sourceRoomListID = modNPC.sourceRoomListID;
                NPC.StrikeInstantKill();
            }
        }
        public override bool? CanFallThroughPlatforms() => true;
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 238, hit.HitDirection, -1f);
                    Main.dust[d].noGravity = true;
                }
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 237 + Main.rand.Next(2), 2.5f * (float)hit.HitDirection, -2.5f);
                }
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, Vector2.UnitX * hit.HitDirection, 684, NPC.scale);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, Vector2.UnitX * hit.HitDirection, 685, NPC.scale);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, Vector2.UnitX * hit.HitDirection, 686, NPC.scale);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, Vector2.UnitX * hit.HitDirection, 684 + Main.rand.Next(3), NPC.scale);
            }
        }
    }
}
