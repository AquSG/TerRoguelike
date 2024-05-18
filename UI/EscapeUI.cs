using System;
using System.Collections.Generic;
using System.Text;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerRoguelike.World;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static TerRoguelike.Managers.TextureManager;
using Terraria.Localization;
using Terraria.Map;

namespace TerRoguelike.UI
{
    public static class EscapeUI
    {
        public static float EscapeUiYOff
        {
            get { return MathHelper.Clamp(MathHelper.Lerp(-60, 0, (TerRoguelikeWorld.escapeTimeSet - TerRoguelikeWorld.escapeTime) / 270f), -5000, 0); }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (TerRoguelikeWorld.escape)
            {
                int time = TerRoguelikeWorld.escapeTime;
                int maxTime = TerRoguelikeWorld.escapeTimeSet;
                int minutes = time / 3600;
                int seconds = (time - (minutes * 3600)) / 60;
                int miliseconds = (int)((time - (minutes * 3600) - (seconds * 60)) / 60f * 1000); 
                string secondsString = seconds.ToString().Length == 1 ? "0" + seconds.ToString() : seconds.ToString();
                string milisecondsString = miliseconds.ToString().Length == 1 ? "00" + miliseconds.ToString() : (miliseconds.ToString().Length == 2 ? "0" + miliseconds.ToString() : miliseconds.ToString());
                string timer = minutes.ToString() + ":" + secondsString + "." + milisecondsString;
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, timer, new Vector2((Main.screenWidth / 2) - 80, EscapeUiYOff), Color.White, 0f, Vector2.Zero, new Vector2(1f));
            }
        }
    }
}
