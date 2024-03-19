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
using System.Threading;

namespace TerRoguelike.Systems
{
    public class MusicSystem : ModSystem
    {
        public static bool Initialized = false;
        public static bool PlayedAllSounds = false;
        public static float CalmVolumeCache = 0;
        public static float CombatVolumeCache = 0;
        public static SlotId CalmMusic;
        public static SlotId CombatMusic;
        public static BossTheme ActiveBossTheme;
        public static MusicStyle MusicMode = MusicStyle.Dynamic;
        public enum MusicStyle
        {
            Dynamic = 0,
            AllCalm = 1,
            AllCombat = 2,
            Silent = 3,
            Boss = 4,
        }
        public static bool BufferCalmSilence = false;
        public static bool BufferCombatSilence = false;

        public static SoundStyle Silence = new("TerRoguelike/Tracks/Blank", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle FinalStage = new("TerRoguelike/Tracks/FinalStage", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle FinalBoss = new("TerRoguelike/Tracks/FinalBoss", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };
        public static SoundStyle Escape = new("TerRoguelike/Tracks/Escape", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false };

        public static FloorSoundtrack BaseTheme = new(
            new("TerRoguelike/Tracks/Calm", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false, Volume = 0.18f }, 
            new("TerRoguelike/Tracks/Combat", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false, Volume = 0.18f });

        public static BossTheme PaladinTheme = new(
            new("TerRoguelike/Tracks/PaladinTheme", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false, Volume = 0.33f },
            new("TerRoguelike/Tracks/PaladinThemeStart", SoundType.Music) { IsLooped = false, PlayOnlyIfFocused = false, Volume = 0.33f },
            new("TerRoguelike/Tracks/PaladinThemeEnd", SoundType.Music) { IsLooped = false, PlayOnlyIfFocused = false, Volume = 0.33f });

        public static BossTheme BrambleHollowTheme = new(
            new("TerRoguelike/Tracks/BrambleHollowTheme", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false, Volume = 0.4f },
            new("TerRoguelike/Tracks/BrambleHollowThemeStart", SoundType.Music) { IsLooped = false, PlayOnlyIfFocused = false, Volume = 0.4f },
            new("TerRoguelike/Tracks/BrambleHollowThemeEnd", SoundType.Music) { IsLooped = false, PlayOnlyIfFocused = false, Volume = 0.4f });

        public static BossTheme CrimsonVesselTheme = new(
            new("TerRoguelike/Tracks/CrimsonVesselTheme", SoundType.Music) { IsLooped = true, PlayOnlyIfFocused = false, Volume = 0.8f },
            new("TerRoguelike/Tracks/CrimsonVesselThemeStart", SoundType.Music) { IsLooped = false, PlayOnlyIfFocused = false, Volume = 0.8f },
            new("TerRoguelike/Tracks/CrimsonVesselThemeEnd", SoundType.Music) { IsLooped = false, PlayOnlyIfFocused = false, Volume = 0.8f });

        public static void PlayAllSounds()
        {
            if (PlayedAllSounds)
                return;

            PlayedAllSounds = true;

            ThreadPool.QueueUserWorkItem(_ => PlayAllSoundsCallback());
        }
        private static void PlayAllSoundsCallback()
        {
            CalmMusic = SoundEngine.PlaySound(Silence with { Volume = 0f });
            CalmMusic = SoundEngine.PlaySound(BaseTheme.CalmTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(BaseTheme.CombatTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(FinalStage with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(FinalBoss with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(Escape with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(PaladinTheme.BattleTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(PaladinTheme.StartTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(PaladinTheme.EndTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(BrambleHollowTheme.BattleTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(BrambleHollowTheme.StartTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(BrambleHollowTheme.EndTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(CrimsonVesselTheme.BattleTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(CrimsonVesselTheme.StartTrack with { Volume = 0f });
            CombatMusic = SoundEngine.PlaySound(CrimsonVesselTheme.EndTrack with { Volume = 0f });
            if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
            {
                calmMusic.Stop();
            }
            if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
            {
                combatMusic.Stop();
            }
        }
        public override void OnModLoad()
        {
            MusicLoader.AddMusic(TerRoguelike.Instance, "Tracks/Blank");
        }
        public override void SetStaticDefaults()
        {
            PlayAllSounds();
        }
        public static void SetBossTrack(BossTheme bossTheme)
        {
            ActiveBossTheme = bossTheme;
            ActiveBossTheme.startFlag = true;
            SetCombat(bossTheme.StartTrack);
            SetMusicMode(MusicStyle.Boss);
        }
        public static void SetMusicMode(MusicStyle newMode)
        {
            MusicMode = newMode;
            if (newMode != MusicStyle.Boss)
                ActiveBossTheme = null;

            BufferCalmSilence = newMode == MusicStyle.AllCombat || newMode == MusicStyle.Silent || newMode == MusicStyle.Boss;
            BufferCombatSilence = newMode == MusicStyle.AllCalm || newMode == MusicStyle.Silent;
        }
        public override void PostUpdateEverything()
        {
            MusicUpdate();
        }
        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            if (!Main.hasFocus || Main.gamePaused)
                MusicUpdate();
        }
        public override void PostDrawTiles()
        {
            if (!Main.hasFocus || Main.gamePaused)
                MusicUpdate();
        }
        public static void SetCalm(SoundStyle sound)
        {
            if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
            {
                CalmVolumeCache = calmMusic.Volume;
                calmMusic.Pause();
                calmMusic.Stop();
            }
            CalmMusic = SoundEngine.PlaySound(sound);
            if (SoundEngine.TryGetActiveSound(CalmMusic, out var newMusic))
            {
                newMusic.Volume = CalmVolumeCache;
            }
        }
        public static void SetCombat(SoundStyle sound)
        {
            if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
            {
                CombatVolumeCache = combatMusic.Volume;
                combatMusic.Pause();
                combatMusic.Stop();
            }
            CombatMusic = SoundEngine.PlaySound(sound);
            if (SoundEngine.TryGetActiveSound(CombatMusic, out var newMusic))
            {
                newMusic.Volume = CombatVolumeCache;
            }
        }
        public void MusicUpdate()
        {
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            TerRoguelikePlayer modPlayer = Main.player[Main.myPlayer].GetModPlayer<TerRoguelikePlayer>();
            if (!Initialized && modPlayer != null && modPlayer.currentFloor != null)
            {
                SetMusicMode(MusicStyle.Dynamic);
                FloorSoundtrack soundtrack = modPlayer.currentFloor.Soundtrack;

                CalmMusic = SoundEngine.PlaySound(soundtrack.CalmTrack);
                CombatMusic = SoundEngine.PlaySound(soundtrack.CombatTrack);
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
                    combatMusic.Update();
                }
                return;
            }
            else
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    calmMusic.Volume = CalmVolumeCache;
                    calmMusic.Update();
                    //Main.NewText(calmMusic.Volume);
                }


                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    combatMusic.Volume = CombatVolumeCache;
                    combatMusic.Update();
                    //Main.NewText(combatMusic.Volume);
                }
            }

            if (MusicMode == MusicStyle.Dynamic)
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
            
            if (MusicMode == MusicStyle.AllCalm)
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
                    if (BufferCombatSilence)
                    {
                        if (combatMusic.Volume <= 0)
                        {
                            SetCombat(Silence with { Volume = 0f });
                            BufferCombatSilence = false;
                        }
                    }
                }
            }

            if (MusicMode == MusicStyle.AllCombat)
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    float interpolant = calmMusic.Volume - (1f / 60f);
                    calmMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    calmMusic.Update();
                    CalmVolumeCache = calmMusic.Volume;
                    if (BufferCalmSilence)
                    {
                        if (calmMusic.Volume <= 0)
                        {
                            SetCalm(Silence with { Volume = 0f });
                            BufferCalmSilence = false;
                        }
                    }
                }

                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    float interpolant = combatMusic.Volume + (1f / 60f);
                    combatMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    combatMusic.Update();
                    CombatVolumeCache = combatMusic.Volume;
                }
            }

            if (MusicMode == MusicStyle.Silent)
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    float interpolant = calmMusic.Volume - (1f / 180f);
                    calmMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    calmMusic.Update();
                    CalmVolumeCache = calmMusic.Volume;
                    if (BufferCalmSilence)
                    {
                        if (calmMusic.Volume <= 0)
                        {
                            SetCalm(Silence with { Volume = 0f });
                            BufferCalmSilence = false;
                        }
                    }
                }

                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    float interpolant = combatMusic.Volume - (1f / 180f);
                    combatMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    combatMusic.Update();
                    CombatVolumeCache = combatMusic.Volume;
                    if (BufferCombatSilence)
                    {
                        if (combatMusic.Volume <= 0)
                        {
                            SetCombat(Silence with { Volume = 0f });
                            BufferCombatSilence = false;
                        }
                    }
                }
            }

            if (MusicMode == MusicStyle.Boss)
            {
                if (SoundEngine.TryGetActiveSound(CalmMusic, out var calmMusic))
                {
                    float interpolant = calmMusic.Volume - (1f / 60f);
                    calmMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    calmMusic.Update();
                    CalmVolumeCache = calmMusic.Volume;
                    if (BufferCalmSilence)
                    {
                        if (calmMusic.Volume <= 0)
                        {
                            SetCalm(Silence with { Volume = 0f });
                            BufferCalmSilence = false;
                        }
                    }
                }

                if (SoundEngine.TryGetActiveSound(CombatMusic, out var combatMusic))
                {
                    float interpolant = combatMusic.Volume + (1f / 60f);
                    combatMusic.Volume = MathHelper.Clamp(MathHelper.Lerp(0, 1f, interpolant), 0f, 1f);
                    if (ActiveBossTheme.endFlag)
                    {
                        SetCombat(ActiveBossTheme.EndTrack);
                        ActiveBossTheme.endFlag = false;
                        ActiveBossTheme.startFlag = false;
                    }
                    combatMusic.Update();
                    CombatVolumeCache = combatMusic.Volume;
                }
                else
                {
                    if (ActiveBossTheme.startFlag)
                    {
                        SetCombat(ActiveBossTheme.BattleTrack);
                        ActiveBossTheme.startFlag = false;
                    }
                    else
                    {
                        SetCombat(Silence);
                        CombatVolumeCache = 0;
                        SetMusicMode(MusicStyle.Silent);
                    }   
                }
            }
        }
    }
    public class BossTheme
    {
        public SoundStyle BattleTrack;
        public SoundStyle EndTrack;
        public SoundStyle StartTrack;
        public bool endFlag = false;
        public bool startFlag = true;
        public BossTheme(SoundStyle battleTrack, SoundStyle startTrack, SoundStyle endTrack)
        {
            BattleTrack = battleTrack;
            StartTrack = startTrack;
            EndTrack = endTrack;
        }
    }
    public class FloorSoundtrack
    {
        public SoundStyle CalmTrack;
        public SoundStyle CombatTrack;
        public FloorSoundtrack(SoundStyle calmTrack, SoundStyle combatTrack)
        {
            CalmTrack = calmTrack;
            CombatTrack = combatTrack;
        }
    }
}
