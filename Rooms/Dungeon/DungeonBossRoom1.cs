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
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.ModLoader;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.World;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace TerRoguelike.Rooms
{
    public class DungeonBossRoom1 : Room
    {
        public Texture2D skeletronTex = null;
        public override int AssociatedFloor => FloorDict["Dungeon"];
        public override string Key => "DungeonBossRoom1";
        public override string Filename => "Schematics/RoomSchematics/DungeonBossRoom1.csch";
        public override bool IsBossRoom => true;
        public override bool HasTransition => true;
        public override Vector2 bossSpawnPos => new Vector2(RoomDimensions.X * 8f, RoomDimensions.Y * 8f);
        public override void InitializeRoom()
        {
            base.InitializeRoom();
            AddBoss(bossSpawnPos, ModContent.NPCType<Skeletron>());
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

            int npcType = ModContent.NPCType<Skeletron>();
            if (skeletronTex == null)
                skeletronTex = TextureAssets.Npc[npcType].Value;
            Vector2 drawPos = bossSpawnPos + RoomPosition16;
            drawPos = TileCollidePositionInLine(drawPos, drawPos + new Vector2(0, 1000));
            drawPos += new Vector2(0, -40);
            Rectangle frame = new Rectangle(0, 0, skeletronTex.Width, (skeletronTex.Height / Main.npcFrameCount[npcType]) - 2);
            Main.EntitySpriteDraw(skeletronTex, drawPos - Main.screenPosition, frame, Lighting.GetColor(drawPos.ToTileCoordinates()), Skeletron.spawnRotation, frame.Size() * 0.5f, 1f, SpriteEffects.None);
        }
    }
}
