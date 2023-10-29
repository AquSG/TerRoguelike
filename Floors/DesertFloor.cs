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
    public class DesertFloor : Floor
    {
        public override int FloorID => 5;
        public override int StartRoomID => RoomDict["DesertStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["DesertBossRoom1"] };
        public override int Stage => 2;
    }
}
