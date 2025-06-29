using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using TerRoguelike.TerPlayer;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using TerRoguelike.Projectiles;

namespace TerRoguelike.Systems
{
    public class ExtraSoundSystem : ModSystem
    {
        public static List<ExtraSound> ExtraSounds = [];
        public static List<SoundEffectInstance> SpecialSoundReplace = []; //all sounds but the final sound are faded out at a fast rate instead of instantly cancelling, causing a pop in the audio.
        public static Stopwatch lastSpecialSoundUpdate = new Stopwatch();
        private static CancellationTokenSource _cts;
        public override void PostUpdateEverything()
        {
            UpdateExtraSounds();
            if (StuckClingyGrenade.soundCooldown > 0)
                StuckClingyGrenade.soundCooldown--;
        }
        public override void Load()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => SpecialSoundReplaceUpdateLoop(_cts.Token));
        }
        public override void Unload()
        {
            _cts.Cancel();
        }
        private static async Task SpecialSoundReplaceUpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                UpdateSpecialSounds();

                await Task.Delay(1, token);
            }
        }
        public static void UpdateSpecialSounds()
        {
            if (SpecialSoundReplace == null || SpecialSoundReplace.Count == 0)
            {
                lastSpecialSoundUpdate.Restart();
                return;
            }

            try
            {
                int final = SpecialSoundReplace.Count - 1;
                for (int i = 0; i <= final; i++)
                {
                    var sound = SpecialSoundReplace[i];
                    if (sound.IsDisposed || sound.Volume < 0 || Main.gamePaused || !Main.hasFocus || sound.State != SoundState.Playing)
                    {
                        if (!sound.IsDisposed)
                            sound.Dispose();
                        SpecialSoundReplace.RemoveAt(i);
                        i--;
                        final--;
                        continue;
                    }
                    if (i < final)
                        sound.Volume -= (float)lastSpecialSoundUpdate.Elapsed.TotalSeconds * 15 * Main.soundVolume;

                    if (sound.Volume < 0 || sound.Volume > 1)
                    {
                        if (!sound.IsDisposed)
                            sound.Dispose();
                        SpecialSoundReplace.RemoveAt(i);
                        i--;
                        final--;
                    }
                }
            }
            catch (Exception e)
            {
                TerRoguelike.Instance.Logger.Warn(e);
            }


            lastSpecialSoundUpdate.Restart();
        }
        public static void UpdateExtraSounds()
        {
            if (ExtraSounds == null)
                return;
            if (ExtraSounds.Count == 0)
                return;

            Player player = Main.LocalPlayer;
            if (player == null)
                return;

            for (int i = 0; i < ExtraSounds.Count; i++)
            {
                ExtraSound potentialSound = ExtraSounds[i];
                potentialSound.time++;
                bool soundPresent = SoundEngine.TryGetActiveSound(potentialSound.slot, out var sound);
                if (!soundPresent || !sound.IsPlaying)
                {
                    potentialSound.setToBeRemoved = true;
                    continue;
                }
                sound.Volume = potentialSound.volume;
                if (potentialSound.followPlayer)
                {
                    sound.Position = player.Center + potentialSound.followPlayerAnchor;
                }
                if (potentialSound.fadeOutStart >= 0 && potentialSound.time > potentialSound.fadeOutStart)
                {
                    float fadeOutInterpolant = 1 - (float)Math.Pow((potentialSound.time - potentialSound.fadeOutStart) / (float)potentialSound.fadeOutTime, 0.5d);
                    sound.Volume *= fadeOutInterpolant;
                    if (sound.Volume <= 0)
                    {
                        sound.Stop();
                        potentialSound.setToBeRemoved = true;
                    }
                }
            }
            ExtraSounds.RemoveAll(x => x.setToBeRemoved);
        }
        public static void ForceStopAllExtraSounds()
        {
            if (ExtraSounds == null)
                return;
            if (ExtraSounds.Count == 0)
                return;

            for (int i = 0; i < ExtraSounds.Count; i++)
            {
                ExtraSound potentialSound = ExtraSounds[i];
                bool soundPresent = SoundEngine.TryGetActiveSound(potentialSound.slot, out var sound);
                if (soundPresent)
                {
                    sound.Stop();
                    continue;
                }
            }
            ExtraSounds.Clear();
        }
    }
    public class ExtraSound
    {
        public SlotId slot;
        public bool setToBeRemoved = false;
        public int time = 0;
        public int fadeOutStart;
        public int fadeOutTime;
        public int endTime;
        public float volume;
        public bool followPlayer;
        public Vector2 followPlayerAnchor = Vector2.Zero;
        public ExtraSound(SlotId sound, float Volume = 1f, int FadeOutStart = -1, int FadeOutTime = 1, bool FollowPlayer = false)
        {
            slot = sound;
            fadeOutStart = FadeOutStart;
            volume = Volume;
            fadeOutTime = FadeOutTime;
            followPlayer = FollowPlayer;
            if (followPlayer && SoundEngine.TryGetActiveSound(sound, out var outSound) && outSound.IsPlaying && Main.LocalPlayer != null)
            {
                Vector2 soundPos = outSound.Position == null ? Vector2.Zero : (Vector2)outSound.Position;
                followPlayerAnchor = soundPos - Main.LocalPlayer.Center;
            }
        }
    }
}
