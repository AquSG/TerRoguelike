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
using static TerRoguelike.Systems.MusicSystem;
using TerRoguelike.Floors;

namespace TerRoguelike.Packets
{
    public sealed class StartCutscenePacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.StartCutsceneSync;
        public static void Send(Vector2 cameraTarget, int time, int easeIn, int easeOut, float targetZoom, int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            var packet = NewPacket(PacketType.StartCutsceneSync);

            packet.WriteVector2(cameraTarget);
            packet.Write(time);
            packet.Write(easeIn);
            packet.Write(easeOut);
            packet.Write(targetZoom);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            Vector2 cameraTarget = packet.ReadVector2();
            int time = packet.ReadInt32();
            int easeIn = packet.ReadInt32();
            int easeOut = packet.ReadInt32();
            float targetZoom = packet.ReadSingle();

            CutsceneSystem.SetCutscene(cameraTarget, time, easeIn, easeOut, targetZoom);
        }
    }
}
