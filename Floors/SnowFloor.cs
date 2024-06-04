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
    public class SnowFloor : Floor
    {
        public override int StartRoomID => RoomDict["SnowStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["SnowBossRoom1"] };
        public override int Stage => 2;
        public override string Name => "Snow";
        public override FloorSoundtrack Soundtrack => MusicSystem.SnowTheme;
    }
}
