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
    public class CrimsonEnemyRoom7 : Room
    {
        public override string Key => "CrimsonEnemyRoom7";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom7.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2((RoomDimensions.X * 16f) - 48f, (RoomDimensions.Y * 16f) - 32f), NPCID.FaceMonster, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2((RoomDimensions.X * 16f) - 96f, (RoomDimensions.Y * 16f) - 48f), NPCID.FaceMonster, 60, 120, 0.45f);
            AddRoomNPC(2, new Vector2((RoomDimensions.X * 16f) - 144f, (RoomDimensions.Y * 16f) - 64f), NPCID.FaceMonster, 60, 120, 0.45f);
            AddRoomNPC(3, new Vector2((RoomDimensions.X * 16f) - 188f, (RoomDimensions.Y * 16f) - 72f), NPCID.FaceMonster, 60, 120, 0.45f);
            AddRoomNPC(0, new Vector2((RoomDimensions.X * 16f) - 48f, (RoomDimensions.Y * 16f) - 32f), NPCID.CrimsonAxe, 300, 120, 0.6f);
        }
    }
}
