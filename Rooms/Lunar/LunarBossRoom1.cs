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
using TerRoguelike.World;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Rooms
{
    public class LunarBossRoom1 : Room
    {
        public override int AssociatedFloor => 10;
        public override string Key => "LunarBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/LunarBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
        }
        public override void Update()
        {
            base.Update();
            if (TerRoguelikeWorld.lunarFloorInitialized && !TerRoguelikeWorld.lunarBossSpawned)
            {
                if (RoomID[RoomDict["LunarPillarRoomTopLeft"]].closedTime > 120 && RoomID[RoomDict["LunarPillarRoomTopRight"]].closedTime > 120 && RoomID[RoomDict["LunarPillarRoomBottomLeft"]].closedTime > 120 && RoomID[RoomDict["LunarPillarRoomBottomRight"]].closedTime > 120)
                {
                    AddRoomNPC(new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f), NPCID.MoonLordCore, 1, 1, 0.9f);
                    TerRoguelikeWorld.lunarBossSpawned = true;
                }
                else
                    awake = false;
            }
        }
        public override bool ClearCondition(bool anyAlive)
        {
            if (!TerRoguelikeWorld.lunarBossSpawned)
                return false;
            else
                return base.ClearCondition(anyAlive);
        }
        public override bool StartCondition(bool awake)
        {
            if (!TerRoguelikeWorld.lunarBossSpawned)
                return false;
            else
                return base.StartCondition(awake);
        }
    }
}
