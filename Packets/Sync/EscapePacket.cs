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
using TerRoguelike.MainMenu;

namespace TerRoguelike.Packets
{
    public sealed class EscapePacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.EscapeSync;
        public enum EscapeContext
        {
            Start,
            Complete,
        }
        public static void Send(EscapeContext context, int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            var packet = NewPacket(PacketType.EscapeSync);

            packet.Write((byte)context);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            EscapeContext context = (EscapeContext)packet.ReadByte();

            if (context == EscapeContext.Start)
            {
                TerRoguelikeWorld.StartEscapeSequence();
            }
            else if (context == EscapeContext.Complete)
            {
                TerRoguelikeWorld.escaped = true;
                TerRoguelikeWorld.escape = false;
                foreach (Player player in Main.ActivePlayers)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer != null)
                    {
                        modPlayer.escaped = true;
                    }
                }
                for (int L = 0; L < RoomSystem.RoomList.Count; L++)
                {
                    RoomSystem.ResetRoomID(RoomSystem.RoomList[L].ID);
                }
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    Main.npc[i].active = false;
                }
                foreach (var enemy in SpawnManager.pendingEnemies)
                {
                    enemy.spent = true;
                }
                if (!Main.dedServ)
                {
                    MusicSystem.SetBossTrack(MusicSystem.FinalBoss2PreludeTheme, 2);
                    MusicSystem.CombatVolumeInterpolant = 0;
                }
            }
        }
    }
}
