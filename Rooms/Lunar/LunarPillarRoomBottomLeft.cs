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
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Rooms
{
    public class LunarPillarRoomBottomLeft : Room
    {
        //nebula
        public override int AssociatedFloor => 10;
        public override string Key => "LunarPillarRoomBottomLeft";
        public override string Filename => "Schematics/RoomSchematics/LunarPillarRoomBottomLeft.csch";
        public override bool IsPillarRoom => true;
        public int SpawnCountdown = 0;
        public List<int> SpawnSelection = new List<int>()
        {
            421,
            423,
            420,
            424
        };
        public override void InitializeRoom()
        {
            base.InitializeRoom();
        }
        public override void Update()
        {
            base.Update();
            if (!awake || !active)
                return;
            if (ClearCondition())
                return;
            if (SpawnCountdown <= 0)
            {
                SpawnCountdown = Main.rand.Next(300, 360);
                Vector2 position = new Vector2(Main.rand.Next(6, (int)RoomDimensions.X - 6) * 16, Main.rand.Next(6, (int)RoomDimensions.Y - 6) * 16);
                position = Main.player[Main.myPlayer].GetModPlayer<TerRoguelikePlayer>().FindAirToPlayer(position);
                int randomNPC = SpawnSelection[Main.rand.Next(SpawnSelection.Count)];
                AddRoomNPC(position, randomNPC, roomTime + 60, 120, 0.45f);
            }
            else
                SpawnCountdown--;
        }
    }
}
