using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using TerRoguelike.TerPlayer;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Managers.RoomManager;
using static TerRoguelike.Systems.RoomSystem;
using TerRoguelike.Managers;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using static TerRoguelike.Systems.MusicSystem;
using TerRoguelike.Utilities;

namespace TerRoguelike.Systems
{
    public class CreditsSystem : ModSystem
    {
        public static int creditsTime;
        public static Vector2 cameraTargetCenter;
        private static Vector2 overrideCameraCenter;
        public static bool creditsActive = false;
        public static bool fadeIn = false;
        public static List<Vector2> creditsPath = [];
        public static int currentViewStage = -1;
        public static int currentViewStageDuration = 0;
        public static bool weird
        {
            get {
                Player player = Main.LocalPlayer;
                if (player != null)
                {
                    var modPlayer = player.ModPlayer();
                    if (modPlayer != null && modPlayer.escapeFail)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public override void PostUpdateEverything()
        {
            TickCredits();
        }
        public override void ModifyScreenPosition()
        {
            UpdateCutscene();
            if (creditsActive)
                Main.screenPosition = Main.Camera.UnscaledPosition + TranslationVector;
        }
        public static Vector2 TranslationVector => overrideCameraCenter;
        public void TickCredits()
        {
            if (creditsActive)
            {
                for (int i = 0; i < Main.combatText.Length; i++)
                {
                    Main.combatText[i].active = false;
                }
                CutsceneSystem.cutsceneDisableControl = true;

                creditsTime++;
                if (fadeIn || creditsTime > 60)
                {
                    if (creditsPath.Count == 1)
                        cameraTargetCenter = creditsPath[0];
                    else
                    {
                        cameraTargetCenter = creditsPath[0];
                        List<float> creditsPathLengths = [];
                        for (int i = 0; i < creditsPath.Count - 1; i++)
                        {
                            creditsPathLengths.Add(creditsPath[i].Distance(creditsPath[i + 1]));
                        }
                        float totalLength = 0;
                        for (int i = 0; i < creditsPathLengths.Count; i++)
                        {
                            totalLength += creditsPathLengths[i];
                        }

                        float creditsMoveRate = totalLength / currentViewStageDuration;
                        float currentWantedLength = creditsMoveRate * creditsTime;
                        float checkLength = currentWantedLength;
                        bool pass = false;
                        for (int i = 0; i < creditsPathLengths.Count; i++)
                        {
                            float thisLength = creditsPathLengths[i];
                            if (thisLength < checkLength)
                            {
                                checkLength -= thisLength;
                                continue;
                            }

                            Vector2 creditVector = creditsPath[i + 1] - creditsPath[i];
                            cameraTargetCenter = creditsPath[i] + creditVector.SafeNormalize(Vector2.UnitX) * checkLength;
                            pass = true;
                            break;
                        }
                        if (!pass)
                        {
                            cameraTargetCenter = creditsPath[creditsPath.Count - 1];
                        }
                    }

                    if (creditsTime >= currentViewStageDuration)
                    {
                        SetNextViewStage();
                    }
                }
            }
        }
        public override void PostDrawTiles()
        {
            if (creditsActive)
            {
                Player player = Main.LocalPlayer;
                var modPlayer = player.ModPlayer();
                if (modPlayer != null)
                    modPlayer.creditsViewTime++;

                int fadingTime = 180;
                if (fadeIn && creditsTime < fadingTime)
                {
                    float fadeCompletion = 1 - (creditsTime / (float)fadingTime);
                    postDrawEverythingCache.Add(new(TextureAssets.MagicPixel.Value, Main.Camera.ScaledPosition, null, Color.Black * fadeCompletion, 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f) / ZoomSystem.ScaleVector * 1.1f, SpriteEffects.None));
                }
                else if (currentViewStageDuration - creditsTime < fadingTime)
                {
                    float fadeCompletion = 1 - ((currentViewStageDuration - creditsTime) / (float)fadingTime);
                    postDrawEverythingCache.Add(new(TextureAssets.MagicPixel.Value, Main.Camera.ScaledPosition, null, Color.Black * fadeCompletion, 0, Vector2.Zero, new Vector2(Main.screenWidth, Main.screenHeight * 0.0011f) / ZoomSystem.ScaleVector * 1.1f, SpriteEffects.None));
                }
            }
        }
        public void UpdateCutscene()
        {
            if (creditsActive)
            {
                overrideCameraCenter = cameraTargetCenter - Main.Camera.Center;
            }
        }
        public static void PlayCreditsTheme()
        {
            SetMusicMode(MusicStyle.AllCalm);
            Player player = Main.LocalPlayer;
            if (weird)
            {
                SetCalm(Darkness, true, 4);
                CalmVolumeLevel = 0.6f;
                return;
            }
            SetCalm(Credits, false);
            CalmVolumeLevel = 0.5f;
            PauseWhenIngamePaused = true;
        }
        public static void StartCredits()
        {
            PlayCreditsTheme();

            currentViewStageDuration = 1323;
            currentViewStage = -1;
            creditsActive = true;
            creditsTime = 0;
            cameraTargetCenter = Main.LocalPlayer.Center;
            creditsPath = [Main.LocalPlayer.Center];
            fadeIn = false;
        }
        public static void StopCredits()
        {
            currentViewStage = -1;
            creditsActive = false;
            creditsTime = 0;
            fadeIn = false;
        }
        public static void SetNextViewStage()
        {
            fadeIn = true;
            creditsTime = 0;
            if (currentViewStage == -2)
            {
                currentViewStage = FloorIDsInPlay.Count - 1;
            }
            else if (currentViewStage == FloorIDsInPlay.Count - 1)
            {
                currentViewStage = -1;
            }
            else
            {
                currentViewStage = currentViewStage + 1;
                if (FloorID[FloorIDsInPlay[currentViewStage]].ID == FloorDict["Lunar"])
                {
                    currentViewStage = -2;
                }
            }
            if (currentViewStage == -2)
            {
                currentViewStageDuration = 960;
                Room targetRoom = RoomID[FloorID[FloorDict["Sanctuary"]].StartRoomID];
                creditsPath = [targetRoom.RoomPosition16 + targetRoom.RoomCenter16];
            }
            else if (currentViewStage == -1)
            {
                currentViewStageDuration = 1323;
                creditsPath = [Main.LocalPlayer.Center];
                if (!weird)
                    PlayCreditsTheme();
            }
            else
            {
                currentViewStageDuration = currentViewStage != 3 ? (currentViewStage != 5 ? 2618 : 4291) : 2946;
                creditsPath = [];
                int startRoom = RoomID[FloorID[FloorIDsInPlay[currentViewStage]].StartRoomID].myRoom;
                for (int i = 0; i < 100; i++)
                {
                    int roomListCheck = startRoom + i;
                    if (RoomList.Count <= roomListCheck)
                        break;

                    Room room = RoomList[roomListCheck];
                    creditsPath.Add(room.RoomPosition16 + room.RoomCenter16);
                    if (room.IsBossRoom)
                        break;
                }
                if (creditsPath.Count == 0)
                    creditsPath = [Main.LocalPlayer.Center];
            }
        }
        public override void ClearWorld()
        {
            
        }
    }
}
