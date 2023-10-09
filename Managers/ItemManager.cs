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
            float combatChance = 33f;
            float healingChance = 33f;
            float utilityChance = 33f;
            switch (tier)
            {
                case 0:
                    combatChance = GetItemListWeight(CommonCombatItems);
                    healingChance = GetItemListWeight(CommonHealingItems);
                    utilityChance = GetItemListWeight(CommonUtilityItems);
                    break;
                case 1:
                    combatChance = GetItemListWeight(UncommonCombatItems);
                    healingChance = GetItemListWeight(UncommonHealingItems);
                    utilityChance = GetItemListWeight(UncommonUtilityItems);
                    break;
                case 2:
                    combatChance = GetItemListWeight(RareCombatItems);
                    healingChance = GetItemListWeight(RareHealingItems);
                    utilityChance = GetItemListWeight(RareUtilityItems);
                    break;
            }

            if (!PastRoomRewardCategories.Any())
            {
                combatChance *= 2f;
            }
            else if (!PastRoomRewardCategories.Contains(0))
            {
                combatChance *= 2f;
            }
            else
            {
                if ((float)PastRoomRewardCategories.FindAll(x => x == 0).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    combatChance *= 2f;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 1).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    healingChance *= 2f;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 2).Count() / (float)PastRoomRewardCategories.Count() < 1f / 6f)
                {
                    utilityChance *= 2f;
                }
            }

            float chance = Main.rand.NextFloat(combatChance + healingChance + utilityChance + float.Epsilon);
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
            float weight = GetItemListWeight(list);
            float randomFloat = Main.rand.NextFloat(weight + float.Epsilon);
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
        public static float GetItemListWeight(List<BaseRoguelikeItem> list)
        {
            float totalWeight = 0;
            for (int i = 0; i < list.Count; i++)
            {
                totalWeight += list[i].ItemDropWeight;
            }
            return totalWeight;
        }
        internal static void Load()
        {
            AllItems = new List<BaseRoguelikeItem>()
            {
                new ClingyGrenade(),
                new PocketSpotter(),
                new CoolantCanister(),
                new AntiqueLens(),
                new InstigatorsBrace(),
                new HotPepper(),
                new BrazenNunchucks(),
                new AttackPlan(),
                new SanguineOrb(),
                new LivingCrystal(),
                new SoulstealCoating(),
                new BottleOfVigor(),
                new BenignFungus(),
                new SentientPutty(),
                new MemoryFoam(),
                new RunningShoe(),
                new BunnyHopper(),
                new TimesHaveBeenTougher(),
                new RustedShield(),
                new AmberBead(),
                new FlimsyPauldron(),
                new ProtectiveBubble(),
                new EvilEye(),
                new SpentShell(),
                new HeatSeekingChip(),
                new LockOnMissile(),
                new BackupDagger(),
                new BloodSiphon(),
                new EnchantingEye(),
                new AutomaticDefibrillator(),
                new BouncyBall(),
                new AirCanister(),
                new UnencumberingStone(),
                new BallAndChain(),
                new SoulOfLena(),
                new VolatileRocket(),
                new TheDreamsoul(),
                new Cornucopia(),
                new ItemPotentiometer()
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
