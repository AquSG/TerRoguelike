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
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Rooms
{
    public class JungleBossRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Jungle"];
        public override string Key => "JungleBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/JungleBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override bool HasTransition => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), NPCID.QueenBee, 60, 120, 0.9f);
        }
    }
}
