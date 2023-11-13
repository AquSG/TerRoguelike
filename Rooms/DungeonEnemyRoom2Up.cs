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
    public class DungeonEnemyRoom2Up : Room
    {
        public override string Key => "DungeonEnemyRoom2Up";
        public override string Filename => "Schematics/RoomSchematics/DungeonEnemyRoom2Up.csch";
        public override bool CanExitRight => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), NPCID.DiabolistRed, 60, 120, 0.45f);
        }
    }
}
