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
    public class BaseFloor : Floor
    {
        public override int FloorID => 0;
        public override List<int> PotentialNextFloors => new List<int>() { 1 };
        public override int StartRoomID => 0;
        public override int Stage => 0;
    }
}
