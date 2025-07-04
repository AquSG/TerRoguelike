﻿using System;
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
using Terraria.Audio;
using TerRoguelike.Items.Weapons;

namespace TerRoguelike.Managers
{
    public class ItemManager
    {
        public static bool loaded = false;
        public static List<StarterItem> StarterRanged = [];
        public static List<StarterItem> StarterMelee = [];
        public static void LoadStarterItems()
        {
            StarterRanged = [
                new(ModContent.ItemType<AdaptiveGun>()),
                new(ModContent.ItemType<AdaptiveCannon>()),
                new(ModContent.ItemType<AdaptiveRifle>()),
                new(ModContent.ItemType<AdaptiveSpaceGun>())];
            StarterMelee = [
                new(ModContent.ItemType<AdaptiveBlade>()),
                new(ModContent.ItemType<AdaptiveSpear>()),
                new(ModContent.ItemType<AdaptiveDagger>()),
                new(ModContent.ItemType<AdaptiveSaber>())];
            loaded = true;
        }
        public static void UnloadStarterItems()
        {
            StarterRanged = null;
            StarterMelee = null;
            loaded = false;
        }
        public class StarterItem
        {
            public int id;
            public StarterItem(int ID)
            {
                id = ID;
            }
        }

        public static readonly SoundStyle ItemSpawn = new SoundStyle("TerRoguelike/Sounds/ItemSpawn", 3);
        public static readonly SoundStyle ItemLand = new SoundStyle("TerRoguelike/Sounds/ItemLand", 3);
        public enum ItemTier
        {
            Common = 0,
            Uncommon = 1,
            Rare = 2
        }

        //ITEM TIERS: 0 - Common, 1 - Uncommon, 2 - Rare
        public static int RoomRewardCooldown = 0;
        public static List<int> PastRoomRewardCategories = new List<int>();
        public static int GiveCommon(bool giveCooldown = false)
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
        public static int GiveUncommon(bool giveCooldown = false)
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
        public static int GiveRare(bool giveCooldown = false)
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
                //obtain weights of each list so that every item in each tier has the same chance of appearing as every other.
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

            //attempt to steer the rewarded category towards a category the player is reportedly lacking in.
            if (PastRoomRewardCategories.Count == 0)
            {
                combatChance *= 2f;
            }
            else if (!PastRoomRewardCategories.Contains(0))
            {
                combatChance *= 2f;
            }
            else
            {
                if ((float)PastRoomRewardCategories.FindAll(x => x == 0).Count / PastRoomRewardCategories.Count < 1f / 6f)
                {
                    combatChance *= 2f;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 1).Count / PastRoomRewardCategories.Count < 1f / 6f)
                {
                    healingChance *= 2f;
                }
                else if ((float)PastRoomRewardCategories.FindAll(x => x == 2).Count / PastRoomRewardCategories.Count < 1f / 6f)
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
        public static int ChooseItemUnbiased(int tier)
        {
            float combatChance;
            float healingChance;
            float utilityChance;
            List<BaseRoguelikeItem> combatItemList;
            List<BaseRoguelikeItem> healingItemList;
            List<BaseRoguelikeItem> utilityItemList;
            switch (tier)
            {
                //obtain weights of each list so that every item in each tier has the same chance of appearing as every other.
                default:
                case 0:
                    combatItemList = CommonCombatItems;
                    healingItemList = CommonHealingItems;
                    utilityItemList = CommonUtilityItems;
                    break;
                case 1:
                    combatItemList = UncommonCombatItems;
                    healingItemList = UncommonHealingItems;
                    utilityItemList = UncommonUtilityItems;
                    break;
                case 2:
                    combatItemList = RareCombatItems;
                    healingItemList = RareHealingItems;
                    utilityItemList = RareUtilityItems;
                    break;
            }

            combatChance = GetItemListWeight(combatItemList);
            healingChance = GetItemListWeight(healingItemList);
            utilityChance = GetItemListWeight(utilityItemList);

            float chance = Main.rand.NextFloat(combatChance + healingChance + utilityChance + float.Epsilon);
            int chosenCategory;
            if (chance <= combatChance)
                chosenCategory = 0;
            else if (chance <= combatChance + healingChance)
                chosenCategory = 1;
            else
                chosenCategory = 2;

            BaseRoguelikeItem chosenItem;
            if (chosenCategory == 0)
                chosenItem = GetItemFromListWithWeights(combatItemList);
            else if (chosenCategory == 1)
                chosenItem = GetItemFromListWithWeights(healingItemList);
            else
                chosenItem = GetItemFromListWithWeights(utilityItemList);
            return chosenItem.modItemID;
        }
        public static void QuickSpawnRoomItem(Vector2 position, bool boss = false)
        {
            int chance = Main.rand.Next(1, 101);
            int itemType;
            int itemTier;

            if (boss)
            {
                if (chance <= 80)
                {
                    itemType = GiveUncommon(false);
                    itemTier = 1;
                }
                else
                {
                    itemType = GiveRare(false);
                    itemTier = 2;
                }
                SpawnManager.SpawnItem(itemType, position, itemTier, 75, 0.5f, -1);
                return;
            }

            if (RoomRewardCooldown > 0)
            {
                RoomRewardCooldown--;
                return;
            }

            if (chance <= 80)
            {
                itemType = GiveCommon();
                itemTier = 0;
            }
            else if (chance <= 98)
            {
                itemType = GiveUncommon();
                itemTier = 1;
            }
            else
            {
                itemType = GiveRare();
                itemTier = 2;
            }
            SpawnManager.SpawnItem(itemType, position, itemTier, 75, 0.5f, -1);
        }
        internal static void Load()
        {
            AllItems = new List<BaseRoguelikeItem>()
            {
                new ClingyGrenade(),
                new PocketSpotter(),
                new CoolantBarrel(),
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
                new BurningCharcoal(),
                new ReactiveMicrobots(),
                new RemedialTapeworm(),

                new EvilEye(),
                new SpentShell(),
                new HeatSeekingChip(),
                new LockOnMissile(),
                new BackupDagger(),
                new ClusterBombSatchel(),
                new RetaliatoryFist(),
                new BloodSiphon(),
                new EnchantingEye(),
                new AutomaticDefibrillator(),
                new StimPack(),
                new BarbedLasso(),
                new SteamEngine(),
                new BouncyBall(),
                new AirCanister(),
                new UnencumberingStone(),
                new BallAndChain(),
                new SoulOfLena(),
                new EmeraldRing(),
                new AmberRing(),
                new ThrillOfTheHunt(),
                new GiftBox(),
                new AncientTwig(),
                new DisposableTurret(),
                new WayfarersWaistcloth(),

                new VolatileRocket(),
                new TheDreamsoul(),
                new DroneBuddy(),
                new Overclocker(),
                new ShotgunComponent(),
                new SniperComponent(),
                new MinigunComponent(),
                new Cornucopia(),
                new NutritiousSlime(),
                new AllSeeingEye(),
                new SymbioticFungus(),
                new ItemPotentiometer(),
                new BarrierSynthesizer(),
                new JetLeg(),
                new GiantDoorShield(),
                new TrumpCard(),
                new PortableGenerator(),
                new ForgottenBioWeapon(),
                new LunarCharm(),
                new CeremonialCrown(),
                new ThermitePowder(),
                new EverlastingJellyfish(),
                new HeartyHoneycomb(),
                new PrimevalRattle(),
                new TheFalseSun(),
                new PuppeteersHand(),
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
