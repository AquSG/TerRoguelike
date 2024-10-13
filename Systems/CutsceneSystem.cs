using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using TerRoguelike.TerPlayer;
using TerRoguelike.World;

namespace TerRoguelike.Systems
{
    public class CutsceneSystem : ModSystem
    {
        public static int cutsceneTimer;
        public static int cutsceneDuration;
        private static float cutsceneZoom;
        public static Vector2 cameraTargetCenter;
        private static Vector2 overrideCameraCenter;
        private static int easeInTime;
        private static int easeOutTime;
        public static bool cutsceneActive;
        public static bool easeInActivated;
        public static bool easeOutActivated;
        public static bool cutsceneDisableControl;

        public override void PostUpdateEverything()
        {
            TickCutscene();
        }
        public override void ModifyScreenPosition()
        {
            UpdateCutscene();
            if (cutsceneActive)
                Main.screenPosition = Main.Camera.UnscaledPosition + TranslationVector;
        }
        public static Vector2 TranslationVector => overrideCameraCenter;
        public void TickCutscene()
        {
            if (cutsceneTimer > 0)
                cutsceneTimer--;
            else if (cutsceneTimer < 0)
                cutsceneTimer = 0;
        }
        public void UpdateCutscene()
        {
            if (cutsceneActive)
            {
                int easeInTimestamp = cutsceneDuration - easeInTime;
                if (!easeInActivated)
                {
                    easeInActivated = true;
                    ZoomSystem.SetZoomAnimation(cutsceneZoom, easeInTime);
                }
                if (cutsceneTimer >= easeInTimestamp)
                {
                    float completion = MathHelper.SmoothStep(0, 1f, (cutsceneTimer - easeInTimestamp) / (float)(easeInTime));
                    overrideCameraCenter = (cameraTargetCenter - Main.Camera.Center) * (1f - completion);
                }
                else if (cutsceneTimer <= easeOutTime)
                {
                    cutsceneDisableControl = false;
                    if (!easeOutActivated)
                    {
                        if (Main.player[Main.myPlayer].dead)
                        {
                            ZoomSystem.SetZoomAnimation(2.5f, easeOutTime);
                        }
                        else
                        {
                            ZoomSystem.SetZoomAnimation(Main.GameZoomTarget, easeOutTime);
                        }
                        easeOutActivated = true;
                    }
                    float completion = MathHelper.SmoothStep(0, 1f, cutsceneTimer / (float)(easeOutTime));
                    overrideCameraCenter = (cameraTargetCenter - Main.Camera.Center) * (completion);
                }
                else
                {
                    overrideCameraCenter = (cameraTargetCenter - Main.Camera.Center);
                }

                if (cutsceneTimer == 0)
                {
                    if (!easeOutActivated)
                    {
                        ZoomSystem.SetZoomAnimation(Main.GameZoomTarget, 1);
                        easeOutActivated = true;
                    }
                    cutsceneActive = false;
                    Main.screenPosition = Main.Camera.UnscaledPosition;
                    overrideCameraCenter = Vector2.Zero;
                    cutsceneDisableControl = false;
                }
            }
        }
        public static void SetCutscene(Vector2 cameraTarget, int time, int easeIn, int easeOut, float targetZoom, CutsceneSource source = CutsceneSource.Misc)
        {
            if (TerRoguelikeWorld.escape && source == CutsceneSource.Boss)
                return;

            cutsceneTimer = time;
            cutsceneDuration = time;
            cameraTargetCenter = cameraTarget;
            easeInTime = easeIn;
            easeOutTime = easeOut;
            cutsceneActive = true;
            easeInActivated = false;
            easeOutActivated = false;
            cutsceneZoom = targetZoom;
            cutsceneDisableControl = true;
        }

        public enum CutsceneSource
        {
            Misc = 0,
            Boss = 1,
        }
    }
}
