using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using TerRoguelike.Rooms;
using TerRoguelike.Managers;
using TerRoguelike.Floors;
using Terraria.ModLoader;
using TerRoguelike.Tiles;
using TerRoguelike.Items;

namespace TerRoguelike.Schematics
{
    public static class SchematicManager
    {
        //schematic code lifted from the Calamity Mod. Edits made in places to support the way we do things in TerRoguelike
        //You NEED the Calamity Schematic Exporter mod to be able to create the necessary files that are used for this schematic logic

        public static List<Room> RoomID = new List<Room>();
        public static List<Floor> FloorID = new List<Floor>();
        public static Dictionary<string, int> RoomDict = new Dictionary<string, int>(); // fetches the RoomID based on the given key. does not work with room variants
        public static Dictionary<string, int> FloorDict = new Dictionary<string, int>(); // fetches the FloorID based on the given key.

        internal static Dictionary<string, SchematicMetaTile[,]> TileMaps;
        internal static Dictionary<string, PilePlacementFunction> PilePlacementMaps;
        public delegate void PilePlacementFunction(int x, int y, Rectangle placeInArea);

        #region Load/Unload
        internal static void Load()
        {
            LoadRoomIDs();
            LoadFloorIDs();
            PilePlacementMaps = new Dictionary<string, PilePlacementFunction>();
            TileMaps = new Dictionary<string, SchematicMetaTile[,]>();

            for (int i = 0; i < RoomID.Count; i++)
            {
                if (RoomID[i].IsRoomVariant)
                    continue;

                TileMaps.Add(RoomID[i].Key, TerRoguelikeSchematicIO.LoadSchematic(RoomID[i].Filename));
            }
        }
        public static void LoadFloorIDs()
        {
            FloorID.Add(new BaseFloor());
            FloorID.Add(new CrimsonFloor());
            FloorID.Add(new ForestFloor());
            FloorID.Add(new CorruptFloor());
            FloorID.Add(new SnowFloor());
            FloorID.Add(new DesertFloor());
            FloorID.Add(new JungleFloor());
            FloorID.Add(new HellFloor());
            FloorID.Add(new DungeonFloor());
            FloorID.Add(new TempleFloor());
            FloorID.Add(new LunarFloor());
            FloorID.Add(new SanctuaryFloor());

            for (int i = 0; i < FloorID.Count; i++)
            {
                FloorID[i].ID = i;

                FloorDict.Add(FloorID[i].Name, i);
            }
        }
        public static void LoadRoomIDs()
        {
            RoomID.Add(new DefaultRoom());
            RoomID.Add(new BaseEnemyRoom1());
            RoomID.Add(new BaseEnemyRoom2());
            RoomID.Add(new BaseEnemyRoom1Var1());
            RoomID.Add(new BaseEnemyRoom2Var1());
            RoomID.Add(new BaseBossRoom1());
            RoomID.Add(new BaseEnemyRoom3Up());
            RoomID.Add(new BaseEnemyRoom4());
            RoomID.Add(new BaseEnemyRoom4Down());
            RoomID.Add(new BaseEnemyRoom4Up());
            RoomID.Add(new BaseEnemyRoom5());
            RoomID.Add(new BaseEnemyRoom5Down());
            RoomID.Add(new BaseEnemyRoom5Up());
            RoomID.Add(new BaseEnemyRoom6());
            RoomID.Add(new BaseEnemyRoom6Down());
            RoomID.Add(new BaseEnemyRoom6Up());
            RoomID.Add(new BaseEnemyRoom7());
            RoomID.Add(new BaseEnemyRoom7Up());
            RoomID.Add(new CrimsonStartRoom());
            RoomID.Add(new CrimsonEnemyRoom1());
            RoomID.Add(new CrimsonEnemyRoom2());
            RoomID.Add(new CrimsonEnemyRoom2Up());
            RoomID.Add(new CrimsonEnemyRoom3Down());
            RoomID.Add(new CrimsonEnemyRoom3Up());
            RoomID.Add(new CrimsonEnemyRoom4());
            RoomID.Add(new CrimsonEnemyRoom4Up());
            RoomID.Add(new CrimsonEnemyRoom5());
            RoomID.Add(new CrimsonEnemyRoom5Down());
            RoomID.Add(new CrimsonEnemyRoom5Up());
            RoomID.Add(new CrimsonEnemyRoom6Down());
            RoomID.Add(new CrimsonEnemyRoom7());
            RoomID.Add(new CrimsonBossRoom1());
            RoomID.Add(new ForestStartRoom());
            RoomID.Add(new ForestBossRoom1());
            RoomID.Add(new ForestEnemyRoom1());
            RoomID.Add(new ForestEnemyRoom1Up());
            RoomID.Add(new ForestEnemyRoom2());
            RoomID.Add(new ForestEnemyRoom2Down());
            RoomID.Add(new ForestEnemyRoom2Up());
            RoomID.Add(new ForestEnemyRoom3Down());
            RoomID.Add(new ForestEnemyRoom4Down());
            RoomID.Add(new ForestEnemyRoom4Up());
            RoomID.Add(new ForestEnemyRoom5());
            RoomID.Add(new ForestEnemyRoom5Down());
            RoomID.Add(new ForestEnemyRoom5Up());
            RoomID.Add(new ForestEnemyRoom6());
            RoomID.Add(new ForestEnemyRoom6Up());
            RoomID.Add(new CorruptStartRoom());
            RoomID.Add(new CorruptBossRoom1());
            RoomID.Add(new CorruptEnemyRoom1Down());
            RoomID.Add(new CorruptEnemyRoom2Down());
            RoomID.Add(new CorruptEnemyRoom3());
            RoomID.Add(new CorruptEnemyRoom3Down());
            RoomID.Add(new CorruptEnemyRoom4Down());
            RoomID.Add(new CorruptEnemyRoom5());
            RoomID.Add(new CorruptEnemyRoom5Up());
            RoomID.Add(new CorruptEnemyRoom6());
            RoomID.Add(new CorruptEnemyRoom7());
            RoomID.Add(new CorruptEnemyRoom8());
            RoomID.Add(new CorruptEnemyRoom9());
            RoomID.Add(new CorruptEnemyRoom10());
            RoomID.Add(new SnowStartRoom());
            RoomID.Add(new SnowBossRoom1());
            RoomID.Add(new SnowEnemyRoom1());
            RoomID.Add(new SnowEnemyRoom1Down());
            RoomID.Add(new SnowEnemyRoom2());
            RoomID.Add(new SnowEnemyRoom3Down());
            RoomID.Add(new SnowEnemyRoom3Up());
            RoomID.Add(new SnowEnemyRoom4());
            RoomID.Add(new SnowEnemyRoom5());
            RoomID.Add(new SnowEnemyRoom6());
            RoomID.Add(new SnowEnemyRoom7Down());
            RoomID.Add(new SnowEnemyRoom7Up());
            RoomID.Add(new SnowEnemyRoom8());
            RoomID.Add(new SnowEnemyRoom9());
            RoomID.Add(new DesertStartRoom());
            RoomID.Add(new DesertBossRoom1());
            RoomID.Add(new DesertBossRoom1Transition());
            RoomID.Add(new DesertEnemyRoom1());
            RoomID.Add(new DesertEnemyRoom1Down());
            RoomID.Add(new DesertEnemyRoom2Down());
            RoomID.Add(new DesertEnemyRoom2Up());
            RoomID.Add(new DesertEnemyRoom3());
            RoomID.Add(new DesertEnemyRoom4());
            RoomID.Add(new DesertEnemyRoom4Down());
            RoomID.Add(new DesertEnemyRoom4Up());
            RoomID.Add(new DesertEnemyRoom5());
            RoomID.Add(new DesertEnemyRoom6());
            RoomID.Add(new DesertEnemyRoom7());
            RoomID.Add(new DesertEnemyRoom8());
            RoomID.Add(new JungleStartRoom());
            RoomID.Add(new JungleBossRoom1());
            RoomID.Add(new JungleBossRoom1Transition());
            RoomID.Add(new JungleEnemyRoom1());
            RoomID.Add(new JungleEnemyRoom1Down());
            RoomID.Add(new JungleEnemyRoom1Up());
            RoomID.Add(new JungleEnemyRoom2());
            RoomID.Add(new JungleEnemyRoom2Down());
            RoomID.Add(new JungleEnemyRoom3());
            RoomID.Add(new JungleEnemyRoom3Up());
            RoomID.Add(new JungleEnemyRoom4());
            RoomID.Add(new JungleEnemyRoom4Down());
            RoomID.Add(new JungleEnemyRoom4Up());
            RoomID.Add(new JungleEnemyRoom5());
            RoomID.Add(new JungleEnemyRoom5Down());
            RoomID.Add(new JungleEnemyRoom5Up());
            RoomID.Add(new HellStartRoom());
            RoomID.Add(new HellBossRoom1());
            RoomID.Add(new HellEnemyRoom1());
            RoomID.Add(new HellEnemyRoom2());
            RoomID.Add(new HellEnemyRoom3());
            RoomID.Add(new HellEnemyRoom4());
            RoomID.Add(new HellEnemyRoom5());
            RoomID.Add(new HellEnemyRoom6());
            RoomID.Add(new HellEnemyRoom7());
            RoomID.Add(new HellEnemyRoom8());
            RoomID.Add(new HellEnemyRoom9());
            RoomID.Add(new HellEnemyRoom10());
            RoomID.Add(new DungeonStartRoom());
            RoomID.Add(new DungeonBossRoom1());
            RoomID.Add(new DungeonBossRoom1Transition());
            RoomID.Add(new DungeonEnemyRoom1());
            RoomID.Add(new DungeonEnemyRoom1Down());
            RoomID.Add(new DungeonEnemyRoom1Up());
            RoomID.Add(new DungeonEnemyRoom2());
            RoomID.Add(new DungeonEnemyRoom2Down());
            RoomID.Add(new DungeonEnemyRoom2Up());
            RoomID.Add(new DungeonEnemyRoom3());
            RoomID.Add(new DungeonEnemyRoom3Down());
            RoomID.Add(new DungeonEnemyRoom3Up());
            RoomID.Add(new DungeonEnemyRoom4());
            RoomID.Add(new DungeonEnemyRoom5());
            RoomID.Add(new DungeonEnemyRoom6());
            RoomID.Add(new TempleStartRoom());
            RoomID.Add(new TempleBossRoom1());
            RoomID.Add(new TempleEnemyRoom1());
            RoomID.Add(new TempleEnemyRoom1Down());
            RoomID.Add(new TempleEnemyRoom1Up());
            RoomID.Add(new TempleEnemyRoom2());
            RoomID.Add(new TempleEnemyRoom3());
            RoomID.Add(new TempleEnemyRoom4Down());
            RoomID.Add(new TempleEnemyRoom4Up());
            RoomID.Add(new TempleEnemyRoom5());
            RoomID.Add(new TempleEnemyRoom5Down());
            RoomID.Add(new TempleEnemyRoom5Up());
            RoomID.Add(new TempleEnemyRoom6());
            RoomID.Add(new TempleEnemyRoom7());
            RoomID.Add(new LunarStartRoom());
            RoomID.Add(new LunarBossRoom1());
            RoomID.Add(new LunarHallRoom1());
            RoomID.Add(new LunarHallRoom2());
            RoomID.Add(new LunarPillarRoomTopLeft());
            RoomID.Add(new LunarPillarRoomTopRight());
            RoomID.Add(new LunarPillarRoomBottomLeft());
            RoomID.Add(new LunarPillarRoomBottomRight());
            RoomID.Add(new SanctuaryRoom1());

            for (int i = 0; i < RoomID.Count; i++)
            {
                RoomID[i].ID = i;

                if (RoomID[i].IsRoomVariant)
                    continue;

                RoomDict.Add(RoomID[i].Key, i);
            }
        }
        internal static void Unload()
        {
            TileMaps = null;
            PilePlacementMaps = null;
            RoomID = null;
            FloorID = null;
        }
        #endregion

        #region Get Schematic Area
        public static Vector2? GetSchematicArea(string name)
        {
            // If no schematic exists with this name, simply return null.
            if (!TileMaps.TryGetValue(name, out SchematicMetaTile[,] schematic))
                return null;

            return new Vector2(schematic.GetLength(0), schematic.GetLength(1));
        }
        #endregion Get Schematic Area

        #region Place Schematic
        public static void PlaceSchematic(string name, Point pos, SchematicAnchor anchorType)
        {
            // If no schematic exists with this name, cancel with a helpful log message.
            if (!TileMaps.ContainsKey(name))
            {
                TerRoguelike.Instance.Logger.Warn($"Tried to place a schematic with name \"{name}\". No matching schematic file found.");
                return;
            }

            PilePlacementMaps.TryGetValue(name, out PilePlacementFunction pilePlacementFunction);

            // Grab the schematic itself from the dictionary of loaded schematics.
            SchematicMetaTile[,] schematic = TileMaps[name];
            int width = schematic.GetLength(0);
            int height = schematic.GetLength(1);

            // Calculate the appropriate location to start laying down schematic tiles.
            int cornerX = pos.X;
            int cornerY = pos.Y;
            switch (anchorType)
            {
                case SchematicAnchor.TopLeft: // Provided point is top-left corner = No change
                case SchematicAnchor.Default: // This is also default behavior
                default:
                    break;
                case SchematicAnchor.TopCenter: // Provided point is top center = Top-left corner is 1/2 width to the left
                    cornerX -= width / 2;
                    break;
                case SchematicAnchor.TopRight: // Provided point is top-right corner = Top-left corner is 1 width to the left
                    cornerX -= width;
                    break;
                case SchematicAnchor.CenterLeft: // Provided point is left center: Top-left corner is 1/2 height above
                    cornerY -= height / 2;
                    break;
                case SchematicAnchor.Center: // Provided point is center = Top-left corner is 1/2 width and 1/2 height up-left
                    cornerX -= width / 2;
                    cornerY -= height / 2;
                    break;
                case SchematicAnchor.CenterRight: // Provided point is right center: Top-left corner is 1 width and 1/2 height up-left
                    cornerX -= width;
                    cornerY -= height / 2;
                    break;
                case SchematicAnchor.BottomLeft: // Provided point is bottom-left corner = Top-left corner is 1 height above
                    cornerY -= height;
                    break;
                case SchematicAnchor.BottomCenter: // Provided point is bottom center: Top-left corner is 1/2 width and 1 height up-left
                    cornerX -= width / 2;
                    cornerY -= height;
                    break;
                case SchematicAnchor.BottomRight: // Provided point is bottom-right corner = Top-left corner is 1 width to the left and 1 height above
                    cornerX -= width;
                    cornerY -= height;
                    break;
            }

            // Make sure that all four corners of the target area are actually in the world.
            if (!WorldGen.InWorld(cornerX, cornerY) || !WorldGen.InWorld(cornerX + width, cornerY + height))
            {
                TerRoguelike.Instance.Logger.Warn("Schematic failed to place: Part of the target location is outside the game world.");
                return;
            }

            // Make an array for the tiles that used to be where this schematic will be pasted.
            SchematicMetaTile[,] originalTiles = new SchematicMetaTile[width, height];

            // Schematic area pre-processing has three steps.
            // Step 1: Kill all trees and cacti specifically. This prevents ugly tree/cactus pieces from being restored later.
            // Step 2: Fill the original tiles array with everything that was originally in the target rectangle.
            // Step 3: Destroy everything in the target rectangle (except chests -- that'll cause infinite recursion).
            // The third step is necessary so that multi tiles on the edge of the region are properly destroyed (e.g. Life Crystals).

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    Tile t = Main.tile[x + cornerX, y + cornerY];
                    if (t.TileType == TileID.Trees || t.TileType == TileID.PineTree || t.TileType == TileID.Cactus)
                        WorldGen.KillTile(x + cornerX, y + cornerY, noItem: true);
                }

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    Tile t = Main.tile[x + cornerX, y + cornerY];
                    originalTiles[x, y] = new SchematicMetaTile(t);
                }

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                    if (originalTiles[x, y].TileType != TileID.Containers)
                        WorldGen.KillTile(x + cornerX, y + cornerY, noItem: true);

            // Lay down the schematic. If the schematic calls for it, bring back tiles that are stored in the old tiles array.
            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                {
                    SchematicMetaTile smt = schematic[x, y];
                    smt.ApplyTo(x + cornerX, y + cornerY, originalTiles[x, y]);
                    Tile worldTile = Main.tile[x + cornerX, y + cornerY];

                    // Now that the tile data is correctly set, place appropriate tile entities.
                    TryToInitializeBasin(x + cornerX, y + cornerY, worldTile);

                    // Activate the pile placement function if defined.
                    Rectangle placeInArea = new Rectangle(x, y, width, height);
                    pilePlacementFunction?.Invoke(x + cornerX, y + cornerY, placeInArea);
                }
        }
        #endregion

        #region Place Schematic Helper Methods
        private static void TryToInitializeBasin(int x, int y, Tile t)
        {
            // A tile entity in an empty spot would make no sense.
            if (!t.HasTile)
                return;
            // Ignore tiles that aren't at the top left of the tile.
            if (t.TileFrameX != 0 || t.TileFrameY != 0)
                return;

            if (t.TileType == ModContent.TileType<Tiles.ItemBasin>())
            {
                TileLoader.PlaceInWorld(x + 1, y + 1, new Item(ModContent.ItemType<Items.ItemBasin>()));
            }
        }
        #endregion
    }
}
        