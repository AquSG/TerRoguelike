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
using TerRoguelike.Utilities;
using TerRoguelike.TerPlayer;
using TerRoguelike.NPCs.Enemy.Boss;
using TerRoguelike.Items;
using TerRoguelike.Particles;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using Microsoft.CodeAnalysis;
using ReLogic.Utilities;
using TerRoguelike.Packets;
using static TerRoguelike.Packets.TeleportToPositionPacket;
using Terraria.UI.Chat;
using Terraria.Localization;

namespace TerRoguelike.Rooms
{
    public class SanctuaryLobbyRoom1 : Room
    {
        public Rectangle RunStartBox => new Rectangle(((int)RoomPosition.X + 62) * 16, (int)RoomPosition.Y * 16, 12 * 16, 24 * 16);
        public override int AssociatedFloor => FloorDict["Sanctuary"];
        public override string Key => "SanctuaryLobbyRoom1";
        public override string Filename => "Schematics/RoomSchematics/" + Key + ".csch";
        public override bool IsStartRoom => true;
        public override bool AllowSettingPlayerCurrentRoom => true;
        public override void InitializeRoom()
        {
            active = false;
        }
        public override void Update()
        {
            active = false;
        }

        public override bool CanDescend(Player player, TerRoguelikePlayer modPlayer)
        {
            var hitbox = RunStartBox;
            if (player.whoAmI == Main.myPlayer && Main.rand.NextBool(8))
            {
                Color color = Color.Lerp(Color.Lime, Color.White, runStartMeter);
                Vector2 particlePos = hitbox.BottomLeft() + new Vector2(Main.rand.NextFloat(hitbox.Width), 0);
                Vector2 particleVel = Vector2.UnitY * -4 + new Vector2(Main.rand.NextFloat(-0.3f, -0.3f), Main.rand.NextFloat(-1f, 0.5f));
                if (Main.rand.NextBool(4))
                    particleVel *= Main.rand.NextFloat(0.33f, 0.75f);
                ParticleManager.AddParticle(new Square(
                    particlePos, particleVel, 60, color, new Vector2(Main.rand.NextFloat(0.5f, 0.75f)), 0, 0.98f, 30, true),
                    ParticleManager.ParticleLayer.BehindTiles);
            }
            if (hitbox.Intersects(player.getRect()))
            {
                int count = 0;
                for (int i = 0; i < Main.maxPlayers; i++)
                    if (Main.player[i].active && !Main.player[i].dead) count++;
                float rate =  1f / (Math.Max(count, 1) * 210);
                runStartMeter += rate;
                runStartTouched = true;
            }
            if (!Main.dedServ)
            {
                Vector2 pos = hitbox.BottomLeft();
                for (int i = 0; i <= hitbox.Width; i += 16)
                {
                    Color color = Color.Lerp(Color.Lime, Color.White, runStartMeter * 15);
                    Point tilePos = (pos + new Vector2(i, 0)).ToTileCoordinates();
                    Lighting.AddLight(tilePos.X, tilePos.Y, color.R / 256f * 0.75f, color.G / 256f * 0.75f, color.B / 256f * 0.75f);
                }
            }
            return false;
        }
        public override void PostDrawTilesRoom()
        {
            base.PostDrawTilesRoom();
            Texture2D tex = TexDict["CornerFade"];
            TerRoguelikeUtils.StartAdditiveSpritebatch();
            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 drawPos = RunStartBox.Bottom() + new Vector2(i * 0, -96);

                int targetHeight = tex.Height - 50;
                int heightStart = (int)(targetHeight * (1 - (runStartMeter * 0.95f)));
                Rectangle frameGreen = new Rectangle(0, 0, tex.Width, heightStart);
                Rectangle frameWhite = new Rectangle(0, heightStart, tex.Width, targetHeight - heightStart);
                float scale = 0.38f;

                for (int d = 0; d < 2; d++)
                    Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition + (d == 0 ? Vector2.Zero : new Vector2(0, heightStart * scale)), d == 0 ? frameGreen : frameWhite, (d == 0 ? Color.Lime : Color.White) * 0.75f, 0, new Vector2(tex.Width, targetHeight) * 0.5f, scale, i == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
            TerRoguelikeUtils.StartAlphaBlendSpritebatch();

            var font = FontAssets.DeathText.Value;
            string text = Language.GetOrRegister("Mods.TerRoguelike.MultiplayerReady").Value;
            Vector2 origin = font.MeasureString(text) * 0.5f;
            Color color = Color.Lerp(Color.Lime, Color.White, runStartMeter);
            float dist = Math.Abs(Main.LocalPlayer.Center.X - RunStartBox.Bottom().X);
            if (dist > RunStartBox.Width * 0.5f)
            {
                dist -= RunStartBox.Width * 0.5f;
                color *= 1 - (dist / 80f);
            }
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, RunStartBox.Bottom() - Main.screenPosition + new Vector2(0, 80), color, 0, origin, new Vector2(0.75f));
        }
    }
}
