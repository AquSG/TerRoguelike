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
    public sealed class ActivateOnKillPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.ActivateOnKillSync;
        public static void Send(int npc, int npcType, Vector2 pos, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            var packet = NewPacket(PacketType.ActivateOnKillSync);

            packet.Write(npc);
            packet.Write(npcType);
            packet.WriteVector2(pos);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int who = packet.ReadInt32();
            int whoType = packet.ReadInt32();
            Vector2 pos = packet.ReadVector2();

            if (who < 0) return;

            var modPlayer = Main.LocalPlayer.ModPlayer();
            if (modPlayer == null) return;

            NPC npc = Main.npc[who];
            bool jank = false;
            if (!npc.active || npc.type != whoType)
            {
                jank = true;
                npc = NPC.NewNPCDirect(NPC.GetSource_None(), pos, whoType);
            }

            modPlayer.OnKillEffects(npc);

            if (jank)
                npc.active = false;
        }
    }
}
