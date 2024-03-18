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
    public class CrimsonEnemyRoom7 : Room
    {
        public override int AssociatedFloor => FloorDict["Crimson"];
        public override string Key => "CrimsonEnemyRoom7";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom7.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -3, -3), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -7, -4), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -11, -4), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -15, -4), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, -3, -3), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(Center, -15, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -3, -3), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -14, -6), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
        }
    }
}
