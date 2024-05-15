using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Systems
{
    public class ScreenshakeSystem : ModSystem
    {
        public static int screenshakeTimer;
        public static int screenshakeDuration;
        public static float screenshakeMagnitude;
        private static Vector2 screenshakeVector;

        public override void PostUpdateEverything()
        {
            TickScreenshake();
        }
        public override void ModifyScreenPosition()
        {
            UpdateScreenshake();
            
            if (screenshakeTimer > 0)
            {
                float magnitude = ModContent.GetInstance<TerRoguelikeConfig>().ScreenshakeIntensity;
                if (magnitude > 0)
                    Main.screenPosition = (CutsceneSystem.cutsceneActive ? Main.screenPosition : Main.Camera.UnscaledPosition) + (TranslationVector / ZoomSystem.zoomOverride) * magnitude;
            }
        }
        public static Vector2 TranslationVector => screenshakeVector;
        public void TickScreenshake()
        {
            if (screenshakeTimer > 0)
            {
                int time = screenshakeDuration - screenshakeTimer;
                if (time % 6 == 0)
                {
                    float effectiveMagnitude = screenshakeMagnitude * (1 - (float)Math.Pow(1 - (screenshakeTimer / (float)screenshakeDuration), 2d));
                    screenshakeVector = Main.rand.NextVector2CircularEdge(effectiveMagnitude, effectiveMagnitude);
                }
                screenshakeTimer--;
                if (screenshakeTimer == 0)
                    screenshakeVector = Vector2.Zero;
            }   
            else if (screenshakeTimer < 0)
                screenshakeTimer = 0;
        }
        public void UpdateScreenshake()
        {
            if (screenshakeTimer > 0)
            {
                screenshakeVector *= 0.8f;
            }
        }
        public static void SetScreenshake(int time, float magnitude)
        {
            screenshakeTimer = screenshakeDuration = time;
            screenshakeMagnitude = magnitude;
        }
    }
}
