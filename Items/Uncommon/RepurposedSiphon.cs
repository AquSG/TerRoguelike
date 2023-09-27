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

namespace TerRoguelike.Items.Uncommon
{
    public class RepurposedSiphon : BaseRoguelikeItem, ILocalizedModType
    {
        public override bool HealingItem => true;
        public override int itemTier => 1;
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 48;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void ItemEffects(Terraria.Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().repurposedSiphon += Item.stack;
        }
    }
}
