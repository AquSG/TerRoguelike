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
using TerRoguelike.MainMenu;

namespace TerRoguelike.Packets
{
    public sealed class RegenerateWorldPacket : TerRoguelikePacket
    {
        public override PacketType MessageType => PacketType.RegenerateWorldSync;
        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            var packet = NewPacket(PacketType.RegenerateWorldSync);

            packet.Write(RoomSystem.regeneratingWorld);
            packet.Write(TerRoguelikeWorld.currentLoop > 0);

            packet.Send(toClient, ignoreClient);
        }
        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            bool regenworld = packet.ReadBoolean();
            bool loop = packet.ReadBoolean();
            if (regenworld && !RoomSystem.regeneratingWorld)
            {
                TerRoguelikeMenu.prepareForRoguelikeGeneration = true;
                for (int i = 0; i < Main.maxNPCs; i++)
                    Main.npc[i].active = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                    Main.projectile[i].active = false;
                for (int i = 0; i < Main.maxDust; i++)
                    Main.dust[i].active = false;
                for (int i = 0; i < Main.maxGore; i++)
                    Main.gore[i].active = false;
                for (int i = 0; i < Main.maxCombatText; i++)
                    Main.combatText[i].active = false;

                SetCalm(Silence);
                SetCombat(Silence);
                SetMusicMode(MusicStyle.Silent);
                RoomSystem.ClearWorldTerRoguelike();
                Main.LocalPlayer.Center = new Vector2(Main.spawnTileX, Main.spawnTileY) * 16;

                if (!loop)
                {
                    IEnumerable<Item> vanillaItems = [];
                    for (int i = 0; i < 58; i++)
                        Main.LocalPlayer.inventory[i].type = Main.LocalPlayer.inventory[i].stack = 0;
                    List<Item> startingItems = PlayerLoader.GetStartingItems(Main.LocalPlayer, vanillaItems);
                    PlayerLoader.SetStartInventory(Main.LocalPlayer, startingItems);
                    Main.LocalPlayer.trashItem = new(ItemID.None, 0);
                    TerRoguelikeMenu.desiredPlayer = Main.ActivePlayerFileData;
                }


                for (int i = 0; i < RoomSystem.RoomList.Count; i++)
                    RoomSystem.ResetRoomID(RoomSystem.RoomList[i].ID);
                foreach (var floor in FloorID)
                {
                    floor.Reset();
                }
            }
            RoomSystem.regeneratingWorld = regenworld;
        }
    }
}
