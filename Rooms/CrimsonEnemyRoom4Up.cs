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
    public class CrimsonEnemyRoom4Up : Room
    {
        public override string Key => "CrimsonEnemyRoom4Up";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom4Up.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2(RoomDimensions.X * 16f / 2f, RoomDimensions.Y * 16f / 2f), NPCID.CrimsonAxe, 60, 120, 0.45f);
        }
    }
}
