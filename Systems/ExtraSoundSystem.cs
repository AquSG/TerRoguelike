using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using TerRoguelike.TerPlayer;

namespace TerRoguelike.Systems
{
    public class ExtraSoundSystem : ModSystem
    {
        public static List<ExtraSound> ExtraSounds = [];
        public override void PostUpdateWorld()
        {
            if (ExtraSounds == null)
                return;
            if (ExtraSounds.Count == 0)
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
        public ExtraSound(SlotId sound, float Volume = 1f, int FadeOutStart = -1, int FadeOutTime = 1)
        {
            slot = sound;
            fadeOutStart = FadeOutStart;
            volume = Volume;
            fadeOutTime = FadeOutTime;
        }
    }
}
