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
using static TerRoguelike.Managers.NPCManager;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.World;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy;
using TerRoguelike.NPCs.Enemy.Boss;

namespace TerRoguelike.Rooms
{
    public class BaseBossRoom1 : Room
    {
        public Texture2D paladinHammerTex = null;
        public override int AssociatedFloor => FloorDict["Base"];
        public override string Key => "BaseBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/BaseBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddBoss(bossSpawnPos, ModContent.NPCType<Paladin>());
        }
        public override void Update()
        {
            if (bossSpawnPos == Vector2.Zero)
                bossSpawnPos = new Vector2(RoomDimensions16.X * 0.5f, RoomDimensions16.Y - 32f);
            base.Update();
        }
        public override void PostDrawTilesRoom()
        {
            base.PostDrawTilesRoom();
            if (initialized)
                return;

            if (paladinHammerTex == null)
                paladinHammerTex = TexDict["PaladinHammer"];
            Vector2 drawPos = bossSpawnPos + RoomPosition16 + new Vector2(-8, 0);
            Main.EntitySpriteDraw(paladinHammerTex, drawPos - Main.screenPosition, null, Lighting.GetColor(new Point((int)(drawPos.X / 16f), (int)(drawPos.Y / 16f))), MathHelper.Pi, paladinHammerTex.Size() * 0.5f, 1f, SpriteEffects.None);
        }
    }
}
