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
    public class BaseEnemyRoom6 : Room
    {
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseEnemyRoom6";
        public override string Filename => "Schematics/RoomSchematics/BaseEnemyRoom6.csch";
        public override bool CanExitRight => true;
        public override bool CanExitDown => true;
        public override bool CanExitUp => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddRoomNPC(PercentPosition(0.16f, 0.94f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.84f, 0.94f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.16f, 0.55f), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.84f, 0.55f), ChooseEnemy(AssociatedFloor, 2), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.16f, 0.25f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.84f, 0.25f), ChooseEnemy(AssociatedFloor, 0), 60, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.33f, 0.28f), ChooseEnemy(AssociatedFloor, 1), 180, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.67f, 0.28f), ChooseEnemy(AssociatedFloor, 1), 180, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.05f, 0.08f), ChooseEnemy(AssociatedFloor, 1), 180, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.95f, 0.08f), ChooseEnemy(AssociatedFloor, 1), 180, 120, 0.45f);
            AddRoomNPC(PercentPosition(0.5f, 0.08f), ChooseEnemy(AssociatedFloor, 0), 180, 120, 0.45f);
        }
    }
}
