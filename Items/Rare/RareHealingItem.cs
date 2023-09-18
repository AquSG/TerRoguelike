using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Player;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Items.Rare
{
    public class RareHealingItem : BaseRoguelikeItem, ILocalizedModType
    {
        public override bool HealingItem => true;
        public override int itemTier => 2;
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 24;
            Item.rare = ItemRarityID.Red;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void ItemEffects(Terraria.Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().rareHealingItem += Item.stack;
        }
    }
}
