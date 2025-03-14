using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Rooms;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TerRoguelike.Schematics;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Items;
using TerRoguelike.Items.Common;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Items.Rare;
using Terraria.ModLoader.Core;
using TerRoguelike.NPCs.Enemy;
using TerRoguelike.NPCs.Enemy.Pillar;
using TerRoguelike.NPCs;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Particles;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using ReLogic.Threading;
using System.Diagnostics;
using rail;
using System.IO;
using System.Reflection;
using TerRoguelike.World;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static TerRoguelike.NPCs.TerRoguelikeGlobalNPC;
using static TerRoguelike.Systems.MusicSystem;
using TerRoguelike.Floors;

namespace TerRoguelike.Packets
{
    public sealed class TeleportToPositionPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.TeleportToPosition;
        public enum TeleportContext
        {
            Room,
            FloorTransition,
            NewFloor,
            Sanctuary,
            TrueBrain,
            Misc,
        }
        public static void Send(Vector2 position, TeleportContext context, int targetRoomID = -1, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.TeleportToPosition);

            packet.WriteVector2(position);
            packet.Write((byte)context);
            packet.Write(targetRoomID);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            Vector2 pos = packet.ReadVector2();
            TeleportContext context = (TeleportContext)packet.ReadByte();
            int roomID = packet.ReadInt32();

            foreach (Player player in Main.ActivePlayers)
            {
                if (context != TeleportContext.Room && player.dead)
                    player.Spawn(PlayerSpawnContext.ReviveFromDeath);
                player.Center = pos;
                player.fallStart = (int)(player.position.Y / 16f);
            }
            

            if (context == TeleportContext.FloorTransition)
            {
                RoomSystem.FloorTransitionEffects();
            }
            if (context == TeleportContext.Sanctuary)
            {
                RoomSystem.FloorTransitionEffects();
                var modPlayer = Main.LocalPlayer.ModPlayer();
                if (modPlayer != null)
                {
                    modPlayer.LunarCharmLogic(Main.LocalPlayer.Center);
                }
                if (modPlayer?.escaped != true)
                {
                    SetCalm(FloorID[FloorDict["Sanctuary"]].Soundtrack.CalmTrack);
                    SetCombat(FloorID[FloorDict["Sanctuary"]].Soundtrack.CombatTrack);
                    SetMusicMode(MusicStyle.AllCalm);
                    CombatVolumeInterpolant = 0;
                    CalmVolumeInterpolant = 0;
                    CalmVolumeLevel = FloorID[FloorDict["Sanctuary"]].Soundtrack.Volume;
                    CombatVolumeLevel = FloorID[FloorDict["Sanctuary"]].Soundtrack.Volume;
                }
            }
            if (context == TeleportContext.NewFloor)
            {
                Room room = RoomID[roomID];
                if (Main.LocalPlayer.ModPlayer() != null)
                    RoomSystem.NewFloorEffects(room, Main.LocalPlayer.ModPlayer());
                RoomSystem.FloorTransitionEffects();
            }
            if (context == TeleportContext.TrueBrain)
            {
                Room room = RoomID[roomID];
                if (Main.LocalPlayer.ModPlayer() != null)
                    RoomSystem.NewFloorEffects(room, Main.LocalPlayer.ModPlayer());
                RoomSystem.FloorTransitionEffects();
                SetBossTrack(FinalBoss2Theme);
            }
        }
    }
}
