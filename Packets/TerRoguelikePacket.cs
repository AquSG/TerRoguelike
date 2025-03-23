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
using Terraria.Graphics;

namespace TerRoguelike.Packets
{
    public abstract class TerRoguelikePacket
    {
        public virtual PacketType MessageType { get; private set; }
        public enum PacketType
        {
            RoomUnmovingDataSync,
            RoomSync,
            PendingEnemySync,
            PendingItemSync,
            SpecialPendingItemSync,
            EscapeSync,
            BasinSync,
            StageCountSync,
            EnemyHealthbarSync,
            MouseWorldSync,
            RegenerateWorldSync,
            TerPlayerSync,
            JstcSync,

            TeleportToPosition,
            RequestUnmovingDataSync,
            StartCreditsSync,
            StartBossThemeSync,
            StartCutsceneSync,
            RequestBasinSync,
            DifficultySetSync,
            ApplyBleedSync,
            ApplyIgniteSync,
            ApplyBallAndChainSync,
            StartRoomGenerationSync,
            MakeNPCPuppetSync,
            StartLunarFloorSync,
            StartLoopPortalSync,
            ActivateOnKillSync,
            ItemPotentiometerSync,
            ProgressDialogueSync,
        }
        public abstract void HandlePacket(in BinaryReader packet, int sender);

        internal PropertyInfo _Prop_Static_Instance;

        public static void CloneAndBroadcast(in BinaryReader packet, PacketType type, long startIndex, int length, int ignoreClient = -1)
        {
            if (!Main.dedServ || startIndex < 0)
                return;

            packet.BaseStream.Position = startIndex;

            //limit stack size to 256 bytes
            Span<byte> buffer = length <= 256 ? stackalloc byte[length] : new byte[length];
            packet.BaseStream.Read(buffer);

            var newPacket = NewPacket(type);
            newPacket.Write(buffer);
            newPacket.Send(ignoreClient);
        }

        public static ModPacket NewPacket(PacketType type)
        {
            var packet = TerRoguelike.Instance.GetPacket();
            packet.Write((byte)type);
            return packet;
        }
    }
}
