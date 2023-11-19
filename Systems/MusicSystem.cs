using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.Rooms;
using Terraria.WorldBuilding;
using static tModPorter.ProgressUpdate;
using Terraria.GameContent.Generation;
using tModPorter;
using Terraria.Localization;
using TerRoguelike.World;
using TerRoguelike.MainMenu;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using ReLogic.Utilities;
using TerRoguelike.TerPlayer;
using Terraria.ModLoader.IO;

namespace TerRoguelike.Systems
{
    public class MusicSystem : ModSystem
    {
        public static bool Initialized = false;
        float CalmVolumeCache = 0;
        float CombatVolumeCache = 0;
        public SlotId CalmMusic;
        public SlotId CombatMusic;

        public static SoundStyle KeygenCalm = new("TerRoguelike/Tracks/Calm", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle KeygenCombat = new("TerRoguelike/Tracks/Combat", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public override void OnModLoad()
        {
            MusicLoader.AddMusic(TerRoguelike.Instance, "Tracks/Blank");
            CalmMusic = SoundEngine.PlaySound(KeygenCalm with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(KeygenCombat with { Volume = 0f });
            if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
            {
                calmMusic.Stop();
            }
            if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
            {
                combatMusic.Stop();
            }
        }
        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            MusicUpdate();
        }
        public override void PostDrawTiles()
        {
            MusicUpdate();
        }
        public void MusicUpdate()
        {
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            TerRoguelikePlayer modPlayer = Main.player[Main.myPlayer].GetModPlayer<TerRoguelikePlayer>();
            if (!Initialized)
            {
                CalmMusic = SoundEngine.PlaySound(KeygenCalm with { Volume = 0.25f });
                CombatMusic = SoundEngine.PlaySound(KeygenCombat with { Volume = 0.25f });
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    calmMusic.Volume = 0f;
                    calmMusic.Update();
                    CalmVolumeCache = calmMusic.Volume;
                }
                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    combatMusic.Volume = 0f;
                    combatMusic.Update();
                    CombatVolumeCache = combatMusic.Volume;
                }
                Initialized = true;
            }

            if (!Initialized)
                return;


            if (!Main.hasFocus)
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    calmMusic.Volume = 0;
                    calmMusic.Update();
                }


                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    combatMusic.Volume = 0;
                    calmMusic.Update();
                }
                return;
            }
            else
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    calmMusic.Volume = CalmVolumeCache;
                    calmMusic.Update();
                }


                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    combatMusic.Volume = CombatVolumeCache;
                    calmMusic.Update();
                }
            }

            if (modPlayer.currentRoom == -1)
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    float interpolant = calmMusic.Volume + (1f / 120f);
                    calmMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    calmMusic.Update();
                    CalmVolumeCache = calmMusic.Volume;
                }


                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    float interpolant = combatMusic.Volume - (1f / 120f);
                    combatMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    combatMusic.Update();
                    CombatVolumeCache = combatMusic.Volume;
                }
            }
            else
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    float interpolant = calmMusic.Volume - (1f / 60f);
                    calmMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    calmMusic.Update();
                    CalmVolumeCache = calmMusic.Volume;
                }

                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    float interpolant = combatMusic.Volume + (1f / 60f);
                    combatMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    combatMusic.Update();
                    CombatVolumeCache = combatMusic.Volume;
                }

            }
        }
    }
}
