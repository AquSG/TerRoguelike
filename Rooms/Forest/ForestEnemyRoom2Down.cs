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

namespace TerRoguelike.Rooms
{
    public class ForestEnemyRoom2Down : Room
    {
        public override int AssociatedFloor => FloorDict["Forest"];
        public override string Key => "ForestEnemyRoom2Down";
        public override string Filename => "Schematics/RoomSchematics/ForestEnemyRoom2Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -2, -3), ChooseEnemy(AssociatedFloor, 0), 180, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -2, 8, 17), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -2, -8, 17), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 2, 2, -1), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 2, -12), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
        }
    }
}
