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
    public class ForestEnemyRoom7Down : Room
    {
        public override int AssociatedFloor => FloorDict["Forest"];
        public override string Key => "ForestEnemyRoom7Down";
        public override string Filename => "Schematics/RoomSchematics/ForestEnemyRoom7Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Left, 4, 3, -1), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -3, -3), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            Vector2 nextSpawnPos = Main.rand.NextBool() ? MakeEnemySpawnPos(TopRight, -5, 4) : MakeEnemySpawnPos(TopLeft, 8, 5);
            AddRoomNPC(nextSpawnPos, ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(TopRight, -5, 4), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 8, 5), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Right, -1, 6, 1), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
        }
    }
}
