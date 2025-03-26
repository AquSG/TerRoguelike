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
    public sealed class NpcStickPacket : TerRoguelikePacket
    {
        // used for brain suckler
        public override PacketType MessageType => PacketType.NpcStickSync;
        public static void Send(NPC npc, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.NpcStickSync);

            packet.Write(npc.whoAmI);
            packet.Write(npc.type);
            packet.Write(npc.ai[0]);
            packet.Write(npc.ai[1]);
            packet.Write(npc.ai[2]);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int who = packet.ReadInt32();
            int type = packet.ReadInt32();
            float ai0 = packet.ReadSingle();
            float ai1 = packet.ReadSingle();
            float ai2 = packet.ReadSingle();
            NPC npc = Main.npc[who];
            if (!npc.active || npc.type != type) return;

            npc.ai[0] = ai0;
            npc.ai[1] = ai1;
            npc.ai[2] = ai2;

            if (Main.dedServ)
                Send(npc, -1, sender);
        }
    }
}
