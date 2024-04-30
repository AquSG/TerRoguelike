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

namespace TerRoguelike.Rooms
{
    public class JungleEnemyRoom5Down : Room
    {
        public override int AssociatedFloor => FloorDict["Jungle"];
        public override string Key => "JungleEnemyRoom5Down";
        public override string Filename => "Schematics/RoomSchematics/JungleEnemyRoom5Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(Top, 21, 10), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Top, -8, 8), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 24, -9), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 18, -16), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Left, 6, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, -2, -8), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, -5, 0), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
        }
    }
}
