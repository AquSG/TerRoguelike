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
    public class CrimsonEnemyRoom3Down : Room
    {
        public override int ID => 20;
        public override string Key => "CrimsonEnemyRoom3Down";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom3Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2(RoomDimensions.X * 16f / 2f, RoomDimensions.Y * 16f / 2f), NPCID.FloatyGross, 60, 120, 0.6f);
            AddRoomNPC(1, new Vector2(48f, RoomDimensions.Y * 16f / 4f), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
            AddRoomNPC(2, new Vector2((RoomDimensions.X * 16f) - 48f, RoomDimensions.Y * 16f / 4f), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
            AddRoomNPC(3, new Vector2(RoomDimensions.X * 16f / 2f, (RoomDimensions.Y * 16f / 2f) + 64f), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
            AddRoomNPC(4, new Vector2(RoomDimensions.X * 16f / 2f, (RoomDimensions.Y * 16f / 2f) + 104f), NPCID.Crimera, 60, 120, 0.45f);
            AddRoomNPC(5, new Vector2((RoomDimensions.X * 16f) - 48f, (RoomDimensions.Y * 16f) - 32f), NPCID.FaceMonster, 60, 120, 0.45f);
        }
    }
}
