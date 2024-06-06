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
    public class BaseEnemyRoom2 : Room
    {
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseEnemyRoom2";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom2.csch";
        public override bool CanExitRight => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(PercentPosition(0.25f, 0.10f), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.75f, 0.10f), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.95f, 0.83f), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.5f, 0.10f), ChooseEnemy(AssociatedFloor, 0), 120, 120, 0.45f);

            AddRoomNPC(PercentPosition(0.5f, 0.8f) - Vector2.UnitY * 16, ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
        }
    }
}
