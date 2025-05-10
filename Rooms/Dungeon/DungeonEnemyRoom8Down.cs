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
    public class DungeonEnemyRoom8Down : Room
    {
        public override int AssociatedFloor => FloorDict["Dungeon"];
        public override string Key => "DungeonEnemyRoom8Down";
        public override string Filename => "Schematics/RoomSchematics/DungeonEnemyRoom8Down.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -13, 10), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 9, 15, 0), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 4, 4, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, -5, 7), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, -3, -4), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 10, -4), ChooseEnemy(AssociatedFloor, 0), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -8, -4, 0), ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
        }
    }
}
