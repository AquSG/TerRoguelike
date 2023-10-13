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
    public class ForestEnemyRoom5 : Room
    {
        public override string Key => "ForestEnemyRoom5";
        public override string Filename => "Schematics/RoomSchematics/ForestEnemyRoom5.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), NPCID.Splinterling, 60, 120, 0.45f);

        }
    }
}
