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
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;

namespace TerRoguelike.Rooms
{
    public class DesertEnemyRoom2Down : Room
    {
        public override string Key => "DesertEnemyRoom2Down";
        public override string Filename => "Schematics/RoomSchematics/DesertEnemyRoom2Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), ModContent.NPCType<DesertSpirit>(), 60, 120, 0.45f);
            AddRoomNPC(new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), ModContent.NPCType<SandWorm>(), 180, 120, 0.45f);
            AddRoomNPC(new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), ModContent.NPCType<Antlion>(), 300, 120, 0.45f);
        }
    }
}
