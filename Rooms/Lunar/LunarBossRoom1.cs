using System;
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
        public override void InitializeRoom()
        {
            if (!TerRoguelikeWorld.lunarBossSpawned)
                base.InitializeRoom();
            else
                initialized = true;
        }
        public override void Update()
        {
            if (bossSpawnPos == Vector2.Zero)
                bossSpawnPos = new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 11f);
            if (TerRoguelikeWorld.lunarFloorInitialized)
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
                AddBoss(bossSpawnPos, ModContent.NPCType<MoonLord>());
                TerRoguelikeWorld.lunarBossSpawned = true;
            }
            base.Update();
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
        }
        public override void PostDrawTilesRoom()
        {
            if (false && TerRoguelikeWorld.lunarFloorInitialized && (!TerRoguelikeWorld.lunarBossSpawned || (!awake && closedTime <= 0)))
            {
                if (moonLordTex == null)
                    moonLordTex = TexDict["StillMoonLord"];
                Main.EntitySpriteDraw(moonLordTex, (RoomPosition + (RoomDimensions * 0.5f)) * 16f - Main.screenPosition, null, Color.White * (0.5f + (MathHelper.Lerp(0, 0.125f, 0.5f + ((float)Math.Cos(Main.GlobalTimeWrappedHourly * 2f) * 0.5f)))), 0f, moonLordTex.Size() * 0.5f, 1f, SpriteEffects.None);
            }
            base.PostDrawTilesRoom();
        }
    }
}
