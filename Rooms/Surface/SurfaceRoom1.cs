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
using TerRoguelike.World;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using Terraria.Audio;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;
using Microsoft.Xna.Framework.Graphics;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Systems.RoomSystem;
using TerRoguelike.Utilities;
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Rooms
{
    public class SurfaceRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Surface"];
        public override string Key => "SurfaceRoom1";
        public override string Filename => "Schematics/RoomSchematics/SurfaceRoom1.csch";
        public override bool IsStartRoom => true;
        public override bool IsBossRoom => true;
        public override Point WallInflateModifier => new Point(-48, 0);
        public override bool AllowWallDrawing => false;
        public override void Update()
        {
            if (bossSpawnPos == Vector2.Zero)
                bossSpawnPos = new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f);
            base.Update();
        }
        public override void RoomClearReward()
        {
            ClearSpecificProjectiles();
        }
        public override bool CanAscend(Player player, TerRoguelikePlayer modPlayer)
        {
            return false;
        }
        public override bool CanDescend(Player player, TerRoguelikePlayer modPlayer)
        {
            return false;
        }
    }
}
