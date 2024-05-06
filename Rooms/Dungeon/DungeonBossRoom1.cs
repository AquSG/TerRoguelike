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
using static TerRoguelike.Schematics.SchematicManager;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.World;

namespace TerRoguelike.Rooms
{
    public class DungeonBossRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Dungeon"];
        public override string Key => "DungeonBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/DungeonBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override bool HasTransition => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            if (!TerRoguelikeWorld.escape)
                AddBoss(bossSpawnPos, ModContent.NPCType<Skeletron>());
        }
        public override void Update()
        {
            if (bossSpawnPos == Vector2.Zero)
                bossSpawnPos = new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f);
            base.Update();
        }
    }
}
