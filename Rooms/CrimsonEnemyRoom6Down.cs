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
    public class CrimsonEnemyRoom6Down : Room
    {
        public override string Key => "CrimsonEnemyRoom6Down";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom6Down.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2(RoomDimensions.X * 16f / 2f, (RoomDimensions.Y * 16f / 2f) + 32), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2((RoomDimensions.X * 16f / 2f) + 48f, (RoomDimensions.Y * 16f / 2f) + 48f), NPCID.BloodCrawlerWall, 90, 120, 0.45f);
            AddRoomNPC(2, new Vector2((RoomDimensions.X * 16f / 2f) + 96f, (RoomDimensions.Y * 16f / 2f) + 68f), NPCID.BloodCrawlerWall, 120, 120, 0.45f);
            AddRoomNPC(3, new Vector2((RoomDimensions.X * 16f / 2f) + 126f, (RoomDimensions.Y * 16f / 2f) + 96f), NPCID.BloodCrawlerWall, 150, 120, 0.45f);
            AddRoomNPC(4, new Vector2(232f, 96f), NPCID.FloatyGross, 350, 120, 0.6f);
        }
    }
}
