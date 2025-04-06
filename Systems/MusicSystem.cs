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
using static TerRoguelike.Systems.MusicSystem;
using TerRoguelike.Packets;
using TerRoguelike.Floors;
using TerRoguelike.Schematics;

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
        public static string Escape = "TerRoguelike/Tracks/Escape";
        public static string Credits = "TerRoguelike/Tracks/Credits";
        public static string Darkness = "TerRoguelike/Tracks/Darkness";

        public static FloorSoundtrack SanctuaryTheme = new(
            "TerRoguelike/Tracks/SanctuaryTheme",
            Silence,
            0.37f);

        public static FloorSoundtrack BaseTheme = new(
            "TerRoguelike/Tracks/BaseThemeCalm",
            "TerRoguelike/Tracks/BaseThemeCombat",
            0.26f);
        public static FloorSoundtrack ForestTheme = new(
            "TerRoguelike/Tracks/ForestThemeCalm",
            "TerRoguelike/Tracks/ForestThemeCombat",
            0.22f);
        public static FloorSoundtrack CrimsonTheme = new(
            "TerRoguelike/Tracks/CrimsonThemeCalm",
            "TerRoguelike/Tracks/CrimsonThemeCombat",
            0.22f);
        public static FloorSoundtrack CorruptTheme = new(
            "TerRoguelike/Tracks/CorruptThemeCalm",
            "TerRoguelike/Tracks/CorruptThemeCombat",
            0.22f);
        public static FloorSoundtrack SnowTheme = new(
            "TerRoguelike/Tracks/SnowThemeCalm",
            "TerRoguelike/Tracks/SnowThemeCombat",
            0.22f);
        public static FloorSoundtrack DesertTheme = new(
            "TerRoguelike/Tracks/DesertThemeCalm",
            "TerRoguelike/Tracks/DesertThemeCombat",
            0.22f);
        public static FloorSoundtrack JungleTheme = new(
            "TerRoguelike/Tracks/JungleThemeCalm",
            "TerRoguelike/Tracks/JungleThemeCombat",
            0.22f);
        public static FloorSoundtrack HellTheme = new(
            "TerRoguelike/Tracks/HellThemeCalm",
            "TerRoguelike/Tracks/HellThemeCombat",
            0.22f);
        public static FloorSoundtrack DungeonTheme = new(
            "TerRoguelike/Tracks/DungeonThemeCalm",
            "TerRoguelike/Tracks/DungeonThemeCombat",
            0.20f);
        public static FloorSoundtrack TempleTheme = new(
            "TerRoguelike/Tracks/TempleThemeCalm",
            "TerRoguelike/Tracks/TempleThemeCombat",
            0.24f);

        public static BossTheme PaladinTheme = new(
            "TerRoguelike/Tracks/PaladinTheme",
            "TerRoguelike/Tracks/PaladinThemeStart",
            "TerRoguelike/Tracks/PaladinThemeEnd",
            0.26f,
            BossThemeSyncType.Paladin);

        public static BossTheme BrambleHollowTheme = new(
            "TerRoguelike/Tracks/BrambleHollowTheme",
            "TerRoguelike/Tracks/BrambleHollowThemeStart",
            "TerRoguelike/Tracks/BrambleHollowThemeEnd",
            0.26f,
            BossThemeSyncType.BrambleHollow);

        public static BossTheme CrimsonVesselTheme = new(
            "TerRoguelike/Tracks/CrimsonVesselTheme",
            "TerRoguelike/Tracks/CrimsonVesselThemeStart",
            "TerRoguelike/Tracks/CrimsonVesselThemeEnd",
            0.325f,
            BossThemeSyncType.CrimsonVessel);

        public static BossTheme CorruptionParasiteTheme = new(
            "TerRoguelike/Tracks/CorruptionParasiteTheme",
            "TerRoguelike/Tracks/CorruptionParasiteThemeStart",
            "TerRoguelike/Tracks/CorruptionParasiteThemeEnd",
            0.25f,
            BossThemeSyncType.CorruptionParasite);

        public static BossTheme IceQueenTheme = new(
            "TerRoguelike/Tracks/IceQueenTheme",
            null,
            null,
            0.265f,
            BossThemeSyncType.IceQueen);

        public static BossTheme PharaohSpiritTheme = new(
            "TerRoguelike/Tracks/PharaohSpiritTheme",
            null,
            null,
            0.22f,
            BossThemeSyncType.PharaohSpirit);

        public static BossTheme QueenBeeTheme = new(
            "TerRoguelike/Tracks/QueenBeeTheme",
            "TerRoguelike/Tracks/QueenBeeThemeStart",
            "TerRoguelike/Tracks/QueenBeeThemeEnd",
            0.3f,
            BossThemeSyncType.QueenBee);

        public static BossTheme WallOfFleshTheme = new(
            "TerRoguelike/Tracks/WallOfFleshTheme",
            "TerRoguelike/Tracks/WallOfFleshThemeStart",
            "TerRoguelike/Tracks/WallOfFleshThemeEnd",
            0.4f,
            BossThemeSyncType.WallOfFlesh);

        public static BossTheme SkeletronTheme = new(
            "TerRoguelike/Tracks/SkeletronTheme",
            "TerRoguelike/Tracks/SkeletronThemeStart",
            "TerRoguelike/Tracks/SkeletronThemeEnd",
            0.33f,
            BossThemeSyncType.Skeletron);

        public static BossTheme TempleGolemTheme = new(
            "TerRoguelike/Tracks/TempleGolemTheme",
            "TerRoguelike/Tracks/TempleGolemThemeStart",
            "TerRoguelike/Tracks/TempleGolemThemeEnd",
            0.3f,
            BossThemeSyncType.TempleGolem);

        public static BossTheme FinalBoss1Theme = new(
            "TerRoguelike/Tracks/FinalBoss1",
            null,
            "TerRoguelike/Tracks/FinalBoss1End",
            0.35f,
            BossThemeSyncType.FinalBoss1);

        public static BossTheme FinalBoss2PreludeTheme = new(
            "TerRoguelike/Tracks/FinalBoss2Prelude",
            "TerRoguelike/Tracks/FinalBoss2PreludeStart",
            Silence,
            0.32f,
            BossThemeSyncType.FinalBoss2Prelude);

        public static BossTheme FinalBoss2Theme = new(
            "TerRoguelike/Tracks/FinalBoss2",
            null,
            "TerRoguelike/Tracks/FinalBoss2End",
            0.32f,
            BossThemeSyncType.FinalBoss2);

        public enum BossThemeSyncType
        {
            Paladin,
            BrambleHollow,
            CrimsonVessel,
            CorruptionParasite,
            IceQueen,
            PharaohSpirit,
            QueenBee,
            WallOfFlesh,
            Skeletron,
            TempleGolem,
            FinalBoss1,
            FinalBoss2Prelude,
            FinalBoss2
        }
        public static BossTheme BossThemeFromEnum(BossThemeSyncType type)
        {
            switch (type)
            {
                default:
                case BossThemeSyncType.Paladin:
                    return PaladinTheme;
                case BossThemeSyncType.BrambleHollow:
                    return BrambleHollowTheme;
                case BossThemeSyncType.CrimsonVessel:
                    return CrimsonVesselTheme;
                case BossThemeSyncType.CorruptionParasite:
                    return CorruptionParasiteTheme;
                case BossThemeSyncType.IceQueen:
                    return IceQueenTheme;
                case BossThemeSyncType.PharaohSpirit:
                    return PharaohSpiritTheme;
                case BossThemeSyncType.QueenBee:
                    return QueenBeeTheme;
                case BossThemeSyncType.WallOfFlesh:
                    return WallOfFleshTheme;
                case BossThemeSyncType.Skeletron:
                    return SkeletronTheme;
                case BossThemeSyncType.TempleGolem:
                    return TempleGolemTheme;
                case BossThemeSyncType.FinalBoss1:
                    return FinalBoss1Theme;
                case BossThemeSyncType.FinalBoss2Prelude:
                    return FinalBoss2PreludeTheme;
                case BossThemeSyncType.FinalBoss2:
                    return FinalBoss2Theme;
            }
        }

        public static void FillMusicDictionary()
        {
            if (MusicDictionaryFilled)
                return;

            MusicDictionaryFilled = true;

            List<string> pathList = new List<string>()
            {
                Silence,
                FinalStage,
                Escape,
                Credits,
                Darkness,
                SanctuaryTheme.CalmTrack,
                BaseTheme.CalmTrack,
                BaseTheme.CombatTrack,
                ForestTheme.CalmTrack,
                ForestTheme.CombatTrack,
                CrimsonTheme.CalmTrack,
                CrimsonTheme.CombatTrack,
                CorruptTheme.CalmTrack,
                CorruptTheme.CombatTrack,
                SnowTheme.CalmTrack,
                SnowTheme.CombatTrack,
                DesertTheme.CalmTrack,
                DesertTheme.CombatTrack,
                JungleTheme.CalmTrack,
                JungleTheme.CombatTrack,
                HellTheme.CalmTrack,
                HellTheme.CombatTrack,
                DungeonTheme.CalmTrack,
                DungeonTheme.CombatTrack,
                TempleTheme.CalmTrack,
                TempleTheme.CombatTrack,
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
                FinalBoss2Theme.EndTrack,
            };
            foreach (string path in pathList)
            {
                AddMusic(path);
            }
        }
        internal static void AddMusic(string path)
        {
            if (path == null)
                return;
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
            if (Main.dedServ)
            {
                StartBossThemePacket.Send(bossTheme, fadeRateMulti);
                return;
            }
            if (TerRoguelikeWorld.escape)
                return;

            BossIntroStopwatch.Reset();

            ActiveBossTheme = new BossTheme(bossTheme);
            string startTrack = bossTheme.StartTrack;
            ActiveBossTheme.startFlag = startTrack != null;
            SoundEffect introTrack = MusicDict[startTrack == null ? ActiveBossTheme.BattleTrack : startTrack].Value;
            if (startTrack != null)
            {
                BossIntroDuration = introTrack.Duration.TotalSeconds;
                BossIntroProgress = 0;
                BossIntroPreviousTime = 0;
            }
                
            SetCombat(introTrack, startTrack == null, fadeRateMulti);
            SetMusicMode(MusicStyle.Boss);
            CombatVolumeLevel = bossTheme.Volume;

            if (startTrack != null)
                BossIntroStopwatch.Start();
        }
        public static void SetMusicMode(MusicStyle newMode)
        {
            if (Main.dedServ)
                return;
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
            if (Main.dedServ)
                return;
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
            if (Main.dedServ)
                return;
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
            if (Main.dedServ)
                return;
            if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
                return;

            TerRoguelikePlayer modPlayer = Main.player[Main.myPlayer].GetModPlayer<TerRoguelikePlayer>();
            if (!Initialized && modPlayer != null && modPlayer.currentFloor != null)
            {
                if (!TerRoguelike.mpClient || true)
                {
                    FloorSoundtrack soundtrack = modPlayer.currentFloor.Soundtrack;
                    SetMusicMode(modPlayer.currentFloor.ID == SchematicManager.FloorDict["Sanctuary"] ? MusicStyle.AllCalm : MusicStyle.Dynamic);

                    SetCalm(soundtrack.CalmTrack);
                    SetCombat(soundtrack.CombatTrack);
                    CalmVolumeInterpolant = 1;
                    CombatVolumeInterpolant = 0;
                    CalmVolumeLevel = soundtrack.Volume;
                    CombatVolumeLevel = soundtrack.Volume;
                }
                else
                {
                    var floor = SchematicManager.FloorID[SchematicManager.FloorDict["Sanctuary"]];
                    SetCalm(floor.Soundtrack.CalmTrack);
                    SetCombat(floor.Soundtrack.CombatTrack);
                    SetMusicMode(MusicStyle.AllCalm);
                    CombatVolumeInterpolant = 1;
                    CalmVolumeInterpolant = 0;
                    CalmVolumeLevel = floor.Soundtrack.Volume;
                    CombatVolumeLevel = floor.Soundtrack.Volume;
                }
                
                Initialized = true;
            }
            if (!Initialized)
                return;
            float mute = 1;
            if (!Main.hasFocus || (PauseWhenIngamePaused && Main.gamePaused))
            {
                if (TerRoguelike.mpClient && PauseWhenIngamePaused)
                {
                    mute = 0;
                }
                else
                {
                    if (CalmMusic.State == SoundState.Playing)
                        CalmMusic.Pause();
                    if (CombatMusic.State == SoundState.Playing)
                        CombatMusic.Pause();
                }
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
                            bool enable = false;
                            if (PauseWhenIngamePaused)
                                enable = true;

                            SetCombat(ActiveBossTheme.BattleTrack, true, fadeRateMultiplier);
                            ActiveBossTheme.startFlag = false;
                            BossIntroStopwatch.Reset();

                            if (enable)
                                PauseWhenIngamePaused = true;
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
                        if (ActiveBossTheme.EndTrack == null)
                        {
                            ActiveBossTheme.endFlag = false;
                            ActiveBossTheme.startFlag = false;
                            SetMusicMode(MusicStyle.Silent);
                            fadeRateMultiplier = 2;
                        }
                        else
                        {
                            bool enable = false;
                            if (PauseWhenIngamePaused)
                                enable = true;

                            SetCombat(ActiveBossTheme.EndTrack, false, fadeRateMultiplier);
                            ActiveBossTheme.endFlag = false;
                            ActiveBossTheme.startFlag = false;

                            if (enable)
                                PauseWhenIngamePaused = true;
                        }
                        
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
            CalmMusic.Volume = CalmVolumeInterpolant * Main.musicVolume * CalmVolumeLevel * musicFade * mute;
            CombatMusic.Volume = CombatVolumeInterpolant * Main.musicVolume * CombatVolumeLevel * musicFade * mute;
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
        public BossThemeSyncType Type;
        public BossTheme(string battleTrack, string startTrack, string endTrack, float volume, BossThemeSyncType type)
        {
            BattleTrack = battleTrack;
            StartTrack = startTrack;
            EndTrack = endTrack;
            Volume = volume;
            Type = type;
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
