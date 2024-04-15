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
using static TerRoguelike.Managers.NPCManager;
using static TerRoguelike.Schematics.SchematicManager;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;

namespace TerRoguelike.Rooms
{
    public class SnowEnemyRoom4 : Room
    {
        public override int AssociatedFloor => FloorDict["Snow"];
        public override string Key => "SnowEnemyRoom4";
        public override string Filename => "Schematics/RoomSchematics/SnowEnemyRoom4.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Center, 0, 1), ModContent.NPCType<IceSpirit>(), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -7, 5), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 5, -5), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 11, -2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -11, 11), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, -20, 11), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
        }
    }
}
