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

namespace TerRoguelike.Rooms
{
    public class ForestStartRoom : Room
    {
        public override int ID => 30;
        public override int AssociatedFloor => 2;
        public override string Key => "ForestStartRoom";
        public override string Filename => "Schematics/RoomSchematics/ForestStartRoom.csch";
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
