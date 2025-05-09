﻿using System;
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
using TerRoguelike.Packets;
using Terraria.IO;

namespace TerRoguelike.Systems
{
    public class TerRoguelikeWorldManagementSystem : ModSystem
    {
        public static bool currentlyGeneratingTerRoguelikeWorld = false;
        public static bool GenDebugWorld = false;
        public override void PreWorldGen()
        {
            if (WorldGen.currentWorldSeed == "TerRoguelikeMakeRoomDebugWorldPleaseTY")
            {
                GenDebugWorld = true;
            }
            else
                GenDebugWorld = false;
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            if (TerRoguelikeMenu.prepareForRoguelikeGeneration || GenDebugWorld)
            {
                tasks.RemoveAll(x => x.Name != "Reset");
                tasks.Add(new PassLegacy("Building the Map", (progress, config) =>
                {
                    currentlyGeneratingTerRoguelikeWorld = true;

                    progress.CurrentPassWeight = 1;
                    progress.Value = 0;
                    progress.Message = Language.GetOrRegister("Mods.TerRoguelike.MapBuildingMessage").Value;
                    Main.worldSurface = 200;
                    Main.rockLayer = 225;
                    Main.spawnTileX = (Main.maxTilesX / 32) + 12;
                    Main.spawnTileY = (Main.maxTilesY / 2) + 12;
                    if (!GenDebugWorld)
                        FillTheFuckingWorld(ref progress);
                    RoomManager.GenerateRoomStructure();
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
        public static void RegenerateWorld()
        {
            Main.worldSurface = 200;

            WorldGen.PlaceTile(1, (int)Main.worldSurface, ModContent.TileType<Tiles.BlackTile>(), true);
            foreach (Room room in RoomSystem.RoomList)
            {
                int width = (int)room.RoomDimensions.X;
                int height = (int)room.RoomDimensions.Y;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int x = i + (int)room.RoomPosition.X;
                        int y = j + (int)room.RoomPosition.Y;

                        Main.tile[x, y].CopyFrom(Main.tile[1, (int)Main.worldSurface]);
                        if (y == (int)Main.worldSurface)
                            continue;

                        WorldGen.PlaceWall(x, y, ModContent.WallType<Tiles.BlackWall>(), true);
                    }
                }
            }
            RoomManager.GenerateRoomStructure();

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.sectionManager = new WorldSections((Main.maxTilesX - 1) / 200 + 1, (Main.maxTilesY - 1) / 150 + 1);
                if (ModContent.GetInstance<TerRoguelikeConfig>().LoadEntireWorldUponEnteringWorld)
                {
                    WorldGen.EveryTileFrame();
                }
                else
                {
                    foreach (Room room in RoomSystem.RoomList)
                    {
                        int width = (int)room.RoomDimensions.X;
                        int height = (int)room.RoomDimensions.Y;

                        for (int i = 0; i < width; i++)
                        {
                            for (int j = 0; j < height; j++)
                            {
                                int x = i + (int)room.RoomPosition.X;
                                int y = j + (int)room.RoomPosition.Y;
                                var tile = Main.tile[x, y];

                                if (tile.HasTile)
                                    WorldGen.SquareTileFrame(x, y);
                                if (tile.WallType > 0)
                                    WorldGen.SquareWallFrame(x, y);
                            }
                        }
                    }
                }
            }

            if (Main.dedServ)
                WorldFile.SaveWorld();

            RoomSystem.regeneratingWorld = false;
        }
    }
}
