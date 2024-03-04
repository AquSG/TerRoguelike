using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Rooms
{
    public class DungeonBossRoom1Transition : Room
    {
        public override int AssociatedFloor => FloorDict["Dungeon"];
        public override string Key => "DungeonBossRoom1Transition";
        public override string Filename => "Schematics/RoomSchematics/DungeonBossRoom1Transition.csch";
        public override int TransitionDirection => 0;
        public override void InitializeRoom()
        {
            active = false;
        }
        public override void Update()
        {
            active = false;
        }
    }
}
