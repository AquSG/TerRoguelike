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
        public static List<int> FloorIDsInPlay;
        public static List<int> oldRoomDirections;
        public static int currentFloorGen;

        public static List<Room> RoomGenPool;


        //The ultimate worldgen function
        public static void GenerateRoomStructure()
        {
            List<int> stage0Floors = new List<int>()
            {
                0,
                2
            };
            currentFloorGen = stage0Floors[Main.rand.Next(stage0Floors.Count)];

            FloorIDsInPlay = new List<int>();
            RoomSystem.RoomList = new List<Room>();
            oldRoomDirections = new List<int>();
            RoomGenPool = RoomID.FindAll(x => true);

            int firstRoomID = FloorID[currentFloorGen].StartRoomID;
            int roomCount = 8;
            string mapKey = RoomID[firstRoomID].Key;
            var schematic = TileMaps[mapKey];

            int startpositionX = Main.maxTilesX / 32;
            int startpositionY = Main.maxTilesY / 2;

            Point placementPoint = new Point(startpositionX, startpositionY);

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            var firstRoom = RoomID[firstRoomID];
            firstRoom.RoomPosition = placementPoint.ToVector2();
            firstRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(firstRoom);
            RoomGenPool.Remove(firstRoom);
            
            PlaceSchematic(mapKey, placementPoint, anchorType);

            PlaceRoom(roomCount, firstRoom);
        }
        public static string GetFloorKey()
        {
            switch (currentFloorGen)
            {
                case 0:
                    return "Base";
                case 1:
                    return "Crimson";
                case 2:
                    return "Forest";
                case 3:
                    return "Corrupt";
                case 4:
                    return "Snow";
                case 5:
                    return "Desert";
                case 6:
                    return "Jungle";
                case 7:
                    return "Hell";
                case 8:
                    return "Dungeon";
                case 9:
                    return "Temple";
                default:
                    return null;
            }
        }
        public static void PlaceBossRoom(Room previousRoom)
        {
            var selectedRoom = RoomID[FloorID[currentFloorGen].BossRoomIDs[Main.rand.Next(FloorID[currentFloorGen].BossRoomIDs.Count)]];
            if (selectedRoom.HasTransition)
            {
                for (int i = 0; i < RoomGenPool.Count; i++)
                {
                    Room room = RoomGenPool[i];
                    if (!room.Key.Contains("Transition"))
                        continue;
                    if (room.Key.Contains(selectedRoom.Key))
                    {
                        PlaceTransitionRoom(room, previousRoom);
                        previousRoom = room;
                        RoomGenPool.Remove(room);
                        break;
                    }
                }
            }

            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            selectedRoom.RoomDimensions = schematicSize;
            Point placementPoint;

            switch (previousRoom.TransitionDirection)
            {
                case 0:
                    placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - selectedRoom.RoomDimensions.Y));
                    break;
                case 1:
                    placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - selectedRoom.RoomDimensions.X), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - 1));
                    if (placementPoint.X < (int)previousRoom.RoomPosition.X)
                        placementPoint.X = (int)previousRoom.RoomPosition.X;
                    break;
                case 2:
                    placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - selectedRoom.RoomDimensions.X), (int)(previousRoom.RoomPosition.Y - selectedRoom.RoomDimensions.Y + 1));
                    if (placementPoint.X < (int)previousRoom.RoomPosition.X)
                        placementPoint.X = (int)previousRoom.RoomPosition.X;
                    break;
                default:
                    placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - selectedRoom.RoomDimensions.Y));
                    break;

            }
            if (placementPoint.Y + selectedRoom.RoomDimensions.Y < (int)previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y)
                placementPoint.Y = (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - selectedRoom.RoomDimensions.Y);

            SchematicAnchor anchorType = SchematicAnchor.TopLeft;
            selectedRoom.RoomPosition = placementPoint.ToVector2();

            RoomSystem.NewRoom(selectedRoom);
            RoomGenPool.Remove(selectedRoom);
            PlaceSchematic(mapKey, placementPoint, anchorType);

            FloorIDsInPlay.Add(currentFloorGen);
            ChooseNextFloor();
            if (currentFloorGen == -1)
                return;

            GenerateNextFloor(selectedRoom);
        }
        public static void PlaceTransitionRoom(Room transitionRoom, Room previousRoom)
        {
            string mapKey = transitionRoom.Key;
            var schematic = TileMaps[mapKey];

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            transitionRoom.RoomDimensions = schematicSize;

            Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - transitionRoom.RoomDimensions.Y));

            if (placementPoint.Y + transitionRoom.RoomDimensions.Y < (int)previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y)
                placementPoint.Y = (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - transitionRoom.RoomDimensions.Y);

            SchematicAnchor anchorType = SchematicAnchor.TopLeft;
            transitionRoom.RoomPosition = placementPoint.ToVector2();

            RoomSystem.NewRoom(transitionRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);
        }
        public static void PlaceRoom(int roomCount, Room previousRoom)
        {
            roomCount--;
            if (roomCount == 0)
            {
                PlaceBossRoom(previousRoom);
            }
            else
            {
                Room placedRoom;
                bool anyRight = false;
                bool anyDown = false;
                bool anyUp = false;

                CheckDirAvailability(ref anyRight, ref anyDown, ref anyUp);

                List<int> directionsAvailable = new List<int>();
                if (previousRoom.CanExitRight && anyRight)
                {
                    directionsAvailable.Add(0);
                }
                    
                if (previousRoom.CanExitDown && anyDown)
                {
                    if (oldRoomDirections.Count > 2)
                    {
                        if (oldRoomDirections[oldRoomDirections.Count - 3] != 2)
                            directionsAvailable.Add(1);
                    }
                    else
                        directionsAvailable.Add(1);
                }
                    
                if (previousRoom.CanExitUp && anyUp)
                {
                    if (oldRoomDirections.Count > 2)
                    {
                        if (oldRoomDirections[oldRoomDirections.Count - 3] != 1)
                            directionsAvailable.Add(2);
                    }
                    else
                        directionsAvailable.Add(2);
                }

                if (!directionsAvailable.Any() || directionsAvailable.Count == 0) //failsafe. if no directions available, just plop down the boss room
                {
                    PlaceRoom(1, previousRoom);
                    return;
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
            Room selectedRoom = SelectRoom(0);
            
            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            selectedRoom.RoomDimensions = schematicSize;

            Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - selectedRoom.RoomDimensions.Y));

            if (placementPoint.Y + selectedRoom.RoomDimensions.Y < (int)previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y)
                placementPoint.Y = (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - selectedRoom.RoomDimensions.Y);

            SchematicAnchor anchorType = SchematicAnchor.TopLeft;
            selectedRoom.RoomPosition = placementPoint.ToVector2();
            
            RoomSystem.NewRoom(selectedRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            return selectedRoom;
        }
        public static Room PlaceDown(Room previousRoom)
        {
            Room selectedRoom = SelectRoom(1);

            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            selectedRoom.RoomDimensions = schematicSize;

            Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - selectedRoom.RoomDimensions.X), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - 1));
            if (placementPoint.X < (int)previousRoom.RoomPosition.X)
                placementPoint.X = (int)previousRoom.RoomPosition.X;

            SchematicAnchor anchorType = SchematicAnchor.TopLeft;
            selectedRoom.RoomPosition = placementPoint.ToVector2();
            
            RoomSystem.NewRoom(selectedRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            return selectedRoom;
        }
        public static Room PlaceUp(Room previousRoom)
        {
            Room selectedRoom = SelectRoom(2);

            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            selectedRoom.RoomDimensions = schematicSize;

            Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - selectedRoom.RoomDimensions.X), (int)(previousRoom.RoomPosition.Y - selectedRoom.RoomDimensions.Y + 1));
            if (placementPoint.X < (int)previousRoom.RoomPosition.X)
                placementPoint.X = (int)previousRoom.RoomPosition.X;

            SchematicAnchor anchorType = SchematicAnchor.TopLeft;
            selectedRoom.RoomPosition = placementPoint.ToVector2();
            
            RoomSystem.NewRoom(selectedRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            return selectedRoom;
        }
        public static void GenerateNextFloor(Room previousRoom)
        {
            oldRoomDirections.Clear();

            Room floorStartingRoom = RoomID[FloorID[currentFloorGen].StartRoomID];
            int roomCount = 8;

            string mapKey = floorStartingRoom.Key;
            var schematic = TileMaps[mapKey];

            Point placementPoint = new Point((int)previousRoom.RoomPosition.X + 200, FloorID[currentFloorGen].InHell ? Main.maxTilesY -  150 : Main.maxTilesY / 2);

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            
            floorStartingRoom.RoomPosition = placementPoint.ToVector2();
            floorStartingRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(floorStartingRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            PlaceRoom(roomCount, floorStartingRoom);
        }
        public static void ChooseNextFloor()
        {
            int currentStage = FloorID[currentFloorGen].Stage;
            List<Floor> nextFloors = FloorID.FindAll(x => x.Stage == currentStage + 1);
            if (nextFloors.Any())
            {
                Floor nextFloor = nextFloors[Main.rand.Next(nextFloors.Count)];
                currentFloorGen = nextFloor.FloorID;
            }
            else
            {
                currentFloorGen = -1;
            }
        }
        public static Room SelectRoom(int direction)
        {
            if (!RoomGenPool.Any())
                return RoomID[0];

            string floorKey = GetFloorKey();

            List<Room> roomSelection = new List<Room>();
            
            for (int i = 0; i < RoomGenPool.Count; i++)
            {
                Room room = RoomGenPool[i];
                if (!room.Key.Contains(floorKey))
                    continue;
                if (room.Key.Contains("Boss"))
                    continue;
                if (room.Key.Contains("Start"))
                    continue;
                if (room.Key.Contains("Transition"))
                    continue;

                bool containsDir = false;
                if (direction == 1 && room.Key.Contains("Down"))
                    containsDir = true;
                else if (direction == 2 && room.Key.Contains("Up"))
                    containsDir = true;
                else if (direction == 0 && !room.Key.Contains("Down") && !room.Key.Contains("Up"))
                    containsDir = true;

                if (containsDir)
                {
                    roomSelection.Add(room);
                }
            }
            if (roomSelection.Any())
            {
                Room selectedRoom = roomSelection[Main.rand.Next(roomSelection.Count)];
                RoomGenPool.Remove(selectedRoom);
                return selectedRoom;
            }

            return RoomID[0];
        }
        public static void CheckDirAvailability(ref bool anyRight, ref bool anyDown, ref bool anyUp)
        {
            string floorKey = GetFloorKey();
            for (int i = 0; i < RoomGenPool.Count; i++)
            {
                Room room = RoomGenPool[i];
                if (!room.Key.Contains(floorKey))
                    continue;
                if (room.Key.Contains("Boss"))
                    continue;
                if (room.Key.Contains("Start"))
                    continue;
                if (room.Key.Contains("Transition"))
                    continue;

                if (room.Key.Contains("Down"))
                {
                    anyDown = true;
                }
                else if (room.Key.Contains("Up"))
                {
                    anyUp = true;
                }
                else
                    anyRight = true;
            }
        }
    }
}
