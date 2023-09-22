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

namespace TerRoguelike.Floors
{
    public class CrimsonFloor : Floor
    {
        public override int FloorID => 1;
        public override int StartRoomID => 16;
        public override int Stage => 1;
    }
}
