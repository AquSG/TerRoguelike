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
    public class HellEnemyRoom6 : Room
    {
        public override int AssociatedFloor => FloorDict["Hell"];
        public override string Key => "HellEnemyRoom6";
        public override string Filename => "Schematics/RoomSchematics/HellEnemyRoom6.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Center, -24, 11), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 13, 5), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 0, 0), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(Right, -10, -5), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Right, -5, 5), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Left, 8, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
        }
    }
}
