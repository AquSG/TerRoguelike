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
    public class DesertStartRoom : Room
    {
        public override int AssociatedFloor => FloorDict["Desert"];
        public override string Key => "DesertStartRoom";
        public override string Filename => "Schematics/RoomSchematics/DesertStartRoom.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool IsStartRoom => true;
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
