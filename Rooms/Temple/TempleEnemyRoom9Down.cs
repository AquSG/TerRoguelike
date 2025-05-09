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
    public class TempleEnemyRoom9Down : Room
    {
        public override int AssociatedFloor => FloorDict["Temple"];
        public override string Key => "TempleEnemyRoom9Down";
        public override string Filename => "Schematics/RoomSchematics/TempleEnemyRoom9Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => false;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(MakeEnemySpawnPos(TopLeft, 7, 16, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(TopRight, -6, 16, 0), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Center, 1, 3, 0), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomLeft, 12, -6), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(BottomRight, -11, -6), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 0);
            Vector2 spawnPos = Main.rand.NextBool() ? MakeEnemySpawnPos(Center, -9, -5) : MakeEnemySpawnPos(BottomLeft, 7, -16);
            AddRoomNPC(spawnPos, ChooseEnemy(AssociatedFloor, 0), 240, 120, 0.45f, 0);
            spawnPos = Main.rand.NextBool() ? MakeEnemySpawnPos(Center, 9, -5) : MakeEnemySpawnPos(BottomRight, -7, -16);
            AddRoomNPC(spawnPos, ChooseEnemy(AssociatedFloor, 0), 240, 120, 0.45f, 0);
            AddRoomNPC(MakeEnemySpawnPos(Bottom, 1, -13, 0), ChooseEnemy(AssociatedFloor, 1), 240, 120, 0.45f, 0);

        }
    }
}
