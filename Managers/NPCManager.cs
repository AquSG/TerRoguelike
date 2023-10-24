using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Rooms;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TerRoguelike.Schematics;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Items;
using TerRoguelike.Items.Common;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Items.Rare;
using Terraria.ModLoader.Core;
using TerRoguelike.NPCs.Enemy;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Managers
{
    public class NPCManager
    {
        internal static void Load()
        {
            AllNPCs = new List<BaseRoguelikeNPC>()
            {
                new Splinter(),
                new Spookrow()
            };
        }
        internal static void Unload()
        {
            AllNPCs = null;
        }
        public static List<BaseRoguelikeNPC> AllNPCs;
    }
}
