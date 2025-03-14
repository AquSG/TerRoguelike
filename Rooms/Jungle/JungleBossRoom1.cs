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
using static TerRoguelike.Schematics.SchematicManager;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.World;

namespace TerRoguelike.Rooms
{
    public class JungleBossRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Jungle"];
        public override string Key => "JungleBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/JungleBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override bool HasTransition => true;
        public override Vector2 bossSpawnPos => new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f);
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddBoss(bossSpawnPos, ModContent.NPCType<QueenBee>());
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
