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
using static TerRoguelike.Managers.NPCManager;
using static TerRoguelike.Schematics.SchematicManager;

namespace TerRoguelike.Rooms
{
    public class SnowEnemyRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Snow"];
        public override string Key => "SnowEnemyRoom1";
        public override string Filename => "Schematics/RoomSchematics/SnowEnemyRoom1.csch";
        public override bool CanExitRight => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Center, 18, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 14, 2), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 0, 0), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);

            AddRoomNPC(MakeEnemySpawnPos(Left, 4, -1), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
            AddRoomNPC(MakeEnemySpawnPos(Left, 9, 3), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
        }
    }
}
