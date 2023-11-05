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
    public class HellBossRoom1 : Room
    {
        public override int AssociatedFloor => 7;
        public override string Key => "HellBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/HellBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(RoomDimensions.X * 16f - 32f, RoomDimensions.Y * 8f), NPCID.WallofFlesh, 60, 120, 0.9f);
        }
    }
}
