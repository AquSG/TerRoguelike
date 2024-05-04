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
    public class HellEnemyRoom10 : Room
    {
        public override int AssociatedFloor => FloorDict["Hell"];
        public override string Key => "HellEnemyRoom10";
        public override string Filename => "Schematics/RoomSchematics/HellEnemyRoom10.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Center, 1, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 16, 16), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -9, 8), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -26, -11), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -7, 3), ChooseEnemy(AssociatedFloor, 0), 300, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, -20, -17), ChooseEnemy(AssociatedFloor, 1), 300, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 19, -13), ChooseEnemy(AssociatedFloor, 1), 300, 120, 0.45f, 0);
        }
    }
}
