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
    public class ForestFloor : Floor
    {
        public override int FloorID => 2;
        public override int StartRoomID => 30;
        public override List<int> BossRoomIDs => new List<int>() { 31 };
        public override int Stage => 0;
    }
}
