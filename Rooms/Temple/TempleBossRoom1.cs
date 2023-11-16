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
    public class TempleBossRoom1 : Room
    {
        public override int AssociatedFloor => 9;
        public override string Key => "TempleBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/TempleBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), NPCID.Golem, 60, 120, 0.9f);
        }
    }
}
