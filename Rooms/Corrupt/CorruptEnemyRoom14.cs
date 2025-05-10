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
    public class CorruptEnemyRoom14 : Room
    {
        public override int AssociatedFloor => FloorDict["Corrupt"];
        public override string Key => "CorruptEnemyRoom14";
        public override string Filename => "Schematics/RoomSchematics/CorruptEnemyRoom14.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => false;
        public override bool CanExitUp => false;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Right, -5, -8, 0), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 8, -7, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 0, -4), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 0, 4), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Right, -3, 8), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
        }
    }
}
