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
    public class JungleBossRoom1Transition : Room
    {
        public override int AssociatedFloor => FloorDict["Jungle"];
        public override string Key => "JungleBossRoom1Transition";
        public override string Filename => "Schematics/RoomSchematics/JungleBossRoom1Transition.csch";
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
