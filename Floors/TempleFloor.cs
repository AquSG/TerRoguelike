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
    public class TempleFloor : Floor
    {
        public override int FloorID => 9;
        public override int StartRoomID => RoomDict["TempleStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["TempleBossRoom1"] };
        public override int Stage => 4;
    }
}
