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
    public sealed class ItemPotentiometerPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.ItemPotentiometerSync;
        public static void Send(Vector2 pos, int toClient = -1, int ignoreClient = -1)
        {
            if (!TerRoguelike.mpClient)
                return;

            var packet = NewPacket(PacketType.ItemPotentiometerSync);

            packet.WriteVector2(pos);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            Vector2 pos = packet.ReadVector2();
            if (sender < 0)
                return;

            if (!Main.player[sender].active) return;

            var modPlayer = Main.player[sender].ModPlayer();
            if (modPlayer == null) return;

            modPlayer.SpawnRoguelikeItem(pos);
        }
    }
}
