using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerRoguelike;
using TerRoguelike.TerPlayer;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.GameContent;
using TerRoguelike.NPCs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using TerRoguelike.Projectiles;
using Terraria.Graphics.Shaders;
using TerRoguelike.Systems;
using TerRoguelike.World;

namespace TerRoguelike.NPCs.Enemy.Pillar
{
    public class StardustPillar : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<StardustPillar>();
        public override List<int> associatedFloors => new List<int>() { 10 };
        public override int CombatStyle => -1;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 140;
            NPC.height = 320;
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.lifeMax = 10000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.behindTiles = true;
            modNPC.drawCenter = new Vector2(0, -18);
        }
        public override void AI()
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld && modNPC.isRoomNPC)
            {
                if (RoomSystem.RoomList[modNPC.sourceRoomListID].awake)
                    NPC.immortal = false;
                else
                    NPC.immortal = true;
            }
            NPC.velocity.Y = MathHelper.Lerp(0, 0.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly));
        }
        public override Color? GetAlpha(Color drawColor) => Color.White;
        public override bool? CanBeHitByItem(Player player, Item item)
        {
            return CanBeHit();
        }
        public override bool CanBeHitByNPC(NPC attacker)
        {
            return CanBeHit() == false ? false : true;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return CanBeHit();
        }
        public bool? CanBeHit()
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld && modNPC.isRoomNPC)
            {
                if (RoomSystem.RoomList[modNPC.sourceRoomListID].awake)
                    return null;
                return false;
            }
            return null;
        }
        public override void OnKill()
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld && modNPC.isRoomNPC)
            {
                RoomSystem.RoomList[modNPC.sourceRoomListID].haltSpawns = true;
            }
        }
    }
}
