using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Rooms;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Microsoft.Xna.Framework;
using TerRoguelike.Schematics;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Managers
{
    public class RoomManager
    {
        public static List<Room> BaseRooms = new List<Room>();
        public static void GenerateRoomStructure()
        {
            SetBaseRoomIDs();
            int roomCount = 5;
            string mapKey = RoomID[0].Key;
            var schematic = TileMaps[mapKey];

            int startpositionX = Main.maxTilesX / 2;
            int startpositionY = Main.maxTilesY / 2;

            Point placementPoint = new Point(startpositionX, startpositionY);

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            DefaultRoom defaultRoom = new DefaultRoom();
            defaultRoom.RoomPosition = placementPoint.ToVector2();
            defaultRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(defaultRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            PlaceRoomRight(roomCount, defaultRoom);
        }
        public static void PlaceRoomRight(int roomCount, Room previousRoom)
        {
            roomCount--;
            if (roomCount == 0)
                return;

            var selectedRoom = BaseRooms[Main.rand.Next(BaseRooms.Count)];
            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X), (int)previousRoom.RoomPosition.Y);
            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            selectedRoom.RoomPosition = placementPoint.ToVector2();
            selectedRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(selectedRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            PlaceRoomRight(roomCount, selectedRoom);
        }
        public static void SetBaseRoomIDs()
        {
            BaseRooms.Add(RoomID[1]);
            BaseRooms.Add(RoomID[2]);
        }
    }
}
