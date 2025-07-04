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
using Terraria.ModLoader.IO;
using static TerRoguelike.Managers.ItemManager;
using Terraria.Audio;
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Packets
{
    public sealed class TerPlayerPacket : TerRoguelikePacket
    {
        public static int cooldown;
        public override PacketType MessageType => PacketType.TerPlayerSync;
        public static void Send(TerRoguelikePlayer modPlayer, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;
            if (TerRoguelike.mpClient && cooldown > 0)
                return;
            cooldown = 10;

            var packet = NewPacket(PacketType.TerPlayerSync);

            packet.Write((byte)modPlayer.Player.whoAmI);

            packet.Write(modPlayer.swingAnimCompletion);
            packet.Write(modPlayer.verticalSwingDirection);
            packet.Write(modPlayer.sluggedTime);
            packet.Write(modPlayer.barrierHealth);

            packet.Write(modPlayer.enableTrash);

            bool falseSun = modPlayer.theFalseSun > 0;
            packet.Write(falseSun);
            if (falseSun)
            {
                packet.Write(modPlayer.theFalseSunIntensity);
                packet.Write(modPlayer.theFalseSunIntensityTarget);
                for (int i = 0; i < modPlayer.theFalseSunTarget.Length; i++)
                {
                    packet.Write(modPlayer.theFalseSunTarget[i]);
                    packet.Write(modPlayer.theFalseSunTargetExtra[i]);
                }
            }

            bool thrill = modPlayer.thrillOfTheHunt > 0;
            packet.Write(thrill);
            if (thrill)
            {
                packet.Write(modPlayer.thrillOfTheHuntStacks.Count);
            }

            bool evilEye = modPlayer.evilEye > 0;
            packet.Write(evilEye);
            if (evilEye)
            {
                packet.Write(modPlayer.evilEyeStacks.Count);
            }

            bool rattle = modPlayer.primevalRattle > 0;
            packet.Write(rattle);
            if (rattle)
            {
                packet.Write(modPlayer.primevalRattleStacks.Count);
            }

            bool steamEngine = modPlayer.steamEngine > 0;
            packet.Write(steamEngine);
            if (steamEngine)
            {
                packet.Write(modPlayer.steamEngineStacks.Count);
            }

            bool jetLeg = modPlayer.jetLeg > 0;
            packet.Write(jetLeg);
            if (jetLeg)
            {
                packet.Write(modPlayer.DashDir);
                packet.Write(modPlayer.DashDirCache);
                packet.Write(modPlayer.DashTime);
                packet.Write(modPlayer.DashDelay);
                if (modPlayer.DashTime == 20)
                    packet.WriteVector2(modPlayer.Player.velocity);
            }

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int who = (int)packet.ReadByte();

            float swingAnim = packet.ReadSingle();
            int verticalSwingDir = packet.ReadInt32();
            int sluggedTime = packet.ReadInt32();
            float barrierHealth = packet.ReadSingle();

            bool enableTrash = packet.ReadBoolean();

            bool falseSun = packet.ReadBoolean();
            int[] target = [-1, -1, -1];
            int[] targetExtra = [-1, -1, -1];
            float intensity = 0;
            float intensityTarget = 0;
            if (falseSun)
            {
                intensity = packet.ReadSingle();
                intensityTarget = packet.ReadSingle();
                for (int i = 0; i < target.Length; i++)
                {
                    target[i] = packet.ReadInt32();
                    targetExtra[i] = packet.ReadInt32();
                }
            }

            bool thrill = packet.ReadBoolean();
            int thrillStackCount = 0;
            if (thrill)
            {
                thrillStackCount = packet.ReadInt32();
            }

            bool evilEye = packet.ReadBoolean();
            int evilEyeStackCount = 0;
            if (evilEye)
            {
                evilEyeStackCount = packet.ReadInt32();
            }

            bool rattle = packet.ReadBoolean();
            int rattleStackCount = 0;
            if (rattle)
            {
                rattleStackCount = packet.ReadInt32();
            }

            bool steamEngine = packet.ReadBoolean();
            int steamEngineStackCount = 0;
            if (steamEngine)
            {
                steamEngineStackCount = packet.ReadInt32();
            }

            bool jetLeg = packet.ReadBoolean();
            int dashDir = 0;
            int dashDirCache = 0;
            int dashTime = 0;
            int dashDelay = 0;
            Vector2 dashVelocity = Vector2.Zero;
            if (jetLeg)
            {
                dashDir = packet.ReadInt32();
                dashDirCache = packet.ReadInt32();
                dashTime = packet.ReadInt32();
                dashDelay = packet.ReadInt32();
                if (dashTime == 20)
                    dashVelocity = packet.ReadVector2();
            }

            Player player = Main.player[who];
            if (!player.active)
                return;
            var modPlayer = player.ModPlayer();
            if (modPlayer == null)
                return;

            modPlayer.swingAnimCompletion = swingAnim;
            modPlayer.verticalSwingDirection = verticalSwingDir;
            modPlayer.sluggedTime = sluggedTime;
            modPlayer.barrierHealth = barrierHealth;

            modPlayer.enableTrash = enableTrash;

            if (falseSun)
            {
                for (int i = 0; i < target.Length; i++)
                {
                    if (modPlayer.theFalseSunTime[i] <= 0)
                        continue;
                    if (target[i] == -1 && modPlayer.theFalseSunTarget[i] != -1)
                    {
                        modPlayer.theFalseSunTime[i] = -10;
                        continue;
                    }
                    if (targetExtra[i] == -1 && modPlayer.theFalseSunTargetExtra[i] != -1)
                    {
                        modPlayer.theFalseSunTime[i] = -10;
                        continue;
                    }
                }
                modPlayer.theFalseSunTarget = target;
                modPlayer.theFalseSunTargetExtra = targetExtra;
                modPlayer.theFalseSunIntensity = intensity;
                modPlayer.theFalseSunIntensityTarget = intensityTarget;
            }

            if (thrill)
            {
                modPlayer.thrillOfTheHuntStacks.Clear();
                for (int i = 0; i < thrillStackCount; i++)
                    modPlayer.thrillOfTheHuntStacks.Add(480);
            }

            if (evilEye)
            {
                modPlayer.evilEyeStacks.Clear();
                for (int i = 0; i < evilEyeStackCount; i++)
                    modPlayer.evilEyeStacks.Add(180);
            }

            if (rattle)
            {
                modPlayer.primevalRattleStacks.Clear();
                for (int i = 0; i < rattleStackCount; i++)
                    modPlayer.primevalRattleStacks.Add(540);
            }

            if (steamEngine)
            {
                modPlayer.steamEngineStacks.Clear();
                for (int i = 0; i < steamEngineStackCount; i++)
                    modPlayer.steamEngineStacks.Add(7200);
            }

            if (jetLeg)
            {
                if (modPlayer.DashTime == 0 && dashTime > 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot with { Volume = 1f }, player.Center);
                }
                modPlayer.DashDir = dashDir;
                modPlayer.DashDirCache = dashDirCache;
                modPlayer.DashTime = dashTime;
                modPlayer.DashDelay = dashDelay;
                if (dashTime == 20)
                    player.velocity = dashVelocity;
            }

            if (Main.dedServ)
                Send(modPlayer, -1, sender);
        }
    }
}
