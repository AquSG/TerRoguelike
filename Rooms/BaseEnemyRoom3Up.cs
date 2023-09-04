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
    public class BaseEnemyRoom3Up : Room
    {
        public override int ID => 6;
        public override string Key => "BaseEnemyRoom3Up";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom3Up.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            AddRoomNPC(0, new Vector2(64f, 72f), NPCID.Crimslime, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2((RoomDimensions.X*16f) - 64f, 72f), NPCID.Crimslime, 60, 120, 0.45f);
            AddRoomNPC(2, new Vector2((RoomDimensions.X * 16f) / 2f, 72f), NPCID.Mimic, 360, 120, 0.45f);
        }
    }
}
