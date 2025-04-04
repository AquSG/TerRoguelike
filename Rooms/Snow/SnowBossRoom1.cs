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
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.World;

namespace TerRoguelike.Rooms
{
    public class SnowBossRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Snow"];
        public override string Key => "SnowBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/SnowBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override Vector2 bossSpawnPos => new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f);
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddBoss(bossSpawnPos, ModContent.NPCType<IceQueen>());
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
