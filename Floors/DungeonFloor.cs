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
    public class DungeonFloor : Floor
    {
        public override int StartRoomID => RoomDict["DungeonStartRoom"];
        public override List<int> BossRoomIDs => new List<int>() { RoomDict["DungeonBossRoom1"] };
        public override int Stage => 4;
        public override string Name => "Dungeon";
        public override FloorSoundtrack Soundtrack => MusicSystem.DungeonTheme;
    }
}
