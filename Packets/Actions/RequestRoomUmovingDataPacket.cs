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

namespace TerRoguelike.Packets
{
    public sealed class RequestRoomUmovingDataPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.RequestUnmovingDataSync;
        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            if (!TerRoguelike.mpClient)
                return;

            var packet = NewPacket(PacketType.RequestUnmovingDataSync);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            if (Main.dedServ)
            {
                RoomUnmovingDataPacket.Send();
            }
        }
    }
}
