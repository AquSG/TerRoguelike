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
    public class BaseEnemyRoom8Down : Room
    {
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseEnemyRoom8Down";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom8Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Right, -4, -3), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 4, -3), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 6, -2), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -12, -8), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Left, 3, -3), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
        }
    }
}