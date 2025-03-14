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
using static TerRoguelike.Schematics.SchematicManager;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.World;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.Rooms
{
    public class TempleBossRoom1 : Room
    {
        public Texture2D templeGolemTex = null;
        public override int AssociatedFloor => FloorDict["Temple"];
        public override string Key => "TempleBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/TempleBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override Vector2 bossSpawnPos => new Vector2(RoomDimensions.X * 8f - 8f, RoomDimensions.Y * 8f);
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddBoss(bossSpawnPos, ModContent.NPCType<TempleGolem>());
        }
        public override void Update()
        {
            base.Update();
        }
        public override void PostDrawTilesRoom()
        {
            base.PostDrawTilesRoom();
            if (initialized || (TerRoguelikeWorld.escape && FloorID[AssociatedFloor].jstcProgress == Floor.JstcProgress.Start))
                return;

            int npcType = ModContent.NPCType<TempleGolem>();
            if (templeGolemTex == null)
                templeGolemTex = TextureAssets.Npc[npcType].Value;
            Vector2 drawPos = bossSpawnPos + RoomPosition16;
            drawPos = TileCollidePositionInLine(drawPos, drawPos + new Vector2(0, -1000));
            drawPos += new Vector2(0, 45);
            Rectangle frame = new Rectangle(0, 0, templeGolemTex.Width, (templeGolemTex.Height / Main.npcFrameCount[npcType]) - 2);
            Main.EntitySpriteDraw(templeGolemTex, drawPos - Main.screenPosition, frame, Lighting.GetColor(drawPos.ToTileCoordinates()), 0, frame.Size() * 0.5f, 1f, SpriteEffects.None);
        }
    }
}
