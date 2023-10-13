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
        public static int currentFloor;

        public static List<Room> BaseRoomRight;
        public static List<Room> BaseRoomDown;
        public static List<Room> BaseRoomUp;
        public static List<Room> CrimsonRoomRight;
        public static List<Room> CrimsonRoomDown;
        public static List<Room> CrimsonRoomUp;
        public static List<Room> ForestRoomRight;
        public static List<Room> ForestRoomDown;
        public static List<Room> ForestRoomUp;

        
        //The ultimate worldgen function
        public static void GenerateRoomStructure()
        {
            List<int> floor1Floors = new List<int>()
            {
                0,
                2
            };
            currentFloor = floor1Floors[Main.rand.Next(floor1Floors.Count)];

            FloorIDsInPlay = new List<int>();
            RoomSystem.RoomList = new List<Room>();
            oldRoomDirections = new List<int>();
            SetAllRoomIDs();

            int firstRoomID = FloorID[currentFloor].StartRoomID;
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
            
            PlaceSchematic(mapKey, placementPoint, anchorType);

            PlaceRoom(roomCount, firstRoom);
        }
        public static void PlaceRoom(int roomCount, Room previousRoom)
        {
            roomCount--;
            if (roomCount == 0)
            {
                var selectedRoom = RoomID[FloorID[currentFloor].BossRoomIDs[Main.rand.Next(FloorID[currentFloor].BossRoomIDs.Count)]];
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

                FloorIDsInPlay.Add(currentFloor);
                switch (currentFloor)
                {
                    case 0:
                        currentFloor = 1;
                        break;
                    case 1:
                        return;
                    case 2:
                        currentFloor = 1;
                        break;
                }

                GenerateNextFloor(selectedRoom);
            }
            else
            {
                Room placedRoom;
                bool anyRight = true;
                bool anyDown = true;
                bool anyUp = true;

                switch (currentFloor)
                {
                    case 0:
                        if (!BaseRoomRight.Any())
                            anyRight = false;
                        if (!BaseRoomDown.Any())
                            anyDown = false;
                        if (!BaseRoomUp.Any())
                            anyUp = false;
                        break;
                    case 1:
                        if (!CrimsonRoomRight.Any())
                            anyRight = false;
                        if (!CrimsonRoomDown.Any())
                            anyDown = false;
                        if (!CrimsonRoomUp.Any())
                            anyUp = false;
                        break;
                    case 2:
                        if (!ForestRoomRight.Any())
                            anyRight = false;
                        if (!ForestRoomDown.Any())
                            anyDown = false;
                        if (!ForestRoomUp.Any())
                            anyUp = false;
                        break;
                }

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

                if (!directionsAvailable.Any() || directionsAvailable.Count == 0)
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
            Room selectedRoom = RoomID[0];
            switch (currentFloor)
            {
                case 0:
                    selectedRoom = BaseRoomRight[Main.rand.Next(BaseRoomRight.Count)];
                    BaseRoomRight.Remove(selectedRoom);
                    break;
                case 1:
                    selectedRoom = CrimsonRoomRight[Main.rand.Next(CrimsonRoomRight.Count)];
                    CrimsonRoomRight.Remove(selectedRoom);
                    break;
                case 2:
                    selectedRoom = ForestRoomRight[Main.rand.Next(ForestRoomRight.Count)];
                    ForestRoomRight.Remove(selectedRoom);
                    break;
            }
            
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
            Room selectedRoom = RoomID[0];
            switch (currentFloor)
            {
                case 0:
                    selectedRoom = BaseRoomDown[Main.rand.Next(BaseRoomDown.Count)];
                    BaseRoomDown.Remove(selectedRoom);
                    break;
                case 1:
                    selectedRoom = CrimsonRoomDown[Main.rand.Next(CrimsonRoomDown.Count)];
                    CrimsonRoomDown.Remove(selectedRoom);
                    break;
                case 2:
                    selectedRoom = ForestRoomDown[Main.rand.Next(ForestRoomDown.Count)];
                    ForestRoomDown.Remove(selectedRoom);
                    break;
            }

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
            Room selectedRoom = RoomID[0];
            switch (currentFloor)
            {
                case 0:
                    selectedRoom = BaseRoomUp[Main.rand.Next(BaseRoomUp.Count)];
                    BaseRoomUp.Remove(selectedRoom);
                    break;
                case 1:
                    selectedRoom = CrimsonRoomUp[Main.rand.Next(CrimsonRoomUp.Count)];
                    CrimsonRoomUp.Remove(selectedRoom);
                    break;
                case 2:
                    selectedRoom = ForestRoomUp[Main.rand.Next(ForestRoomUp.Count)];
                    ForestRoomUp.Remove(selectedRoom);
                    break;
            }

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

            Room floorStartingRoom = RoomID[FloorID[currentFloor].StartRoomID];
            int roomCount = 8;

            string mapKey = floorStartingRoom.Key;
            var schematic = TileMaps[mapKey];

            Point placementPoint = new Point((int)previousRoom.RoomPosition.X + 200, Main.maxTilesY / 2);

            Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.TopLeft;

            
            floorStartingRoom.RoomPosition = placementPoint.ToVector2();
            floorStartingRoom.RoomDimensions = schematicSize;
            RoomSystem.NewRoom(floorStartingRoom);

            PlaceSchematic(mapKey, placementPoint, anchorType);

            PlaceRoom(roomCount, floorStartingRoom);
        }
        public static void ResetRoomGenLists()
        {
            BaseRoomRight = new List<Room>();
            BaseRoomDown = new List<Room>();
            BaseRoomUp = new List<Room>();

            CrimsonRoomRight = new List<Room>();
            CrimsonRoomDown = new List<Room>();
            CrimsonRoomUp = new List<Room>();

            ForestRoomRight = new List<Room>();
            ForestRoomDown = new List<Room>();
            ForestRoomUp = new List<Room>();
        }
        public static void SetAllRoomIDs()
        {
            ResetRoomGenLists();

            for (int i = 0; i < RoomID.Count; i++)
            {
                string key = RoomID[i].Key;
                if (key.Contains("Boss") || key.Contains("Start"))
                    continue;

                if (key.Contains("Base"))
                {
                    SortBaseRoomIDs(i, key);
                    continue;
                }
                if (key.Contains("Crimson"))
                {
                    SortCrimsonRoomIDs(i, key);
                    continue;
                }
                if (key.Contains("Forest"))
                {
                    SortForestRoomIDs(i, key);
                    continue;
                }
            }
        }
        public static void SortBaseRoomIDs(int id, string key)
        {
            if (key.Contains("Down"))
            {
                BaseRoomDown.Add(RoomID[id]);
                return;
            }
            if (key.Contains("Up"))
            {
                BaseRoomUp.Add(RoomID[id]);
                return;
            }
            BaseRoomRight.Add(RoomID[id]);
        }
        public static void SortCrimsonRoomIDs(int id, string key)
        {
            if (key.Contains("Down"))
            {
                CrimsonRoomDown.Add(RoomID[id]);
                return;
            }
            if (key.Contains("Up"))
            {
                CrimsonRoomUp.Add(RoomID[id]);
                return;
            }
            CrimsonRoomRight.Add(RoomID[id]);
        }
        public static void SortForestRoomIDs(int id, string key)
        {
            if (key.Contains("Down"))
            {
                ForestRoomDown.Add(RoomID[id]);
                return;
            }
            if (key.Contains("Up"))
            {
                ForestRoomUp.Add(RoomID[id]);
                return;
            }
            ForestRoomRight.Add(RoomID[id]);
        }
    }
}
