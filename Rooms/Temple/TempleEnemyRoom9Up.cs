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
    public class TempleEnemyRoom9Up : Room
    {
        public override int AssociatedFloor => FloorDict["Temple"];
        public override string Key => "TempleEnemyRoom9Up";
        public override string Filename => "Schematics/RoomSchematics/TempleEnemyRoom9Up.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => false;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 7, 16, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -6, 16, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 7, -16), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -7, -16), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 0);
            Vector2 spawnPos = Main.rand.NextBool() ? MakeEnemySpawnPos(BottomRight, -9, -6, 0) : MakeEnemySpawnPos(BottomLeft, 11, -6, 0);
            AddRoomNPC(spawnPos, ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            spawnPos = Main.rand.NextBool() ? MakeEnemySpawnPos(TopLeft, 15, 13) : MakeEnemySpawnPos(Left, 17, -6);
            AddRoomNPC(spawnPos, ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
            spawnPos = Main.rand.NextBool() ? MakeEnemySpawnPos(TopRight, -14, 13) : MakeEnemySpawnPos(Right, -16, -6);
            AddRoomNPC(spawnPos, ChooseEnemy(AssociatedFloor, 2), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 1, 3, 0), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);
        }
    }
}
