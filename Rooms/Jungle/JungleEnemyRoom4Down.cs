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
    public class JungleEnemyRoom4Down : Room
    {
        public override int AssociatedFloor => FloorDict["Jungle"];
        public override string Key => "JungleEnemyRoom4Down";
        public override string Filename => "Schematics/RoomSchematics/JungleEnemyRoom4Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 5, 10), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -5, 10), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -5, -10), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 6, -2), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -6, -2), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 5, -10), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(Top, 0, 4), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 0, -2), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
        }
    }
}
