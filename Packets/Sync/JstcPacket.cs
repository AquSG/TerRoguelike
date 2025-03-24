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
using TerRoguelike.MainMenu;
using static TerRoguelike.Managers.Floor;
using Terraria.Audio;

namespace TerRoguelike.Packets
{
    public sealed class JstcPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.JstcSync;
        public static void Send(Floor floor, Vector2? teleportPos = null, int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;
            Vector2 pos = teleportPos == null ? Vector2.Zero : (Vector2)teleportPos;

            var packet = NewPacket(PacketType.JstcSync);

            packet.Write((byte)floor.ID);
            packet.Write((byte)floor.jstcProgress);
            packet.WriteVector2(TerRoguelikeWorld.jstcPortalPos);
            packet.Write(TerRoguelikeWorld.jstcPortalRot);
            packet.WriteVector2(TerRoguelikeWorld.jstcPortalScale);
            packet.WriteVector2(pos);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            int floorid = packet.ReadByte();
            JstcProgress context = (JstcProgress)packet.ReadByte();
            TerRoguelikeWorld.jstcPortalPos = packet.ReadVector2();
            TerRoguelikeWorld.jstcPortalRot = packet.ReadSingle();
            TerRoguelikeWorld.jstcPortalScale = packet.ReadVector2();
            Vector2 teleportPos = packet.ReadVector2();

            Floor floor = FloorID[floorid];


            if (context == JstcProgress.Start)
            {

            }
            else if (context == JstcProgress.Enemies)
            {
                if (floor.jstcProgress == JstcProgress.Start)
                {
                    TerRoguelikeWorld.jstcPortalTime = 1;
                    SoundEngine.PlaySound(TerRoguelikeWorld.JstcSpawn with { Volume = 1f, Pitch = -0.2f }, TerRoguelikeWorld.jstcPortalPos);
                }
            }
            else if (context == JstcProgress.EnemyPortal)
            {
                if (floor.jstcProgress == JstcProgress.Enemies)
                {
                    foreach (Player player in Main.ActivePlayers)
                    {
                        if (player.dead)
                            continue;
                        player.velocity = Vector2.Zero;
                        var modPlayer = player.ModPlayer();
                        modPlayer.jstcTeleportTime = 1;
                        modPlayer.jstcTeleportStart = player.Center;
                        modPlayer.jstcTeleportEnd = teleportPos;
                    }
                    TerRoguelikeWorld.jstcPortalTime = -60;
                    CutsceneSystem.SetCutscene(Main.LocalPlayer.Center, 180, 60, 30, 1.5f);
                    SoundEngine.PlaySound(TerRoguelikeWorld.WorldTeleport with { Volume = 0.2f, Variants = [2], Pitch = -0.25f });
                }
            }
            else if (context == JstcProgress.Boss)
            {
                if (floor.jstcProgress == JstcProgress.EnemyPortal)
                {
                    foreach (Player player in Main.ActivePlayers)
                    {
                        if (player.dead)
                            continue;
                        player.velocity = player.position - player.oldPosition;

                        var modPlayer = player.ModPlayer();
                        if (modPlayer == null) continue;

                        player.Center = teleportPos;
                        modPlayer.jstcTeleportTime = 0;
                    }
                }
            }
            else if (context == JstcProgress.BossDeath)
            {
                if (floor.jstcProgress == JstcProgress.Boss)
                {
                    floor.jstcProgress = context;
                    bool allow = true;
                    TerRoguelikeWorld.jstcPortalTime = 1;
                    SoundEngine.PlaySound(TerRoguelikeWorld.JstcSpawn with { Volume = 1f, Pitch = -0.2f }, TerRoguelikeWorld.jstcPortalPos);
                    for (int f = floor.Stage; f < 5; f++)
                    {
                        if (SchematicManager.FloorID[RoomManager.FloorIDsInPlay[f]].jstcProgress < JstcProgress.BossDeath)
                            allow = false;
                    }
                    if (allow)
                        SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/weird") with { Volume = 0.25f });
                }
            }
            else if (context == JstcProgress.BossPortal)
            {
                if (floor.jstcProgress == JstcProgress.BossDeath)
                {
                    foreach (Player player2 in Main.ActivePlayers)
                    {
                        if (player2.dead)
                            continue;
                        player2.velocity = Vector2.Zero;
                        var modPlayer = player2.ModPlayer();
                        modPlayer.jstcTeleportTime = 1;
                        modPlayer.jstcTeleportStart = player2.Center;
                        modPlayer.jstcTeleportEnd = teleportPos;
                    }

                    TerRoguelikeWorld.jstcPortalTime = -60;
                    CutsceneSystem.SetCutscene(Main.LocalPlayer.Center, 180, 60, 30, 1.5f);
                    SoundEngine.PlaySound(TerRoguelikeWorld.WorldTeleport with { Volume = 0.2f, Variants = [2], Pitch = -0.25f });
                }
            }
            else if (context == JstcProgress.Jstc)
            {
                if (floor.jstcProgress == JstcProgress.BossPortal)
                {
                    foreach (Player player in Main.ActivePlayers)
                    {
                        if (player.dead) continue;
                        player.velocity = player.position - player.oldPosition;

                        var modPlayer = player.ModPlayer();
                        if (modPlayer == null) continue;

                        player.Center = teleportPos;
                        modPlayer.jstcTeleportTime = 0;
                    }
                }
            }

            floor.jstcProgress = context;
        }
    }
}
