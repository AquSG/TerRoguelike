using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace TerRoguelike.Items
{
    public class TerRoguelikeGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int RogueItemTier = -1; // global variant of itemTier from BaseRoguelikeItem
    }
}
