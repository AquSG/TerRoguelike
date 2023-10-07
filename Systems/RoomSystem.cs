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

namespace TerRoguelike.Systems
{
    public class RoomSystem : ModSystem
    {
        public static List<Room> RoomList;
        public static List<HealingPulse> healingPulses = new List<HealingPulse>();
        public static List<AttackPlanRocketBundle> attackPlanRocketBundles = new List<AttackPlanRocketBundle>();
        public static void NewRoom(Room room)
        {
            RoomList.Add(room);
        }
        public override void PostUpdateWorld()
        {
            SpawnManager.UpdateSpawnManager();
            UpdateHealingPulse();
            UpdateAttackPlanRocketBundles();

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

                var player = Main.player[Main.myPlayer];

                bool roomXcheck = player.Center.X - (player.width / 2f) > (room.RoomPosition.X + 1f) * 16f - 1f && player.Center.X + (player.width / 2f) < (room.RoomPosition.X - 1f + room.RoomDimensions.X) * 16f + 1f;
                bool roomYcheck = player.Center.Y - (player.height / 2f) > (room.RoomPosition.Y + 1f) * 16f && player.Center.Y + (player.height / 2f) < (room.RoomPosition.Y - (15f / 16f) + room.RoomDimensions.Y) * 16f;
                if (roomXcheck && roomYcheck)
                {
                    if (room.AssociatedFloor != -1)
                        player.GetModPlayer<TerRoguelikePlayer>().currentFloor = FloorID[room.AssociatedFloor];

                    room.awake = true;
                    bool teleportCheck = room.closedTime > 180 && room.IsBossRoom && player.position.X + player.width >= ((room.RoomPosition.X + room.RoomDimensions.X) * 16f) - 22f;
                    if (teleportCheck)
                    {
                        int nextFloorID = player.GetModPlayer<TerRoguelikePlayer>().currentFloor.Stage + 1;
                        if (nextFloorID >= RoomManager.FloorIDsInPlay.Count)
                            nextFloorID = 0;

                        var nextFloor = FloorID[RoomManager.FloorIDsInPlay[nextFloorID]];
                        var targetRoom = RoomID[nextFloor.StartRoomID];
                        player.Center = (targetRoom.RoomPosition + (targetRoom.RoomDimensions / 2f)) * 16f;
                        player.GetModPlayer<TerRoguelikePlayer>().currentFloor = nextFloor;
                    }

                    if (room.closedTime == 1)
                    {
                        player.statLife = player.statLifeMax2;
                    }
                }

                room.myRoom = loopCount;
                room.Update();
            }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            var isTerRoguelikeWorld = TerRoguelikeWorld.IsTerRoguelikeWorld;
            tag["isTerRoguelikeWorld"] = isTerRoguelikeWorld;

            if (RoomList == null)
                return;

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
            var isTerRoguelikeWorld = tag.GetBool("isTerRoguelikeWorld");
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
            TerRoguelikeWorld.IsTerRoguelikeWorld = isTerRoguelikeWorld;
        }
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
