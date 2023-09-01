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
    public class BaseEnemyRoom1 : Room
    {
        public override int ID => 1;
        public override string Key => "BaseEnemyRoom1";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom1.csch";
        public override void InitializeRoom()
        {
            AddRoomNPC(0, new Vector2(320, 240), NPCID.EyeofCthulhu, 60, 120, 0.7f);
        }
    }
}
