using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Rooms;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TerRoguelike.Schematics;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Items.Common;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Items.Rare;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Managers
{
    public class ItemManager
    {
        public static int RoomRewardCooldown = 0;
        public static List<int> PastRoomRewardCategories = new List<int>();
        public static int GiveCommon(bool giveCooldown = true)
        {
            int chosenItem;
            int category = ChooseCategory();
            if (category == 0)
                chosenItem = CommonCombatItems[Main.rand.Next(CommonCombatItems.Count)];
            else if (category == 1)
                chosenItem = CommonHealingItems[Main.rand.Next(CommonHealingItems.Count)];
            else
                chosenItem = CommonUtilityItems[Main.rand.Next(CommonUtilityItems.Count)];

            return chosenItem;
        }
        public static int GiveUncommon(bool giveCooldown = true)
        {
            if (giveCooldown)
                RoomRewardCooldown += 1;

            int chosenItem;
            int category = ChooseCategory();
            if (category == 0)
                chosenItem = UncommonCombatItems[Main.rand.Next(UncommonCombatItems.Count)];
            else if (category == 1)
                chosenItem = UncommonHealingItems[Main.rand.Next(UncommonHealingItems.Count)];
            else
                chosenItem = UncommonUtilityItems[Main.rand.Next(UncommonUtilityItems.Count)];

            return chosenItem;
        }
        public static int GiveRare(bool giveCooldown = true)
        {
            if (giveCooldown)
                RoomRewardCooldown += 2;

            int chosenItem;
            int category = ChooseCategory();
            if (category == 0)
                chosenItem = RareCombatItems[Main.rand.Next(RareCombatItems.Count)];
            else if (category == 1)
                chosenItem = RareHealingItems[Main.rand.Next(RareHealingItems.Count)];
            else
                chosenItem = RareUtilityItems[Main.rand.Next(RareUtilityItems.Count)];

            return chosenItem;
        }

        public static int ChooseCategory()
        {
            int combatChance = 33;
            int healingChance = 33;
            int utilityChance = 33;

            if (!PastRoomRewardCategories.Any())
            {
                combatChance = 50;
                healingChance = 25;
                utilityChance = 25;
            }
            else if (!PastRoomRewardCategories.Contains(0))
            {
                combatChance = 50;
                healingChance = 25;
                utilityChance = 25;
            }
            else
            {
                if ((float)PastRoomRewardCategories.FindAll(x => x == 0).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    combatChance = 50;
                    healingChance = 25;
                    utilityChance = 25;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 1).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    combatChance = 25;
                    healingChance = 50;
                    utilityChance = 25;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 2).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    combatChance = 25;
                    healingChance = 25;
                    utilityChance = 50;
                }
            }

            int chance = Main.rand.Next(1, combatChance + healingChance + utilityChance + 1);
            int chosenCategory;
            if (chance <= combatChance)
                chosenCategory = 0;
            else if (chance <= combatChance + healingChance)
                chosenCategory = 1;
            else
                chosenCategory = 2;

            PastRoomRewardCategories.Add(chosenCategory);
            return chosenCategory;
        }

        #region Common Item Lists
        public static List<int> CommonCombatItems = new List<int>()
        {
            ModContent.ItemType<ClingyGrenade>(),
            ModContent.ItemType<CriticalSights>()
        };
        public static List<int> CommonHealingItems = new List<int>()
        {
            ModContent.ItemType<LivingCrystal>(),
            ModContent.ItemType<SoulstealCoating>()
        };
        public static List<int> CommonUtilityItems = new List<int>()
        {
            ModContent.ItemType<RunningShoe>(),
            ModContent.ItemType<BunnyHopper>()
        };
        #endregion

        #region Uncommon Item Lists
        public static List<int> UncommonCombatItems = new List<int>()
        {
            ModContent.ItemType<EvilEye>(),
            ModContent.ItemType<SpentShell>(),
            ModContent.ItemType<HeatSeekingChip>()
        };
        public static List<int> UncommonHealingItems = new List<int>()
        {
            ModContent.ItemType<UncommonHealingItem>(),
            ModContent.ItemType<UncommonHealingItem>()
        };
        public static List<int> UncommonUtilityItems = new List<int>()
        {
            ModContent.ItemType<UncommonUtilityItem>(),
            ModContent.ItemType<UncommonUtilityItem>()
        };
        #endregion

        #region Rare Item Lists
        public static List<int> RareCombatItems = new List<int>()
        {
            ModContent.ItemType<RareCombatItem>(),
            ModContent.ItemType<RareCombatItem>()
        };
        public static List<int> RareHealingItems = new List<int>()
        {
            ModContent.ItemType<RareHealingItem>(),
            ModContent.ItemType<RareHealingItem>()
        };
        public static List<int> RareUtilityItems = new List<int>()
        {
            ModContent.ItemType<RareUtilityItem>(),
            ModContent.ItemType<RareUtilityItem>()
        };
        #endregion
    }
}
