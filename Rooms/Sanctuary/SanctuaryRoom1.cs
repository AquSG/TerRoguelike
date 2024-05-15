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
    public class SanctuaryRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Sanctuary"];
        public override string Key => "SanctuaryRoom1";
        public override string Filename => "Schematics/RoomSchematics/SanctuaryRoom1.csch";
        public override bool IsStartRoom => true;
        public override bool IsSanctuary => true;
        public override bool ActivateNewFloorEffects => false;
        public override void InitializeRoom()
        {
            if (!initialized && Main.LocalPlayer != null)
            {
                for (int i = 0; i < TerRoguelikeWorld.itemBasins.Count; i++)
                {
                    var basin = TerRoguelikeWorld.itemBasins[i];
                    basin.itemDisplay = ItemManager.ChooseItemUnbiased((int)basin.tier);
                    basin.GenerateItemOptions(Main.LocalPlayer);
                }
            }
            initialized = true;
        }
        public override void Update()
        {
            if (!awake)
                initialized = false;

            active = false;
            base.Update();
            awake = false;
        }
        public override bool CanDescend(Player player, TerRoguelikePlayer modPlayer)
        {
            modPlayer.noThrow = true;
            return !TerRoguelikeWorld.escape && player.position.X + player.width >= ((RoomPosition.X + RoomDimensions.X) * 16f) - 22f && !player.dead;
        }
        public override Vector2 DescendTeleportPosition()
        {
            return RoomPosition16 + RoomDimensions16 * new Vector2(0.125f, 0.6f);
        }
        public override bool CanAscend(Player player, TerRoguelikePlayer modPlayer)
        {
            return player.position.X <= (RoomPosition.X * 16f) + 22f && !player.dead && modPlayer.escaped;
        }
        public override void Ascend(Player player)
        {
            player.Center = new Vector2(Main.maxTilesX * 8, 3000);
            SetBossTrack(FinalBoss2Theme);
        }
    }
}
