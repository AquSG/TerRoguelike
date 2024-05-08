﻿using System;
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
    public class DungeonEnemyRoom3 : Room
    {
        public override int AssociatedFloor => FloorDict["Dungeon"];
        public override string Key => "DungeonEnemyRoom3";
        public override string Filename => "Schematics/RoomSchematics/DungeonEnemyRoom3.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 9, 6), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -5, -3), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 17, 3), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(Center, 0, 7), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Center, -15, 11), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Top, 1, 10), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
        }
    }
}
