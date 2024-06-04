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
using ReLogic.Threading;

namespace TerRoguelike.Systems
{
    public class TerRoguelikeWorldManagementSystem : ModSystem
    {
        public static bool GenDebugWorld = false;
        //int taskCounter = 0;
        public override void PreWorldGen()
        {
            if (WorldGen.currentWorldSeed == "TerRoguelikeMakeRoomDebugWorldPleaseTY")
            {
                GenDebugWorld = true;
                TerRoguelikeMenu.prepareForRoguelikeGeneration = true;
            }
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            if (TerRoguelikeMenu.prepareForRoguelikeGeneration)
            {
                tasks.RemoveAll(x => x.Name != "Reset");
                tasks.Add(new PassLegacy("Building the Map", (progress, config) =>
                {
                    progress.CurrentPassWeight = 1;
                    progress.Value = 0;
                    progress.Message = Language.GetOrRegister("Mods.TerRoguelike.MapBuildingMessage").Value;
                    Main.worldSurface = 200;
                    Main.rockLayer = 225;
                    if (!GenDebugWorld)
                        FillTheFuckingWorld(ref progress);
                    RoomManager.GenerateRoomStructure();
                    Main.spawnTileX = (Main.maxTilesX / 32) + 12;
                    Main.spawnTileY = (Main.maxTilesY / 2) + 12;
                    if (GenDebugWorld)
                    {
                        Main.spawnTileY = (Main.maxTilesY / 12) + 12;
                        GenDebugWorld = false;
                        TerRoguelikeWorld.IsDebugWorld = true;
                    }
                    ItemManager.RoomRewardCooldown = 0;
                    TerRoguelikeWorld.IsTerRoguelikeWorld = true;
                    if (!GenDebugWorld)
                        TerRoguelikeWorld.IsDeletableOnExit = true;
                }));
            }
        }
        public void FillTheFuckingWorld(ref GenerationProgress progress)
        {
            //Can't do progress updates with fast parallel, but fast parallel makes it twice as fast. who cares.
            //double progPerTile = 0.98d / ((Main.maxTilesY - Main.worldSurface) * Main.maxTilesX);
            FastParallel.For((int)Main.worldSurface, Main.maxTilesY, delegate (int start, int end, object context)
            {
                for (int i = start; i < end; i++)
                {
                    for (int x = 1; x < Main.maxTilesX; x++)
                    {
                        //progress.Value += progPerTile;
                        if (x == 1 && i == (int)Main.worldSurface)
                        {
                            WorldGen.PlaceTile(x, i, ModContent.TileType<Tiles.BlackTile>(), true);
                            continue;
                        }
                        Main.tile[x, i].CopyFrom(Main.tile[1, (int)Main.worldSurface]);
                        if (i == (int)Main.worldSurface)
                        {
                            continue;
                        }
                        WorldGen.PlaceWall(x, i, ModContent.WallType<Tiles.BlackWall>(), true);
                    }
                }
            });
            
        }
    }
}
