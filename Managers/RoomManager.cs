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
using TerRoguelike.Floors;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.TerRoguelikeWorldManagementSystem;

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
            currentFloorGen = GenDebugWorld ? 0 : stage0Floors[Main.rand.Next(stage0Floors.Count)];

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
            if (GenDebugWorld)
            {
                startpositionY = Main.maxTilesY / 12;
            }

            Point placementPoint = new Point(startpositionX, startpositionY);

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            var firstRoom = RoomID[firstRoomID];
            firstRoom.RoomPosition = placementPoint.ToVector2();
            firstRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(firstRoom);
            RoomGenPool.Remove(firstRoom);
            
            PlaceSchematic(mapKey, placementPoint, anchorType);

            if (GenDebugWorld)
                roomCount = 200;

            PlaceRoom(roomCount, firstRoom);

            var sanctuaryRoom = RoomID[FloorID[FloorDict["Sanctuary"]].StartRoomID];
            schematic = TileMaps[sanctuaryRoom.Key];
            schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));

            sanctuaryRoom.RoomPosition = (placementPoint + new Point(0, GenDebugWorld ? -60 : -300)).ToVector2();
            sanctuaryRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(sanctuaryRoom);
            RoomGenPool.Remove(sanctuaryRoom);

            PlaceSchematic(sanctuaryRoom.Key, sanctuaryRoom.RoomPosition.ToPoint(), anchorType);
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
                case 10:
                    return "Lunar";
                case 11:
                    return "Sanctuary";
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

            if (GenDebugWorld && !selectedRoom.HasTransition)
                placementPoint.X += 3;

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

            if (GenDebugWorld)
                placementPoint.X += 3;

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
                if ((previousRoom.CanExitRight || GenDebugWorld) && anyRight)
                {
                    directionsAvailable.Add(0);
                }
                    
                if ((previousRoom.CanExitDown || GenDebugWorld) && anyDown)
                {
                    if (oldRoomDirections.Count > 2)
                    {
                        if (oldRoomDirections[oldRoomDirections.Count - 3] != 2)
                            directionsAvailable.Add(1);
                    }
                    else
                        directionsAvailable.Add(1);
                }
                    
                if ((previousRoom.CanExitUp || GenDebugWorld) && anyUp)
                {
                    if (oldRoomDirections.Count > 2)
                    {
                        if (oldRoomDirections[oldRoomDirections.Count - 3] != 1)
                            directionsAvailable.Add(2);
                    }
                    else
                        directionsAvailable.Add(2);
                }

                if (directionsAvailable.Count == 0) //failsafe. if no directions available, just plop down the boss room
                {
                    PlaceRoom(1, previousRoom);
                    return;
                }
                    
                int chosenDirection = GenDebugWorld ? 0 : directionsAvailable[Main.rand.Next(directionsAvailable.Count)];

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

                if (placedRoom == RoomID[0])
                    roomCount = 1;

                PlaceRoom(roomCount, placedRoom);
            }
        }
        public static Room PlaceRight(Room previousRoom)
        {
            Room selectedRoom = SelectRoom(0);
            if (selectedRoom == RoomID[0])
            {
                return selectedRoom;
            }
            
            string mapKey = selectedRoom.Key;
            var schematic = TileMaps[mapKey];

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            selectedRoom.RoomDimensions = schematicSize;

            Point placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - selectedRoom.RoomDimensions.Y));

            if (GenDebugWorld)
                placementPoint.X += 3;

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
            if (GenDebugWorld)
                roomCount = 200;

            string mapKey = floorStartingRoom.Key;
            var schematic = TileMaps[mapKey];

            Point placementPoint = new Point((int)previousRoom.RoomPosition.X + 200, Main.maxTilesY / 2);

            if (GenDebugWorld)
            {
                placementPoint.X = Main.maxTilesX / 32;
                placementPoint.Y = Main.maxTilesY / 12 * (currentFloorGen + (currentFloorGen > FloorDict["Hell"] ? 0 : 1));
            }
            if (FloorID[currentFloorGen].InHell)
                placementPoint.Y = Main.maxTilesY - 150;

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            
            floorStartingRoom.RoomPosition = placementPoint.ToVector2();
            floorStartingRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(floorStartingRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            if (currentFloorGen == 10) // lunar floor ID
            {
                PlaceFinalFloor(floorStartingRoom);
                return;
            }

            PlaceRoom(roomCount, floorStartingRoom);
        }

        public static void PlaceFinalFloor(Room startRoom)
        {
            FloorIDsInPlay.Add(currentFloorGen);
            Room nextRoom = RoomID[RoomDict["LunarHallRoom1"]];

            string mapKey = nextRoom.Key;
            var schematic = TileMaps[mapKey];

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            nextRoom.RoomDimensions = schematicSize;

            Point placementPoint = new Point((int)(startRoom.RoomPosition.X + startRoom.RoomDimensions.X - 1), (int)(startRoom.RoomPosition.Y + startRoom.RoomDimensions.Y - nextRoom.RoomDimensions.Y));

            if (placementPoint.Y + nextRoom.RoomDimensions.Y < (int)startRoom.RoomPosition.Y + startRoom.RoomDimensions.Y)
                placementPoint.Y = (int)(startRoom.RoomPosition.Y + startRoom.RoomDimensions.Y - nextRoom.RoomDimensions.Y);

            SchematicAnchor anchorType = SchematicAnchor.TopLeft;
            nextRoom.RoomPosition = placementPoint.ToVector2();

            RoomSystem.NewRoom(nextRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            Room previousRoom = nextRoom;
            nextRoom = RoomID[RoomDict["LunarHallRoom2"]];

            mapKey = nextRoom.Key;
            schematic = TileMaps[mapKey];

            schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            nextRoom.RoomDimensions = schematicSize;

            placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - nextRoom.RoomDimensions.Y));

            if (placementPoint.Y + nextRoom.RoomDimensions.Y < (int)previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y)
                placementPoint.Y = (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - nextRoom.RoomDimensions.Y);

            anchorType = SchematicAnchor.TopLeft;
            nextRoom.RoomPosition = placementPoint.ToVector2();

            RoomSystem.NewRoom(nextRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            previousRoom = nextRoom;
            Room bossRoom = RoomID[RoomDict["LunarBossRoom1"]];
            nextRoom = RoomID[RoomDict["LunarBossRoom1"]];

            mapKey = nextRoom.Key;
            schematic = TileMaps[mapKey];

            schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            nextRoom.RoomDimensions = schematicSize;

            placementPoint = new Point((int)(previousRoom.RoomPosition.X + previousRoom.RoomDimensions.X - 1), (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - nextRoom.RoomDimensions.Y));

            if (placementPoint.Y + nextRoom.RoomDimensions.Y < (int)previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y)
                placementPoint.Y = (int)(previousRoom.RoomPosition.Y + previousRoom.RoomDimensions.Y - nextRoom.RoomDimensions.Y);

            anchorType = SchematicAnchor.TopLeft;
            nextRoom.RoomPosition = placementPoint.ToVector2();

            RoomSystem.NewRoom(nextRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        nextRoom = RoomID[RoomDict["LunarPillarRoomTopLeft"]];
                        break;
                    case 1:
                        nextRoom = RoomID[RoomDict["LunarPillarRoomTopRight"]];
                        break;
                    case 2:
                        nextRoom = RoomID[RoomDict["LunarPillarRoomBottomLeft"]];
                        break;
                    case 3:
                        nextRoom = RoomID[RoomDict["LunarPillarRoomBottomRight"]];
                        break;
                }

                mapKey = nextRoom.Key;
                schematic = TileMaps[mapKey];

                schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
                nextRoom.RoomDimensions = schematicSize;

                if (i < 2)
                {
                    if (i % 2 == 0)
                    {
                        placementPoint = new Point((int)(bossRoom.RoomPosition.X + (bossRoom.RoomDimensions.X * 0.5f) - nextRoom.RoomDimensions.X), (int)(bossRoom.RoomPosition.Y - nextRoom.RoomDimensions.Y + 1));
                    }
                    else
                    {
                        placementPoint = new Point((int)(bossRoom.RoomPosition.X + (bossRoom.RoomDimensions.X * 0.5f)), (int)(bossRoom.RoomPosition.Y - nextRoom.RoomDimensions.Y + 1));
                    }

                    anchorType = SchematicAnchor.TopLeft;
                    nextRoom.RoomPosition = placementPoint.ToVector2();

                    RoomSystem.NewRoom(nextRoom);

                    PlaceSchematic(mapKey, placementPoint, anchorType);
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        placementPoint = new Point((int)(bossRoom.RoomPosition.X + (bossRoom.RoomDimensions.X * 0.5f) - nextRoom.RoomDimensions.X), (int)(bossRoom.RoomPosition.Y + bossRoom.RoomDimensions.Y - 1));
                    }
                    else
                    {
                        placementPoint = new Point((int)(bossRoom.RoomPosition.X + (bossRoom.RoomDimensions.X * 0.5f)), (int)(bossRoom.RoomPosition.Y + bossRoom.RoomDimensions.Y - 1));
                    }
                    

                    anchorType = SchematicAnchor.TopLeft;
                    nextRoom.RoomPosition = placementPoint.ToVector2();

                    RoomSystem.NewRoom(nextRoom);

                    PlaceSchematic(mapKey, placementPoint, anchorType);
                }
            }
            if (GenDebugWorld)
            {
                ChooseNextFloor();
                if (currentFloorGen != -1)
                {
                    GenerateNextFloor(previousRoom);
                }
            }
        }
        public static void ChooseNextFloor()
        {
            if (GenDebugWorld)
            {
                currentFloorGen++;
                if (currentFloorGen == FloorDict["Sanctuary"])
                    currentFloorGen++;
                if (currentFloorGen >= FloorID.Count)
                    currentFloorGen = -1;
                return;
            }
            int currentStage = FloorID[currentFloorGen].Stage;
            List<Floor> nextFloors = FloorID.FindAll(x => x.Stage == currentStage + 1);
            if (nextFloors.Count > 0)
            {
                Floor nextFloor = nextFloors[Main.rand.Next(nextFloors.Count)];
                currentFloorGen = nextFloor.ID;
            }
            else
            {
                currentFloorGen = -1;
            }
        }
        public static Room SelectRoom(int direction)
        {
            if (RoomGenPool.Count == 0)
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

                bool containsDir = GenDebugWorld;

                if (direction == 1 && room.Key.Contains("Down"))
                    containsDir = true;
                else if (direction == 2 && room.Key.Contains("Up"))
                    containsDir = true;
                else if (direction == 0 && !room.Key.Contains("Down") && !room.Key.Contains("Up"))
                    containsDir = true;

                if (containsDir)
                {
                    roomSelection.Add(room);
                    //if (GenDebugWorld)
                        //break;
                }
            }
            if (roomSelection.Count > 0)
            {
                Room selectedRoom = roomSelection[Main.rand.Next(roomSelection.Count)];
                if (GenDebugWorld)
                    selectedRoom = roomSelection[0];
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
