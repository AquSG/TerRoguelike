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
    public class DefaultRoom : Room
    {
        public override int ID => 0;
        public override string Key => "DefaultRoom";
        public override string Filename => "Schematics/RoomSchematics/DefaultRoom.csch";
        public override void InitializeRoom()
        {
            active = false;
        }
    }
}
