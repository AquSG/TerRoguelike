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
    public class JungleEnemyRoom7 : Room
    {
        public override int AssociatedFloor => FloorDict["Jungle"];
        public override string Key => "JungleEnemyRoom7";
        public override string Filename => "Schematics/RoomSchematics/JungleEnemyRoom7.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => false;
        public override bool CanExitUp => false;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 13, -6), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 23, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -6, -9), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom,-4, -6), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom,-9, -16, 4), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 18, -6), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
        }
    }
}
