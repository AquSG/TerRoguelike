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
    public class BaseEnemyRoom5 : Room
    {
        public override string Key => "BaseEnemyRoom5";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom5.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(48f, 72f), NPCID.BoneLee, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X*16f) - 48f, 72f), NPCID.BoneLee, 60, 120, 0.45f);
            AddRoomNPC(new Vector2(RoomDimensions.X / 2f * 16f, 32f), NPCID.Crimslime, 380, 120, 0.45f);
        }
    }
}
