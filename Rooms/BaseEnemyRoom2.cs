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
    public class BaseEnemyRoom2 : Room
    {
        public override int ID => 2;
        public override string Key => "BaseEnemyRoom2";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom2.csch";
        public override void InitializeRoom()
        {
            AddRoomNPC(0, new Vector2(320, 240), NPCID.QueenBee, 60, 120, 0.7f);
        }
    }
}
