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
using TerRoguelike.Packets;

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
        public override Vector2 bossSpawnPos => new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f);
        public override void Update()
        {
            base.Update();
        }
        public override void RoomClearReward()
        {
            Player player = Main.LocalPlayer;
            if (Main.netMode == NetmodeID.SinglePlayer && (!player.active || player.dead))
                return;
            ClearGhosts();
            ClearSpecificProjectiles();
            if (Main.dedServ)
                StartCreditsPacket.Send();
            
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                CreditsSystem.StartCredits();
                var modPlayer = player.ModPlayer();
                if (modPlayer != null)
                {
                    modPlayer.playthroughTime.Stop();
                }
            }
            
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
