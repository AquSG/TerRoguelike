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
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;
using static TerRoguelike.Managers.NPCManager;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Rooms
{
    public class SnowEnemyRoom3Up : Room
    {
        public override int AssociatedFloor => FloorDict["Snow"];
        public override string Key => "SnowEnemyRoom3Up";
        public override string Filename => "Schematics/RoomSchematics/SnowEnemyRoom3Up.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -13, 6), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 5, -4), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -18, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 24, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 10, -3), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -21, 1), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);
        }
    }
}
