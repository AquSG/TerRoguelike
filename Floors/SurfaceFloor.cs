using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace TerRoguelike.Floors
{
    public class SurfaceFloor : Floor
    {
        public override int StartRoomID => RoomDict["SurfaceRoom1"];
        public override int Stage => -1;
        public override string Name => "Surface";
    }
}
