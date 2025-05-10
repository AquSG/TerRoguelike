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
    public class BaseEnemyRoom10 : Room
    {
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseEnemyRoom10";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom10.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => false;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -8, -2), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 10, 5), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, 10, 5), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, -13, 3, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, -15, -2, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 12, 9, 0), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 4, 9, 0), ChooseEnemy(AssociatedFloor, 0), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -11, 3, 8, 0), ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
        }
    }
}
