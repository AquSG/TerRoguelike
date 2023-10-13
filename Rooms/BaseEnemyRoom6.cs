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
    public class BaseEnemyRoom6 : Room
    {
        public override string Key => "BaseEnemyRoom6";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom6.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(0, new Vector2((RoomDimensions.X * 16f / 2f) - 48, 48f), NPCID.GiantBat, 60, 120, 0.45f);
            AddRoomNPC(1, new Vector2((RoomDimensions.X * 16f / 2f) + 48, 48f), NPCID.GiantBat, 60, 120, 0.45f);
            AddRoomNPC(2, new Vector2((RoomDimensions.X * 16f / 4f), 48f), NPCID.GiantBat, 240, 120, 0.45f);
            AddRoomNPC(3, new Vector2((RoomDimensions.X * 16f * 3f / 4f), 48f), NPCID.GiantBat, 240, 120, 0.45f);
            AddRoomNPC(4, new Vector2(72f, 48f), NPCID.GiantBat, 420, 120, 0.45f);
            AddRoomNPC(5, new Vector2((RoomDimensions.X * 16f) - 72f, 48f), NPCID.GiantBat, 420, 120, 0.45f);
            AddRoomNPC(6, new Vector2(288f, (RoomDimensions.Y * 16f) / 2f), NPCID.SkeletonSniper, 60, 120, 0.45f);
            AddRoomNPC(7, new Vector2((RoomDimensions.X * 16f) - 288f, (RoomDimensions.Y * 16f) / 2f), NPCID.SkeletonSniper, 60, 120, 0.45f);
            AddRoomNPC(8, new Vector2(288f, (RoomDimensions.Y * 16f) / 4f), NPCID.TacticalSkeleton, 60, 120, 0.45f);
            AddRoomNPC(9, new Vector2((RoomDimensions.X * 16f) - 288f, (RoomDimensions.Y * 16f) / 4f), NPCID.TacticalSkeleton, 60, 120, 0.45f);
            AddRoomNPC(10, new Vector2(288f, (RoomDimensions.Y * 16f) - 48f), NPCID.BlueArmoredBones, 240, 120, 0.45f);
            AddRoomNPC(11, new Vector2((RoomDimensions.X * 16f) - 288f, (RoomDimensions.Y * 16f) - 48f), NPCID.BlueArmoredBones, 240, 120, 0.45f);
            AddRoomNPC(12, new Vector2((RoomDimensions.X * 16f / 2f) - 64, (RoomDimensions.Y * 16f) - 48f), NPCID.BlueArmoredBones, 480, 120, 0.45f);
            AddRoomNPC(13, new Vector2((RoomDimensions.X * 16f / 2f) + 64, (RoomDimensions.Y * 16f) - 48f), NPCID.BlueArmoredBones, 480, 120, 0.45f);
        }
    }
}
