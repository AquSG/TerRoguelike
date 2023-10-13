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
    public class CrimsonFloor : Floor
    {
        public override int FloorID => 1;
        public override int StartRoomID => RoomDict["CrimsonStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["CrimsonBossRoom1"] };
        public override int Stage => 1;
    }
}
