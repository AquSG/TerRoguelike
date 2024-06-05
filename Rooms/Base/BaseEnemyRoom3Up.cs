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
    public class BaseEnemyRoom3Up : Room
    {
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseEnemyRoom3Up";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom3Up.csch";
        public override bool CanExitRight => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(PercentPosition(0.2f, 0.33f), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.8f, 0.33f), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.21f, 0.85f) - Vector2.UnitY * 16, ChooseEnemy(AssociatedFloor, 0), 180, 120, 0.45f);

            AddRoomNPC(PercentPosition(0.2f, 0.33f), ChooseEnemy(AssociatedFloor, 1), 60, 120, 0.45f, 1);
            AddRoomNPC(PercentPosition(0.8f, 0.33f), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f, 1);
        }
    }
}
