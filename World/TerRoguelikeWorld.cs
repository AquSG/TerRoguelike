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

        public static readonly SoundStyle EarthTremor = new SoundStyle("TerRoguelike/Sounds/EarthTremor", 5);
        public static readonly SoundStyle EarthPound = new SoundStyle("TerRoguelike/Sounds/EarthPound", 4);
        public static readonly SoundStyle WorldTeleport = new SoundStyle("TerRoguelike/Sounds/WorldTeleport", 2);
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
        }
    }
    public class ItemBasinEntity
    {
        public Point position;
        public Rectangle rect;
        public ItemTier tier;
        public int nearby = 0;
        public int itemDisplay;
        
        public ItemBasinEntity(Point Position, ItemTier Tier)
        {
            position = Position;
            ResetRect();
            tier = Tier;
        }
        public void ResetRect()
        {
            rect = new Rectangle(position.X, position.Y, 3, 2);
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
