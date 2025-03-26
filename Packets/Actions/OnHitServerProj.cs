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
    public sealed class OnHitServerProj : TerRoguelikePacket
    {
        public static bool noSend = false;
        public override PacketType MessageType => PacketType.OnHitServerProjSync;
        public static void Send(Projectile proj, int target, bool player, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            if (noSend) return;

            var modProj = proj.ModProj();
            if (modProj == null) return;

            var packet = NewPacket(PacketType.OnHitServerProjSync);

            packet.Write(modProj.multiplayerIdentifier);
            packet.Write(proj.type);
            packet.Write(target);
            packet.Write(player);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int who = packet.ReadInt32();
            int type = packet.ReadInt32();
            int target = packet.ReadInt32();
            bool player = packet.ReadBoolean();

            if (who < 0 || target < 0) return;

            Projectile proj = Main.projectile[who];
            if (!proj.active || proj.type != type) return;

            Entity entity = player ? Main.player[target] : Main.npc[target];
            if (!entity.active) return;

            if (Main.dedServ)
                Send(proj, target, player, -1, sender);

            noSend = true;
            if (player)
                ProjectileLoader.OnHitPlayer(proj, Main.player[target], new());
            else
                ProjectileLoader.OnHitNPC(proj, Main.npc[target], new(), 1);
            noSend = false;
        }
    }
}
