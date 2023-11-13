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
    public class CrimsonEnemyRoom2 : Room
    {
        public override string Key => "CrimsonEnemyRoom2";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom2.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f) / 3f, (RoomDimensions.Y * 16f) - 32f), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f) - 64f, (RoomDimensions.Y * 16f) - 32f), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f) - 128f, (RoomDimensions.Y * 16f) - 32f), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
            AddRoomNPC(new Vector2(72f, 32f), NPCID.IchorSticker, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f / 2f) - 48f, (RoomDimensions.Y * 16f) / 4f), NPCID.Crimera, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f / 2f) - 96f, (RoomDimensions.Y * 16f / 4f) + 20f), NPCID.Crimera, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f / 2f), (RoomDimensions.Y * 16f / 4f) - 20f), NPCID.Crimera, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f / 2f), (RoomDimensions.Y * 16f / 2f)), NPCID.CrimsonAxe, 60, 120, 0.6f);
        }
    }
}
