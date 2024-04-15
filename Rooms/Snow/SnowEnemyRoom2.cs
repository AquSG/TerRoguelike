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
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;
using static TerRoguelike.Managers.NPCManager;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Rooms
{
    public class SnowEnemyRoom2 : Room
    {
        public override int AssociatedFloor => FloorDict["Snow"];
        public override string Key => "SnowEnemyRoom2";
        public override string Filename => "Schematics/RoomSchematics/SnowEnemyRoom2.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 4, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 9, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 22, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -4, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -9, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -17, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 5, -1), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -5, -1), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 6, 7), ChooseEnemy(AssociatedFloor, 1), 300, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -6, 7), ChooseEnemy(AssociatedFloor, 1), 300, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 0, -18), ChooseEnemy(AssociatedFloor, 0), 300, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, -10, -1), ChooseEnemy(AssociatedFloor, 0), 300, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 10, -1), ChooseEnemy(AssociatedFloor, 0), 300, 120, 0.45f, 0);


        }
    }
}
