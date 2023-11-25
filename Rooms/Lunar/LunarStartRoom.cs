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
    public class LunarStartRoom : Room
    {
        public override int AssociatedFloor => 10;
        public override string Key => "LunarStartRoom";
        public override string Filename => "Schematics/RoomSchematics/LunarStartRoom.csch";
        public override bool CanExitRight => true;
        public override bool IsStartRoom => true;
        public override void InitializeRoom()
        {
            active = false;
        }
        public override void Update()
        {
            active = false;
        }
    }
}
