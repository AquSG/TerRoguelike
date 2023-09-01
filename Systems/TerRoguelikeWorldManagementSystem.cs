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

namespace TerRoguelike.Systems
{
    public class TerRoguelikeWorldManagementSystem : ModSystem
    {
        int taskCounter = 0;
        public override void PreWorldGen()
        {
            
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            tasks.Clear();
            tasks.Insert(taskCounter, new PassLegacy("Building the Map", (progress, config) =>
            {
                progress.Message = Language.GetOrRegister("Mods.TerRoguelike.MapBuildingMessage").Value;
                RoomManager.GenerateRoomStructure();
                Main.spawnTileX = (Main.maxTilesX / 2) + 12;
                Main.spawnTileY = (Main.maxTilesY / 2) + 12;
            }));
        }
    }
}
