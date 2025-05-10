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
    public class SnowEnemyRoom12Up : Room
    {
        public override int AssociatedFloor => FloorDict["Snow"];
        public override string Key => "SnowEnemyRoom12Up";
        public override string Filename => "Schematics/RoomSchematics/SnowEnemyRoom12Up.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => false;
        public override bool CanExitUp => false;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 11, -4, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 25, -4), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, -1, 8), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 19, 7), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 33, 7), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, 22, 16), ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -8, -6, 0), ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -10, -10), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 30, -12), ChooseEnemy(AssociatedFloor, 0), 240, 120, 0.45f, 0);
        }
    }
}
