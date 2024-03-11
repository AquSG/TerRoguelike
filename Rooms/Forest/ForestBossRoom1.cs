using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static TerRoguelike.Managers.NPCManager;
using static TerRoguelike.Schematics.SchematicManager;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.World;

namespace TerRoguelike.Rooms
{
    public class ForestBossRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Forest"];
        public override string Key => "ForestBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/ForestBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            if (!TerRoguelikeWorld.escape)
                AddBoss(bossSpawnPos, ModContent.NPCType<BrambleHollow>());
        }
        public override void Update()
        {
            if (bossSpawnPos == Vector2.Zero)
                bossSpawnPos = new Vector2(RoomDimensions16.X * 0.5f, RoomDimensions16.Y - 96f);
            base.Update();
        }
    }
}
