using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Terraria.DataStructures;
using TerRoguelike.World;
using Terraria.ID;
using TerRoguelike.MainMenu;
using Terraria.Localization;
using TerRoguelike.Utilities;

namespace TerRoguelike.Items
{
    public class TerRoguelikeGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int RogueItemTier = -1; // global variant of itemTier from BaseRoguelikeItem
        public override void OnSpawn(Item item, IEntitySource source)
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld && source is EntitySource_TileBreak)
            {
                if (ShouldBeDeletedInTerRoguelikeWorld(item.type))
                    item.active = false;
            }

        }
        public bool ShouldBeDeletedInTerRoguelikeWorld(int type)
        {
            switch (type)
            {
                default:
                    return false;
                case ItemID.Deathweed:
                case ItemID.DeathweedSeeds:
                case ItemID.Shiverthorn:
                case ItemID.ShiverthornSeeds:
                case ItemID.Fireblossom:
                case ItemID.FireblossomSeeds:
                case ItemID.Moonglow:
                case ItemID.MoonglowSeeds:
                case ItemID.JungleGrassSeeds:
                case ItemID.GrassSeeds:
                case ItemID.NaturesGift:
                case ItemID.Pumpkin:
                case ItemID.Blinkroot:
                case ItemID.BlinkrootSeeds:
                    return true;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (TerRoguelikeMenu.RuinedMoonActive && item.rare == ItemRarityID.Gray)
            {
                tooltips.Add(new(TerRoguelike.Instance, "TrashDescription", Language.GetOrRegister("Mods.TerRoguelike.TrashDescription").Value));
            }
        }
        public override void UpdateInventory(Item item, Player player)
        {
            if (item.rare == ItemRarityID.Gray && player.ModPlayer().enableTrash)
            {
                player.ModPlayer().trash += item.stack;
            }
        }
    }
}
