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
    public class CorruptFloor : Floor
    {
        public override int StartRoomID => RoomDict["CorruptStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["CorruptBossRoom1"] };
        public override int Stage => 1;
        public override string Name => "Corrupt";
        public override FloorSoundtrack Soundtrack => MusicSystem.CorruptTheme;
    }
}
