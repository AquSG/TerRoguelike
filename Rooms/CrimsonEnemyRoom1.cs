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
    public class CrimsonEnemyRoom1 : Room
    {
        public override int ID => 17;
        public override string Key => "CrimsonEnemyRoom1";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom1.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2((RoomDimensions.X * 16f) - 32f, (RoomDimensions.Y * 16f) - 96f), NPCID.FaceMonster, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2((RoomDimensions.X * 16f) - 32f, (RoomDimensions.Y * 16f) - 64f), NPCID.FaceMonster, 240, 120, 0.45f);
            AddRoomNPC(2, new Vector2((RoomDimensions.X * 16f) - 32f, (RoomDimensions.Y * 16f) - 32f), NPCID.FaceMonster, 420, 120, 0.45f);
        }
    }
}
