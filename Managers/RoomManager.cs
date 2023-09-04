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
        public static List<Room> BaseRoomRight;
        public static List<Room> BaseRoomDown;
        public static List<Room> BaseRoomUp;
        public static List<int> oldRoomDirections;
        public static void GenerateRoomStructure()
        {
            RoomSystem.RoomList = new List<Room>();
            oldRoomDirections = new List<int>();
            SetAllBaseRoomIDs();

            int roomCount = 9;
            string mapKey = RoomID[0].Key;
            var schematic = TileMaps[mapKey];

            int startpositionX = Main.maxTilesX / 8;
            int startpositionY = Main.maxTilesY / 2;

            Point placementPoint = new Point(startpositionX, startpositionY);

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            DefaultRoom defaultRoom = new DefaultRoom();
            defaultRoom.RoomPosition = placementPoint.ToVector2();
            defaultRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(defaultRoom);
            
            PlaceSchematic(mapKey, placementPoint, anchorType);

            PlaceRoom(roomCount, defaultRoom);
        }
        public static void PlaceRoom(int roomCount, Room previousRoom)
        {
            roomCount--;
            if (roomCount == 0)
            {
                var selectedRoom = RoomID[5];
                string mapKey = selectedRoom.Key;
                var schematic = TileMaps[mapKey];

                Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)previousRoom.RoomPosition.Y);
                Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
                SchematicAnchor anchorType = SchematicAnchor.TopLeft;

                selectedRoom.RoomPosition = placementPoint.ToVector2();
                selectedRoom.RoomDimensions = schematicSize;
                RoomSystem.NewRoom(selectedRoom);

                PlaceSchematic(mapKey, placementPoint, anchorType);
            }
            else
            {
                Room placedRoom;

                List<int> directionsAvailable = new List<int>();
                if (previousRoom.CanExitRight && BaseRoomRight.Any())
                {
                    directionsAvailable.Add(0);
                }
                    
                if (previousRoom.CanExitDown && BaseRoomDown.Any())
                {
                    if (oldRoomDirections.Count > 2)
                    {
                        if (oldRoomDirections[oldRoomDirections.Count - 2] != 1)
                            directionsAvailable.Add(1);
                    }
                    else
                        directionsAvailable.Add(1);
                }
                    
                if (previousRoom.CanExitUp && BaseRoomUp.Any())
                {
                    if (oldRoomDirections.Count > 2)
                    {
                        if (oldRoomDirections[oldRoomDirections.Count - 2] != 2)
                            directionsAvailable.Add(2);
                    }
                    else
                        directionsAvailable.Add(2);
                }

                int chosenDirection = directionsAvailable[Main.rand.Next(directionsAvailable.Count)];

                if (chosenDirection == 1)
                {
                    placedRoom = PlaceDown(previousRoom);
                    oldRoomDirections.Add(chosenDirection);
                }
                    
                else if (chosenDirection == 2)
                {
                    placedRoom = PlaceUp(previousRoom);
                    oldRoomDirections.Add(chosenDirection);
                }

                else
                {
                    placedRoom = PlaceRight(previousRoom);
                    oldRoomDirections.Add(0);
                }
                    

                PlaceRoom(roomCount, placedRoom);
            }
        }
        public static Room PlaceRight(Room previousRoom)
        {
            var selectedRoom = BaseRoomRight[Main.rand.Next(BaseRoomRight.Count)];
            BaseRoomRight.Remove(selectedRoom);
            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)previousRoom.RoomPosition.Y);
            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            selectedRoom.RoomPosition = placementPoint.ToVector2();
            selectedRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(selectedRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            return selectedRoom;
        }
        public static Room PlaceDown(Room previousRoom)
        {
            var selectedRoom = BaseRoomDown[Main.rand.Next(BaseRoomDown.Count)];
            BaseRoomDown.Remove(selectedRoom);
            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Point placementPoint = new Point((int)previousRoom.RoomPosition.X, (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - 1));
            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            selectedRoom.RoomPosition = placementPoint.ToVector2();
            selectedRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(selectedRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            return selectedRoom;
        }
        public static Room PlaceUp(Room previousRoom)
        {
            var selectedRoom = BaseRoomUp[Main.rand.Next(BaseRoomUp.Count)];
            BaseRoomUp.Remove(selectedRoom);
            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Point placementPoint = new Point((int)previousRoom.RoomPosition.X, (int)(previousRoom.RoomPosition.Y - previousRoom.RoomDimensions.Y + 1));
            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            selectedRoom.RoomPosition = placementPoint.ToVector2();
            selectedRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(selectedRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            return selectedRoom;
        }
        public static void SetAllBaseRoomIDs()
        {
            BaseRoomRight = new List<Room>();
            BaseRoomDown = new List<Room>();
            BaseRoomUp = new List<Room>();
            SetBaseRoomRightIDs();
            SetBaseRoomDownIDs();
            SetBaseRoomUpIDs();
        }
        public static void SetBaseRoomRightIDs()
        {
            BaseRoomRight.Add(RoomID[1]);
            BaseRoomRight.Add(RoomID[2]);
            BaseRoomRight.Add(RoomID[3]);
            BaseRoomRight.Add(RoomID[4]);
            BaseRoomRight.Add(RoomID[7]);
            BaseRoomRight.Add(RoomID[10]);
        }
        public static void SetBaseRoomDownIDs()
        {
            BaseRoomDown.Add(RoomID[8]);
            BaseRoomDown.Add(RoomID[11]);
        }
        public static void SetBaseRoomUpIDs()
        {
            BaseRoomUp.Add(RoomID[6]);
            BaseRoomUp.Add(RoomID[9]);
            BaseRoomUp.Add(RoomID[12]);
        }
    }
}
