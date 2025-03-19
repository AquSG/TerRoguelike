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
using Terraria.ModLoader.IO;
using static TerRoguelike.Managers.ItemManager;
using Terraria.Audio;
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Packets
{
    public sealed class TerPlayerPacket : TerRoguelikePacket
    {
        public static int cooldown;
        public override PacketType MessageType => PacketType.TerPlayerSync;
        public static void Send(TerRoguelikePlayer modPlayer, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;
            if (TerRoguelike.mpClient && cooldown > 0)
                return;
            cooldown = 10;

            var packet = NewPacket(PacketType.TerPlayerSync);

            packet.Write((byte)modPlayer.Player.whoAmI);
            bool falseSun = modPlayer.theFalseSun > 0;
            packet.Write(falseSun);
            if (falseSun)
            {
                packet.Write(modPlayer.theFalseSunIntensity);
                packet.Write(modPlayer.theFalseSunIntensityTarget);
                for (int i = 0; i < modPlayer.theFalseSunTarget.Length; i++)
                {
                    packet.Write(modPlayer.theFalseSunTarget[i]);
                    packet.Write(modPlayer.theFalseSunTargetExtra[i]);
                }
            }
            
            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int who = (int)packet.ReadByte();
            bool falseSun = packet.ReadBoolean();
            int[] target = [-1, -1, -1];
            int[] targetExtra = [-1, -1, -1];
            float intensity = 0;
            float intensityTarget = 0;
            if (falseSun)
            {
                intensity = packet.ReadSingle();
                intensityTarget = packet.ReadSingle();
                for (int i = 0; i < target.Length; i++)
                {
                    target[i] = packet.ReadInt32();
                    targetExtra[i] = packet.ReadInt32();
                }
            }

            Player player = Main.player[who];
            if (!player.active)
                return;
            var modPlayer = player.ModPlayer();
            if (modPlayer == null)
                return;

            if (falseSun)
            {
                for (int i = 0; i < target.Length; i++)
                {
                    if (target[i] == -1 && modPlayer.theFalseSunTarget[i] != -1)
                    {
                        modPlayer.theFalseSunTime[i] = -10;
                        continue;
                    }
                    if (targetExtra[i] == -1 && modPlayer.theFalseSunTargetExtra[i] != -1)
                    {
                        modPlayer.theFalseSunTime[i] = -10;
                        continue;
                    }
                }
                modPlayer.theFalseSunTarget = target;
                modPlayer.theFalseSunTargetExtra = targetExtra;
                modPlayer.theFalseSunIntensity = intensity;
                modPlayer.theFalseSunIntensityTarget = intensityTarget;
            }

            if (Main.dedServ)
                Send(modPlayer, -1, sender);
        }
    }
}
