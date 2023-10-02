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
using TerRoguelike.Items;
using TerRoguelike.Items.Common;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Items.Rare;
using Terraria.ModLoader.Core;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Managers
{
    public class ItemManager
    {
        public static int RoomRewardCooldown = 0;
        public static List<int> PastRoomRewardCategories = new List<int>();
        public static int GiveCommon(bool giveCooldown = true)
        {
            BaseRoguelikeItem chosenItem;
            int category = ChooseCategory(0);
            if (category == 0)
                chosenItem = GetItemFromListWithWeights(CommonCombatItems);
            else if (category == 1)
                chosenItem = GetItemFromListWithWeights(CommonHealingItems);
            else
                chosenItem = GetItemFromListWithWeights(CommonUtilityItems);

            return chosenItem.modItemID;
        }
        public static int GiveUncommon(bool giveCooldown = true)
        {
            if (giveCooldown)
                RoomRewardCooldown += 1;

            BaseRoguelikeItem chosenItem;
            int category = ChooseCategory(1);
            if (category == 0)
                
                chosenItem = GetItemFromListWithWeights(UncommonCombatItems);
            else if (category == 1)
                chosenItem = GetItemFromListWithWeights(UncommonHealingItems);
            else
                chosenItem = GetItemFromListWithWeights(UncommonUtilityItems);


            return chosenItem.modItemID;
        }
        public static int GiveRare(bool giveCooldown = true)
        {
            if (giveCooldown)
                RoomRewardCooldown += 2;

            BaseRoguelikeItem chosenItem;
            int category = ChooseCategory(2);
            if (category == 0)
                chosenItem = GetItemFromListWithWeights(RareCombatItems);
            else if (category == 1)
                chosenItem = GetItemFromListWithWeights(RareHealingItems);
            else
                chosenItem = GetItemFromListWithWeights(RareUtilityItems);

            return chosenItem.modItemID;
        }

        public static int ChooseCategory(int tier)
        {
            int combatChance = 33;
            int healingChance = 33;
            int utilityChance = 33;
            switch (tier)
            {
                case 0:
                    combatChance = CommonCombatItems.Count;
                    healingChance = CommonHealingItems.Count;
                    utilityChance = CommonUtilityItems.Count;
                    break;
                case 1:
                    combatChance = UncommonCombatItems.Count;
                    healingChance = UncommonHealingItems.Count;
                    utilityChance = UncommonUtilityItems.Count;
                    break;
                case 2:
                    combatChance = RareCombatItems.Count;
                    healingChance = RareHealingItems.Count;
                    utilityChance = RareUtilityItems.Count;
                    break;
            }

            if (!PastRoomRewardCategories.Any())
            {
                combatChance *= 2;
            }
            else if (!PastRoomRewardCategories.Contains(0))
            {
                combatChance *= 2;
            }
            else
            {
                if ((float)PastRoomRewardCategories.FindAll(x => x == 0).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    combatChance *= 2;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 1).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    healingChance *= 2;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 2).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    utilityChance *= 2;
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

        public static BaseRoguelikeItem GetItemFromListWithWeights(List<BaseRoguelikeItem> list)
        {
            float totalWeight = 0;
            for (int i = 0; i < list.Count; i++)
            {
                totalWeight += list[i].ItemDropWeight;
            }
            float randomFloat = Main.rand.NextFloat(totalWeight + float.Epsilon);
            int returnIndex = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                randomFloat -= list[i].ItemDropWeight;
                if (randomFloat <= 0)
                {
                    returnIndex = i;
                    break;
                }
            }
            return list[returnIndex];
        }

        internal static void Load()
        {
            AllItems = new List<BaseRoguelikeItem>()
            {
                new ClingyGrenade(),
                new PocketSpotter(),
                new CoolantCanister(),
                new AntiqueLens(),
                new LivingCrystal(),
                new SoulstealCoating(),
                new BottleOfVigor(),
                new RunningShoe(),
                new BunnyHopper(),
                new TimesHaveBeenTougher(),
                new RustedShield(),
                new AmberBead(),
                new EvilEye(),
                new SpentShell(),
                new HeatSeekingChip(),
                new LockOnMissile(),
                new RepurposedSiphon(),
                new EnchantingEye(),
                new BouncyBall(),
                new AirCanister(),
                new VolatileRocket(),
                new TheDreamsoul(),
                new RareHealingItem(),
                new RareUtilityItem()
            };
            CommonCombatItems = new List<BaseRoguelikeItem>();
            CommonHealingItems = new List<BaseRoguelikeItem>();
            CommonUtilityItems = new List<BaseRoguelikeItem>();

            UncommonCombatItems = new List<BaseRoguelikeItem>();
            UncommonHealingItems = new List<BaseRoguelikeItem>();
            UncommonUtilityItems = new List<BaseRoguelikeItem>();

            RareCombatItems = new List<BaseRoguelikeItem>();
            RareHealingItems = new List<BaseRoguelikeItem>();
            RareUtilityItems = new List<BaseRoguelikeItem>();

            CommonCombatItems = AllItems.FindAll(x => x.itemTier == 0 && x.CombatItem);
            CommonHealingItems = AllItems.FindAll(x => x.itemTier == 0 && x.HealingItem);
            CommonUtilityItems = AllItems.FindAll(x => x.itemTier == 0 && x.UtilityItem);

            UncommonCombatItems = AllItems.FindAll(x => x.itemTier == 1 && x.CombatItem);
            UncommonHealingItems = AllItems.FindAll(x => x.itemTier == 1 && x.HealingItem);
            UncommonUtilityItems = AllItems.FindAll(x => x.itemTier == 1 && x.UtilityItem);

            RareCombatItems = AllItems.FindAll(x => x.itemTier == 2 && x.CombatItem);
            RareHealingItems = AllItems.FindAll(x => x.itemTier == 2 && x.HealingItem);
            RareUtilityItems = AllItems.FindAll(x => x.itemTier == 2 && x.UtilityItem);
        }
        internal static void Unload()
        {
            AllItems = null;

            CommonCombatItems = null;
            CommonHealingItems = null;
            CommonUtilityItems = null;

            UncommonCombatItems = null;
            UncommonHealingItems = null;
            UncommonUtilityItems = null;

            RareCombatItems = null;
            RareHealingItems = null;
            RareUtilityItems = null;
        }
        public static List<BaseRoguelikeItem> AllItems;
        public static List<BaseRoguelikeItem> CommonCombatItems;
        public static List<BaseRoguelikeItem> CommonHealingItems;
        public static List<BaseRoguelikeItem> CommonUtilityItems;

        public static List<BaseRoguelikeItem> UncommonCombatItems;
        public static List<BaseRoguelikeItem> UncommonHealingItems;
        public static List<BaseRoguelikeItem> UncommonUtilityItems;

        public static List<BaseRoguelikeItem> RareCombatItems;
        public static List<BaseRoguelikeItem> RareHealingItems;
        public static List<BaseRoguelikeItem> RareUtilityItems;
    }
}
