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
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;

namespace TerRoguelike.Items.Uncommon
{
    public class BackupDagger : BaseRoguelikeItem, ILocalizedModType
    {
        public override int modItemID => ModContent.ItemType<BackupDagger>();
        public override bool CombatItem => true;
        public override int itemTier => 1;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Green;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void ItemEffects(Player player)
        {
            player.GetModPlayer<TerRoguelikePlayer>().backupDagger += Item.stack;
        }
        public override bool OnPickup(Player player)
        {
            var modPlayer = player.ModPlayer();
            if (modPlayer == null)
                return true;

            if (modPlayer.backupDagger == 0)
            {
                modPlayer.storedDaggers = modPlayer.visualStoredDaggers = 0;
            }
            return true;
        }
    }
}
