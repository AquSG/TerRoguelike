using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Items.Uncommon
{
    public class SavageRing : BaseRoguelikeItem, ILocalizedModType
    {
        public override int modItemID => ModContent.ItemType<SavageRing>();
        public override bool CombatItem => true;
        public override int itemTier => 1;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void ItemEffects(Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().savageRing += Item.stack;
        }
    }
}
