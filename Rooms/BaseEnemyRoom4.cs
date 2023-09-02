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
    public class BaseEnemyRoom4 : Room
    {
        public override int ID => 4;
        public override string Key => "BaseEnemyRoom2";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom2.csch";
        public override void InitializeRoom()
        {
            AddRoomNPC(0, new Vector2(64f, 132f), NPCID.SkeletonCommando, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2((RoomDimensions.X * 16f) - 64f, 132f), NPCID.SkeletonCommando, 60, 120, 0.45f);
            AddRoomNPC(2, new Vector2(RoomDimensions.X / 2f * 16f, 32f), NPCID.IlluminantSlime, 380, 120, 0.45f);
            AddRoomNPC(3, new Vector2(RoomDimensions.X / 2f * 16f, (RoomDimensions.Y * 16f) - 32f), NPCID.BoneLee, 780, 120, 0.55f);
        }
    }
}
