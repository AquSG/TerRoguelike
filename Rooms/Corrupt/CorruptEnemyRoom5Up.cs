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
    public class CorruptEnemyRoom5Up : Room
    {
        public override int AssociatedFloor => FloorDict["Corrupt"];
        public override string Key => "CorruptEnemyRoom5Up";
        public override string Filename => "Schematics/RoomSchematics/CorruptEnemyRoom5Up.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Center, -5, 2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 3, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -5, -4), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 6, 7), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
        }
    }
}
