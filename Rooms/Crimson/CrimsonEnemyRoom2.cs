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
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;

namespace TerRoguelike.Rooms
{
    public class CrimsonEnemyRoom2 : Room
    {
        public override int AssociatedFloor => FloorDict["Crimson"];
        public override string Key => "CrimsonEnemyRoom2";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom2.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override int[] CantExitInto => [RoomDict["CrimsonEnemyRoom10Down"]];
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -4, -3), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -16, -3), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 10, 3), ModContent.NPCType<IchorSticker>(), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, -5, 13), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, 4, 10), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, -20, 14), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 12, -14), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, -4, -6), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
        }
    }
}
