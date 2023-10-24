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

namespace TerRoguelike.Rooms
{
    public class BaseEnemyRoom1Var1 : Room
    {
        public override string Key => "BaseEnemyRoom1";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom1.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override bool IsRoomVariant => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(64f, 72f), NPCID.BlueArmoredBones, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f) - 64f, 72f), NPCID.BlueArmoredBones, 60, 120, 0.45f);
            AddRoomNPC(new Vector2(RoomDimensions.X / 2f * 16f, (RoomDimensions.Y * 16f) - 32f), NPCID.RainbowSlime, 360, 120, 0.55f);
        }
    }
}
