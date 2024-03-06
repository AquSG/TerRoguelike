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
    public class BaseEnemyRoom1Var1 : Room
    {
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseEnemyRoom1";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom1.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override bool IsRoomVariant => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(PercentPosition(0.25f, 0.30f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.75f, 0.30f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.5f, 0.85f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);

            AddRoomNPC(PercentPosition(0.25f, 0.30f), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
            AddRoomNPC(PercentPosition(0.75f, 0.30f), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
        }
    }
}
