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
    public class BaseEnemyRoom3 : Room
    {
        public override int ID => 3;
        public override string Key => "BaseEnemyRoom1";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom1.csch";
        public override void InitializeRoom()
        {
            AddRoomNPC(0, new Vector2(64f, 72f), NPCID.BlueArmoredBones, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2((RoomDimensions.X * 16f) - 64f, 72f), NPCID.BlueArmoredBones, 60, 120, 0.45f);
            AddRoomNPC(2, new Vector2(RoomDimensions.X / 2f * 16f, (RoomDimensions.Y * 16f) - 32f), NPCID.RainbowSlime, 360, 120, 0.55f);
        }
    }
}
