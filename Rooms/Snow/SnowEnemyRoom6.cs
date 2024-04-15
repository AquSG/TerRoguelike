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
    public class SnowEnemyRoom6 : Room
    {
        public override int AssociatedFloor => FloorDict["Snow"];
        public override string Key => "SnowEnemyRoom6";
        public override string Filename => "Schematics/RoomSchematics/SnowEnemyRoom6.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Right, -7, 1), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 5, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -5, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 5, 5), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -5, 5), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
        }
    }
}
