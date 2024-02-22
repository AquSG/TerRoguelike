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
        public static SlotId CalmMusic;
        public static SlotId CombatMusic;
        public static int MusicMode = 0; // 0: Calm/Combat - 1: All Calm - 2: All Combat - 3: Silent

        public static SoundStyle Silence = new("TerRoguelike/Tracks/Blank", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle KeygenCalm = new("TerRoguelike/Tracks/Calm", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle KeygenCombat = new("TerRoguelike/Tracks/Combat", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle FinalStage = new("TerRoguelike/Tracks/FinalStage", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle FinalBoss = new("TerRoguelike/Tracks/FinalBoss", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle Escape = new("TerRoguelike/Tracks/Escape", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public override void OnModLoad()
        {
            MusicLoader.AddMusic(TerRoguelike.Instance, "Tracks/Blank");
            CalmMusic = SoundEngine.PlaySound(KeygenCalm with { Volume = 0f });
            CalmMusic = SoundEngine.PlaySound(Silence with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(KeygenCombat with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(FinalStage with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(FinalBoss with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(Escape with { Volume = 0f });
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
        public static void SetCalm(SoundStyle sound)
        {
            if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
            {
                calmMusic.Pause();
                calmMusic.Stop();
            }
            CalmMusic = SoundEngine.PlaySound(sound);
        }
        public static void SetCombat(SoundStyle sound)
        {
            if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
            {
                combatMusic.Pause();
                combatMusic.Stop();
            }
            CombatMusic = SoundEngine.PlaySound(sound);
        }
        public void MusicUpdate()
        {
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            TerRoguelikePlayer modPlayer = Main.player[Main.myPlayer].GetModPlayer<TerRoguelikePlayer>();
            if (!Initialized)
            {
                MusicMode = 0;
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
                    combatMusic.Update();
                }
            }

            if (MusicMode == 0)
            {
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
            
            if (MusicMode == 1)
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

            if (MusicMode == 2)
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

            if (MusicMode == 3)
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    float interpolant = calmMusic.Volume - (1f / 180f);
                    calmMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    calmMusic.Update();
                    CalmVolumeCache = calmMusic.Volume;
                }


                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    float interpolant = combatMusic.Volume - (1f / 180f);
                    combatMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    combatMusic.Update();
                    CombatVolumeCache = combatMusic.Volume;
                }
            }
        }
    }
}
