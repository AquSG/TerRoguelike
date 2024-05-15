using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Projectiles;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using static TerRoguelike.Systems.MusicSystem;
using rail;
using TerRoguelike.Schematics;
using static TerRoguelike.Managers.ItemManager;
using TerRoguelike.Particles;
using Steamworks;
using TerRoguelike.Utilities;

namespace TerRoguelike.World
{
    public static class TerRoguelikeWorld
    {
        public static List<ItemBasinEntity> itemBasins = [];
        public static bool IsTerRoguelikeWorld = false;
        public static bool IsDeletableOnExit = false;
        public static bool IsDebugWorld = false;
        public static int currentStage = 0;
        public static bool lunarFloorInitialized = false;
        public static bool lunarBossSpawned = false;
        public static bool escape = false;
        public static int escapeTime = 0;
        public const int escapeTimeSet = 18000;
        public static List<Chain> chainList = new List<Chain>();
        public static int worldTeleportTime = 0;
        public static int quakeTime = 0;
        public static readonly int setQuateTime = 180;
        public static int quakeCooldown = 0;
        public static int sanctuaryTries = 0;
        public static readonly int sanctuaryMaxTries = 3;
        public static readonly int sanctuaryMaxVisits = 2;
        public static int sanctuaryCount = 0;
        public static float sanctuaryChance => 1 / MathHelper.Clamp(sanctuaryMaxTries - sanctuaryTries, 1, sanctuaryMaxTries);

        public static readonly SoundStyle EarthTremor = new SoundStyle("TerRoguelike/Sounds/EarthTremor", 5);
        public static readonly SoundStyle EarthPound = new SoundStyle("TerRoguelike/Sounds/EarthPound", 4);
        public static readonly SoundStyle WorldTeleport = new SoundStyle("TerRoguelike/Sounds/WorldTeleport", 2);
        public static bool TryWarpToSanctuary()
        {
            if (sanctuaryCount >= sanctuaryMaxVisits)
                return false;
            if (currentStage == 5)
            {
                sanctuaryCount++;
                return true;
            }

            if (Main.rand.NextFloat() <= sanctuaryChance)
            {
                sanctuaryTries = 0;
                sanctuaryCount++;
                if (sanctuaryCount == sanctuaryMaxVisits && currentStage <= 2) // if you got a sanctuary on the first two stages, allow a third one since you didn't get the best chance to take advantage of the pools.
                    sanctuaryCount--;
                return true;
            }
            sanctuaryTries++;
            return false;
        }
        public static void StartEscapeSequence()
        {
            escape = true;
            escapeTime = escapeTimeSet;
            currentStage++;
            for (int i = 0; i < RoomSystem.RoomList.Count; i++)
            {
                Room room = RoomSystem.RoomList[i];
                if (room.IsBossRoom)
                    continue;

                RoomSystem.ResetRoomID(room.ID);
                if (room.IsStartRoom)
                {
                    room.awake = true;
                    if (room.AssociatedFloor == SchematicManager.FloorDict["Lunar"]) // don't reset the rest of the lunar floor
                        break;
                }
            }
            SetMusicMode(MusicStyle.AllCalm);
            SetCalm(Escape, false);
            CalmVolumeLevel = 0.43f;
            PauseWhenIngamePaused = true;
            quakeCooldown = Main.rand.Next(1200, 1800);

            Player player = Main.LocalPlayer;
            if (player == null)
                return;
            var modPlayer = player.ModPlayer();
            if (modPlayer == null)
                return;
            modPlayer.escapeArrowTime = 600;
            Room lunarStartRoom = SchematicManager.RoomID[SchematicManager.FloorID[SchematicManager.FloorDict["Lunar"]].StartRoomID];
            modPlayer.escapeArrowTarget = lunarStartRoom.RoomPosition16 + Vector2.UnitY * lunarStartRoom.RoomDimensions.Y * 8f;
        }
    }
    public class ItemBasinEntity
    {
        public Point position;
        public Rectangle rect;
        public Rectangle rect16;
        public ItemTier tier;
        public int nearby = 0;
        public int itemDisplay;
        public List<int> itemOptions = [];
        public int useCount = 0;
        
        public ItemBasinEntity(Point Position, ItemTier Tier)
        {
            position = Position;
            ResetRect();
            tier = Tier;
        }
        public void ResetRect()
        {
            rect = new Rectangle(position.X, position.Y, 3, 2);
            rect16 = new Rectangle(rect.X * 16, rect.Y * 16, 48, 32);
        }
        public void SpawnParticles()
        {
            Vector2 basePos = (position.ToVector2() + new Vector2(1, 0)).ToWorldCoordinates(8, 0);
            var color = tier switch
            {
                ItemTier.Uncommon => new Color(0.4f, 1f, 0.4f),
                ItemTier.Rare => new Color(1f, 0.4f, 0.4f),
                _ => new Color(0.2f, 0.55f, 1f),
            };
            Color fillColor = Color.Lerp(color, Color.Black, 0.3f);
            int time = 30;

            if (!Main.rand.NextBool(4))
            {
                Vector2 particlePos = basePos + new Vector2(Main.rand.NextFloat(-12, 12), 0);
                ParticleManager.AddParticle(new BallOutlined(
                    particlePos, (-Vector2.UnitY * Main.rand.NextFloat(0.7f, 1.2f)).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)), time, color, fillColor, new Vector2(0.15f, 0.15f), 2, 0, 0.92f, time),
                    ParticleManager.ParticleLayer.BehindTiles);
            }
        }
        public void GenerateItemOptions(Player player)
        {
            useCount = 0;
            itemOptions = [];
            for (int invItem = 0; invItem < 51; invItem++)
            {
                var item = invItem switch
                {
                    50 => player.trashItem,
                    _ => player.inventory[invItem],
                };

                if (item.type == itemDisplay || !item.active || item.stack <= 0 || item.type == 0)
                    continue;

                int rogueItemType = AllItems.FindIndex(x => x.modItemID == item.type);
                if (rogueItemType != -1 && (ItemTier)AllItems[rogueItemType].itemTier == tier && !itemOptions.Contains(item.type))
                {
                    itemOptions.Add(item.type);
                }
            }
        }
        public void ShrinkItemOptions(int priority = -1)
        {
            useCount++;
            float removeRatio = useCount / 4f;
            int removeCount = (int)(removeRatio * itemOptions.Count);
            if (removeCount == 0)
                removeCount = 1;
            for (int i = 0; i < removeCount; i++)
            {
                if (itemOptions.Count <= 0)
                    break;
                if (i == 0 && priority >= 0)
                {
                    itemOptions.RemoveAll(x => x == priority);
                    continue;
                }
                itemOptions.RemoveAt(Main.rand.Next(itemOptions.Count));
            }
        }
    }
    public class Chain
    {
        public Chain(Vector2 start, Vector2 end, int length, int maxTimeLeft, int attachedNPC = -1)
        {
            Start = start;
            End = end;
            TimeLeft = maxTimeLeft;
            MaxTimeLeft = maxTimeLeft;
            Length = length;
            AttachedNPC = attachedNPC;
        }
        public int Length;
        public int MaxTimeLeft;
        public int TimeLeft;
        public Vector2 Start;
        public Vector2 End;
        public int AttachedNPC;
    }
}
