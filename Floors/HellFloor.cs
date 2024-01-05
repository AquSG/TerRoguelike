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
    public class HellFloor : Floor
    {
        public override int StartRoomID => RoomDict["HellStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["HellBossRoom1"] };
        public override int Stage => 3;
        public override bool InHell => true;
        public override string Name => "Hell";
    }
}
