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
    public class WorldRoom : Room
    {
        public override void InitializeRoom()
        {
            RoomPosition = new Vector2(9900, 4300);
            AddRoomNPC(0, new Vector2(50, 0), NPCID.Mummy, 180, 120, 0.5f);
            AddRoomNPC(1, new Vector2(350, 0), NPCID.Mummy, 180, 120, 0.5f);
            AddRoomNPC(2, new Vector2(200, 100), NPCID.RainbowSlime, 600, 60, 0.7f);
            AddRoomNPC(3, new Vector2(50, 150), NPCID.CaveBat, 900, 30, 0.4f);
            AddRoomNPC(4, new Vector2(350, 150), NPCID.CaveBat, 960, 30, 0.4f);
            AddRoomNPC(5, new Vector2(50, 150), NPCID.CaveBat, 1020, 30, 0.4f);
            AddRoomNPC(6, new Vector2(350, 150), NPCID.CaveBat, 1080, 30, 0.4f);
            AddRoomNPC(7, new Vector2(50, 150), NPCID.CaveBat, 1140, 30, 0.4f);
            AddRoomNPC(8, new Vector2(350, 150), NPCID.CaveBat, 1200, 30, 0.4f);
            AddRoomNPC(9, new Vector2(50, 150), NPCID.Mummy, 1320, 60, 0.5f);
            AddRoomNPC(10, new Vector2(350, 150), NPCID.Mummy, 1320, 60, 0.5f);
            AddRoomNPC(11, new Vector2(0, 150), NPCID.Mummy, 1340, 60, 0.5f);
            AddRoomNPC(12, new Vector2(400, 150), NPCID.Mummy, 1340, 60, 0.5f);
            AddRoomNPC(13, new Vector2(-50, 150), NPCID.Mummy, 1360, 60, 0.5f);
            AddRoomNPC(14, new Vector2(450, 150), NPCID.Mummy, 1360, 60, 0.5f);
            AddRoomNPC(15, new Vector2(-100, 150), NPCID.Mummy, 1380, 60, 0.5f);
            AddRoomNPC(16, new Vector2(500, 150), NPCID.Mummy, 1380, 60, 0.5f);
            AddRoomNPC(17, new Vector2(-150, 150), NPCID.Mummy, 1400, 60, 0.5f);
            AddRoomNPC(18, new Vector2(550, 150), NPCID.Mummy, 1400, 60, 0.5f);
        }
    }
}
