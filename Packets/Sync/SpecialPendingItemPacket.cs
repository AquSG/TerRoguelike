﻿using System;
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

namespace TerRoguelike.Packets
{
    public sealed class SpecialPendingItemPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.SpecialPendingItemSync;
        public static void Send(PendingItem item, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.SpecialPendingItemSync);

            packet.Write(item.ItemType);
            packet.WriteVector2(item.Position);
            packet.Write(item.ItemTier);
            packet.Write(item.setTelegraphDuration - 120);
            packet.WriteVector2(item.Velocity);
            packet.Write(item.Gravity);
            packet.WriteVector2(item.displayInterpolationStartPos);
            packet.Write(item.itemSacrificeType);
            packet.Write(item.Sound);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int type = packet.ReadInt32();
            Vector2 pos = packet.ReadVector2();
            int tier = packet.ReadInt32();
            int duration = packet.ReadInt32();
            Vector2 velocity = packet.ReadVector2();
            float gravity = packet.ReadSingle();
            Vector2 start = packet.ReadVector2();
            int sacrificetype = packet.ReadInt32();
            bool sound = packet.ReadBoolean();

            PendingItem item = new(type, pos, (ItemTier)tier, duration, velocity, gravity, start, sacrificetype, sound);
            SpawnManager.specialPendingItems.Add(item);

            if (Main.dedServ)
            {
                Send(item);
            }
        }
    }
}
