using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using TerRoguelike.Managers;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;
using Terraria.ID;
using TerRoguelike.Systems;
using TerRoguelike.Projectiles;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Audio;
using System.IO;

namespace TerRoguelike.Systems
{
    public class RoomSystem : ModSystem //This file handles pretty much everything relating to updating rooms
    {
        public static List<Room> RoomList; //List of all rooms currently in play in the world
        public static List<HealingPulse> healingPulses = new List<HealingPulse>();
        public static List<AttackPlanRocketBundle> attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
        public static bool obtainedRoomListFromServer = false;
        public static void NewRoom(Room room)
        {
            RoomList.Add(room);
        }
        public override void PostUpdateWorld()
        {
            SpawnManager.UpdateSpawnManager(); //Run all logic for all pending items and enemies being telegraphed
            UpdateHealingPulse(); //Used for uncommon healing item based on room time
            UpdateAttackPlanRocketBundles(); //Used for the attack plan item that handles future attack plan bundles

            if (RoomList == null)
                return;

            if (!RoomList.Any())
                return;

            int loopCount = -1;
            foreach (Room room in RoomList)
            {
                loopCount++;
                if (room == null)
                    continue;

                room.myRoom = loopCount; //updates the room's 'myRoom' to refer to it's index on RoomList

                for (int i = 0; i < Main.maxPlayers; i++) //Player collision with rooms
                {
                    Player player;
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        player = Main.player[Main.myPlayer];
                    else
                        player = Main.player[i];

                    TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                    bool roomXcheck = player.Center.X - (player.width / 2f) > (room.RoomPosition.X + 1f) * 16f - 1f && player.Center.X + (player.width / 2f) < (room.RoomPosition.X - 1f + room.RoomDimensions.X) * 16f + 1f;
                    bool roomYcheck = player.Center.Y - (player.height / 2f) > (room.RoomPosition.Y + 1f) * 16f && player.Center.Y + (player.height / 2f) < (room.RoomPosition.Y - (15f / 16f) + room.RoomDimensions.Y) * 16f;
                    if (roomXcheck && roomYcheck)
                    {
                        modPlayer.currentRoom = -1; //Current room is -1 unless the player is inside an active room in RoomList
                        if (room.AssociatedFloor != -1)
                            modPlayer.currentFloor = FloorID[room.AssociatedFloor]; //If player is inside a room with a valid value for an associated floor, set it to that.
                        if (room.active)
                            modPlayer.currentRoom = room.myRoom; 

                        room.awake = true;
                        bool teleportCheck = room.closedTime > 180 && room.IsBossRoom && player.position.X + player.width >= ((room.RoomPosition.X + room.RoomDimensions.X) * 16f) - 22f;
                        if (teleportCheck) //New Floor Blue Wall Portal Teleport
                        {
                            int nextFloorID = modPlayer.currentFloor.Stage + 1;
                            if (nextFloorID >= RoomManager.FloorIDsInPlay.Count) // if FloorIDsInPlay overflows, send back to the start
                                nextFloorID = 0;

                            var nextFloor = FloorID[RoomManager.FloorIDsInPlay[nextFloorID]];
                            var targetRoom = RoomID[nextFloor.StartRoomID];
                            player.Center = (targetRoom.RoomPosition + (targetRoom.RoomDimensions / 2f)) * 16f;
                            modPlayer.currentFloor = nextFloor;

                            //New floor item effects
                            modPlayer.soulOfLenaUses = 0;
                            modPlayer.lenaVisualPosition = Vector2.Zero;
                            if (modPlayer.giftBox > 0)
                            {
                                modPlayer.GiftBoxLogic((targetRoom.RoomPosition + (targetRoom.RoomDimensions / 2f)) * 16f);
                            }
                        }

                        if (room.closedTime == 1) // heal players on room clear so no waiting slog for natural life regen
                        {
                            player.statLife = player.statLifeMax2;
                        }
                    }

                    if (Main.netMode == NetmodeID.SinglePlayer) // don't loop through all players if in singleplayer lol
                        break;
                }

                room.Update();
            }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            var isTerRoguelikeWorld = TerRoguelikeWorld.IsTerRoguelikeWorld;
            tag["isTerRoguelikeWorld"] = isTerRoguelikeWorld;

            if (RoomList == null)
                return;

            //Save all critical data, such as what rooms are in play, their positions, dimensions, and what floors are in play
            var roomIDs = new List<int>();
            var roomPositions = new List<Vector2>();
            var roomDimensions = new List<Vector2>();
            var floorIDsInPlay = RoomManager.FloorIDsInPlay;

            foreach (Room room in RoomList)
            {
                if (room == null)
                    continue;

                roomIDs.Add(room.ID);
                roomPositions.Add(room.RoomPosition);
                roomDimensions.Add(room.RoomDimensions);
            }
            tag["roomIDs"] = roomIDs;
            tag["roomPositions"] = roomPositions;
            tag["roomDimensions"] = roomDimensions;
            tag["floorIDsInPlay"] = floorIDsInPlay;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            var isTerRoguelikeWorld = tag.GetBool("isTerRoguelikeWorld");
            TerRoguelikeWorld.IsTerRoguelikeWorld = isTerRoguelikeWorld;
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            if (SpawnManager.pendingEnemies != null)
                SpawnManager.pendingEnemies.Clear();
            else
                SpawnManager.pendingEnemies = new List<PendingEnemy>();

            if (SpawnManager.pendingItems != null)
                SpawnManager.pendingItems.Clear();
            else
                SpawnManager.pendingItems = new List<PendingItem>();

            RoomList = new List<Room>();
            int loopcount = 0;
            var roomIDs = tag.GetList<int>("roomIDs");
            var roomPositions = tag.GetList<Vector2>("roomPositions");
            var roomDimensions = tag.GetList<Vector2>("roomDimensions");
            var floorIDsInPlay = tag.GetList<int>("floorIDsInPlay");
            foreach (int id in roomIDs)
            {
                if (id == -1)
                    continue;

                RoomList.Add(RoomID[id]);
                RoomList[loopcount].RoomPosition = roomPositions[loopcount];
                RoomList[loopcount].RoomDimensions = roomDimensions[loopcount];
                ResetRoomID(id);
                loopcount++;
            }
            RoomManager.FloorIDsInPlay = new List<int>();
            foreach (int floorID in floorIDsInPlay)
            {
                RoomManager.FloorIDsInPlay.Add(floorID);
            }
        }
        /// <summary>
        /// Resets the given room ID to it's default values, aside from position and dimensions
        /// </summary>
        public static void ResetRoomID(int id)
        {
            RoomID[id].active = true;
            RoomID[id].initialized = false;
            RoomID[id].awake = false;
            RoomID[id].roomTime = 0;
            RoomID[id].closedTime = 0;
            RoomID[id].NPCSpawnPosition = new Vector2[Room.RoomSpawnCap];
            RoomID[id].NPCToSpawn = new int[Room.RoomSpawnCap];
            RoomID[id].TimeUntilSpawn = new int[Room.RoomSpawnCap];
            RoomID[id].TelegraphDuration = new int[Room.RoomSpawnCap];
            RoomID[id].TelegraphSize = new float[Room.RoomSpawnCap];
            RoomID[id].NotSpawned = new bool[Room.RoomSpawnCap];
            RoomID[id].anyAlive = true;
            RoomID[id].roomClearGraceTime = -1;
            RoomID[id].wallActive = false;
        }
        public override void PostDrawTiles()
        {
            DrawPendingEnemies();
            DrawHealingPulse();

            if (RoomList == null)
                return;

            Texture2D lightTexture = ModContent.Request<Texture2D>("TerRoguelike/Tiles/TemporaryBlock").Value;
            foreach (Room room in RoomList)
            {
                if (room == null)
                    continue;
                if (!room.awake)
                    continue;

                if (room.closedTime > 60)
                {
                    if (room.IsBossRoom)
                    {
                        bool canDraw = false;
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Vector2.Distance(Main.player[i].Center, (room.RoomPosition + new Vector2(room.RoomDimensions.X, 0)) * 16f) < 2500f)
                            {
                                canDraw = true;
                                break;
                            }
                        }
                        if (!canDraw)
                            continue;

                        //Draw the blue wall portal
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                        for (float i = 0; i < room.RoomDimensions.Y; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(room.RoomDimensions.X - 2, i);
                            if (Main.tile[targetBlock.ToPoint()].TileType != TileID.Platforms)
                            {
                                if (Main.tile[targetBlock.ToPoint()].HasTile)
                                    continue;
                            }

                            Color color = Color.Cyan;

                            color.A = (byte)MathHelper.Clamp(MathHelper.Lerp(0, 255, (room.closedTime - 120) / 60f), 0, 255);

                            Vector2 drawPosition = targetBlock * 16f - Main.screenPosition - new Vector2(0, -16f);
                            float rotation = MathHelper.PiOver2;

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);

                            float scale = MathHelper.Clamp(MathHelper.Lerp(0.85f, 0.75f, (room.closedTime - 120f) / 60f), 0.75f, 0.85f);

                            if (Main.rand.NextBool((int)MathHelper.Clamp(MathHelper.Lerp(30f, 8f, (room.closedTime - 60f) / 120f), 8f, 20f)))
                                Dust.NewDustDirect((targetBlock * 16f) + new Vector2(10f, 0), 2, 16, 206, Scale: scale);
                        }
                        Main.spriteBatch.End();
                    }
                    continue;
                }

                if (room.wallActive)
                {
                    //Draw the pink borders indicating the bounds of the room
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    for (float side = 0; side < 2; side++)
                    {
                        for (float i = 0; i < room.RoomDimensions.X; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(i, side * room.RoomDimensions.Y - side);

                            if (Main.tile[targetBlock.ToPoint()].TileType != TileID.Platforms)
                            {
                                if (Main.tile[targetBlock.ToPoint()].HasTile)
                                    continue;
                            }

                            Color color = Color.White;

                            if (room.active)
                                color.A = (byte)MathHelper.Clamp(MathHelper.Lerp(0, 255f, room.roomTime / 60f), 0, 255f);
                            else
                                color.A = (byte)MathHelper.Lerp(255, 0, room.closedTime / 60f);

                            Vector2 drawPosition = targetBlock * 16f - Main.screenPosition - new Vector2(-16f * side, -16f * side);
                            float rotation = MathHelper.Pi + (MathHelper.Pi * side);

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);
                        }
                        for (float i = 0; i < room.RoomDimensions.Y; i++)
                        {
                            Vector2 targetBlock = room.RoomPosition + new Vector2(side * room.RoomDimensions.X - side, i);
                            if (Main.tile[targetBlock.ToPoint()].TileType != TileID.Platforms)
                            {
                                if (Main.tile[targetBlock.ToPoint()].HasTile)
                                    continue;
                            }

                            Color color = Color.White;

                            if (room.active)
                                color.A = (byte)MathHelper.Clamp(MathHelper.Lerp(0, 255f, room.roomTime / 60f), 0, 255f);
                            else
                                color.A = (byte)MathHelper.Lerp(255, 0, room.closedTime / 60f);

                            Vector2 drawPosition = targetBlock * 16f - Main.screenPosition - new Vector2(-16f * side, (16f) * (side - 1f));
                            float rotation = MathHelper.PiOver2 + (MathHelper.Pi * side);

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);
                        }
                    }
                    Main.spriteBatch.End();
                }
            }
        }
        /// <summary>
        /// Draw the first frame of each pending enemy's animation as an attempt to telegraph what is spawning there
        /// </summary>
        public void DrawPendingEnemies()
        {
            if (SpawnManager.pendingEnemies == null)
                return;
            if (!SpawnManager.pendingEnemies.Any())
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < SpawnManager.pendingEnemies.Count; i++)
            {
                PendingEnemy enemy = SpawnManager.pendingEnemies[i];
                Texture2D texture = TextureAssets.Npc[enemy.NPCType].Value;
                int frameCount = Main.npcFrameCount[enemy.NPCType];
                Color color = Color.HotPink * (0.75f * (1 - enemy.TelegraphDuration / 60f));
                Main.EntitySpriteDraw(texture, enemy.Position - Main.screenPosition, new Rectangle(0, 0, texture.Width, (int)(texture.Height / frameCount)), color, 0f, new Vector2(texture.Width / 2f, texture.Height / frameCount / 2f), 1f, SpriteEffects.None);
            }

            Main.spriteBatch.End();
        }
        public void UpdateHealingPulse()
        {
            if (healingPulses == null)
                healingPulses = new List<HealingPulse>();

            if (!healingPulses.Any())
                return;

            for (int p = 0; p < healingPulses.Count; p++)
            {
                HealingPulse pulse = healingPulses[p];
                if (pulse.Time == 30)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Volume = 0.15f });
                }
                if (pulse.Time == 0)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player player = Main.player[i];
                        if (player == null)
                            continue;
                        if (!player.active)
                            continue;

                        TerRoguelikePlayer modPlayer = player.GetModPlayer<TerRoguelikePlayer>();
                        modPlayer.ScaleableHeal((int)(player.statLifeMax2 / 2f));
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Volume = 0.75f });
                    }
                }
                pulse.Time--;
                healingPulses.RemoveAll(x => x.Time <= -30);
            }
        }
        public void DrawHealingPulse()
        {
            if (healingPulses == null)
                return;
            if (!healingPulses.Any())
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            for (int p = 0; p < healingPulses.Count; p++)
            {
                HealingPulse pulse = healingPulses[p];
                Texture2D telegraphBase = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/InvisibleProj").Value;

                float scale;
                float opacity;
                if (pulse.Time > 2)
                {
                    float interpolant = (pulse.Time - 2) / 28f;
                    scale = MathHelper.Lerp(0.01f, 2f, interpolant);
                    opacity = MathHelper.Lerp(0.5f, 0f, interpolant);
                }
                else
                {
                    float interpolant = Math.Abs(pulse.Time - 2) / 32f;
                    scale = MathHelper.Lerp(0.01f, 20f, interpolant);
                    opacity = MathHelper.Lerp(1f, 0f, interpolant);
                }
                    
                
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseOpacity(0.5f * opacity);
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseColor(Color.Lerp(Color.Green, Color.GreenYellow, 0.5f));
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseSecondaryColor(Color.LightGreen);
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseSaturation(scale);

                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].Apply();

                Vector2 drawPosition = pulse.Position - Main.screenPosition;
                Main.EntitySpriteDraw(telegraphBase, drawPosition, null, Color.White, 0, telegraphBase.Size() / 2f, scale * 156f, 0, 0);
            }
            Main.spriteBatch.End();
        }
        public void UpdateAttackPlanRocketBundles()
        {
            if (attackPlanRocketBundles == null)
                attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
            if (!attackPlanRocketBundles.Any())
                return;

            for (int i = 0; i < attackPlanRocketBundles.Count; i++)
            {
                AttackPlanRocketBundle bundle = attackPlanRocketBundles[i];

                if (!RoomList[bundle.SourceRoom].active)
                {
                    bundle.Time = -1;
                    continue;
                }

                if (bundle.Time % 12 == 0 && bundle.Count > 0)
                {
                    Projectile.NewProjectile(Projectile.GetSource_None(), bundle.Position + new Vector2(0, -32).RotatedBy(bundle.Rotation), (-Vector2.UnitY * 2.2f).RotatedBy(bundle.Rotation), ModContent.ProjectileType<PlanRocket>(), 100, 1f, bundle.Owner, -1);
                    bundle.Count--;
                    bundle.Rotation += MathHelper.PiOver4;
                }
                bundle.Time--;
            }
            attackPlanRocketBundles.RemoveAll(x => x.Time < 0);
        }
        public override void NetSend(BinaryWriter writer)
        {
            //Sorrowful attempt at any semblance of multiplayer compat
            writer.Write(TerRoguelikeWorld.IsTerRoguelikeWorld);
            List<byte> packageRoomLisIDs = new List<byte>();
            for (int i = 0; i < RoomList.Count; i++)
            {
                packageRoomLisIDs.Add((byte)RoomList[i].ID);
            }
            ReadOnlySpan<byte> sentRoomList = packageRoomLisIDs.ToArray();
            writer.Write(sentRoomList.Length);
            writer.Write(sentRoomList);
        }
        public override void NetReceive(BinaryReader reader)
        {
            if (obtainedRoomListFromServer)
                return;

            if (RoomList == null)
                RoomList = new List<Room>();

            TerRoguelikeWorld.IsTerRoguelikeWorld = reader.ReadBoolean();
            int roomListLength = reader.ReadInt32();
            byte[] recievedRoomIDs = reader.ReadBytes(roomListLength);
            for (int i = 0; i < recievedRoomIDs.Length; i++)
            {
                int roomID = (int)recievedRoomIDs[i];
                RoomList.Add(RoomID[roomID]);
                ResetRoomID(roomID);
            }
            obtainedRoomListFromServer = true;
        }
        public override void ClearWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                obtainedRoomListFromServer = false;
            else
                obtainedRoomListFromServer = true;
        }
    }
    public class HealingPulse
    {
        public HealingPulse(Vector2 position)
        {
            Position = position;
        }
        public Vector2 Position;
        public int Time = 30;
    }
    public class AttackPlanRocketBundle
    {
        public AttackPlanRocketBundle(Vector2 position, int count, int owner, int sourceRoom)
        {
            Position = position;
            Count = count;
            Owner = owner;
            Time = count * 12;
            SourceRoom = sourceRoom;
        }
        public Vector2 Position;
        public int Count;
        public int Owner;
        public int Time;
        public float Rotation = 0f;
        public int SourceRoom;
    }
}
