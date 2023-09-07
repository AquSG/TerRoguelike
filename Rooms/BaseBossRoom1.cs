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
    public class BaseBossRoom1 : Room
    {
        public override int ID => 5;
        public override string Key => "BaseBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/BaseBossRoom1.csch";
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2(320, (RoomDimensions.Y * 16f) - 32f), NPCID.Paladin, 60, 120, 0.9f);
        }
    }
}
