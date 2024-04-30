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
    public class JungleEnemyRoom3 : Room
    {
        public override int AssociatedFloor => FloorDict["Jungle"];
        public override string Key => "JungleEnemyRoom3";
        public override string Filename => "Schematics/RoomSchematics/JungleEnemyRoom3.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 16, -2), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, -16, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 1, -2), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -7, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(Right, -7, 5), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Left, 6, 6), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
        }
    }
}
