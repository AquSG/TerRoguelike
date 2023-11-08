using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Rooms;
using Terraria.WorldBuilding;
using static tModPorter.ProgressUpdate;
using Terraria.GameContent.Generation;
using tModPorter;
using Terraria.Localization;
using TerRoguelike.World;
using TerRoguelike.MainMenu;
using Microsoft.Xna.Framework;

namespace TerRoguelike.Systems
{
    public class TerRoguelikeWorldManagementSystem : ModSystem
    {
        //int taskCounter = 0;
        public override void PreWorldGen()
        {

        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            tasks.RemoveAll(x => x.Name != "Reset");
            tasks.Add(new PassLegacy("Building the Map", (progress, config) =>
            {
                progress.CurrentPassWeight = 1;
                progress.Value = 0;
                progress.Message = Language.GetOrRegister("Mods.TerRoguelike.MapBuildingMessage").Value;
                Main.worldSurface = 200;
                Main.rockLayer = 225;
                FillTheFuckingWorld(ref progress);
                RoomManager.GenerateRoomStructure();
                Main.spawnTileX = (Main.maxTilesX / 32) + 12;
                Main.spawnTileY = (Main.maxTilesY / 2) + 12;
                ItemManager.RoomRewardCooldown = 0;
                TerRoguelikeWorld.IsTerRoguelikeWorld = true;
                if (TerRoguelikeMenu.prepareForRoguelikeGeneration)
                    TerRoguelikeWorld.IsDeletableOnExit = true;
            }));
        }
        public void FillTheFuckingWorld(ref GenerationProgress progress)
        {
            double progPerTile = 0.98d / ((Main.maxTilesY - Main.worldSurface) * Main.maxTilesX);
            for (int y = (int)Main.worldSurface; y < Main.maxTilesY; y++)
            {
                for (int x = 1; x < Main.maxTilesX; x++)
                {
                    progress.Value += progPerTile;
                    if (x == 1 && y == (int)Main.worldSurface)
                    {
                        WorldGen.PlaceTile(x, y, ModContent.TileType<Tiles.BlackTile>(), true);
                        continue;
                    }
                    Main.tile[x, y].CopyFrom(Main.tile[1, (int)Main.worldSurface]);
                    if (y == (int)Main.worldSurface)
                    {
                        continue;
                    }
                    WorldGen.PlaceWall(x, y, ModContent.WallType<Tiles.BlackWall>(), true);
                }
            }
        }
    }
}
