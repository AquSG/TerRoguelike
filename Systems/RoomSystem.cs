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
using TerRoguelike.Systems;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;

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
                if (!room.active)
                    continue;

                bool roomXcheck = Main.player[Main.myPlayer].Center.X > room.RoomPosition.X * 16f && Main.player[Main.myPlayer].Center.X < (room.RoomPosition.X + room.RoomDimensions.X) * 16f;
                bool roomYcheck = Main.player[Main.myPlayer].Center.Y > room.RoomPosition.Y * 16f && Main.player[Main.myPlayer].Center.Y < (room.RoomPosition.Y + room.RoomDimensions.Y) * 16f;
                if (roomXcheck && roomYcheck)
                    room.awake = true;

                room.myRoom = loopCount;
                room.Update();

            }
            //RoomList.RemoveAll(room => !room.active);

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
            RoomList = null;
        }
        public override void LoadWorldData(TagCompound tag)
        {
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
                loopcount++;
            }
        }
    }
}
