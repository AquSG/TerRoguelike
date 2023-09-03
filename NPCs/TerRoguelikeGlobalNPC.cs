using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerRoguelike.NPCs
{
    public class TerRoguelikeGlobalNPC : GlobalNPC
    {
        public bool isRoomNPC = false;
        public int sourceRoomListID = -1;

        public override bool InstancePerEntity => true;
    }
}
