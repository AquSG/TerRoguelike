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
    public sealed class ApplyIgnitePacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.ApplyIgniteSync;
        public static void Send(IgnitedStack ignite, int npc, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.ApplyIgniteSync);

            packet.Write(npc);
            packet.Write(ignite.DamageToDeal);
            packet.Write(ignite.Owner);
            packet.Write(ignite.DamageCapPerTick);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int who = packet.ReadInt32();
            int damage = packet.ReadInt32();
            int owner = packet.ReadInt32();
            int damageCap = packet.ReadInt32();

            if (who >= 0)
            {
                NPC npc = Main.npc[who];
                if (!npc.active)
                    return;
                var modNPC = npc.ModNPC();
                if (modNPC == null)
                    return;
                modNPC.AddIgniteStack(new(damage, owner, damageCap), who, true);
            }

            if (Main.dedServ)
                Send(new(damage, owner, damageCap), who, -1, sender);
        }
    }
}
