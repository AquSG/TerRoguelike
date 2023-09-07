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
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2(RoomDimensions.X / 2f * 16f, 32f), NPCID.SkeletonCommando, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2(64f, 72f), NPCID.DungeonSlime, 60, 120, 0.45f);
            AddRoomNPC(2, new Vector2((RoomDimensions.X * 16f) - 64f, 72f), NPCID.DungeonSlime, 60, 120, 0.45f);
        }
    }
}
