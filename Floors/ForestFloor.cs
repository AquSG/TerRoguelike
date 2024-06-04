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
    public class ForestFloor : Floor
    {
        public override int StartRoomID => RoomDict["ForestStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["ForestBossRoom1"] };
        public override int Stage => 0;
        public override string Name => "Forest";
        public override FloorSoundtrack Soundtrack => MusicSystem.ForestTheme;
    }
}
