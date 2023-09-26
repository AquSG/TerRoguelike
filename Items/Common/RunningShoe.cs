using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Player;

namespace TerRoguelike.Items.Common
{
    public class RunningShoe : BaseRoguelikeItem, ILocalizedModType
    {
        public override bool UtilityItem => true;
        public override int itemTier => 0;
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 28;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void ItemEffects(Terraria.Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().runningShoe += Item.stack;
        }
    }
}
