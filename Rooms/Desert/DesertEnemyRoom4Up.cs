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
    public class DesertEnemyRoom4Up : Room
    {
        public override int AssociatedFloor => FloorDict["Desert"];
        public override string Key => "DesertEnemyRoom4Up";
        public override string Filename => "Schematics/RoomSchematics/DesertEnemyRoom4Up.csch";
        public override bool CanExitRight => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Center, 18, 3), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, -11, 3), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -5, -5), ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 5, -2), ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, 2, 5), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);
        }
    }
}
