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
    public class DungeonEnemyRoom7Up : Room
    {
        public override int AssociatedFloor => FloorDict["Dungeon"];
        public override string Key => "DungeonEnemyRoom7Up";
        public override string Filename => "Schematics/RoomSchematics/DungeonEnemyRoom7Up.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Right, -18, -7, 4, 0), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 3, -7, 4, 0), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 7, 8, 4, 0), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -32, 0, 4), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 4, -3, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(Center, -11, 4), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Center, -15, 4), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Top, -6 - 6, 9, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Left, 7, 8, 4, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -10, -3, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
        }
    }
}
