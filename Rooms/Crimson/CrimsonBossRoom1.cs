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
    public class CrimsonBossRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Crimson"];
        public override string Key => "CrimsonBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/CrimsonBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(RoomDimensions.X * 16f / 2f, (RoomDimensions.Y * 16f) - 32f), NPCID.BigMimicCrimson, 60, 120, 0.9f);
        }
    }
}
