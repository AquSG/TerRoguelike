﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerRoguelike.World;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using Terraria.Audio;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;
using Microsoft.Xna.Framework.Graphics;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Systems.RoomSystem;
using Terraria.GameContent.Events;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.TerPlayer;
using TerRoguelike.Utilities;
using TerRoguelike.Items.Rare;
using TerRoguelike.Items;
using TerRoguelike.Packets;
using static TerRoguelike.Packets.EscapePacket;

namespace TerRoguelike.Rooms
{
    public class LunarBossRoom1 : Room
    {
        public override int AssociatedFloor => FloorDict["Lunar"];
        public override string Key => "LunarBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/LunarBossRoom1.csch";
        public override bool IsBossRoom => true;
        public bool musicPlayed = false;
        public static Texture2D moonLordTex;
        public override Vector2 bossSpawnPos => new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 11f);
        public override void InitializeRoom()
        {
            if (!TerRoguelikeWorld.lunarBossSpawned)
                base.InitializeRoom();
            else
                initialized = true;
        }
        public override void Update()
        {
            if (TerRoguelikeWorld.lunarFloorInitialized && active)
            {
                if (RoomID[RoomDict["LunarPillarRoomTopLeft"]].closedTime > 0 && RoomID[RoomDict["LunarPillarRoomTopRight"]].closedTime > 0 && RoomID[RoomDict["LunarPillarRoomBottomLeft"]].closedTime > 0 && RoomID[RoomDict["LunarPillarRoomBottomRight"]].closedTime > 0)
                {
                    if (!awake)
                        SetMusicMode(MusicStyle.Silent);
                }
                else
                {
                    awake = false;
                    roomTime = 0;
                }
            }
            if (TerRoguelikeWorld.lunarFloorInitialized && !TerRoguelikeWorld.lunarBossSpawned)
            {
                if (!TerRoguelike.mpClient)
                    AddBoss(bossSpawnPos, ModContent.NPCType<MoonLord>());
                TerRoguelikeWorld.lunarBossSpawned = true;
            }
            base.Update();
        }
        public override void OnEnter()
        {
            if (!awake)
            {
                entered = false;
                return;
            }

            base.OnEnter();
        }
        public override bool ClearCondition()
        {
            if (!TerRoguelikeWorld.lunarBossSpawned)
                return false;
            else
                return base.ClearCondition();
        }
        public override bool StartCondition()
        {
            if (!awake)
                return false;
            else
                return base.StartCondition();
        }
        public override void RoomClearReward()
        {
            base.RoomClearReward();
            TerRoguelikeWorld.StartEscapeSequence();
            EscapePacket.Send(EscapeContext.Start);

            bool allowLunarGambitSequence = false;
            int playerSubtract = -1;
            Vector2 lunarGambitStartPos = Vector2.Zero;
            int checkType = ModContent.ItemType<LunarGambit>();
            foreach (Player player in Main.ActivePlayers)
            {
                for (int i = 0; i < 50; i++)
                {
                    Item item = player.inventory[i];
                    if (item.type == checkType)
                    {
                        item.stack--;
                        allowLunarGambitSequence = true;
                        lunarGambitStartPos = player.Center;
                        playerSubtract = player.whoAmI;
                        break;
                    }
                }
                if (allowLunarGambitSequence)
                    break;
            }

            if (allowLunarGambitSequence)
            {
                TerRoguelikeWorld.lunarGambitSceneTime = 1;
                TerRoguelikeWorld.lunarGambitSceneStartPos = lunarGambitStartPos;
                StartLoopPortalPacket.Send(playerSubtract);
            }
        }
        public override void PostDrawTilesRoom()
        {
            base.PostDrawTilesRoom();
        }
        public override bool CanAscend(Player player, TerRoguelikePlayer modPlayer)
        {
            if (modPlayer.escapeArrowTime > 0 && modPlayer.escapeArrowTime < 200 && modPlayer.escapeArrowTime % 125 == 0) // basically if the player is just vibing in the boss room not knowing what to do, keep extending the escape arrow time
                modPlayer.escapeArrowTime += 125;
            return base.CanAscend(player, modPlayer);
        }
    }
}
