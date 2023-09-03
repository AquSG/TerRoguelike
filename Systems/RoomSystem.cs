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
using Terraria.ID;
using TerRoguelike.Systems;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;

namespace TerRoguelike.Systems
{
    public class RoomSystem : ModSystem
    {
        public static List<Room> RoomList;
        public static void NewRoom(Room room)
        {
            RoomList.Add(room);
        }
        public override void PostUpdateWorld()
        {
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

                bool roomXcheck = Main.player[Main.myPlayer].Center.X - (Main.player[Main.myPlayer].width / 2f) > (room.RoomPosition.X + 1f) * 16f && Main.player[Main.myPlayer].Center.X + (Main.player[Main.myPlayer].width / 2f) < (room.RoomPosition.X - 1f + room.RoomDimensions.X) * 16f;
                bool roomYcheck = Main.player[Main.myPlayer].Center.Y - (Main.player[Main.myPlayer].height / 2f) > (room.RoomPosition.Y + 1f) * 16f && Main.player[Main.myPlayer].Center.Y + (Main.player[Main.myPlayer].height / 2f) < (room.RoomPosition.Y - (15f/16f) + room.RoomDimensions.Y) * 16f;
                if (roomXcheck && roomYcheck)
                    room.awake = true;

                room.myRoom = loopCount;
                room.Update();
            }
            if (RoomList.All(check => !check.active && check.closedTime > 60))
                RoomList.RemoveAll(room => !room.active);

        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (RoomList == null)
                return;

            var roomIDs = new List<int>();
            var roomPositions = new List<Vector2>();

            foreach (Room room in RoomList)
            {
                if (room == null)
                    continue;

                roomIDs.Add(room.ID);
                roomPositions.Add(room.RoomPosition);
            }
            tag["roomIDs"] = roomIDs;
            tag["roomPositions"] = roomPositions;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            //ReloadRoomIDs();
            RoomList = new List<Room>();
            int loopcount = 0;
            var roomIDs = tag.GetList<int>("roomIDs");
            var roomPositions = tag.GetList<Vector2>("roomPositions");
            foreach (int id in roomIDs)
            {
                if (id == -1)
                    continue;
                
                RoomList.Add(RoomID[id]);
                RoomList[loopcount].RoomPosition = roomPositions[loopcount];
                ResetRoomID(id);
                loopcount++;
            }
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
            
            Texture2D lightTexture = ModContent.Request<Texture2D>("TerRoguelike/Tiles/TemporaryBlock").Value;
            foreach (Room room in RoomList)
            {
                if (room == null)
                    continue;

                if (!room.awake || room.closedTime > 60)
                    continue;

                if (room.wallActive)
                {
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

                            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);

                            Main.spriteBatch.End();
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

                            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                            Main.EntitySpriteDraw(lightTexture, drawPosition, null, color, rotation, lightTexture.Size(), 1f, SpriteEffects.None);

                            Main.spriteBatch.End();
                        }
                    }
                }
            }
            
        }
    }
}
