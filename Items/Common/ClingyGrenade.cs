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

namespace TerRoguelike.Items.Common
{
    public class ClingyGrenade : BaseRoguelikeItem, ILocalizedModType
    {
        public override bool CombatItem => true;
        public override int itemTier => 0;
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 34;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void ItemEffects(Terraria.Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().clingyGrenade += Item.stack;
        }
    }
}
