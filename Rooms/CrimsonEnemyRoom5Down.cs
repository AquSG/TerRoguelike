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
    public class CrimsonEnemyRoom5Down : Room
    {
        public override string Key => "CrimsonEnemyRoom5Down";
        public override string Filename => "Schematics/RoomSchematics/CrimsonEnemyRoom5Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f / 4f), (RoomDimensions.Y * 16f / 2f) - 32f), NPCID.Crimera, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f / 4f * 3f), (RoomDimensions.Y * 16f / 2f) - 32f), NPCID.Crimera, 60, 120, 0.45f);
            AddRoomNPC(new Vector2((RoomDimensions.X * 16f) - 48f, (RoomDimensions.Y * 16f) - 32f), NPCID.BloodCrawlerWall, 60, 120, 0.45f);
        }
    }
}
