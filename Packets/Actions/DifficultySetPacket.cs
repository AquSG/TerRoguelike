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
    public sealed class DifficultySetPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.DifficultySetSync;
        public static void Send(Difficulty diff, int toClient = -1, int ignoreClient = -1)
        {
            if (!TerRoguelike.mpClient)
                return;

            var packet = NewPacket(PacketType.DifficultySetSync);

            // WHY DOES ENUM NOT FUCKING WORK
            // 5
            // read underflow 2 of 1 bytes
            packet.Write(diff == Difficulty.SunnyDay);
            packet.Write(diff == Difficulty.NewMoon);
            packet.Write(diff == Difficulty.FullMoon);
            packet.Write(diff == Difficulty.BloodMoon);
            packet.Write(diff == Difficulty.RuinedMoon);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            if (Main.dedServ && !TerRoguelikeWorld.difficultyReceivedByServer)
            {
                TerRoguelikeWorld.difficultyReceivedByServer = true;
                if (packet.ReadBoolean())
                {
                    difficulty = Difficulty.SunnyDay;
                }
                if (packet.ReadBoolean())
                {
                    difficulty = Difficulty.NewMoon;
                }
                if (packet.ReadBoolean())
                {
                    difficulty = Difficulty.FullMoon;
                }
                if (packet.ReadBoolean())
                {
                    difficulty = Difficulty.BloodMoon;
                }
                if (packet.ReadBoolean())
                {
                    difficulty = Difficulty.RuinedMoon;
                }
            }
        }
    }
}
