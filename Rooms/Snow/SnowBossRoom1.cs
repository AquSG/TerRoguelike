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

namespace TerRoguelike.Rooms
{
    public class SnowBossRoom1 : Room
    {
        public override int AssociatedFloor => 4;
        public override string Key => "SnowBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/SnowBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(RoomDimensions.X * 8f, (RoomDimensions.Y * 16f) - 120f), NPCID.IceQueen, 60, 120, 0.9f);
        }
    }
}
