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
using static TerRoguelike.MainMenu.TerRoguelikeMenu;

namespace TerRoguelike.Packets
{
    public sealed class RemedialOrbPacket : TerRoguelikePacket
    {
        // used for brain suckler
        public override PacketType MessageType => PacketType.RemedialOrbSync;
        public static void Send(RoomSystem.RemedialHealingOrb orb, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.RemedialOrbSync);

            packet.WriteVector2(orb.position);
            packet.WriteVector2(orb.velocity);
            packet.Write(orb.maxUpdates);
            packet.Write(orb.timeLeft);
            packet.Write(orb.owner);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            Vector2 pos = packet.ReadVector2();
            Vector2 vel = packet.ReadVector2();
            int maxUpdate = packet.ReadInt32();
            int time = packet.ReadInt32();
            int owner = packet.ReadInt32();
            RoomSystem.RemedialHealingOrb orb = new(pos, vel, time / maxUpdate, owner, maxUpdate, false);
            RoomSystem.remedialHealingOrbs.Add(orb);

            if (Main.dedServ)
                Send(orb, -1, sender);
        }
    }
}
