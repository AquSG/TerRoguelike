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
    public class BaseEnemyRoom5Down : Room
    {
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseEnemyRoom5Down";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom5Down.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(PercentPosition(0.5f, 0.10f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.92f, 0.45f), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.92f, 0.85f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);

            AddRoomNPC(PercentPosition(0.08f, 0.45f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(PercentPosition(0.92f, 0.45f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f, 1);
            AddRoomNPC(PercentPosition(0.5f, 0.10f), ChooseEnemy(AssociatedFloor, 1), 180, 120, 0.45f, 1);
        }
    }
}
