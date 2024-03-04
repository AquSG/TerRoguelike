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
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Rooms
{
    public class LunarHallRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Lunar"];
        public override string Key => "LunarHallRoom1";
        public override string Filename => "Schematics/RoomSchematics/LunarHallRoom1.csch";
        public override bool CanExitRight => true;
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
