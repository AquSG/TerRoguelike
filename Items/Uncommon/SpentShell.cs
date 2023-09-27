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
    public class SpentShell: BaseRoguelikeItem, ILocalizedModType
    {
        public override bool CombatItem => true;
        public override int itemTier => 1;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Green;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void ItemEffects(Terraria.Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().spentShell += Item.stack;
        }
    }
}
