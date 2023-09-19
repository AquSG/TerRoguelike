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
        public static List<Room> StartRoomIDs;
        public static List<Room> BossRoomIDs;
        public static List<int> oldRoomDirections;
        public static int currentFloor;

        public static List<Room> BaseRoomRight;
        public static List<Room> BaseRoomDown;
        public static List<Room> BaseRoomUp;
        public static List<Room> CrimsonRoomRight;
        public static List<Room> CrimsonRoomDown;
        public static List<Room> CrimsonRoomUp;
        
        public static void GenerateRoomStructure()
        {
            currentFloor = 0;
            RoomSystem.RoomList = new List<Room>();
            oldRoomDirections = new List<int>();
            SetAllRoomIDs();

            int roomCount = 8;
            string mapKey = RoomID[0].Key;
            var schematic = TileMaps[mapKey];

            int startpositionX = Main.maxTilesX / 32;
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
                var selectedRoom = BossRoomIDs[currentFloor];
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

                switch (currentFloor)
                {
                    case 0:
                        currentFloor = 1;
                        break;
                    case 1:
                        return;
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

                if (!directionsAvailable.Any())
                    PlaceRoom(1, previousRoom);

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

            Room floorStartingRoom = RoomID[0];
            int roomCount = 8;
            switch (currentFloor)
            {
                case 0:
                    floorStartingRoom = StartRoomIDs[0];
                    break;
                case 1:
                    floorStartingRoom = StartRoomIDs[1];
                    roomCount = 9;
                    break;
            }

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
        public static void SetAllRoomIDs()
        {
            SetStartRoomIDs();
            SetBossRoomIDs();
            SetAllBaseRoomIDs();
            SetAllCrimsonRoomIDs();
        }
        public static void SetStartRoomIDs()
        {
            StartRoomIDs = new List<Room>();
            StartRoomIDs.Add(RoomID[0]);
            StartRoomIDs.Add(RoomID[16]);
        }
        public static void SetBossRoomIDs()
        {
            BossRoomIDs = new List<Room>();
            BossRoomIDs.Add(RoomID[5]);
            BossRoomIDs.Add(RoomID[29]);
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
            BaseRoomRight.Add(RoomID[13]);
        }
        public static void SetBaseRoomDownIDs()
        {
            BaseRoomDown.Add(RoomID[8]);
            BaseRoomDown.Add(RoomID[11]);
            BaseRoomDown.Add(RoomID[14]);
        }
        public static void SetBaseRoomUpIDs()
        {
            BaseRoomUp.Add(RoomID[6]);
            BaseRoomUp.Add(RoomID[9]);
            BaseRoomUp.Add(RoomID[12]);
            BaseRoomUp.Add(RoomID[15]);
        }
        public static void SetAllCrimsonRoomIDs()
        {
            CrimsonRoomRight = new List<Room>();
            CrimsonRoomDown = new List<Room>();
            CrimsonRoomUp = new List<Room>();
            SetCrimsonRoomRightIDs();
            SetCrimsonRoomDownIDs();
            SetCrimsonRoomUpIDs();
        }
        public static void SetCrimsonRoomRightIDs()
        {
            CrimsonRoomRight.Add(RoomID[17]);
            CrimsonRoomRight.Add(RoomID[18]);
            CrimsonRoomRight.Add(RoomID[22]);
            CrimsonRoomRight.Add(RoomID[24]);
            CrimsonRoomRight.Add(RoomID[28]);
        }
        public static void SetCrimsonRoomDownIDs()
        {
            CrimsonRoomDown.Add(RoomID[20]);
            CrimsonRoomDown.Add(RoomID[25]);
            CrimsonRoomDown.Add(RoomID[27]);
        }
        public static void SetCrimsonRoomUpIDs()
        {
            CrimsonRoomUp.Add(RoomID[19]);
            CrimsonRoomUp.Add(RoomID[21]);
            CrimsonRoomUp.Add(RoomID[23]);
            CrimsonRoomUp.Add(RoomID[26]);
        }
    }
}
