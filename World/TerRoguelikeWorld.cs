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
using TerRoguelike.NPCs.Enemy.Boss;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.TerPlayer;
using ReLogic.Utilities;

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
        public static readonly int escapeTimeSet = 18000;
        public static List<Chain> chainList = new List<Chain>();
        public static int worldTeleportTime = 0;
        public static int quakeTime = 0;
        public static readonly int setQuateTime = 180;
        public static int quakeCooldown = 0;
        public static int lunarGambitSceneTime = 0;
        public static Vector2 lunarGambitSceneStartPos = Vector2.Zero;
        public static int lunarGambitStartDuration = 80;
        public static int lunarGambitFloatOverDuration = 80;
        public static Vector2 jstcPortalPos = Vector2.Zero;
        public static int jstcPortalTime = 0;
        public static Vector2 jstcPortalScale = Vector2.Zero;
        public static Vector2 lunarGambitSceneDisplayPos { get
            {
                Vector2 drawPos = lunarGambitSceneStartPos;
                var targetRoom = SchematicManager.RoomID[SchematicManager.RoomDict["LunarBossRoom1"]];
                Vector2 endTargetPos = targetRoom.RoomPosition16 + targetRoom.RoomCenter16 + Vector2.UnitY * 240;
                if (lunarGambitSceneTime > lunarGambitStartDuration + lunarGambitFloatOverDuration)
                    drawPos = endTargetPos;
                else
                {
                    float startInterpolant = MathHelper.Clamp(((float)lunarGambitSceneTime / lunarGambitStartDuration), 0, 1);
                    drawPos += -Vector2.UnitY * 64 * ((float)Math.Log(startInterpolant, 100) + 1);
                    float floatOverInterpolant = MathHelper.Clamp((float)(lunarGambitSceneTime - lunarGambitStartDuration) / lunarGambitFloatOverDuration, 0, 1);
                    drawPos += (endTargetPos - drawPos) * (float)Math.Pow(floatOverInterpolant, 1.7f)
                        + Vector2.UnitY * 80 * ((float)Math.Pow((floatOverInterpolant - 0.5f) * 2, 2) + 1) * 0.5f + Vector2.UnitY * -80;
                }
                return drawPos;
            } }
        public static float lunarGambitSceneScaleInterpolant
        {
            get
            {
                float interpolant = MathHelper.Clamp((lunarGambitSceneTime - lunarGambitStartDuration - lunarGambitFloatOverDuration) / 180f, 0, 1);
                return MathHelper.SmoothStep(0.1f, 1f, interpolant);
            }
        }
        public static int sanctuaryTries = 0;
        public static readonly int sanctuaryMaxTries = 3;
        public static readonly int sanctuaryMaxVisits = 2;
        public static int sanctuaryCount = 0;
        public static int currentLoop = 0;
        public static bool promoteLoop = false;
        public static int currentStageForScaling => currentStage + currentLoop * 6;
        public static float sanctuaryChance => 1 / MathHelper.Clamp(sanctuaryMaxTries - sanctuaryTries, 1, sanctuaryMaxTries);

        public static readonly SoundStyle EarthTremor = new SoundStyle("TerRoguelike/Sounds/EarthTremor", 5);
        public static readonly SoundStyle EarthPound = new SoundStyle("TerRoguelike/Sounds/EarthPound", 4);
        public static readonly SoundStyle WorldTeleport = new SoundStyle("TerRoguelike/Sounds/WorldTeleport", 2);
        public static readonly SoundStyle PortalSpawn = new SoundStyle("TerRoguelike/Sounds/PortalSpawn");
        public static readonly SoundStyle PortalLoop = new SoundStyle("TerRoguelike/Sounds/PortalLoop");
        public static readonly SoundStyle AahLoop = new SoundStyle("TerRoguelike/Sounds/AahLoop");
        public static SlotId PortalSlot;
        public static bool TryWarpToSanctuary(TerRoguelikePlayer modPlayer)
        {
            if (modPlayer.lunarCharm > 0)
            {
                sanctuaryTries = 0;
                sanctuaryCount++;
                if (sanctuaryCount == sanctuaryMaxVisits && currentStage <= 2) // same thing as below
                    sanctuaryCount--;
                return true;
            }
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
            TerRoguelikePlayer.HealthUpIndicator(player);
            var modPlayer = player.ModPlayer();
            if (modPlayer == null)
                return;
            modPlayer.escapeArrowTime = 600;
            Room lunarStartRoom = SchematicManager.RoomID[SchematicManager.FloorID[SchematicManager.FloorDict["Lunar"]].StartRoomID];
            modPlayer.escapeArrowTarget = lunarStartRoom.RoomPosition16 + Vector2.UnitY * lunarStartRoom.RoomDimensions.Y * 8f;
        }

        public static List<StoredDraw> GetTrueBrainDrawList(Vector2 position, Vector2 eyeVector, Vector2 scale, Color color, int currentFrame, float teleportInterpolant = 0)
        {
            var draws = new List<StoredDraw>();
            float maxLength = 12;
            if (eyeVector.Length() > maxLength)
                eyeVector = eyeVector.SafeNormalize(Vector2.UnitY) * maxLength;

            if (teleportInterpolant > 0)
            {
                float interpolant = teleportInterpolant < 0.5f ? teleportInterpolant / 0.5f : 1f - ((teleportInterpolant - 0.5f) / 0.5f);
                float horizInterpolant = MathHelper.Lerp(1f, 2f, 0.5f + (0.5f * -(float)Math.Cos(interpolant * MathHelper.TwoPi)));
                float verticInterpolant = MathHelper.Lerp(0.5f + (0.5f * (float)Math.Cos(interpolant * MathHelper.TwoPi)), 8f, interpolant * interpolant);
                scale.X *= horizInterpolant;
                scale.Y *= verticInterpolant;

                scale *= 1f - interpolant;
            }

            var trueBrainTex = TextureAssets.Npc[ModContent.NPCType<TrueBrain>()].Value;
            var trueBrainEyeTex = TextureManager.TexDict["TrueBrainEye"];
            var trueBrainInnerEyeTex = TextureManager.TexDict["MoonLordInnerEye"];
            int frameCount = Main.npcFrameCount[ModContent.NPCType<TrueBrain>()];
            int frameHeight = trueBrainTex.Height / frameCount;
            currentFrame = currentFrame % (frameCount - 1) + 1;

            var frame = new Rectangle(0, frameHeight * currentFrame, trueBrainTex.Width, frameHeight - 2);
            draws.Add(new(trueBrainTex, position, frame, color, 0, frame.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(trueBrainEyeTex, position + new Vector2(0, -18) * scale, null, color, 0, trueBrainEyeTex.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(trueBrainInnerEyeTex, position + (new Vector2(0, -19) + eyeVector * new Vector2(0.35f, 1f)) * scale, null, color, 0, trueBrainInnerEyeTex.Size() * 0.5f, scale, SpriteEffects.None));

            return draws;
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
