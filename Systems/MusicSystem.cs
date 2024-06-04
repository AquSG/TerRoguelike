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
using ReLogic.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TerRoguelike.Systems
{
    public class MusicSystem : ModSystem
    {
        public static bool Initialized = false;
        public static bool MusicDictionaryFilled = false;
        public static bool PauseWhenIngamePaused = false;
        public static int BlankMusicSlotId = -1;
        public static float CalmVolumeInterpolant = 0;
        public static float CombatVolumeInterpolant = 0;
        public static float CalmVolumeLevel = 1f;
        public static float CombatVolumeLevel = 1f;
        public static double BossIntroDuration = 0;
        public static double BossIntroProgress = 0;
        public static Stopwatch BossIntroStopwatch = new Stopwatch();
        public static double BossIntroPreviousTime = 0;
        public static SoundEffectInstance CalmMusic;
        public static SoundEffectInstance CombatMusic;
        public static BossTheme ActiveBossTheme;
        public static MusicStyle MusicMode = MusicStyle.Dynamic;
        public static Dictionary<string, Asset<SoundEffect>> MusicDict = new Dictionary<string, Asset<SoundEffect>>();
        public static float fadeRateMultiplier = 1;
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

        public static string Silence = "TerRoguelike/Tracks/Blank";
        public static string FinalStage = "TerRoguelike/Tracks/FinalStage";
        public static string FinalBoss = "TerRoguelike/Tracks/FinalBoss";
        public static string Escape = "TerRoguelike/Tracks/Escape";
        public static string Credits = "TerRoguelike/Tracks/Credits";

        public static FloorSoundtrack SanctuaryTheme = new(
            "TerRoguelike/Tracks/SanctuaryTheme",
            Silence,
            0.5f);

        public static FloorSoundtrack BaseTheme = new(
            "TerRoguelike/Tracks/Calm",
            "TerRoguelike/Tracks/Combat",
            0.18f);

        public static BossTheme PaladinTheme = new(
            "TerRoguelike/Tracks/PaladinTheme",
            "TerRoguelike/Tracks/PaladinThemeStart",
            "TerRoguelike/Tracks/PaladinThemeEnd",
            0.33f);

        public static BossTheme BrambleHollowTheme = new(
            "TerRoguelike/Tracks/BrambleHollowTheme",
            "TerRoguelike/Tracks/BrambleHollowThemeStart",
            "TerRoguelike/Tracks/BrambleHollowThemeEnd",
            0.4f);

        public static BossTheme CrimsonVesselTheme = new(
            "TerRoguelike/Tracks/CrimsonVesselTheme",
            "TerRoguelike/Tracks/CrimsonVesselThemeStart",
            "TerRoguelike/Tracks/CrimsonVesselThemeEnd",
            0.8f);
        public static BossTheme CorruptionParasiteTheme = new(
            "TerRoguelike/Tracks/CorruptionParasiteTheme",
            "TerRoguelike/Tracks/CorruptionParasiteThemeStart",
            "TerRoguelike/Tracks/CorruptionParasiteThemeEnd",
            0.36f);
        public static BossTheme IceQueenTheme = new(
            "TerRoguelike/Tracks/IceQueenTheme",
            "TerRoguelike/Tracks/IceQueenThemeStart",
            "TerRoguelike/Tracks/IceQueenThemeEnd",
            0.6f);
        public static BossTheme PharaohSpiritTheme = new(
            "TerRoguelike/Tracks/PharaohSpiritTheme",
            "TerRoguelike/Tracks/PharaohSpiritThemeStart",
            "TerRoguelike/Tracks/PharaohSpiritThemeEnd",
            0.67f);
        public static BossTheme QueenBeeTheme = new(
            "TerRoguelike/Tracks/QueenBeeTheme",
            "TerRoguelike/Tracks/QueenBeeThemeStart",
            "TerRoguelike/Tracks/QueenBeeThemeEnd",
            0.4f);
        public static BossTheme WallOfFleshTheme = new(
            "TerRoguelike/Tracks/WallOfFleshTheme",
            "TerRoguelike/Tracks/WallOfFleshThemeStart",
            "TerRoguelike/Tracks/WallOfFleshThemeEnd",
            0.44f);
        public static BossTheme SkeletronTheme = new(
            "TerRoguelike/Tracks/SkeletronTheme",
            "TerRoguelike/Tracks/SkeletronThemeStart",
            "TerRoguelike/Tracks/SkeletronThemeEnd",
            0.42f);
        public static BossTheme TempleGolemTheme = new(
            "TerRoguelike/Tracks/TempleGolemTheme",
            "TerRoguelike/Tracks/TempleGolemThemeStart",
            "TerRoguelike/Tracks/TempleGolemThemeEnd",
            0.55f);
        public static BossTheme FinalBoss1Theme = new(
            "TerRoguelike/Tracks/FinalBoss1",
            "TerRoguelike/Tracks/FinalBoss1Start",
            "TerRoguelike/Tracks/FinalBoss1End",
            0.65f);
        public static BossTheme FinalBoss2PreludeTheme = new(
            "TerRoguelike/Tracks/FinalBoss2Prelude",
            "TerRoguelike/Tracks/FinalBoss2PreludeStart",
            Silence,
            0.45f);
        public static BossTheme FinalBoss2Theme = new(
            "TerRoguelike/Tracks/FinalBoss2",
            "TerRoguelike/Tracks/FinalBoss2Start",
            "TerRoguelike/Tracks/FinalBoss2End",
            0.45f);


        public static void FillMusicDictionary()
        {
            if (MusicDictionaryFilled)
                return;

            MusicDictionaryFilled = true;

            List<string> pathList = new List<string>()
            {
                Silence,
                FinalStage,
                FinalBoss,
                Escape,
                Credits,
                SanctuaryTheme.CalmTrack,
                BaseTheme.CalmTrack,
                BaseTheme.CombatTrack,
                PaladinTheme.BattleTrack,
                PaladinTheme.StartTrack,
                PaladinTheme.EndTrack,
                BrambleHollowTheme.BattleTrack,
                BrambleHollowTheme.StartTrack,
                BrambleHollowTheme.EndTrack,
                CrimsonVesselTheme.BattleTrack,
                CrimsonVesselTheme.StartTrack,
                CrimsonVesselTheme.EndTrack,
                CorruptionParasiteTheme.BattleTrack,
                CorruptionParasiteTheme.StartTrack,
                CorruptionParasiteTheme.EndTrack,
                IceQueenTheme.BattleTrack,
                IceQueenTheme.StartTrack,
                IceQueenTheme.EndTrack,
                PharaohSpiritTheme.BattleTrack,
                PharaohSpiritTheme.StartTrack,
                PharaohSpiritTheme.EndTrack,
                QueenBeeTheme.BattleTrack,
                QueenBeeTheme.StartTrack,
                QueenBeeTheme.EndTrack,
                WallOfFleshTheme.BattleTrack,
                WallOfFleshTheme.StartTrack,
                WallOfFleshTheme.EndTrack,
                SkeletronTheme.BattleTrack,
                SkeletronTheme.StartTrack,
                SkeletronTheme.EndTrack,
                TempleGolemTheme.BattleTrack,
                TempleGolemTheme.StartTrack,
                TempleGolemTheme.EndTrack,
                FinalBoss1Theme.BattleTrack,
                FinalBoss1Theme.StartTrack,
                FinalBoss1Theme.EndTrack,
                FinalBoss2PreludeTheme.BattleTrack,
                FinalBoss2PreludeTheme.StartTrack,
                FinalBoss2Theme.BattleTrack,
                FinalBoss2Theme.StartTrack,
                FinalBoss2Theme.EndTrack
            };
            foreach (string path in pathList)
            {
                AddMusic(path);
            }
        }
        internal static void AddMusic(string path)
        {
            MusicDict.Add(path, ModContent.Request<SoundEffect>(path, AssetRequestMode.AsyncLoad));
        }
 
        public override void OnModLoad()
        {
            MusicLoader.AddMusic(TerRoguelike.Instance, "Tracks/Blank");
        }
        public override void Unload()
        {
            MusicDict = null;
        }
        public override void SetStaticDefaults()
        {
            FillMusicDictionary();
            BlankMusicSlotId = MusicLoader.GetMusicSlot(TerRoguelike.Instance, "Tracks/Blank");
        }
        public static void SetBossTrack(BossTheme bossTheme, float fadeRateMulti = 1)
        {
            BossIntroStopwatch.Reset();

            ActiveBossTheme = new BossTheme(bossTheme);
            ActiveBossTheme.startFlag = true;
            SoundEffect introTrack = MusicDict[bossTheme.StartTrack].Value;
            BossIntroDuration = introTrack.Duration.TotalSeconds;
            BossIntroProgress = 0;
            BossIntroPreviousTime = 0;

            SetCombat(introTrack, false, fadeRateMulti);
            SetMusicMode(MusicStyle.Boss);
            CombatVolumeLevel = bossTheme.Volume;

            BossIntroStopwatch.Start();
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
        public static void SetCalm(string track, bool loop = true, float fadeRateMulti = 1)
        {
            SetCalm(MusicDict[track].Value, loop, fadeRateMulti);
        }
        public static void SetCalm(SoundEffect track, bool loop = true, float fadeRateMulti = 1)
        {
            if (!BufferCalmSilence)
            {
                PauseWhenIngamePaused = false;
                fadeRateMultiplier = fadeRateMulti;
            }
            if (CalmMusic != null)
                CalmMusic.Dispose();

            CalmMusic = track.CreateInstance();
            CalmMusic.IsLooped = loop;
            CalmMusic.Play();
            CalmMusic.Volume = 0;
        }
        public static void SetCombat(string track, bool loop = true, float fadeRateMulti = 1)
        {
            SetCombat(MusicDict[track].Value, loop, fadeRateMulti);
        }
        public static void SetCombat(SoundEffect track, bool loop = true, float fadeRateMulti = 1)
        {
            if (!BufferCombatSilence)
            {
                PauseWhenIngamePaused = false;
                fadeRateMultiplier = fadeRateMulti;
            }
            if (CombatMusic != null)
                CombatMusic.Dispose();

            CombatMusic = track.CreateInstance();
            CombatMusic.IsLooped = loop;
            CombatMusic.Play();
            CombatMusic.Volume = 0;
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

                SetCalm(soundtrack.CalmTrack);
                SetCombat(soundtrack.CombatTrack);
                CalmVolumeInterpolant = 1;
                CombatVolumeInterpolant = 0;
                CalmVolumeLevel = soundtrack.Volume;
                CombatVolumeLevel = soundtrack.Volume;
                Initialized = true;
            }
            if (!Initialized)
                return;
            if (!Main.hasFocus || (PauseWhenIngamePaused && Main.gamePaused))
            {
                if (CalmMusic.State == SoundState.Playing)
                    CalmMusic.Pause();
                if (CombatMusic.State == SoundState.Playing)
                    CombatMusic.Pause();
            }
            else
            {
                if (CalmMusic.State == SoundState.Paused)
                    CalmMusic.Resume();
                if (CombatMusic.State == SoundState.Paused)
                    CombatMusic.Resume();
            }
            
            if (MusicMode == MusicStyle.Dynamic)
            {
                if (modPlayer.currentRoom == -1)
                {
                    float calmInterpolant = CalmVolumeInterpolant + (1f / (120f * fadeRateMultiplier));

                    CalmVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, calmInterpolant), 0f, 1f);

                    float combatInterpolant = CombatVolumeInterpolant - (1f / (120f * fadeRateMultiplier));

                    CombatVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, combatInterpolant), 0f, 1f);
                }
                else
                {
                    float calmInterpolant = CalmVolumeInterpolant - (1f / (60f * fadeRateMultiplier));

                    CalmVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, calmInterpolant), 0f, 1f);

                    float combatInterpolant = CombatVolumeInterpolant + (1f / (60f * fadeRateMultiplier));

                    CombatVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, combatInterpolant), 0f, 1f);
                }
            }
           
            
            if (MusicMode == MusicStyle.AllCalm)
            {
                float calmInterpolant = CalmVolumeInterpolant + (1f / (120f * fadeRateMultiplier));

                CalmVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, calmInterpolant), 0f, 1f);


                float combatInterpolant = CombatVolumeInterpolant - (1f / (120f * fadeRateMultiplier));

                CombatVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, combatInterpolant), 0f, 1f);

                if (BufferCombatSilence)
                {
                    if (CombatMusic.Volume <= 0)
                    {
                        SetCombat(Silence);
                        BufferCombatSilence = false;
                    }
                }
            }

            if (MusicMode == MusicStyle.AllCombat)
            {
                float calmInterpolant = CalmVolumeInterpolant - (1f / (60f * fadeRateMultiplier));

                CalmVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, calmInterpolant), 0f, 1f);

                if (BufferCalmSilence)
                {
                    if (CalmMusic.Volume <= 0)
                    {
                        SetCalm(Silence);
                        BufferCalmSilence = false;
                    }
                }

                float combatInterpolant = CombatVolumeInterpolant + (1f / (60f * fadeRateMultiplier));

                CombatVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, combatInterpolant), 0f, 1f);
            }

            if (MusicMode == MusicStyle.Silent)
            {
                float calmInterpolant = CalmVolumeInterpolant - (1f / (180f * fadeRateMultiplier));

                CalmVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, calmInterpolant), 0f, 1f);

                if (BufferCalmSilence)
                {
                    if (CalmMusic.Volume <= 0)
                    {
                        SetCalm(Silence);
                        BufferCalmSilence = false;
                    }
                }

                float combatInterpolant = CombatVolumeInterpolant - (1f / (180f * fadeRateMultiplier));

                CombatVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, combatInterpolant), 0f, 1f);

                if (BufferCombatSilence)
                {
                    if (CombatMusic.Volume <= 0)
                    {
                        SetCombat(Silence);
                        BufferCombatSilence = false;
                    }
                }
            }

            if (MusicMode == MusicStyle.Boss)
            {
                float calmInterpolant = CalmVolumeInterpolant - (1f / (60f * fadeRateMultiplier));

                CalmVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, calmInterpolant), 0f, 1f);

                if (BufferCalmSilence)
                {
                    if (CalmMusic.Volume <= 0)
                    {
                        SetCalm(Silence, true);
                        BufferCalmSilence = false;
                    }
                }

                if (ActiveBossTheme.startFlag)
                {
                    double currentTime = BossIntroStopwatch.Elapsed.TotalSeconds;
                    double difference = currentTime - BossIntroPreviousTime;

                    if (Main.hasFocus && !Main.gamePaused)
                    {
                        BossIntroProgress += difference;
                        if (BossIntroProgress + difference >= BossIntroDuration)
                        {
                            SetCombat(ActiveBossTheme.BattleTrack, true, fadeRateMultiplier);
                            ActiveBossTheme.startFlag = false;
                            BossIntroStopwatch.Reset();
                        }
                    }

                    BossIntroPreviousTime = currentTime;
                }
                if (CombatMusic != null && !CombatMusic.IsDisposed && CombatMusic.State != SoundState.Stopped)
                {
                    float combatInterpolant = CombatVolumeInterpolant + (1f / (60f * fadeRateMultiplier));

                    CombatVolumeInterpolant = MathHelper.Clamp(MathHelper.Lerp(0, 1f, combatInterpolant), 0f, 1f);

                    if (ActiveBossTheme.endFlag)
                    {
                        SetCombat(ActiveBossTheme.EndTrack, false, fadeRateMultiplier);
                        ActiveBossTheme.endFlag = false;
                        ActiveBossTheme.startFlag = false;
                    }
                }
                else
                {
                    if (ActiveBossTheme.startFlag)
                    {
                        SetCombat(ActiveBossTheme.BattleTrack, true, fadeRateMultiplier);
                        ActiveBossTheme.startFlag = false;
                    }
                    else
                    {
                        SetCombat(Silence, true, 1);
                        CombatVolumeInterpolant = 0;
                        SetMusicMode(MusicStyle.Silent);
                    }   
                }
            }
            else
            {
                if (BossIntroStopwatch.IsRunning)
                {
                    BossIntroStopwatch.Reset();
                }
            }

            float musicFade = Main.musicFade[BlankMusicSlotId];
            CalmMusic.Volume = CalmVolumeInterpolant * Main.musicVolume * CalmVolumeLevel * musicFade;
            CombatMusic.Volume = CombatVolumeInterpolant * Main.musicVolume * CombatVolumeLevel * musicFade;
        }
        public override void PreSaveAndQuit()
        {
            ClearMusic();
        }
        public override void ClearWorld()
        {
            ClearMusic();
        }
        public static void ClearMusic()
        {
            Initialized = false;
            if (CalmMusic != null)
                CalmMusic.Dispose();
            if (CombatMusic != null)
                CombatMusic.Dispose();
        }
    }
    public class BossTheme
    {
        public string BattleTrack;
        public string EndTrack;
        public string StartTrack;
        public bool endFlag = false;
        public bool startFlag = true;
        public float Volume;
        public BossTheme(string battleTrack, string startTrack, string endTrack, float volume)
        {
            BattleTrack = battleTrack;
            StartTrack = startTrack;
            EndTrack = endTrack;
            Volume = volume;
        }
        public BossTheme(BossTheme bossTheme)
        {
            BattleTrack = bossTheme.BattleTrack;
            StartTrack = bossTheme.StartTrack;
            EndTrack = bossTheme.EndTrack;
            Volume = bossTheme.Volume;
        }
    }
    public class FloorSoundtrack
    {
        public string CalmTrack;
        public string CombatTrack;
        public float Volume;
        public FloorSoundtrack(string calmTrack, string combatTrack, float volume)
        {
            CalmTrack = calmTrack;
            CombatTrack = combatTrack;
            Volume = volume;
        }
        public FloorSoundtrack (FloorSoundtrack floorSoundtrack)
        {
            CalmTrack = floorSoundtrack.CalmTrack;
            CombatTrack = floorSoundtrack.CombatTrack;
            Volume = floorSoundtrack.Volume;
        }
    }
}
