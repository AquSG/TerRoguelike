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
    public class ForestEnemyRoom6 : Room
    {
        public override int AssociatedFloor => FloorDict["Forest"];
        public override string Key => "ForestEnemyRoom6";
        public override string Filename => "Schematics/RoomSchematics/ForestEnemyRoom6.csch";
        public override bool CanExitRight => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Center, 2, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -2, -3), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -5, -3), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 3, 6) - Vector2.UnitX, ChooseEnemy(AssociatedFloor, 1), 180, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -3, 6) + Vector2.UnitX, ChooseEnemy(AssociatedFloor, 1), 180, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 15, 10), ChooseEnemy(AssociatedFloor, 2), 180, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 2, -3), ChooseEnemy(AssociatedFloor, 0), 180, 120, 0.45f, 1);
        }
    }
}
