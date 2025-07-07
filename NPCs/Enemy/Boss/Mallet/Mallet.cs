using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using TerRoguelike.World;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using static TerRoguelike.MainMenu.TerRoguelikeMenu;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;
using Terraria.Utilities;
using Terraria.Localization;
using Terraria.UI.Chat;
using ReLogic.Graphics;
using ReLogic.Content;
using TerRoguelike.ILEditing;
using Microsoft.Xna.Framework.Audio;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging.Abstractions;
using TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles;
using System.IO;
using TerRoguelike.Packets;
using ReLogic.Threading;
using static TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles.SplittingFeather;
using System.Diagnostics;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet
{
    public class Mallet : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public bool SkipCutscene = false;
        public bool phase2 = false;
        public bool initializedText = false;
        public bool justStartedPhase2 = false;
        public override int modNPCID => ModContent.NPCType<Mallet>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Surface"] };
        public static readonly SoundStyle MalletTalk = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/MalletTalk");
        public static readonly SoundStyle Stab = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Stab");
        public static readonly SoundStyle Pain = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Pain");
        public static readonly SoundStyle GlassBreak = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/GlassBreak");
        public static readonly SoundStyle Slam = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Slam");
        public static readonly SoundStyle Transformation = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Transformation");
        public static readonly SoundStyle Scream = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Scream", 2);
        public static readonly SoundStyle Melt = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Melt");
        public static readonly SoundStyle MeltDust = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/MeltDust");
        public static readonly SoundStyle BitHurt = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/BitHurt");
        public static readonly SoundStyle Knockdown = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Knockdown");
        public static readonly SoundStyle ArmorBreak = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/ArmorBreak");
        public static readonly SoundStyle Feather = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Feather", 3);
        public static readonly SoundStyle FeatherBoom = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/FeatherBoom", 2);
        public static readonly SoundStyle Warning = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Warning");
        public static readonly SoundStyle TalonSwipe = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/TalonSwipe");
        public static readonly SoundStyle WingsOut = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/WingsOut");
        public static readonly SoundStyle Flap = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/Flap");
        public static readonly SoundStyle TrashWind = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/TrashWind");
        public static readonly SoundStyle TrashHit = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/TrashHit", 3);
        public static readonly SoundStyle BackgroundDash = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/BackgroundDash", 2);
        public static readonly SoundStyle MeteorBreak = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/MeteorBreak");
        public static readonly SoundStyle MeteorBoom = new("TerRoguelike/NPCs/Enemy/Boss/Mallet/MeteorBoom");
        public Texture2D starTex, pulseTex, squareTex, IntroTalkTex, IntroTransformEffect, TalkBubble, Syringe, GlassShard, Chest, Hair, Head, Wings, LeftLeg, LeftTalon, RightLeg, RightTalon, Screaming, FlyAway, WingAttack, KnockedDown, BackgroundFly, BGBack, BGFront, BGStars, BlueTemporaryBlock, sparkTex = null;
        public double animationCounter = 0;
        public static Point IntroTalkFrames = new Point(3, 8);
        public bool middleCutsceneStarted = false;
        public Vector2 oldDrawPos = Vector2.Zero;
        public Vector2 maskDrawOff = Vector2.Zero;
        public List<int> YellowPulses = [];
        public int maxYellowPulseTime = 60;
        public void AddPulse()
        {
            YellowPulses.Add(maxYellowPulseTime);
        }
        public List<BackgroundStar> BackgroundStars = [];
        public override int CombatStyle => -1;
        public int currentFrame = 0;

        public int cutsceneDuration = 810;
        public int middleCutsceneDuration = 385;

        public int deadTime = 0;
        public int deathCutsceneDuration = 540;
        public float goopFadeTime = 50;
        public int despawnTime = 0;

        public float textSpeed = 0.5f;
        public float textProgress = -0.5f;
        public int textProgressPause = 0;
        public int talkFrameCounter = 0;
        public bool DrawTalkBubble = false;
        public Vector2 SyringePos = new Vector2(-29, -21);
        public float SyringeRot = 0.08f;
        public List<StringBundle> JstcIntro = [];
        public List<StringBundle> JstcMiddle = [];
        public List<StringBundle> JstcEnd = [];
        public float zDepth = 1;
        public SlotId WindSlot;
        public SlotId DashSlot;
        public int flapSoundCooldown = 0;
        public int attackSpice = 0;
        public static SoundEffect talkSound;
        public class StringBundle
        {
            public bool Event = false;
            public List<string> strings = [];
            public StringBundle(string Strings)
            {
                for (int i = 0; i < Strings.Length; i++)
                {
                    var c = Strings[i];

                    if (c == '\n')
                    {
                        int length = Strings[i..].Length;
                        if (length >= 3)
                        {
                            if (Strings[i + 1] == '\n' && Strings[i + 2] == '\n')
                            {
                                Event = true;
                            }
                        }

                        if (i == 0)
                        {
                            Strings = Strings.Remove(0, 1);
                        }
                        else
                        {
                            strings.Add(Strings[..i]);
                            Strings = Strings.Remove(0, i + 1);
                            i = 0;
                        }
                    }
                    else if (i == Strings.Length - 1)
                    {
                        strings.Add(Strings);
                    }
                }
                for (int i = 0; i < strings.Count; i++)
                {
                    if (strings[i] == "")
                    {
                        strings.RemoveAt(i);
                        i--;
                    }
                }
            }
            public int TotalLength
            {
                get
                {
                    int length = 0;
                    for (int i = 0; i < strings.Count; i++)
                    {
                        length += strings[i].Length;
                    }
                    return length;
                }
            }
            public bool EndOfLine(int index)
            {
                if (index < 0)
                    index = 0;
                int totalLength = TotalLength;
                if (index >= totalLength)
                    index = totalLength - 1;

                for (int i = 0; i < strings.Count; i++)
                {
                    if (index >= strings[i].Length)
                    {
                        index -= strings[i].Length;
                        continue;
                    }
                    if (index == strings[i].Length - 1)
                        return true;
                }
                return false;
            }
            public char GetCharAt(int index)
            {
                if (index < 0)
                    return ' ';
                int totalLength = TotalLength;
                if (index >= totalLength)
                    index = totalLength - 1;

                int who = 0;
                for (int i = 0; i < strings.Count; i++)
                {
                    if (index >= strings[i].Length)
                    {
                        index -= strings[i].Length;
                        continue;
                    }

                    who = i;
                    break;
                }
                return strings[who][index];
            }
            public List<string> StringDisplay(int index)
            {
                List<string> strList = [];

                if (index < 0)
                    return strList;
                int totalLength = TotalLength;
                if (index >= totalLength)
                    index = totalLength - 1;

                for (int i = 0; i < strings.Count; i++)
                {
                    if (index >= strings[i].Length)
                    {
                        index -= strings[i].Length;
                        strList.Add(strings[i]);
                        continue;
                    }

                    
                    strList.Add(strings[i][..(index + 1)]);
                    break;
                }

                return strList;
            }
        }
        public void PlayTalkSound()
        {
            if (Main.dedServ)
                return;

            var instance = talkSound.CreateInstance();
            instance.Volume = Main.soundVolume * 0.67f;
            instance.Pitch = Main.rand.NextFloat(-0.001f, 0.001f);
            instance.Play();
            ExtraSoundSystem.SpecialSoundReplace.Add(instance);
        }
        public bool attackInitialized = false;
        //intro vars
        public bool TextControl
        {
            get {
                var jp = PlayerInput.Triggers.JustPressed;
                return jp.Jump || jp.MouseLeft || jp.MouseRight || jp.QuickMount || jp.Grapple;
            }
        }
        public int eventCounter = 0;
        public int eventTimer = 0;
        public int introTime = 0;
        public int middleTime = 0;
        public int fadeBlackTime = 60;
        public int flyDownTime = 180;
        public List<int> oldAttacks = [0];

        public static Attack None = new Attack(0, 0, 90);
        public static Attack PatternWingsTalons = new Attack(1, 30, 320);
        public static Attack PatternSplit = new Attack(2, 30, 210);
        public static Attack PatternDashingTalon = new Attack(3, 30, 260);
        public static Attack PatternDoubleWing = new Attack(4, 30, 180);
        public static Attack PatternFeatherWalls = new Attack(5, 30, 240);
        public static Attack PatternTalonSplit = new Attack(6, 30, 330);
        public static Attack PatternFeatherCircle = new Attack(7, 30, 245);
        public static Attack PatternRandomness = new Attack(8, 50, 298); // phase 2 only
        public static Attack Trash = new Attack(9, 40, 407); // buffed in phase 2
        public static Attack Meteors = new Attack(10, 40, 240); // buffed in phase 2
        public static Attack Stars = new Attack(11, 40, 240); // buffed in phase 2
        public static Attack Dash = new Attack(12, 50, 330); // phase 2 only

        public List<ExtraHitbox> hitboxes = [];
        public bool CollisionPass = false;
        public Vector2 dashStart = Vector2.Zero;

        public static DynamicSpriteFont DotumChePixel = null;
        public static bool TalkFont
        {
            get {
                return !(DotumChePixel == null || Environment.OSVersion.Platform != PlatformID.Win32NT || !GameCulture.FromCultureName(GameCulture.CultureName.English).IsActive || translationModEnabled);
            }
        }
        public class BackgroundParticle
        {
            public Particle particle;
            public float zDepth;
            public BackgroundParticle(Particle Particle, float ZDepth)
            {
                particle = Particle;
                zDepth = ZDepth;
            }

        }
        public List<BackgroundParticle> bgParticles = [];
        public Vector2 starPosition = Vector2.Zero;
        public Vector2 starVelocity = Vector2.Zero;
        public float starZDepth = 1;

        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
                talkSound = ModContent.Request<SoundEffect>("TerRoguelike/NPCs/Enemy/Boss/Mallet/MalletTalk", AssetRequestMode.ImmediateLoad).Value;

            if (DotumChePixel is null && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                DotumChePixel = ModContent.Request<DynamicSpriteFont>("TerRoguelike/Fonts/DotumChePixel", AssetRequestMode.ImmediateLoad).Value;
            }
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            NPCID.Sets.NoMultiplayerSmoothingByType[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            modNPC.TerRoguelikeBoss = true;
            NPC.width = 150;
            NPC.height = 150;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 50000;
            NPC.knockBackResist = 0f;
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.AdaptiveArmorEnabled = true;
            modNPC.AdaptiveArmorAddRate = 300;
            modNPC.AdaptiveArmorDecayRate = 70;
            modNPC.AdaptiveArmorCap = 20000;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.drawBeforeWalls = true;

            #region Texture setup
            starTex = TexDict["StarrySky"];
            pulseTex = TexDict["InvisibleProj"];
            squareTex = TexDict["Square"];
            sparkTex = TexDict["ThinSpark"];
            IntroTalkTex = TexDict["MalletIntroTalk"];
            IntroTransformEffect = TexDict["TransformEffect"];
            TalkBubble = TexDict["TalkBubble"];
            Syringe = TexDict["Syringe"];
            GlassShard = TexDict["GlassShard"];
            Chest = TexDict["Chest"];
            FlyAway = TexDict["FlyAway"];
            Hair = TexDict["Hair"];
            Head = TexDict["Head"];
            KnockedDown = TexDict["KnockedDown"];
            LeftLeg = TexDict["LeftLeg"];
            LeftTalon = TexDict["LeftTalon"];
            RightLeg = TexDict["RightLeg"];
            RightTalon = TexDict["RightTalon"];
            Screaming = TexDict["Screaming"];
            WingAttack = TexDict["WingAttack"];
            Wings = TexDict["Wings"];
            BackgroundFly = TexDict["BackgroundFly"];
            BGBack = TexDict["BGBack"];
            BGFront = TexDict["BGFront"];
            BGStars = TexDict["BGStars"];
            BlueTemporaryBlock = TexDict["BlueTemporaryBlock"];
            #endregion
        }
        public override void OnSpawn(IEntitySource source)
        {
            bool allow = false;
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC parent = Main.npc[parentSource.Entity.whoAmI];
                    if (parent.type == ModContent.NPCType<TrueBrain>())
                    {
                        NPC.Center = CutsceneSystem.cameraTargetCenter;
                        allow = true;
                    }
                }
            }
            if (!allow)
            {
                NPC.active = false;
                return;
            }
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.localAI[0] = -(cutsceneDuration);
            SetUpStars();
            if (SkipCutscene)
            {
                NPC.localAI[0] = -31;
                NPC.localAI[1] = 200;
                eventCounter = 4;
                SetBossTrack(JstcTheme, 0.8f);
                CombatVolumeInterpolant = 1;
                NPC.frameCounter = 168;
                introTime = 10000;
            }
            else
            {
                SetMusicMode(MusicStyle.Silent);
            }
            
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            oldAttacks.Add(None.Id);
            ableToHit = false;

            InitializeText();
        }
        public void InitializeText()
        {
            initializedText = true;
            string introString = Language.GetOrRegister("Mods.TerRoguelike.JstcIntro").Value;
            string middleString = Language.GetOrRegister("Mods.TerRoguelike.JstcMiddle").Value;
            string endString = Language.GetOrRegister("Mods.TerRoguelike.JstcEnd").Value;
            string username = Main.dedServ ? "John" : Environment.UserName;
            string[] recorders = ["obs64, Streamlabs, StreamlabsOBS"];
            /*
            foreach (string check in recorders) // this didn't work
            {
                if (Process.GetProcessesByName(check).Length > 0)
                {
                    username = Main.LocalPlayer.name;
                    break;
                }
            }
            */
            string nameCheck = "";
            for (int i = 0; i < username.Length; i++)
            {
                if (username[i] == ' ')
                {
                    break;
                }
                nameCheck += username[i];
            }
            username = nameCheck;

            StringToBundleList(introString, ref JstcIntro);
            StringToBundleList(middleString, ref JstcMiddle);
            StringToBundleList(endString, ref JstcEnd);

            void StringToBundleList(string String, ref List<StringBundle> bundleList)
            {
                for (int i = 0; i < String.Length; i++)
                {
                    var c = String[i];
                    if (c == '@')
                    {
                        String = String.Remove(i, 1);
                        String = String.Insert(i, username);
                    }
                }

                for (int i = 0; i < String.Length; i++)
                {
                    if (i == String.Length - 1)
                    {
                        bundleList.Add(new(String));
                        break;
                    }
                    var c = String[i];

                    if (c == '\n')
                    {
                        if (i == String.Length - 2)
                        {
                            bundleList.Add(new(String));
                            break;
                        }
                        if (String[i + 1] == '\n')
                        {
                            int target = i + 1;
                            if (String[i + 2] == '\n')
                                target++;
                            bundleList.Add(new(String[..(target + 1)]));
                            String = String.Remove(0, target + 1);
                            i = 0;
                        }
                    }
                }
            }
        }
        public override bool PreAI()
        {
            if (!initializedText)
            {
                InitializeText();
                SetMusicMode(MusicStyle.Silent);
            }
            return true;
        }
        public override void DrawBehind(int index)
        {
            if (NPC.localAI[0] < 0 || despawnTime > 0)
            {
                NPC.hide = true;
                modNPC.drawAfterEverything = true;
            }
            else
            {
                if (zDepth > 1)
                {
                    NPC.hide = true;
                    Main.instance.DrawCacheNPCsOverPlayers.Add(index);
                }
                NPC.hide = false;
                modNPC.drawAfterEverything = false;
            }
        }
        public override void PostAI()
        {
            if (flapSoundCooldown > 0)
                flapSoundCooldown--;
            
            Being.forceTextControl = false;
            Main.StopRain();

            if (modNPC.currentUpdate == 1)
            {
                if (Junk.junkSoundCooldown > 0)
                    Junk.junkSoundCooldown--;

                if (SoundEngine.TryGetActiveSound(WindSlot, out var sound))
                {
                    if (CutsceneSystem.cutsceneActive)
                        sound.Volume *= 0.8f;
                    else if (NPC.ai[0] != Trash.Id)
                    {
                        sound.Volume *= 0.92f;
                    }

                }
                if (SoundEngine.TryGetActiveSound(DashSlot, out var sound2))
                {
                    if (CutsceneSystem.cutsceneActive)
                        sound2.Volume *= 0.8f;
                    else if (NPC.ai[0] != Dash.Id && NPC.ai[0] != None.Id)
                    {
                        sound2.Volume *= 0.92f;
                    }
                    else
                    {
                        Vector2 basePos = NPC.ai[1] <= 120 ? starPosition : NPC.Center;
                        float thisZ = NPC.ai[1] <= 120 ? starZDepth : zDepth;
                        sound2.Position = (basePos - Main.Camera.Center) * thisZ + Main.Camera.Center;
                    }
                }

                if (YellowPulses.Count > 0)
                {
                    for (int i = 0; i < YellowPulses.Count; i++)
                    {
                        YellowPulses[i]--;
                    }

                    YellowPulses.RemoveAll(x => x < 0);
                }

                int cloneType = ModContent.ProjectileType<LightClone>();
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.type != cloneType)
                        continue;
                    var modProj = proj.ModProj();
                    if (modProj == null || modProj.npcOwner != NPC.whoAmI)
                        continue;

                    Color particleColor = Color.Yellow * 0.87f;
                    float projZDepth = proj.ai[0];
                    if (projZDepth > 1)
                    {
                        particleColor *= 1 - ((projZDepth - 1) * 0.10f);
                    }

                    Rectangle spawnRect = new Rectangle((int)proj.localAI[0], (int)proj.localAI[1], 0, 0);
                    spawnRect.Inflate(400, 40);
                    spawnRect.Y += 40;
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 particlePos = Main.rand.NextVector2FromRectangle(spawnRect);
                        Vector2 particleVel = Main.rand.NextVector2Circular(1, 1) / projZDepth;
                        for (int i = 0; i < 4; i++)
                            bgParticles.Add(new(new ThinSpark(particlePos, particleVel, 40, particleColor, new Vector2(0.05f, 0.035f) * 2 * projZDepth, MathHelper.PiOver2 * i, true, false), projZDepth));
                    }

                }

                if (bgParticles != null && bgParticles.Count >= 0)
                {
                    FastParallel.For(0, bgParticles.Count, delegate (int start, int end, object context)
                    {
                        for (int i = start; i < end; i++)
                        {
                            Particle particle = bgParticles[i].particle;
                            particle.Update();
                        }
                    });
                    bgParticles.RemoveAll(x => x.particle.timeLeft <= 0);
                }
            }

            #region hitboxes
            if (displayBackgroundFly)
            {
                if (backgroundFlyFrameCounter >= 4)
                {
                    hitboxes = new List<ExtraHitbox>()
                    {
                        new ExtraHitbox(new Point(80, 80), new Vector2(0)),
                        new ExtraHitbox(new Point(80, 80), new Vector2(30, 10), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(20, 55), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(-60, 30), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(80, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(-120, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(140, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(-180, 30), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(200, 34), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(-240, 20), true, false),
                        new ExtraHitbox(new Point(80, 80), new Vector2(260, 20), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(-280, 10), true, false),
                        new ExtraHitbox(new Point(60, 60), new Vector2(300, 10), true, false),
                    };
                }
                else
                    hitboxes = [];
            }
            else if (displayFlyAway)
            {
                hitboxes = new List<ExtraHitbox>()
                {
                    new ExtraHitbox(new Point(80, 80), new Vector2(0)),
                    new ExtraHitbox(new Point(80, 80), new Vector2(0, 60)),
                    new ExtraHitbox(new Point(80, 80), new Vector2(0, -60)),
                };
                switch (flyAwayFrameCounter)
                {
                    default:
                    case 0:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, -60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, -60), true, false));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 110)));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 145)));
                        break;
                    case 1:
                    case 2:
                    case 5:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, -25), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, -25), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, -60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, -60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, -80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, -80), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(-220, -140), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(220, -140), true, false));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 110)));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 145)));
                        break;
                    case 3:
                    case 4:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-100, -80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(100, -80), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(-120, -160), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(120, -160), true, false));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 110)));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 145)));
                        break;
                    case 6:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-100, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(100, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-150, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(150, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-200, 50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(200, 50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-230, 20), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(230, 20), true, false));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 110)));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(-10, 145)));
                        break;
                    case 7:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-30, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(30, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-45, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(45, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-75, 120), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(75, 120), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-90, 160), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(90, 160), true, false));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(0, 110)));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(0, 145)));
                        break;
                    case 8:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-30, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(30, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-30, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(30, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-30, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(30, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-30, 120), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(30, 120), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-30, 160), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(30, 160), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-30, 200), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(30, 200), true, false));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(0, 110)));
                        hitboxes.Add(new(new Point(36, 36), new Vector2(0, 145)));
                        break;
                }
            }
            else if (displayScream)
            {
                hitboxes =
                [
                    new ExtraHitbox(new Point(80, 80), new Vector2(0)),
                    new ExtraHitbox(new Point(80, 80), new Vector2(0, -60)),
                    new ExtraHitbox(new Point(50, 50), new Vector2(-30, 45)),
                    new ExtraHitbox(new Point(50, 50), new Vector2(30, 45)),
                    new ExtraHitbox(new Point(50, 50), new Vector2(-50, 75)),
                    new ExtraHitbox(new Point(50, 50), new Vector2(50, 75)),
                    new ExtraHitbox(new Point(30, 30), new Vector2(-75, 105), true, false),
                    new ExtraHitbox(new Point(30, 30), new Vector2(75, 105), true, false),
                    new ExtraHitbox(new Point(30, 30), new Vector2(-95, 125), true, false),
                    new ExtraHitbox(new Point(30, 30), new Vector2(95, 125), true, false),
                    new ExtraHitbox(new Point(30, 30), new Vector2(-105, 145), true, false),
                    new ExtraHitbox(new Point(30, 30), new Vector2(105, 145), true, false),
                    new ExtraHitbox(new Point(30, 30), new Vector2(-115, 165), true, false),
                    new ExtraHitbox(new Point(30, 30), new Vector2(115, 165), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(-60, -10), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(60, -10), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(-120, -20), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(120, -20), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(-180, -40), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(180, -40), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(-220, -70), true, false),
                    new ExtraHitbox(new Point(80, 80), new Vector2(220, -70), true, false),
                    new ExtraHitbox(new Point(60, 60), new Vector2(-250, -100), true, false),
                    new ExtraHitbox(new Point(60, 60), new Vector2(250, -100), true, false),
                ];
            }
            else if (displayFlap)
            {
                hitboxes = new List<ExtraHitbox>()
                {
                    new ExtraHitbox(new Point(80, 80), new Vector2(0)),
                    new ExtraHitbox(new Point(80, 80), new Vector2(10, 60)),
                    new ExtraHitbox(new Point(80, 80), new Vector2(0, -60)),
                    new ExtraHitbox(new Point(36, 36), new Vector2(-10, 110)),
                    new ExtraHitbox(new Point(36, 36), new Vector2(-10, 145)),
                };
                switch (flapFrameCounter)
                {
                    case 0:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, -20), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, -20), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, -40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, -40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, -60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, -60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, -90), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, -90), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(-210, -130), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(210, -130), true, false));
                        break;
                    case 1:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, -50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, -50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-190, -90), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(190, -90), true, false));
                        break;
                    case 2:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-100, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(100, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-150, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(150, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-210, 50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(210, 50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-250, 20), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(250, 20), true, false));
                        break;
                    case 3:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-100, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(100, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-110, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(110, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-150, 110), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(150, 110), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-230, 110), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(230, 110), true, false));
                        break;
                    case 4:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, -10), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(-180, 0), true, false));
                        hitboxes.Add(new(new Point(100, 100), new Vector2(180, 0), true, false));
                        hitboxes.Add(new(new Point(120, 120), new Vector2(-220, 0), true, false));
                        hitboxes.Add(new(new Point(120, 120), new Vector2(220, 0), true, false));
                        break;
                    default:
                    case 5:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, -60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, -60), true, false));
                        break;
                }
            }
            else
            {
                hitboxes = new List<ExtraHitbox>()
                {
                    new ExtraHitbox(new Point(80, 80), new Vector2(0)),
                    new ExtraHitbox(new Point(80, 80), new Vector2(10, 60)),
                    new ExtraHitbox(new Point(80, 80), new Vector2(0, -60)),
                    new ExtraHitbox(new Point(36, 36), new Vector2(-10, 110), true, false), //foot hitbox too op
                    new ExtraHitbox(new Point(36, 36), new Vector2(-10, 145), true, false),
                };
                switch (wingsFrameCounter)
                {
                    default:
                    case 0:
                    case 1:
                    case 7:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, -60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, -60), true, false));
                        break;
                    case 2:
                    case 6:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, -10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, -30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, -30), true, false));
                        break;
                    case 3:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, 30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, 30), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, 50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, 30), true, false));
                        break;
                    case 5:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 0), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-120, 10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(120, 10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-180, 10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(180, 10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-220, 10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(220, 10), true, false));
                        break;
                    case 4:
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-60, 10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(60, 10), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-100, 40), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(100, 50), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-140, 70), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(140, 70), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-210, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(210, 80), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(-240, 60), true, false));
                        hitboxes.Add(new(new Point(80, 80), new Vector2(240, 60), true, false));
                        break;
                }
            }
            #endregion
        }
        public void UpdateSpeech(List<StringBundle> jstcSpeech, bool allowControl = true, bool forceControl = false, bool keepBubbleThroughEvent = false)
        {
            var speech = jstcSpeech[(int)NPC.localAI[1]];
            if (Being.forceTextControl)
                forceControl = true;
            bool control = allowControl && (forceControl || (TextControl && !TerRoguelike.mpClient));

            int speechLength = speech.TotalLength;
            if (TerRoguelike.mpClient && TextControl && textProgress >= speechLength)
            {
                ProgressDialoguePacket.Send();
            }

            
            if (textProgress < speechLength)
            {
                if (textProgressPause == 0)
                {
                    DrawTalkBubble = true;
                    textProgress += textSpeed;
                    if (textProgress < speechLength)
                    {
                        if (textProgress == (int)textProgress)
                        {
                            char speechChar = speech.GetCharAt((int)textProgress);
                            if (speech.EndOfLine((int)textProgress))
                            {
                                textProgressPause += 2;
                            }
                            if (speechChar != ' ' && speechChar != '\n')
                            {
                                if (speechChar == '.' || speechChar == '?' || speechChar == ',' || speechChar == '!' || speechChar == '？' || speechChar == '…' || speechChar == '，')
                                    textProgressPause += 9;
                                else if (talkFrameCounter == 0)
                                    talkFrameCounter = 20;

                                PlayTalkSound();
                            }
                        }
                    }
                }
            }
            else
            {
                talkFrameCounter = 0;
            }


            if (control)
            {
                if (textProgress < speechLength && !TerRoguelike.mpClient)
                {
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        textProgressPause = 0;
                        textProgress = speechLength;
                    }
                }
                else
                {
                    if (Main.dedServ)
                        ProgressDialoguePacket.Send();
                    textProgressPause = 0;
                    textProgress = -0.5f;
                    if (jstcSpeech[(int)NPC.localAI[1]].Event)
                    {
                        DrawTalkBubble = keepBubbleThroughEvent;
                        eventCounter++;
                        eventTimer = 0;
                    }
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] >= jstcSpeech.Count)
                    {
                        NPC.localAI[1] = 100;
                        if (deadTime == 0)
                        {
                            if (middleCutsceneStarted)
                            {
                                SetBossTrack(Jstc2Theme);
                                PauseWhenIngamePaused = true;
                                CombatVolumeInterpolant = 1;
                                phase2 = true;
                                justStartedPhase2 = true;
                            }
                            else
                            {
                                SetBossTrack(JstcTheme, 0.8f);
                                PauseWhenIngamePaused = true;
                                CombatVolumeInterpolant = 1;
                            }
                        }
                    }
                    NPC.netUpdate = true;
                }
            }
        }
        public override void AI()
        {
            NPC.netSpam = 0;
            animationCounter += 0.1667d;
            if (talkFrameCounter > 0)
                talkFrameCounter--;
            if (textProgressPause > 0)
                textProgressPause--;
            eventTimer++;

            modNPC.diminishingDR += 100;

            if (NPC.localAI[0] + cutsceneDuration > 5)
            {
                Main.time = 16500;
                Main.dayTime = false;
                
            }

            if (deadTime > 0)
            {
                CheckDead();
                return;
            }

            ableToHit = NPC.localAI[0] >= 0;
            canBeHit = true;

            if (NPC.localAI[0] < -30)
            {
                for (int i = 0; i < Main.combatText.Length; i++)
                {
                    var text = Main.combatText[i];
                    text.active = false;
                }

                target = modNPC.GetTarget(NPC);

                if (!middleCutsceneStarted)
                {
                    if (!attackInitialized && NPC.localAI[0] <= -cutsceneDuration + 10)
                    {
                        attackInitialized = true;
                        CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 60, 30, 1.25f, CutsceneSystem.CutsceneSource.Boss);
                        NPC.localAI[0]++;
                        CutsceneSystem.cutsceneTimer = cutsceneDuration - 61;
                        var room = modNPC.GetParentRoom();
                        if (room != null)
                            room.bossDead = false;
                    }

                    bool intro = NPC.localAI[1] < 200;
                    if (intro)
                    {
                        if (CutsceneSystem.cutsceneTimer < cutsceneDuration - 62)
                            CutsceneSystem.cutsceneTimer++;
                        introTime++;

                        if (introTime <= fadeBlackTime)
                        {
                            NPC.frameCounter = (int)(introTime / 6d) % 6;
                            CutsceneSystem.cameraTargetCenter = spawnPos;
                            CutsceneSystem.cameraTargetCenter += Vector2.UnitY * 500 * (float)Math.Pow(introTime / 60f, 1.5f);

                        }
                        else if (introTime < fadeBlackTime + flyDownTime)
                        {
                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                Main.projectile[i].active = false;
                            }
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                if (i == NPC.whoAmI)
                                    continue;

                                Main.npc[i].active = false;
                            }

                            if (!(introTime > fadeBlackTime + flyDownTime - 30 && (int)NPC.frameCounter == 0))
                                NPC.frameCounter = (int)(introTime / 6d) % 6;
                            if (introTime == fadeBlackTime + 1)
                            {
                                ZoomSystem.SetZoomAnimation(2.25f, 1);
                                if (modNPC.isRoomNPC)
                                {
                                    Vector2 off = new Vector2(0, 1200);
                                    NPC.Center += off;
                                    spawnPos += off;
                                }
                                CutsceneSystem.cameraTargetCenter = NPC.Center + new Vector2(0, 32);
                            }
                        }
                        else
                        {
                            if (eventCounter < 2)
                            {
                                if (NPC.frameCounter < 6)
                                    NPC.frameCounter = 6;
                                if (NPC.frameCounter < 12)
                                {
                                    NPC.frameCounter += 0.125d;
                                }
                                else
                                {
                                    NPC.frameCounter = 12;
                                    if (eventCounter != 1 || eventTimer > 140)
                                        UpdateSpeech(JstcIntro);
                                }

                            }
                            else if (eventCounter == 2)
                            {
                                if (NPC.frameCounter < 13)
                                    NPC.frameCounter = 13;
                                if (NPC.frameCounter < 23)
                                    NPC.frameCounter += 0.1667d;
                                else
                                {
                                    NPC.frameCounter = 23;
                                    UpdateSpeech(JstcIntro);
                                    if (eventCounter == 3)
                                        NPC.frameCounter = 1;
                                }

                            }
                            else if (eventCounter >= 3)
                            {
                                if (NPC.localAI[1] < 100)
                                {
                                    if (eventCounter == 3)
                                    {
                                        if (NPC.frameCounter < 77)
                                        {
                                            double increment1 = 0.1667d;
                                            double increment2 = 0.1d;
                                            if (NPC.frameCounter < 60)
                                                NPC.frameCounter += increment1;
                                            else
                                                NPC.frameCounter += increment2;

                                            if ((int)NPC.frameCounter == 8 && NPC.frameCounter - increment1 < 8)
                                            {
                                                SoundEngine.PlaySound(Stab with { Volume = 1 });
                                                ScreenshakeSystem.SetScreenshake(40, 22, 3, 1);
                                            }


                                            if (NPC.frameCounter >= 51)
                                            {
                                                if ((int)NPC.frameCounter == 51 && NPC.frameCounter - increment1 < 51)
                                                {
                                                    SoundEngine.PlaySound(Pain with { Volume = 1f });
                                                    ScreenshakeSystem.SetScreenshake(40, 22, 3, 1);
                                                }

                                                SyringeRot -= 0.08f;
                                                float posInterpolant = Math.Min(((float)NPC.frameCounter - 51) / 4f, 1);
                                                SyringePos += new Vector2(-0.28f, 2.6f) * posInterpolant;
                                                if ((int)NPC.frameCounter == 61 && NPC.frameCounter - increment2 < 61)
                                                {
                                                    SoundEngine.PlaySound(GlassBreak with { Volume = 1f });
                                                    for (float i = -2.5f; i < 3; i++)
                                                    {
                                                        ParticleManager.AddParticle(new GlassShard(
                                                            SyringePos + NPC.Center + new Vector2(i * 4 + Main.rand.NextFloat(-4, 4), 0),
                                                            new Vector2((i + Main.rand.NextFloat()) * 0.24f, -2),
                                                            130, Color.White, new Vector2(0.25f), Main.rand.Next(3), Main.rand.NextFloat(MathHelper.TwoPi), 0.3f * Math.Sign(i),
                                                            Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                                                            0.065f, 2.6f, 65),
                                                            ParticleManager.ParticleLayer.AfterEverything);
                                                    }
                                                }
                                            }
                                        }
                                        if (NPC.frameCounter > 77)
                                            NPC.frameCounter = 77;
                                    }
                                    else
                                    {
                                        if (NPC.frameCounter < 83)
                                        {
                                            NPC.frameCounter += 0.15d;
                                            if (NPC.frameCounter >= 83)
                                            {
                                                SoundEngine.PlaySound(Slam with { Volume = 1f });
                                                ScreenshakeSystem.SetScreenshake(30, 22, 3, 1);
                                            }
                                        }
                                        if (NPC.frameCounter > 83)
                                            NPC.frameCounter = 83;
                                    }
                                }
                                else
                                {
                                    double increment = 0.1667d;
                                    DrawTalkBubble = false;
                                    if ((int)NPC.frameCounter == 135 || (int)NPC.frameCounter == 146)
                                        NPC.frameCounter += 0.0834d;
                                    else
                                        NPC.frameCounter += increment;
                                    if ((int)NPC.frameCounter == 89 && NPC.frameCounter - increment < 89)
                                    {
                                        ScreenshakeSystem.SetScreenshake(400, 9.5f, 3, 1);
                                    }
                                }



                                if (NPC.frameCounter == 77 || NPC.frameCounter == 83)
                                    UpdateSpeech(JstcIntro);
                                if (NPC.frameCounter == 83 && NPC.localAI[1] == 100)
                                {
                                    SoundEngine.PlaySound(Transformation with { Volume = 1f });
                                }
                                if (NPC.frameCounter >= 168)
                                {
                                    NPC.localAI[1] = 200;
                                    NPC.frameCounter = 168;
                                    animationCounter = 168;
                                    eventCounter = 0;
                                    eventTimer = 0;
                                }

                            }
                        }

                    }
                    else
                    {
                        NPC.frameCounter += 0.1d;
                        NPC.localAI[0]++;
                    }

                    int time = (int)NPC.localAI[0] + cutsceneDuration;
                    if (time == 120)
                        NPC.Center += new Vector2(0, -500);
                    if (time > 120 && time < 460)
                        NPC.Center += new Vector2(0, 1.3f);
                    if (time == 535)
                    {
                        SoundEngine.PlaySound(Scream with { Volume = 1f, Variants = [1] });
                        ScreenshakeSystem.SetScreenshake(150, 10, 2, 1);
                    }
                    if (time >= 740)
                    {
                        BossAI();
                    }
                }
                else
                {
                    if (NPC.localAI[0] == -middleCutsceneDuration)
                    {
                        CutsceneSystem.SetCutscene(NPC.Center, middleCutsceneDuration, 30, 30, 2.25f, CutsceneSystem.CutsceneSource.Boss);
                    }


                    bool middle = NPC.localAI[1] < 100;
                    if (middle)
                    {
                        if (NPC.localAI[0] + middleCutsceneDuration <= 60)
                        {
                            NPC.localAI[0]++;
                        }
                        else
                        {
                            CutsceneSystem.cutsceneTimer++;
                        }
                        middleTime++;
                        modNPC.AdaptiveArmor = 0;

                        if (eventCounter == 0)
                        {
                            if (middleTime == 60)
                            {
                                SoundEngine.PlaySound(BitHurt with { Volume = 0.7f, Pitch = -1, MaxInstances = 3 });
                                SoundEngine.PlaySound(BitHurt with { Volume = 0.7f, Pitch = -0.5f, MaxInstances = 3 });
                                SoundEngine.PlaySound(BitHurt with { Volume = 0.7f, Pitch = 0.2f, MaxInstances = 3 });
                                SoundEngine.PlaySound(Knockdown with { Volume = 0 });
                                for (int i = 0; i < Main.maxProjectiles; i++)
                                {
                                    Main.projectile[i].active = false;
                                }
                                for (int i = 0; i < Main.maxNPCs; i++)
                                {
                                    if (i == NPC.whoAmI)
                                        continue;

                                    Main.npc[i].active = false;
                                }
                            }
                            if (middleTime > 210)
                                UpdateSpeech(JstcMiddle);
                            else if (middleTime > 90 && middleTime < 180)
                            {
                                CutsceneSystem.cameraTargetCenter += Vector2.UnitY * (1 - (middleTime / 180f)) * 4;
                            }
                        }
                        else if (eventCounter == 1)
                        {
                            if (eventTimer > 120)
                                UpdateSpeech(JstcMiddle);
                        }
                        else if (eventCounter == 2)
                        {
                            if (eventTimer > 75)
                                UpdateSpeech(JstcMiddle);
                        }
                    }
                    else
                    {
                        DrawTalkBubble = false;
                        NPC.frameCounter += 0.1d;
                        NPC.localAI[0]++;
                        eventCounter = 0;
                        eventTimer = 0;
                    }

                    int time = (int)NPC.localAI[0] + middleCutsceneDuration;
                    if (time == 210)
                    {
                        SoundEngine.PlaySound(Scream with { Volume = 1f, Variants = [1] });
                        ScreenshakeSystem.SetScreenshake(150, 10, 2, 1);
                    }
                    if (time >= 210 && (time - 210) % 10 == 0 && (time - 210) < 80)
                    {
                        AddPulse();
                    }
                }
                

                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    if (!TerRoguelikeWorld.escape)
                    {
                        enemyHealthBar = new EnemyHealthBar([NPC.whoAmI], NPC.GivenOrTypeName);
                        if (middleCutsceneStarted)
                            enemyHealthBar.MainBar = enemyHealthBar.ExtraBar = 0.5f;
                    }
                    PauseWhenIngamePaused = false;
                }
            }
            else
            {
                NPC.localAI[0]++;
                BossAI();
            }
        }
        public void BossAI()
        {
            NPC.frameCounter += 0.10d;

            bool hardMode = (int)difficulty >= (int)Difficulty.BloodMoon;

            target = modNPC.GetTarget(NPC);
            
            NPC.ai[1]++;

            if (NPC.ai[0] == None.Id)
            {
                zDepth = 1;
                if (target == null || despawnTime > 0)
                {
                    NPC.immortal = true;
                    NPC.dontTakeDamage = true;
                    despawnTime++;
                    if (despawnTime < 30)
                    {
                        NPC.velocity *= 0.92f;
                        NPC.rotation = NPC.rotation.AngleLerp(0, 0.1f);
                        SetMusicMode(MusicStyle.Silent);
                        fadeRateMultiplier = 1;
                    }
                    else if (despawnTime == 30)
                    {
                        NPC.rotation = 0;
                        NPC.velocity = Vector2.Zero;
                    }
                    else if (despawnTime > 33)
                    {
                        if (NPC.velocity.Y > -40)
                            NPC.velocity.Y -= 5;
                    }
                    if (despawnTime > 120)
                    {
                        foreach (Player player in Main.ActivePlayers)
                        {
                            var modPlayer = player.ModPlayer();
                            if (modPlayer != null)
                                modPlayer.escapeFail = true;
                        }
                        enemyHealthBar.ForceEnd(NPC.life);
                        NPC.active = false;
                    }
                }
                else if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {
                    NPC.localAI[2] = 0;
                    attackInitialized = false;
                    NPC.direction = NPC.spriteDirection = -1;
                    DefaultMovement();

                    if (hardMode)
                    {
                        NPC.ai[1]++;
                    }
                }
            }

            if (NPC.ai[0] == PatternWingsTalons.Id) // big wing + talons
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                }
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : NPC.Center + Vector2.UnitY * 240;

                var projectiles = new List<PatternProjectile>()
                {
                    new YellowWingProj(NPC, 0, targetPos + new Vector2(600 * NPC.direction, -580), Vector2.UnitY * 12, NPC.direction),
                    new YellowWingProj(NPC, 60, targetPos + new Vector2(-600 * NPC.direction, -580), Vector2.UnitY * 12, -NPC.direction),
                    new TalonProj(NPC, 60, targetPos, 60, -NPC.direction),
                    new YellowWingProj(NPC, 120, targetPos + new Vector2(600 * NPC.direction, -580), Vector2.UnitY * 12, NPC.direction),
                    new TalonProj(NPC, 120, targetPos, 60, NPC.direction),
                    new YellowWingProj(NPC, 180, targetPos + new Vector2(-600 * NPC.direction, -580), Vector2.UnitY * 12, -NPC.direction),
                    new YellowWingProj(NPC, 260, targetPos + new Vector2(600 * NPC.direction, -580), Vector2.UnitY * 12, NPC.direction),
                    new YellowWingProj(NPC, 260, targetPos + new Vector2(-600 * NPC.direction, -580), Vector2.UnitY * 12, -NPC.direction),
                };

                int thisTime = (int)NPC.ai[1];
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (thisTime != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }

                if (NPC.ai[1] >= PatternWingsTalons.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternWingsTalons.Id);
                }
            }
            else if (NPC.ai[0] == PatternSplit.Id) // splitting feathers
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                }
                DefaultMovement();

                Vector2 pos1 = (-Vector2.UnitY).RotatedBy(MathHelper.Pi / 3f * NPC.direction) * 300;
                Vector2 pos2 = (-Vector2.UnitY).RotatedBy(MathHelper.Pi / -3f * NPC.direction) * 300;
                Vector2 targetVel = target == null ? Vector2.Zero : target.velocity;
                var projectiles = new List<PatternProjectile>()
                {
                    new SplittingFeatherProj(NPC, 0, NPC.Center, Vector2.UnitY * -300, ReticleType.Spread, target.velocity.SafeNormalize(Vector2.Zero) * -60),
                    new SplittingFeatherProj(NPC, 40, NPC.Center, Vector2.UnitX * -300 * NPC.direction, ReticleType.Circle, Vector2.UnitX * -400 * NPC.direction),
                    new SplittingFeatherProj(NPC, 60, NPC.Center, Vector2.UnitX * 300 * NPC.direction, ReticleType.Circle, Vector2.UnitX * 400 * NPC.direction),
                    new SplittingFeatherProj(NPC, 120, NPC.Center, pos1, ReticleType.Circle, pos1 * 1.4f),
                    new SplittingFeatherProj(NPC, 135, NPC.Center, pos2, ReticleType.Circle, pos2 * 1.4f),
                    new SplittingFeatherProj(NPC, 150, NPC.Center, Vector2.UnitY * 300, ReticleType.Spread, Vector2.UnitY * 400),
                };

                int thisTime = (int)NPC.ai[1];
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (thisTime != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }
                if (NPC.ai[1] >= PatternSplit.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternSplit.Id);
                }
            }
            else if (NPC.ai[0] == PatternDashingTalon.Id) // dashing feather + talon
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                }
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : NPC.Center + Vector2.UnitY * 240;

                var projectiles = new List<PatternProjectile>()
                {
                    new TalonProj(NPC, 60, targetPos, 60, -NPC.direction),
                    new TalonProj(NPC, 100, targetPos, 60, NPC.direction),
                };

                for (int i = 0; i < 12; i++)
                {
                    float yoff = 32 * i;
                    projectiles.Add(new DashingFeatherProj(NPC, 0, NPC.Center, new Vector2(-600 * NPC.direction, yoff - 10), new Vector2(0, yoff - 10)));
                    projectiles.Add(new DashingFeatherProj(NPC, 30, NPC.Center, new Vector2(600 * NPC.direction, -yoff + 10), new Vector2(0, -yoff + 10)));
                }

                for (int i = 0; i < 9; i++)
                {
                    projectiles.Add(new DashingFeatherProj(NPC, 180 + i * 4, NPC.Center, (Vector2.UnitY * -600).RotatedBy(MathHelper.Pi / 8f * i * NPC.direction)));
                }


                int thisTime = (int)NPC.ai[1];
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (thisTime != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }
                if (NPC.ai[1] >= PatternDashingTalon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternDashingTalon.Id);
                }
            }
            else if (NPC.ai[0] == PatternDoubleWing.Id) // double big wings
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                }
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : NPC.Center + Vector2.UnitY * 240;

                var projectiles = new List<PatternProjectile>()
                {
                    new YellowWingProj(NPC, 0, targetPos + new Vector2(0, -580), Vector2.UnitY * 12, 1),
                    new YellowWingProj(NPC, 0, targetPos + new Vector2(0, -580), Vector2.UnitY * 12, -1),
                    new YellowWingProj(NPC, 60, targetPos + new Vector2(600 * NPC.direction, -580), Vector2.UnitY * 12, 1, 8),
                    new YellowWingProj(NPC, 60, targetPos + new Vector2(600 * NPC.direction, -580), Vector2.UnitY * 12, -1, 8),
                    new YellowWingProj(NPC, 120, targetPos + new Vector2(600 * -NPC.direction, -580), Vector2.UnitY * 12, 1, 8),
                    new YellowWingProj(NPC, 120, targetPos + new Vector2(600 * -NPC.direction, -580), Vector2.UnitY * 12, -1, 8),
                };

                int thisTime = (int)NPC.ai[1];
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (thisTime != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }
                if (NPC.ai[1] >= PatternDoubleWing.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternDoubleWing.Id);
                }
            }
            else if (NPC.ai[0] == PatternFeatherWalls.Id) // feather walls
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                    NPC.localAI[3] = Main.rand.NextBool() ? 0 : 2;
                    NPC.localAI[2] = 20;
                }
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : NPC.Center + Vector2.UnitY * 240;

                var projectiles = new List<PatternProjectile>();
                int thisTime = (int)NPC.ai[1];
                int rate = 36;
                if (thisTime % rate == 0 && thisTime < 210)
                {
                    Vector2 forceUp = Vector2.Zero;
                    float length = 600;
                    if ((int)NPC.localAI[3] % 2 == 0) // if the player is on the floor (they don't have space to dodge downwards) try skewing the horizontal projectiles more upwards
                    {
                        forceUp = (TileCollidePositionInLine(targetPos, targetPos + new Vector2(0, 250)).Distance(targetPos) - 250) * Vector2.UnitY;
                    }
                    else
                    {
                        length *= 0.75f;
                    }

                    float rot = (int)NPC.localAI[3] * MathHelper.PiOver2;

                    int count = 40;
                    int halfCount = count / 2;
                    int randGap = Main.rand.Next(halfCount - 2, halfCount + 3);
                    if (randGap < halfCount)
                        randGap -= 2;
                    else if (randGap > halfCount)
                        randGap += 2;
                    else
                        randGap += 2 * (Main.rand.NextBool() ? -1 : 1);
                    NPC.localAI[2] = randGap;
                    float step = 32;
                    float halfHeight = count * step * 0.5f;
                    
                    for (int j = 0; j < count; j++)
                    {
                        int gap = j - randGap;
                        if (gap >= -3 && gap <= 3)
                            continue;
                        float yOff = -halfHeight + step * j;
                        projectiles.Add(new DashingFeatherProj(NPC, thisTime, NPC.Center, new Vector2(length, yOff).RotatedBy(rot) + forceUp, new Vector2(0, yOff).RotatedBy(rot) + forceUp));
                    }
                    int newDir = Main.rand.Next(3);
                    NPC.localAI[3] = newDir >= (int)NPC.localAI[3] ? newDir + 1 : newDir;
                }

                
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (thisTime != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }
                if (NPC.ai[1] >= PatternFeatherWalls.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternFeatherWalls.Id);
                }
            }
            else if (NPC.ai[0] == PatternTalonSplit.Id) //talons + splittingFeather
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                }
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : NPC.Center + Vector2.UnitY * 240;

                var projectiles = new List<PatternProjectile>()
                {
                    new SplittingFeatherProj(NPC, 0, NPC.Center, new Vector2(-300 * NPC.direction, 0), ReticleType.Spread),
                    new SplittingFeatherProj(NPC, 30, NPC.Center, new Vector2(-300 * NPC.direction, 0), ReticleType.Circle),
                    new TalonProj(NPC, 0, targetPos, 100, NPC.direction),
                    new SplittingFeatherProj(NPC, 150, NPC.Center, new Vector2(300 * NPC.direction, 0), ReticleType.Spread),
                    new SplittingFeatherProj(NPC, 180, NPC.Center, new Vector2(300 * NPC.direction, 0), ReticleType.Circle),
                    new TalonProj(NPC, 150, targetPos, 100, -NPC.direction),
                    new SplittingFeatherProj(NPC, 250, NPC.Center, new Vector2(0, -300), ReticleType.Circle),
                    new SplittingFeatherProj(NPC, 250, NPC.Center, new Vector2(0, 300), ReticleType.Circle),
                    new SplittingFeatherProj(NPC, 270, NPC.Center, new Vector2(300 * NPC.direction, 0), ReticleType.Spread, new Vector2(800 * NPC.direction, 0)),
                    new SplittingFeatherProj(NPC, 270, NPC.Center, new Vector2(-300 * NPC.direction, 0), ReticleType.Spread, new Vector2(-300 * NPC.direction, 0)),
                };

                int thisTime = (int)NPC.ai[1];
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (thisTime != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }
                if (NPC.ai[1] >= PatternTalonSplit.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternTalonSplit.Id);
                }
            }
            else if (NPC.ai[0] == PatternFeatherCircle.Id)
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                }
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : NPC.Center + Vector2.UnitY * 240;

                var projectiles = new List<PatternProjectile>();

                Vector2 baseVect = Vector2.UnitX * 600;
                float rotPer = MathHelper.TwoPi / 100f;
                int time = (int)NPC.ai[1];
                float startRot = Main.rand.NextFloat(MathHelper.TwoPi);
                if (time == 160)
                {
                    for (int i = 0; i < 10; i++) // anti-undodgeable setup
                    {
                        Vector2 rotVect = startRot.ToRotationVector2();
                        if (targetPos.Distance(TileCollidePositionInLine(targetPos, targetPos + rotVect * 260)) > 250)
                            break;
                        startRot = Main.rand.NextFloat(MathHelper.TwoPi);
                    }
                }
                float fivepercentRot = MathHelper.TwoPi * 0.05f;
                bool spawnSmall = time == 0 || time == 36 || time == 72 || time == 108 || time == 144;
                for (int i = 0; i < 40; i++)
                {
                    if (i < 25 && spawnSmall)
                    {
                        projectiles.Add(new DashingFeatherProj(NPC, time, NPC.Center, baseVect.RotatedBy(startRot + MathHelper.PiOver4 + i * rotPer)));
                    }

                    if (time == 190 && (i < 40 || i > 50))
                        projectiles.Add(new DashingFeatherProj(NPC, time, NPC.Center, baseVect.RotatedBy(startRot + fivepercentRot + i * rotPer)));
                }

                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (time != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }
                if (NPC.ai[1] >= PatternFeatherCircle.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternFeatherCircle.Id);
                }
            }
            else if (NPC.ai[0] == PatternRandomness.Id) // random bullshit go !
            {
                if (!attackInitialized)
                {
                    attackInitialized = true;
                    NPC.direction = Main.rand.NextBool() ? -1 : 1;
                }
                DefaultMovement();
                Vector2 targetPos = target != null ? target.Center : NPC.Center + Vector2.UnitY * 240;

                var projectiles = new List<PatternProjectile>();

                int thisTime = (int)NPC.ai[1];
                if (thisTime % 30 == 0)
                {
                    switch (Main.rand.Next(8))
                    {
                        default:
                        case 0:
                        case 1:
                        case 2:
                            projectiles.Add(new SplittingFeatherProj(NPC, thisTime, NPC.Center, Main.rand.NextVector2CircularEdge(300, 300), (ReticleType)Main.rand.Next(2)));
                            break;
                        case 3:
                        case 4:
                        case 5:
                            Vector2 circleEdge = Main.rand.NextVector2CircularEdge(600, 600);
                            int count = Main.rand.Next(4, 7);
                            for (int i = 0; i < count; i++)
                            {
                                circleEdge = circleEdge.RotatedBy(Main.rand.NextFloat(0.1f, 0.3f));
                                projectiles.Add(new DashingFeatherProj(NPC, thisTime, NPC.Center, circleEdge, Vector2.Zero));
                            }
                            break;
                        case 6:
                            projectiles.Add(new TalonProj(NPC, thisTime, targetPos, 60, Main.rand.NextBool() ? -1 : 1));
                            break;
                        case 7:
                            int dir = Main.rand.NextBool() ? -1 : 1;
                            projectiles.Add(new YellowWingProj(NPC, thisTime, targetPos + new Vector2(400 * dir, -480), Vector2.UnitY * 12, dir));
                            break;
                    }
                }

                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (thisTime != projectiles[i].time)
                        continue;

                    projectiles[i].SpawnProj(NPC);
                }
                if (NPC.ai[1] >= PatternRandomness.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(PatternRandomness.Id);
                }
            }
            else if (NPC.ai[0] == Trash.Id)
            {
                if (NPC.ai[1] < 20)
                {
                    if (NPC.ai[1] < 10)
                    {
                        if (!attackInitialized)
                        {
                            SoundEngine.PlaySound(WingsOut with { Volume = 0.7f, Pitch = -0.5f }, NPC.Center);
                            attackInitialized = true;
                        }
                    }
                    else
                        attackInitialized = false;
                    
                    NPC.frameCounter = 0;
                    NPC.velocity *= 0.96f;
                    NPC.rotation = NPC.rotation.AngleLerp(0, 0.1f);
                }
                else
                {
                    if (!attackInitialized)
                    {
                        WindSlot = SoundEngine.PlaySound(TrashWind with { Volume = 0.8f });
                        attackInitialized = true;
                    }
                    if (flapFrameCounter == 2 && flapSoundCooldown == 0)
                    {
                        flapSoundCooldown = 30;
                        SoundEngine.PlaySound(Flap with { Volume = 0.3f, Pitch = -1f }, NPC.Center);
                        ExtraSoundSystem.ExtraSounds.Add(new(SoundEngine.PlaySound(SoundID.Item32 with { Volume = 1f, Pitch = -0.5f }, NPC.Center), 6));
                    }
                    NPC.velocity *= 0.92f;
                    NPC.rotation = NPC.rotation.AngleLerp(0, 0.1f);
                    int thisTime = (int)NPC.ai[1] - 20;
                    if (thisTime > 40 && thisTime % 2 == 0)
                    {
                        Color particleColor = Color.White * 0.7f;
                        ParticleManager.AddParticle(new LerpLine(
                            Main.screenPosition + Vector2.UnitX * (Main.rand.NextFloat(-100, Main.screenWidth + 100) + Main.LocalPlayer.velocity.X * 10) + Vector2.UnitY * Main.rand.NextFloat(-60, -300),
                            Vector2.UnitY * 20, 50, particleColor, new Vector2(1.5f, 0.75f), -MathHelper.PiOver2, 1.02f, 40, true),
                            ParticleManager.ParticleLayer.Default);
                    }
                    if (thisTime % 6 == 0)
                    {
                        Vector2 targetPos = target != null ? target.Center : spawnPos;
                        targetPos.Y -= 600;
                        float width = 4000;
                        var parentRoom = modNPC.GetParentRoom();
                        if (parentRoom != null)
                        {
                            width = parentRoom.RoomDimensions16.X + (parentRoom.WallInflateModifier.X * 2);
                            targetPos.X = (int)(parentRoom.RoomPosition16.X - parentRoom.WallInflateModifier.X);
                            float newTarget = parentRoom.RoomPosition16.Y + 400;
                            if (targetPos.Y > newTarget)
                                targetPos.Y = newTarget;
                        }
                        else
                        {
                            targetPos.X -= width * 0.5f;
                        }

                        int projCount = 5;
                        float segmentedWidth = width / projCount;
                        if (!TerRoguelike.mpClient)
                        {
                            for (float i = 0; i < width; i += segmentedWidth)
                            {
                                Vector2 thisPos = targetPos;
                                thisPos.X += Main.rand.NextFloat(i, i + segmentedWidth);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), thisPos, Vector2.Zero, ModContent.ProjectileType<Junk>(), NPC.damage, 0, -1, Main.rand.NextFloat(0.10f, 0.14f));
                            }

                            if (phase2 && attackSpice < 7 && Main.rand.NextBool(6 + attackSpice))
                            {
                                attackSpice++;
                                Vector2 predictPos = target == null ? spawnPos : target.Center;
                                Vector2 targetVel = target == null ? Vector2.Zero : target.velocity;
                                int dir = Main.rand.NextBool() ? -1 : 1;
                                if (Math.Sign(targetVel.X) == dir)
                                    predictPos.X += targetVel.X * 120;
                                Vector2 projPos = MeteorSpawnPosition(predictPos, dir);

                                Vector2 meteorVelocity = 16 * new Vector2(dir, 2).ToRotation().AngleTowards((predictPos - projPos).ToRotation(), Main.rand.NextFloat(0.47f)).AngleTowards(-MathHelper.PiOver2, 0.3f).ToRotationVector2();
                                NPC.NewNPC(NPC.GetSource_FromThis(), (int)projPos.X, (int)projPos.Y, ModContent.NPCType<Meteor>(), 0, 0, dir, meteorVelocity.X, meteorVelocity.Y);
                            }
                        }
                    }
                    foreach (Player player in Main.ActivePlayers)
                    {
                        if (player.dead)
                            continue;
                        var modPlayer = player.ModPlayer();
                        if (modPlayer == null)
                            continue;
                        modPlayer.majorGravity = true;
                    }
                }
                if (NPC.ai[1] >= Trash.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(Trash.Id);
                }
            }
            else if (NPC.ai[0] == Meteors.Id)
            {
                if (NPC.ai[1] > Meteors.Duration - 60)
                {
                    DefaultMovement();
                }
                else
                {
                    if (NPC.ai[1] > 135)
                    {
                        DefaultMovement();
                    }
                    else
                    {
                        NPC.velocity *= 0.92f;
                        NPC.rotation = NPC.rotation.AngleLerp(0, 0.13f);
                    }
                    
                    if (!attackInitialized && NPC.ai[1] < 5)
                    {
                        NPC.direction = NPC.spriteDirection = NPC.Center.X < spawnPos.X ? 1 : -1;
                        attackInitialized = true;
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[1] > 5 && NPC.ai[1] < 10)
                        attackInitialized = false;

                    int time = (int)NPC.ai[1];
                    int screamTime = time - 20;
                    if (screamTime >= 0)
                    {
                        if (!attackInitialized)
                        {
                            attackInitialized = true;
                            SoundEngine.PlaySound(Scream with { Volume = 0.8f, Variants = [2], Pitch = 0f, MaxInstances = 10 }, NPC.Center);
                            if (phase2)
                                AddPulse();
                        }
                        else if (screamTime < 80 && screamTime % 10 == 0)
                        {
                            if (phase2)
                                AddPulse();
                            float volume = 0.4f;
                            int screamCount = screamTime / 10;
                            volume -= screamCount * 0.04f;
                            SoundEngine.PlaySound(Scream with { Volume = volume, Variants = [2], Pitch = -0.2f, MaxInstances = 10 }, NPC.Center);
                        }
                    }

                    int meteorRate = 20;
                    if (time % meteorRate == 0 && !TerRoguelike.mpClient)
                    {
                        int count = 1;
                        if (phase2 && attackSpice < 3 && Main.rand.NextBool(4 + attackSpice))
                        {
                            count = 2;
                            attackSpice++;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 targetPos = target == null ? spawnPos : target.Center;
                            Vector2 targetVel = target == null ? Vector2.Zero : target.velocity;
                            if (Math.Sign(targetVel.X) == NPC.direction)
                                targetPos.X += targetVel.X * 120;
                            Vector2 projPos = MeteorSpawnPosition(targetPos, NPC.direction);

                            bool star = i == 1;

                            if (star)
                            {
                                Vector2 rawTargetPos = target == null ? spawnPos : target.Center;
                                NPC.NewNPC(NPC.GetSource_FromThis(), (int)projPos.X, (int)projPos.Y, ModContent.NPCType<SplittingStar>(), 0, 0, (rawTargetPos - projPos).ToRotation(), 10);
                            }
                            else
                            {
                                Vector2 meteorVelocity = 16 * new Vector2(NPC.direction, 2).ToRotation().AngleTowards((targetPos - projPos).ToRotation(), Main.rand.NextFloat(0.47f)).AngleTowards(-MathHelper.PiOver2, 0.3f).ToRotationVector2();
                                NPC.NewNPC(NPC.GetSource_FromThis(), (int)projPos.X, (int)projPos.Y, ModContent.NPCType<Meteor>(), 0, 0, NPC.direction, meteorVelocity.X, meteorVelocity.Y);
                            }
                            
                        }
                    }
                }
                if (NPC.ai[1] >= Meteors.Duration)
                {
                    NPC.direction = NPC.spriteDirection = -1;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(Meteors.Id);
                }
            }
            else if (NPC.ai[0] == Stars.Id)
            {
                if (NPC.ai[1] > Meteors.Duration - 60)
                {
                    DefaultMovement();
                }
                else
                {
                    if (NPC.ai[1] > 135)
                    {
                        DefaultMovement();
                    }
                    else
                    {
                        NPC.velocity *= 0.92f;
                        NPC.rotation = NPC.rotation.AngleLerp(0, 0.13f);
                    }

                    int time = (int)NPC.ai[1];
                    int screamTime = time - 20;
                    if (screamTime >= 0)
                    {
                        if (!attackInitialized)
                        {
                            attackInitialized = true;
                            SoundEngine.PlaySound(Scream with { Volume = 0.8f, Variants = [2], Pitch = 0f, MaxInstances = 10 }, NPC.Center);
                            if (phase2)
                                AddPulse();
                        }
                        else if (screamTime < 80 && screamTime % 10 == 0)
                        {
                            if (phase2)
                            {
                                AddPulse();
                                if (!TerRoguelike.mpClient && attackSpice < 3 && Main.rand.NextBool())
                                {
                                    attackSpice++;
                                    Vector2 targetPos = target == null ? spawnPos : target.Center;
                                    Vector2 targetVel = target == null ? Vector2.Zero : target.velocity;
                                    int dir = Main.rand.NextBool() ? -1 : 1;
                                    if (Math.Sign(targetVel.X) == dir)
                                        targetPos.X += targetVel.X * 120;

                                    Vector2 projPos = MeteorSpawnPosition(targetPos, dir);

                                    Vector2 meteorVelocity = 16 * new Vector2(dir, 2).ToRotation().AngleTowards((targetPos - projPos).ToRotation(), Main.rand.NextFloat(0.47f)).AngleTowards(-MathHelper.PiOver2, 0.3f).ToRotationVector2();
                                    NPC.NewNPC(NPC.GetSource_FromThis(), (int)projPos.X, (int)projPos.Y, ModContent.NPCType<Meteor>(), 0, 0, dir, meteorVelocity.X, meteorVelocity.Y);
                                }
                            }
                                
                            float volume = 0.4f;
                            int screamCount = screamTime / 10;
                            volume -= screamCount * 0.04f;
                            SoundEngine.PlaySound(Scream with { Volume = volume, Variants = [2], Pitch = -0.2f, MaxInstances = 10 }, NPC.Center);
                        }
                    }

                    if (screamTime == 0 && !TerRoguelike.mpClient)
                    {
                        Vector2 targetPos = target == null ? spawnPos : target.Center;
                        var room = modNPC.GetParentRoom();
                        Vector2 basePos = NPC.Center + new Vector2(0, -833);
                        Vector2 anchor = NPC.Center;
                        if (room != null)
                        {
                            basePos = room.RoomPosition16 + new Vector2(room.RoomDimensions.X * 8, -600);
                            anchor = room.RoomPosition16 + room.RoomDimensions16 * new Vector2(0.5f, 1f);
                        }

                        int starCount = 4;
                        float baseRot = -MathHelper.PiOver4;
                        float totalRot = baseRot * -2;
                        float incrementRot = totalRot / starCount;
                        float padding = 0.1f;

                        for (int i = 0; i < starCount; i++)
                        {
                            float myRot = baseRot + (incrementRot * i) + Main.rand.NextFloat(padding, incrementRot - padding);
                            Vector2 npcPos = anchor + (basePos - anchor).RotatedBy(myRot);
                            NPC.NewNPC(NPC.GetSource_FromThis(), (int)npcPos.X, (int)npcPos.Y, ModContent.NPCType<SplittingStar>(), 0, 0, (targetPos - npcPos).ToRotation(), 10);
                        }
                    }
                }
                if (NPC.ai[1] >= Stars.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(Stars.Id);
                }
            }
            else if (NPC.ai[0] == Dash.Id)
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos;
                if (NPC.ai[1] < 30)
                {
                    if (!attackInitialized && NPC.ai[1] < 5)
                    {
                        NPC.frameCounter = 0;
                        SoundEngine.PlaySound(WingsOut with { Volume = 0.7f, Pitch = -0.5f }, NPC.Center);
                        attackInitialized = true;
                    }
                    else if (NPC.ai[1] >= 5)
                        attackInitialized = false;
                    NPC.velocity *= 0.92f;
                    NPC.rotation = NPC.rotation.AngleLerp(0, 0.1f);
                }
                else if (NPC.ai[1] == 30)
                {
                    NPC.rotation = 0;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
                else if (NPC.ai[1] > 33 && NPC.ai[1] < 60)
                {
                    if (NPC.velocity.Y > -40)
                        NPC.velocity.Y -= 5;
                    if (NPC.ai[1] < 40 && !attackInitialized)
                    {
                        SoundEngine.PlaySound(WingsOut with { Volume = 0.4f, Pitch = -1f }, NPC.Center);
                        ExtraSoundSystem.ExtraSounds.Add(new(SoundEngine.PlaySound(SoundID.Item32 with { Volume = 1f, Pitch = -0.5f }, NPC.Center), 6));
                        attackInitialized = true;
                    }
                    else if (NPC.ai[1] >= 40)
                        attackInitialized = false;
                }
                else if (NPC.ai[1] == 60)
                {
                    int dir = -1;
                    if (NPC.Center.X < spawnPos.X)
                        dir = 1;
                    NPC.direction = NPC.spriteDirection = dir;
                    starZDepth = 0.1f;
                    starPosition = targetPos;
                    starPosition += new Vector2(1100 * dir, -100) / starZDepth;
                    starVelocity = new Vector2(-9 * dir, -2f) / starZDepth;
                    ableToHit = false;
                    canBeHit = false;
                    NPC.immortal = NPC.dontTakeDamage = true;
                    NPC.netUpdate = true;
                }
                else if (NPC.ai[1] > 60 && NPC.ai[1] < Dash.Duration - 90)
                {
                    if (!attackInitialized && NPC.ai[1] < 70)
                    {
                        DashSlot = SoundEngine.PlaySound(BackgroundDash with { Volume = 0.5f, Variants = [1] }, (starPosition - Main.Camera.Center) * starZDepth + Main.Camera.Center);
                        attackInitialized = true;
                    }
                    else
                        attackInitialized = false;

                    starPosition += starVelocity;
                    ableToHit = false;
                    canBeHit = false;
                    NPC.immortal = NPC.dontTakeDamage = true;
                    if (NPC.ai[1] > 120)
                    {
                        zDepth = starZDepth;
                        NPC.Center = starPosition;
                        NPC.velocity = Vector2.Zero;
                        starVelocity *= 0.94f;
                    }
                    else if (NPC.velocity.Y > -40)
                    {
                        NPC.velocity.Y -= 5;
                    }

                    if ((int)NPC.ai[1] % 3 == 0)
                        bgParticles.Add(new(new Square(starPosition + Main.rand.NextVector2CircularEdge(4, 4) / starZDepth, Main.rand.NextVector2Circular(1, 1) / starZDepth, 60, Color.White, new Vector2(0.7f), 0, 0.98f, 60, true), starZDepth));

                    int cloneCount = 4;
                    int cloneRate = 18;
                    int thisTime = (int)NPC.ai[1] - ((Dash.Duration - 90) - (cloneRate * cloneCount));
                    if (thisTime >= 0 && thisTime % cloneRate == 0)
                    {   
                        if (!TerRoguelike.mpClient)
                        {
                            Vector2 projectedPos = targetPos;
                            if (target != null)
                                projectedPos = TileCollidePositionInLine(projectedPos, projectedPos + target.velocity * 24);
                            var room = modNPC.GetParentRoom();
                            if (room != null)
                                projectedPos = room.GetRect().ClosestPointInRect(projectedPos);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), starPosition, projectedPos, ModContent.ProjectileType<LightClone>(), (int)(NPC.damage * 1.5f), 0, -1, starZDepth, 1);
                        }
                            
                        if (SoundEngine.TryGetActiveSound(DashSlot, out var sound))
                            sound.Stop();
                    }
                }
                else if (NPC.ai[1] >= Dash.Duration - 90)
                {
                    if (!attackInitialized)
                    {
                        DashSlot = SoundEngine.PlaySound(BackgroundDash with { Volume = 0.5f, Variants = [2], MaxInstances = 3, Pitch = -0.25f }, (NPC.Center - Main.Camera.Center) * zDepth + Main.Camera.Center);
                        attackInitialized = true;
                    }
                    starVelocity = Vector2.Zero;
                    ableToHit = false;
                    canBeHit = false;
                    NPC.immortal = NPC.dontTakeDamage = true;
                    NPC.velocity = Vector2.Zero;
                    int thisTime = (int)NPC.ai[1] - (Dash.Duration - 90);
                    if (thisTime == 0)
                    {
                        dashStart = NPC.Center;
                        NPC.netUpdate = true;
                    }
                    if (thisTime < 90)
                    {
                        float oldZ = zDepth;
                        zDepth *= 1.1f;

                        NPC.Center += (targetPos - NPC.Center) * 10f / 56;

                        if (zDepth >= 1 && oldZ < 1)
                        {
                            ableToHit = true;
                            canBeHit = true;
                            NPC.immortal = NPC.dontTakeDamage = false;
                        }
                        else if (zDepth > 1)
                            NPC.Center += Vector2.UnitY * -1.5f * (zDepth - 1);
                    }
                }

                if (NPC.ai[1] >= Dash.Duration)
                {
                    NPC.immortal = NPC.dontTakeDamage = false;
                    starPosition = Vector2.Zero;
                    starZDepth = 1;
                    NPC.Center = targetPos + Vector2.UnitY * -1000;
                    NPC.velocity = Vector2.Zero;
                    zDepth = 1;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    oldAttacks.Add(Dash.Id);
                    NPC.netUpdate = true;
                }
            }

            if (oldAttacks.Count >= 3)
                oldAttacks.RemoveAt(0);

            void DefaultMovement()
            {
                if (target != null)
                {
                    Vector2 baseTargetPos = target.Center + new Vector2(0, -320);
                    if (oldAttacks[^1] == Dash.Id && NPC.ai[0] == None.Id)
                        baseTargetPos.Y += Math.Min(-180 + NPC.ai[1] * 3, 0);
                    Vector2 targetPos = baseTargetPos;
                    targetPos.X += (float)Math.Sin(animationCounter * 0.01f * MathHelper.TwoPi) * 64;
                    targetPos.Y += (float)Math.Sin(animationCounter * 0.04f * MathHelper.TwoPi) * 16;
                    float distance = NPC.Center.Distance(targetPos);
                    float rotCap = 0.2f + Math.Abs(NPC.Center.X - targetPos.X) * 0.0008f;
                    if (rotCap > 1f)
                        rotCap = 1f;
                    float rotMagnitude;

                    float otherDist = baseTargetPos.Distance(NPC.Center);
                    if (otherDist > 320)
                    {
                        NPC.velocity *= 0.99f;
                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(0.2f, 1f, MathHelper.Clamp((otherDist - 320) / 80f, 0, 1));
                        rotMagnitude = Math.Abs(NPC.velocity.X) * 0.2f;
                        NPC.velocity = NPC.velocity.ToRotation().AngleTowards((targetPos - NPC.Center).ToRotation(), 0.03f).ToRotationVector2() * NPC.velocity.Length();
                    }
                    else
                    {
                        NPC.velocity *= 0.99f;
                        float velLength = NPC.velocity.Length();
                        if (velLength > 2)
                        {
                            NPC.velocity *= 0.98f;
                        }

                        NPC.velocity += (targetPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 0.2f;
                        if (distance < 80 && target.velocity.Length() < 2f && target.oldVelocity.Length() < 2f)
                        {
                            if (velLength > distance)
                                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * distance;
                            NPC.velocity *= 0.9f;
                        }

                        rotMagnitude = Math.Abs(NPC.velocity.X) * 0.2f;
                    }
                    if (NPC.velocity.Length() > 30)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 30;
                    }

                    if (rotMagnitude > rotCap)
                        rotMagnitude = rotCap;
                    NPC.rotation = NPC.rotation.AngleLerp(Math.Sign(NPC.velocity.X) * rotMagnitude, 0.05f);
                }
            }

            Vector2 MeteorSpawnPosition(Vector2 targetPos, int direction)
            {
                Vector2 basePos = NPC.Center + new Vector2(-direction * 800, 800);
                var parentRoom = modNPC.GetParentRoom();
                if (parentRoom != null)
                {
                    Vector2 wallInflate = parentRoom.WallInflateModifier.ToVector2() * 16;
                    basePos = parentRoom.RoomPosition16 - wallInflate;
                    if (direction == -1)
                    {
                        basePos += Vector2.UnitX * (parentRoom.RoomDimensions16.X + (wallInflate.X * 2));
                        float potentialX = targetPos.X + 1100;
                        if (potentialX < basePos.X)
                            basePos.X = potentialX;
                    }
                    else
                    {
                        float potentialX = targetPos.X - 1100;
                        if (potentialX > basePos.X)
                            basePos.X = potentialX;
                    }
                }

                return basePos + new Vector2(Main.rand.NextFloat(-400, 400), 0).RotatedBy(MathHelper.PiOver4 * -direction);
            }
        }
        public void ChooseAttack()
        {
            if (TerRoguelike.mpClient)
                return;
            NPC.netUpdate = true;
            attackSpice = 0;

            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { PatternWingsTalons, PatternSplit, PatternDashingTalon, PatternDoubleWing, PatternFeatherWalls, PatternTalonSplit, PatternFeatherCircle, Trash, Meteors, Stars };

            if (phase2)
            {
                potentialAttacks.Add(PatternRandomness);
                potentialAttacks.Add(Dash);
            }
            if (justStartedPhase2)
            {
                justStartedPhase2 = false;
                potentialAttacks.RemoveAll(x => x.Id <= 7); // use one of the new/buffed attacks first
            }

            for (int i = 0; i < oldAttacks.Count; i++)
            {
                potentialAttacks.RemoveAll(x => x.Id == oldAttacks[i]);
            }

            int totalWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
            {
                totalWeight += potentialAttacks[i].Weight;
            }
            int chosenRandom = Main.rand.Next(totalWeight);

            for (int i = potentialAttacks.Count - 1; i >= 0; i--)
            {
                Attack attack = potentialAttacks[i];
                chosenRandom -= attack.Weight;
                if (chosenRandom < 0)
                {
                    chosenAttack = attack.Id;
                    break;
                }
            }

            NPC.ai[0] = chosenAttack;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if ((projectile.hostile && !NPC.friendly) || (projectile.friendly && NPC.friendly) || NPC.immortal || NPC.dontTakeDamage)
                return false;

            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = projectile.Colliding(projectile.getRect(), hitboxes[i].GetHitbox(NPC.Center, NPC.rotation, NPC.scale));
                if (pass)
                {
                    projectile.ModProj().ultimateCollideOverride = true;
                    return canBeHit ? null : false;
                }
            }

            return false;
        }
        public override bool CheckDead()
        {
            if (deadTime >= 60 && deadTime < 70)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Main.projectile[i].active = false;
                }
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (i == NPC.whoAmI)
                        continue;

                    Main.npc[i].active = false;
                }
            }
            if (despawnTime > 0)
            {
                NPC.dontTakeDamage = true;
                NPC.immortal = true;
                return false;
            }
            if (!middleCutsceneStarted)
            {
                NPC.life = (int)(NPC.lifeMax * 0.5f);
                middleCutsceneStarted = true;
                NPC.active = true;
                modNPC.ignitedStacks.Clear();
                modNPC.bleedingStacks.Clear();
                modNPC.ballAndChainSlow = 0;
                Room.ClearSpecificProjectiles();
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                NPC.scale = 1;
                NPC.ai[0] = None.Id;
                NPC.ai[1] = 0;

                if (SkipCutscene)
                {
                    SetBossTrack(Jstc2Theme);
                    CombatVolumeInterpolant = 1;
                    ableToHit = true;
                    canBeHit = true;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    phase2 = true;
                    justStartedPhase2 = true;
                    return false;
                }

                ableToHit = false;
                canBeHit = false;
                NPC.immortal = true;
                NPC.dontTakeDamage = true;
                enemyHealthBar.ForceEnd(NPC.life);
                SetMusicMode(MusicStyle.Silent);

                eventCounter = 0;
                NPC.localAI[1] = 0;
                NPC.localAI[0] = -middleCutsceneDuration;
                SoundEngine.PlaySound(Knockdown with { Volume = 0.7f });
                NPC.netUpdate = true;
                return false;
            }
            bool kill = NPC.localAI[1] == 200;
            if (kill)
            {
                return true;
            }
            if (SkipCutscene)
            {
                return true;
            }

            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;
            modNPC.OverrideIgniteVisual = true;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            ableToHit = false;
            canBeHit = false;

            if (deadTime == 0)
            {
                NPC.frameCounter = 0;
                SetMusicMode(MusicStyle.Silent);
                fadeRateMultiplier = 1;
                NPC.localAI[1] = 1;
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                NPC.scale = 1;
                modNPC.CleanseDebuffs();
                Room.ClearSpecificProjectiles();
                SoundEngine.PlaySound(Knockdown with { Volume = 0.7f });
                NPC.netUpdate = true;

                if (modNPC.isRoomNPC)
                {
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    ClearChildren();
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration + 60, 30, 30, 2.25f, CutsceneSystem.CutsceneSource.Boss);
            }
            if (deadTime == 60)
            {
                SoundEngine.PlaySound(BitHurt with { Volume = 0.7f, Pitch = -1, MaxInstances = 3 });
                SoundEngine.PlaySound(BitHurt with { Volume = 0.7f, Pitch = -0.5f, MaxInstances = 3 });
                SoundEngine.PlaySound(BitHurt with { Volume = 0.7f, Pitch = 0.2f, MaxInstances = 3 });
                SoundEngine.PlaySound(Knockdown with { Volume = 0 });
            }
            void ClearChildren()
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (i == NPC.whoAmI)
                        continue;

                    NPC childNPC = Main.npc[i];
                    if (childNPC == null)
                        continue;
                    if (!childNPC.active)
                        continue;

                    TerRoguelikeGlobalNPC modChildNPC = childNPC.ModNPC();
                    if (modChildNPC == null)
                        continue;
                    if (modChildNPC.isRoomNPC && modChildNPC.sourceRoomListID == modNPC.sourceRoomListID)
                    {
                        childNPC.StrikeInstantKill();
                        childNPC.active = false;
                    }
                }
            }
            deadTime++;

            bool end = NPC.localAI[1] < 100;
            if (end)
            {
                if (CutsceneSystem.cutsceneDuration - CutsceneSystem.cutsceneTimer > 60)
                    CutsceneSystem.cutsceneTimer++;

                if (eventCounter == 0)
                {
                    if (deadTime >= 270)
                    {
                        UpdateSpeech(JstcEnd);
                        NPC.frameCounter = 1;
                    }   
                    else if (deadTime > 90 && deadTime < 180)
                    {
                        CutsceneSystem.cameraTargetCenter += Vector2.UnitY * (1 - (deadTime / 180f)) * 4;
                        if (deadTime == 108)
                        {
                            SoundEngine.PlaySound(ArmorBreak with { Volume = 0.88f });

                            for (int j = -1; j < 2; j++)
                            {
                                for (float i = -(4 - j); i <= (4 - j); i++)
                                {
                                    Vector2 particleVect = new Vector2(i * 8, j * 12) + Main.rand.NextVector2Circular(3, 2);
                                    if (j < 0)
                                        particleVect.X *= 1.2f;
                                    Vector2 particleVel = (Main.rand.NextVector2Circular(1.6f, 1.6f) + particleVect.SafeNormalize(Vector2.UnitX) * 0.5f) * 0.75f;
                                    particleVel.Y -= 1.6f;
                                    ParticleManager.AddParticle(new GlassShard(
                                        NPC.Center + new Vector2(0, 64) + particleVect,
                                        particleVel,
                                        (Main.rand.NextBool(3) ? 120 : 90) + Main.rand.Next(10), Color.Lerp(new Color(65, 187, 255), Color.White, Main.rand.NextFloat(0.35f)), new Vector2(0.5f), Main.rand.Next(3), Main.rand.NextFloat(MathHelper.TwoPi), 0.3f * (particleVel.X == 0 ? (Main.rand.NextBool() ? -1 : 1) : Math.Sign(particleVel.X)),
                                        Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                                        0.065f, 2.6f, 65),
                                        ParticleManager.ParticleLayer.AfterEverything);
                                }
                            }
                        }
                    }
                }
                else if (eventCounter == 1)
                {
                    if (eventTimer > 130)
                        UpdateSpeech(JstcEnd);
                    NPC.frameCounter += 0.1667d;
                }
                else if (eventCounter == 2)
                {
                    if (eventTimer == 1)
                        SoundEngine.PlaySound(Melt with { Volume = 0.85f, Pitch = -0.1f });
                    if (eventTimer > 180)
                    {
                        eventCounter++;
                        eventTimer = 0;
                    }
                }
                else if (eventCounter == 3)
                {
                    if (eventTimer == 1)
                        SoundEngine.PlaySound(Melt with { Volume = 0.85f, Pitch = -0.1f });
                    if (eventTimer > goopFadeTime)
                        UpdateSpeech(JstcEnd);
                }
                else if (eventCounter == 4)
                {
                    if (eventTimer == 1)
                        SoundEngine.PlaySound(Melt with { Volume = 1f });
                    if (eventTimer > goopFadeTime)
                        UpdateSpeech(JstcEnd);
                }
                else if (eventCounter == 5)
                {
                    if (eventTimer == 1)
                        SoundEngine.PlaySound(Melt with { Volume = 1f });
                    if (eventTimer > goopFadeTime)
                        UpdateSpeech(JstcEnd);
                }
                else if (eventCounter == 6)
                {
                    if (eventTimer > 160)
                        UpdateSpeech(JstcEnd, keepBubbleThroughEvent: true);
                }
                else if (eventCounter == 7)
                {
                    UpdateSpeech(JstcEnd, eventTimer >= 90, eventTimer >= 90);
                }
                else if (eventCounter >= 8 && eventCounter < 12)
                {
                    if (eventTimer == 1)
                        SoundEngine.PlaySound(Melt with { Volume = 1f });
                    if (eventTimer > goopFadeTime)
                        UpdateSpeech(JstcEnd, keepBubbleThroughEvent: eventCounter == 11);
                }
                else
                {
                    if (eventTimer == 1)
                        SoundEngine.PlaySound(Melt with { Volume = 1f });
                    UpdateSpeech(JstcEnd, eventTimer >= 220, eventTimer >= 220);
                    textSpeed = 0.25f;
                }
            }
            else
            {
                if (eventTimer == 410)
                {
                    SoundEngine.PlaySound(MeltDust with { Volume = 1f });
                }
                DrawTalkBubble = false;
                NPC.localAI[0]++;
            }
        

            if (!(eventCounter >= 12 && eventTimer >= 160))
                NPC.localAI[0] = -deathCutsceneDuration + 60;
            if (NPC.localAI[0] > -30)
                NPC.localAI[1] = 200;
            kill = NPC.localAI[1] == 200;

            if (kill)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }
            Player player = Main.LocalPlayer;
            if (player.active)
            {
                var modPlayer = player.ModPlayer();
                if (modPlayer != null && !modPlayer.escapeFail && NPC.localAI[0] >= -91)
                {
                    modPlayer.escapeFail = true;
                }

            }

            NPC.rotation = 0;
            for (int i = 0; i < Main.combatText.Length; i++)
            {
                var text = Main.combatText[i];
                text.active = false;
            }

            return kill;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0 && deadTime <= 1 && NPC.soundDelay <= 0)
            {
                NPC.soundDelay = Main.rand.Next(3, 7);
                SoundEngine.PlaySound(BitHurt with { Volume = 0.5f, PitchVariance = 0.2f }, NPC.Center);
            }
            if (!middleCutsceneStarted && NPC.life / (float)NPC.lifeMax < 0.5f)
            {
                CheckDead();
            }
        }
        public override void OnKill()
        {
            
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetBox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation, NPC.scale));
                if (pass)
                {
                    if (ableToHit)
                    {
                        target.AddBuff(ModContent.BuffType<Retribution>(), 20);
                    }
                    if (!hitboxes[i].contactDamage)
                    {
                        continue;
                    }
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            CollisionPass = false;
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            Rectangle targetBox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active || !hitboxes[i].contactDamage)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation, NPC.scale));
                if (pass)
                {
                    CollisionPass = ableToHit;
                    return ableToHit;
                }
            }
            CollisionPass = false;
            return false;
        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (NPC.ai[0] == Dash.Id && NPC.ai[1] >= Dash.Duration - 90)
            {
                damageMultiplier *= 1.5f;
            }
            if (CollisionPass)
            {
                npcHitbox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
            }
            return CollisionPass;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            InflictRetribution(target);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            
        }
        public static void InflictRetribution(Player player)
        {
            int time = RuinedMoonActive ? 900 : 90;
            player.AddBuff(ModContent.BuffType<Retribution>(), time);
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            if (NPC.localAI[0] >= -30 && deadTime == 0)
                boundingBox = NPC.Hitbox;
            else
                boundingBox = new Rectangle(0, 0, 1, 1);
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position = NPC.Center + new Vector2(0, 180);
            return !ModContent.GetInstance<TerRoguelikeConfig>().BossHealthbar;
        }
        public override void FindFrame(int frameHeight)
        {
            var tex = TextureAssets.Npc[Type];

            NPC.frame = new Rectangle(0, 0, tex.Width(), tex.Height());
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;
            if (modNPC.drawingBeforeWallsCurrently)
            {
                DrawBG();
            }

            var tex = TextureAssets.Npc[Type].Value;
            var font = TalkFont ? DotumChePixel : FontAssets.DeathText.Value;
            Color npcColor = Color.White;
            float scale = NPC.scale * zDepth;
            Vector2 drawPos = (NPC.Center - Main.Camera.Center) * zDepth + Main.Camera.Center;
            

            maskDrawOff += (drawPos - oldDrawPos) * 0.75f;

            Vector2 drawOff = -Main.screenPosition;

            int cutsceneTime = !middleCutsceneStarted ? (int)NPC.localAI[0] + cutsceneDuration : cutsceneDuration;
            int middleCutsceneTime = middleCutsceneStarted ? (int)NPC.localAI[0] + middleCutsceneDuration : 0;

            if (cutsceneTime > 61 && deadTime < 180)
                ILEdits.dualContrastTileShader = true;


            oldDrawPos = drawPos;

            if (!modNPC.drawingBeforeWallsCurrently)
            {
                if (bgParticles != null && bgParticles.Count > 0)
                {
                    StartAlphaBlendSpritebatch();
                    for (int i = 0; i < bgParticles.Count; i++)
                    {
                        var bgparticle = bgParticles[i];
                        if (bgparticle.particle.additive || bgparticle.zDepth < 1)
                            continue;

                        bgparticle.particle.Draw((bgparticle.particle.position - Main.Camera.Center) * bgParticles[i].zDepth + Main.Camera.Center - bgparticle.particle.position);
                    }
                    StartAdditiveSpritebatch();
                    for (int i = 0; i < bgParticles.Count; i++)
                    {
                        var bgparticle = bgParticles[i];
                        if (!bgparticle.particle.additive || bgparticle.zDepth < 1)
                            continue;

                        bgparticle.particle.Draw((bgparticle.particle.position - Main.Camera.Center) * bgParticles[i].zDepth + Main.Camera.Center - bgparticle.particle.position);
                    }
                }
            }
            else
                return false;


            void OverlayDraw(Vector2 texSize, Vector2 frameSize, Vector2 framePos, Action action)
            {
                Main.spriteBatch.End();
                Effect overlay = Filters.Scene["TerRoguelike:ColorMaskOverlay"].GetShader().Shader;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, overlay, Main.GameViewMatrix.TransformationMatrix);

                overlay.Parameters["position"].SetValue(maskDrawOff);
                overlay.Parameters["stretch"].SetValue(new Vector2((scale * 0.75f) / 1f));
                overlay.Parameters["tint"].SetValue(Color.White.ToVector4());
                overlay.Parameters["maskColor"].SetValue(new Color(255, 0, 255).ToVector4());
                overlay.Parameters["maskTint"].SetValue(Color.White.ToVector4());
                overlay.Parameters["rotation"].SetValue(NPC.rotation);
                overlay.Parameters["texSize"].SetValue(texSize);
                overlay.Parameters["frameSize"].SetValue(frameSize);
                overlay.Parameters["framePos"].SetValue(framePos);
                overlay.Parameters["replacementTexSize"].SetValue(starTex.Size());
                overlay.Parameters["replacementTexture"].SetValue(starTex);

                action.Invoke();

                StartVanillaSpritebatch();
            }

            if (NPC.localAI[0] < 0)
            {
                float blackOpacity = 1f;
                if (introTime < fadeBlackTime)
                    blackOpacity *= introTime / (float)fadeBlackTime;
                else if (middleCutsceneStarted && middleTime < fadeBlackTime)
                    blackOpacity *= middleTime / (float)fadeBlackTime;
                else if (deadTime > 0 && deadTime < fadeBlackTime)
                    blackOpacity *= deadTime / (float)fadeBlackTime;

                if (NPC.localAI[0] > -90)
                {
                    blackOpacity *= (Math.Abs(NPC.localAI[0]) - 30) / 60f;
                    if (blackOpacity < 0)
                        blackOpacity = 0;
                }


                Color squareColor = Color.Black;
                if (!middleCutsceneStarted && eventCounter == 4)
                {
                    if ((int)NPC.frameCounter == 135 || (int)NPC.frameCounter == 146 || (int)NPC.frameCounter == 147)
                    {
                        squareColor = new Color((int)25, (int)31, (int)37);
                    }
                }
                Main.EntitySpriteDraw(squareTex, Main.Camera.Center - Main.screenPosition, null, squareColor * blackOpacity, 0, squareTex.Size() * 0.5f, new Vector2(500, 300), SpriteEffects.None);

                if (!middleCutsceneStarted)
                {
                    if (eventCounter >= 3)
                    {
                        var frameTex = FindIntroTransformTexture();
                        OverlayDraw(frameTex.Size(), frameTex.Size(), Vector2.Zero,
                            () => Main.EntitySpriteDraw(frameTex, drawPos - Main.screenPosition - Vector2.UnitY * 7 + Vector2.UnitX * 7, null, Color.White, 0, frameTex.Size() * 0.5f, 1, SpriteEffects.None));

                        if (NPC.frameCounter >= 51)
                        {
                            if (NPC.frameCounter < 57)
                            {
                                Main.EntitySpriteDraw(IntroTransformEffect, drawPos - Main.screenPosition + new Vector2(-4, 437), IntroTransformEffect.Frame(1, 6, 0, (int)NPC.frameCounter - 51), Color.White, 0, IntroTransformEffect.Size() * 0.5f, 1f, SpriteEffects.None);
                            }
                            if (NPC.frameCounter < 61)
                            {
                                Main.EntitySpriteDraw(Syringe, SyringePos + drawPos - Main.screenPosition, null, Color.White, SyringeRot, Syringe.Size() * 0.5f, 1f, SpriteEffects.None);
                            }
                        }
                    }
                    else
                    {
                        Vector2 martDrawOff = Vector2.Zero;
                        if (introTime < fadeBlackTime + flyDownTime)
                        {
                            float flyDownInterpolant = (1 - ((float)introTime / (fadeBlackTime + flyDownTime)));
                            martDrawOff += -Vector2.UnitY * 700 * (float)Math.Pow(flyDownInterpolant, 1.3f);
                        }
                        int introTalkFrame = FindIntroFrame();
                        int introFrameX = introTalkFrame / IntroTalkFrames.Y;
                        int introFrameY = introTalkFrame % IntroTalkFrames.Y;
                        var introFrameRect = IntroTalkTex.Frame(IntroTalkFrames.X, IntroTalkFrames.Y, introFrameX, introFrameY);
                        Main.EntitySpriteDraw(IntroTalkTex, drawPos - Main.screenPosition + martDrawOff + new Vector2(0, -21), introFrameRect, Color.White, 0, introFrameRect.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }
            if (cutsceneTime > 120)
            {
                Rectangle HairFrame()
                {
                    int hairFrameCount = 12;
                    int hairVertFrameCount = 3;
                    int hairHorizFrameCount = 4;
                    int hairCurrentFrame = (int)(NPC.frameCounter * 1.667d + 0.35d) % hairFrameCount;
                    return Hair.Frame(hairHorizFrameCount, hairVertFrameCount, hairCurrentFrame / hairVertFrameCount, hairCurrentFrame % hairVertFrameCount);
                }

                if (NPC.localAI[0] < -30)
                {
                    bool sbReset = false;
                    for (int i = 0; i < YellowPulses.Count; i++)
                    {
                        int pulseTime = YellowPulses[i];
                        float pulseCompletion = 1 - (pulseTime / (float)maxYellowPulseTime);

                        StartAlphaBlendSpritebatch();
                        sbReset = true;

                        float pulseScale = pulseCompletion;
                        float opacity = Math.Min(1, (1 - pulseCompletion) * 6);
                        float drawScale = 2000;
                        float finalMultiplier = 1f;
                        float pixelation = 0.0125f;


                        GameShaders.Misc["TerRoguelike:CircularPulse"].UseOpacity(opacity);
                        GameShaders.Misc["TerRoguelike:CircularPulse"].UseImage0(TextureAssets.Projectile[ModContent.ProjectileType<SandTurret>()]);
                        GameShaders.Misc["TerRoguelike:CircularPulse"].UseColor(Color.Yellow);
                        GameShaders.Misc["TerRoguelike:CircularPulse"].UseShaderSpecificData(new(pulseScale, 2 * finalMultiplier, pixelation * finalMultiplier, 0));

                        GameShaders.Misc["TerRoguelike:CircularPulse"].Apply();

                        Vector2 drawPosition = NPC.Center - Main.screenPosition;
                        Main.EntitySpriteDraw(pulseTex, drawPosition + BodyPartOff(new Vector2(0, -50)), null, Color.White, 0, pulseTex.Size() * 0.5f, drawScale * finalMultiplier, 0, 0);
                    }
                    if (sbReset)
                        StartVanillaSpritebatch();
                }
                
                bool resetAnimation = true;
                bool drawNone = NPC.ai[0] == Dash.Id && NPC.ai[1] < Dash.Duration - 90 && NPC.ai[1] >= 120;
                bool goop = deadTime >= 108;
                bool backgroundFly = displayBackgroundFly;
                bool flyAway = displayFlyAway;
                bool scream = displayScream;
                bool knockdown = displayKnockdown;
                bool flap = displayFlap;
                if (drawNone)
                {

                }
                else if (goop)
                {
                    int GoopFrameByEvent(int evnt)
                    {
                        switch (evnt)
                        {
                            default:
                            case 0:
                            case 1:
                                return (int)MathHelper.Clamp((float)NPC.frameCounter, 1, 22);
                            case 2:
                                return ((int)(animationCounter * 0.5d) % 5) + 23;
                            case 3:
                                return ((int)(animationCounter * 0.5d) % 6) + 28;
                            case 4:
                                return ((int)(animationCounter * 0.5d) % 7) + 34;
                            case 5:
                            case 6:
                            case 7:
                                return ((int)(animationCounter * 0.375d) % 5) + 41;
                            case 8:
                                return ((int)(animationCounter * 0.5d) % 3) + 46;
                            case 9:
                                return 49;
                            case 10:
                                return ((int)(animationCounter * 0.2d) % 2) + 50;
                            case 11:
                                return 52;
                            case 12:
                                return eventTimer < 270 ? 53 : 54;
                        }
                    }
                    resetAnimation = false;
                    float goopOpacity = 1;
                    if (eventCounter > 1 && eventCounter != 6 && eventCounter != 7)
                    {
                        goopOpacity = eventTimer / goopFadeTime;
                    }

                    int goopframe = GoopFrameByEvent(eventCounter);
                    var goopTex = FindGoopTexture(goopframe);
                    if (goopframe == 54)
                    {
                        Vector2 texSize = goopTex.Size();
                        int texWidth = (int)texSize.X;
                        int texHeight = (int)texSize.Y;
                        int dustTime = eventTimer - 410;
                        int dustSeparateTime = 64;
                        float dustSeparateCompletion = MathHelper.Clamp((float)dustTime / dustSeparateTime, 0, 1);
                        int dustChunkHeight = 10;
                        int totalChunks = texHeight / dustChunkHeight;
                        int increment = 0;
                        for (int j = 0; j < texHeight; j += 2)
                        {
                            Vector2 dustingOffset = drawPos + drawOff - (texSize * new Vector2(0.5f, 1f));
                            int vertChunk = j / dustChunkHeight;
                            float verticalProgress = (float)vertChunk / totalChunks;
                            float opacity = 1;
                            float individualCompletion = 0;
                            if (verticalProgress <= dustSeparateCompletion)
                            {
                                int separateStartTime = (int)(verticalProgress * dustSeparateTime);
                                int individualTime = dustTime - separateStartTime;
                                individualCompletion = MathHelper.Clamp((float)individualTime / 60, 0, 1);
                                opacity = (float)Math.Pow(1 - individualCompletion, 0.8f);
                                dustingOffset.Y -= 70 * individualCompletion;
                            }

                            for (int i = 0; i < texWidth; i += 26)
                            {
                                increment++;
                                Rectangle frameSnippet = new(i, j, 26, 2);
                                Vector2 specificOffset = dustingOffset;
                                if (individualCompletion > 0)
                                {
                                    specificOffset.X += (int)((float)Math.Cos(increment + Main.GlobalTimeWrappedHourly * 3) * 6 * individualCompletion);
                                }
                                Vector2 finalDrawPos =new Vector2(-30 + i, 230 + j) + specificOffset;
                                Main.EntitySpriteDraw(goopTex, finalDrawPos, frameSnippet, Color.White * opacity, NPC.rotation, Vector2.Zero, scale, SpriteEffects.None);
                            }
                        }
                    }
                    else
                    {
                        Main.EntitySpriteDraw(goopTex, drawPos + drawOff + (goopframe < 23 ? new Vector2(-1, 217) : new Vector2(-30, 230)), null, Color.White * ((float)Math.Pow(goopOpacity, 0.5f)), NPC.rotation, goopTex.Size() * new Vector2(0.5f, 1f), scale, SpriteEffects.None);
                        if (goopOpacity < 1)
                        {
                            int pastGoopFrame = GoopFrameByEvent(eventCounter - 1);
                            var pastGoopTex = FindGoopTexture(pastGoopFrame);
                            Main.EntitySpriteDraw(pastGoopTex, drawPos + drawOff + (pastGoopFrame < 23 ? new Vector2(-1, 217) : new Vector2(-30, 230)), null, Color.White * (1 - goopOpacity), NPC.rotation, pastGoopTex.Size() * new Vector2(0.5f, 1f), scale, SpriteEffects.None);
                        }
                    }
                }
                else if (backgroundFly)
                {
                    int frameCounter = backgroundFlyFrameCounter;
                    var bgFlyFrame = BackgroundFly.Frame(2, 4, frameCounter / 4, frameCounter % 4);

                    Vector2 quickEdit = new Vector2(30, 0).RotatedBy(NPC.rotation);
                    maskDrawOff += quickEdit;
                    OverlayDraw(BackgroundFly.Size(), bgFlyFrame.Size(), new Vector2(bgFlyFrame.X, bgFlyFrame.Y), new Action(
                        () => Main.EntitySpriteDraw(BackgroundFly, drawPos + drawOff, bgFlyFrame, Color.White, NPC.rotation, bgFlyFrame.Size() * 0.5f - new Vector2(40, 0), scale, SpriteEffects.None)));
                    maskDrawOff -= quickEdit;
                }
                else if (flyAway)
                {
                    int frameCounter = flyAwayFrameCounter;

                    int flyAwayFrameX = frameCounter / 3;
                    int flyAwayFrameY = frameCounter % 3;
                    var flyAwayFrame = FlyAway.Frame(3, 3, flyAwayFrameX, flyAwayFrameY);

                    if (frameCounter != 8)
                    {
                        var hairFrame = HairFrame();
                        Main.EntitySpriteDraw(Hair, drawPos + BodyPartOff(new Vector2(0, -100)) + drawOff, hairFrame, Color.White, NPC.rotation, hairFrame.Size() * new Vector2(0.5f, 0), scale, SpriteEffects.None);
                    }
                    
                    Vector2 quickEdit = new Vector2(9, 12).RotatedBy(NPC.rotation);
                    maskDrawOff += quickEdit;
                    OverlayDraw(FlyAway.Size(), flyAwayFrame.Size(), new Vector2(flyAwayFrame.X, flyAwayFrame.Y), new Action(
                        () => Main.EntitySpriteDraw(FlyAway, drawPos + drawOff, flyAwayFrame, Color.White, NPC.rotation, flyAwayFrame.Size() * 0.5f - new Vector2(12, 16), scale, SpriteEffects.None)));
                    maskDrawOff -= quickEdit;

                    if (despawnTime > 40)
                    {
                        float blackOpacity = (despawnTime - 40) / 60f;
                        Main.EntitySpriteDraw(squareTex, Main.Camera.Center - Main.screenPosition, null, Color.Black * blackOpacity, 0, squareTex.Size() * 0.5f, new Vector2(500, 300), SpriteEffects.None);
                    }
                }
                else if (scream)
                {
                    var hairFrame = HairFrame();
                    Main.EntitySpriteDraw(Hair, drawPos + BodyPartOff(new Vector2(0, -100)) + drawOff, hairFrame, Color.White, NPC.rotation, hairFrame.Size() * new Vector2(0.5f, 0), scale, SpriteEffects.None);

                    Vector2 quickEdit = new Vector2(2.25f, 18).RotatedBy(NPC.rotation);
                    maskDrawOff += quickEdit;
                    OverlayDraw(Screaming.Size(), Screaming.Size(), Vector2.Zero, new Action(
                        () => Main.EntitySpriteDraw(Screaming, drawPos + drawOff, null, Color.White, NPC.rotation, Screaming.Size() * 0.5f - new Vector2(3, 24), scale, SpriteEffects.None)));
                    maskDrawOff -= quickEdit; 
                }
                else if (knockdown)
                {
                    int knockdownTimer = deadTime > 0 ? deadTime : middleTime;

                    int knockdownFrameCounter = 0;
                    int animationStartPoint = 60;
                    if (knockdownTimer > animationStartPoint)
                    {
                        knockdownFrameCounter = (knockdownTimer - animationStartPoint) / 8;
                        if (knockdownFrameCounter >= 8)
                            knockdownFrameCounter = 7;
                        if (knockdownFrameCounter < 0)
                            knockdownFrameCounter = 0;
                    }
                    if (deadTime == 0)
                    {
                        int backUpPoint = 174;
                        if (middleCutsceneTime > backUpPoint)
                        {
                            knockdownFrameCounter -= (middleCutsceneTime - backUpPoint) / 5;
                            if (knockdownFrameCounter >= 8)
                                knockdownFrameCounter = 7;
                            if (knockdownFrameCounter < 1)
                                knockdownFrameCounter = 1;
                        }
                    }
                    
                    
                    var knockdownFrame = KnockedDown.Frame(2, 4, knockdownFrameCounter / 4, knockdownFrameCounter % 4);

                    if (knockdownFrameCounter == 0)
                    {
                        var hairFrame = HairFrame();
                        Main.EntitySpriteDraw(Hair, drawPos + BodyPartOff(new Vector2(0, -100)) + drawOff, hairFrame, Color.White, NPC.rotation, hairFrame.Size() * new Vector2(0.5f, 0), scale, SpriteEffects.None);
                    }

                    Vector2 quickEdit = new Vector2(-0.75f, 22.5f).RotatedBy(NPC.rotation);
                    maskDrawOff += quickEdit;
                    OverlayDraw(KnockedDown.Size(), knockdownFrame.Size(), new Vector2(knockdownFrame.X, knockdownFrame.Y), new Action(
                        () => Main.EntitySpriteDraw(KnockedDown, drawPos + drawOff, knockdownFrame, Color.White, NPC.rotation, knockdownFrame.Size() * 0.5f - new Vector2(-1, 30), scale, SpriteEffects.None)));
                    maskDrawOff -= quickEdit;
                }
                else if (flap)
                {
                    int frameCounter = flapFrameCounter;
                    var flapFrame = WingAttack.Frame(2, 3, frameCounter / 3, frameCounter % 3);
                    var hairOffset = frameCounter switch
                    {
                        1 => 2,
                        2 => -4,
                        3 => -6,
                        4 => -4,
                        5 => -2,
                        _ => 0,
                    };
                    var hairFrame = HairFrame();
                    Main.EntitySpriteDraw(Hair, drawPos + BodyPartOff(new Vector2(0, -100 + hairOffset)) + drawOff, hairFrame, Color.White, NPC.rotation, hairFrame.Size() * new Vector2(0.5f, 0), scale, SpriteEffects.None);

                    Vector2 quickEdit = new Vector2(9.75f, 8.25f).RotatedBy(NPC.rotation);
                    maskDrawOff += quickEdit;
                    OverlayDraw(WingAttack.Size(), flapFrame.Size(), new Vector2(flapFrame.X, flapFrame.Y), new Action(
                        () => Main.EntitySpriteDraw(WingAttack, drawPos + drawOff, flapFrame, Color.White, NPC.rotation, flapFrame.Size() * 0.5f - new Vector2(13, 11), scale, SpriteEffects.None)));
                    maskDrawOff -= quickEdit;
                }
                else
                {
                    resetAnimation = false;
                    var hairFrame = HairFrame();
                    Main.EntitySpriteDraw(Hair, drawPos + BodyPartOff(new Vector2(0, -100)) + drawOff, hairFrame, Color.White, NPC.rotation, hairFrame.Size() * new Vector2(0.5f, 0), scale, SpriteEffects.None);

                    Main.EntitySpriteDraw(Head, drawPos + BodyPartOff(new Vector2(0, -74)) + drawOff, null, Color.White, NPC.rotation, Head.Size() * 0.5f, scale, SpriteEffects.None);

                    int wingsFrameCount = 8;
                    int wingsCurrentFrame = wingsFrameCounter;
                    var wingsFrame = Wings.Frame(1, wingsFrameCount, 0, wingsCurrentFrame);

                    OverlayDraw(Wings.Size(), wingsFrame.Size(), new Vector2(wingsFrame.X, wingsFrame.Y), new Action(
                        () => Main.EntitySpriteDraw(Wings, drawPos + drawOff, wingsFrame, Color.White, NPC.rotation, wingsFrame.Size() * 0.5f, scale, SpriteEffects.None)));

                    float chestMultiplier = (float)Math.Sin((float)animationCounter * 0.02f * MathHelper.TwoPi);
                    float leftLegMultiplier = (float)Math.Sin((float)animationCounter * 0.033f * MathHelper.TwoPi);
                    float leftTalonMultiplier = (float)Math.Sin((float)animationCounter * 0.04f * MathHelper.TwoPi);
                    float rightLegInterpolant = (-(float)Math.Cos((float)animationCounter * 0.03f * MathHelper.TwoPi) + 1) * 0.5f;
                    if (!middleCutsceneStarted && cutsceneTime < 550)
                    {
                        chestMultiplier = 0;
                        leftLegMultiplier = 0;
                        leftTalonMultiplier = 0;
                        rightLegInterpolant = 0;
                    }

                    Main.EntitySpriteDraw(Chest, drawPos + BodyPartOff(new Vector2(0, -28 - (2 * chestMultiplier))) + drawOff, null, Color.White, NPC.rotation, Chest.Size() * 0.5f, scale, SpriteEffects.None);

                    Main.EntitySpriteDraw(LeftTalon, drawPos + BodyPartOff(new Vector2(-12, 126 + (2 * leftLegMultiplier) - (2 * leftTalonMultiplier) - (2 * chestMultiplier))) + drawOff, null, Color.White, NPC.rotation, LeftTalon.Size() * 0.5f, scale, SpriteEffects.None);
                    Main.EntitySpriteDraw(LeftLeg, drawPos + BodyPartOff(new Vector2(-6, 57 + (2 * leftLegMultiplier) - (2 * chestMultiplier))) + drawOff, null, Color.White, NPC.rotation, LeftLeg.Size() * 0.5f, scale, SpriteEffects.None);

                    
                    Main.EntitySpriteDraw(RightLeg, drawPos + BodyPartOff(new Vector2(23, 32 + (6 * rightLegInterpolant) + (1 * leftLegMultiplier))) + drawOff, null, Color.White, NPC.rotation, RightLeg.Size() * 0.5f, scale * new Vector2(1, 1 + 0.1f * rightLegInterpolant), SpriteEffects.None);
                    Main.EntitySpriteDraw(RightTalon, drawPos + BodyPartOff(new Vector2(33, 67 + (8 * rightLegInterpolant) + (1 * leftLegMultiplier))) + drawOff, null, Color.White, NPC.rotation, RightTalon.Size() * 0.5f, scale * new Vector2(1, 1 + 0.1f * rightLegInterpolant), SpriteEffects.None);
                }

                if (resetAnimation)
                    animationCounter = 0;
            }

            if (DrawTalkBubble)
            {
                void DrawDialogue(List<StringBundle> list, Vector2? offset = null)
                {
                    Vector2 vector = offset ?? Vector2.Zero;
                    Main.EntitySpriteDraw(TalkBubble, drawPos + new Vector2(55, -90) + vector - Main.screenPosition, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None);

                    if ((int)NPC.localAI[1] < 0 || (int)NPC.localAI[1] >= list.Count)
                        return;

                    var strings = list[(int)NPC.localAI[1]].StringDisplay(textProgress < 0 ? -1 : (int)textProgress);
                    int verticalStep = TalkFont ? 15 : 18;
                    for (int i = 0; i < strings.Count; i++)
                    {
                        Vector2 textScale = TalkFont ? new Vector2(0.081f) : new Vector2(0.38f);
                        float stringSize = (font.MeasureString(list[(int)NPC.localAI[1]].strings[i]) * textScale).X;
                        float shrinkThreshold = 195;
                        if (stringSize > shrinkThreshold)
                        {
                            textScale.X *= shrinkThreshold / stringSize;
                        }
                        ChatManager.DrawColorCodedString(Main.spriteBatch, font, strings[i], drawPos + new Vector2(77, -82) + Vector2.UnitY * verticalStep * i + vector - Main.screenPosition, Color.Black, 0, Vector2.Zero, textScale);
                    }
                }
                if (deadTime > 0)
                {
                    DrawDialogue(JstcEnd, new Vector2(28, 110));
                }
                else if (middleCutsceneStarted)
                {
                    DrawDialogue(JstcMiddle, new Vector2(28, 110));
                }
                else
                {
                    DrawDialogue(JstcIntro);
                }
            }

            bool drawHitboxes = false;
            if (drawHitboxes)
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    if (!hitboxes[i].active)
                        continue;
                    Color color = hitboxes[i].contactDamage ? Color.Red : Color.Magenta;

                    Rectangle hitbox = hitboxes[i].GetHitbox(NPC.Center, NPC.rotation, NPC.scale);
                    for (int d = 0; d <= 1; d++)
                    {
                        for (int x = 0; x < hitbox.Width; x++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(x, hitbox.Height * d) - Main.screenPosition, null, color, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                        for (int y = 0; y < hitbox.Height; y++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(hitbox.Width * d, y) - Main.screenPosition, null, color, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                    }
                }
            }
            return false;
        }
        public Vector2 BodyPartOff(Vector2 offset)
        {
            return offset.RotatedBy(NPC.rotation) * NPC.scale * zDepth;
        }
        public int wingsFrameCounter
        {
            get {
                return (int)animationCounter % 8;
            }
        }
        public bool displayBackgroundFly
        {
            get {
                return NPC.ai[0] == Dash.Id && NPC.ai[1] >= 120;
            }
        }
        public int backgroundFlyFrameCounter
        {
            get {
                return Math.Min(7, (int)NPC.frameCounter);
            }
        }

        public bool displayFlyAway
        {
            get {
                if (despawnTime > 0)
                    return true;
                else
                {
                    return NPC.ai[0] == Dash.Id && NPC.ai[1] < 120;
                }
            }
        }
        public int flyAwayFrameCounter
        {
            get {
                int flyAwayFrameCounter = despawnTime > 0 ? despawnTime / 5 : (int)(NPC.frameCounter * 2);
                if (flyAwayFrameCounter > 4)
                {
                    flyAwayFrameCounter = Math.Max(flyAwayFrameCounter - 1, 4);
                    if (flyAwayFrameCounter > 8)
                        flyAwayFrameCounter = 8;
                }
                return flyAwayFrameCounter;
            }
        }

        public bool displayScream
        {
            get {
                int cutsceneTime = !middleCutsceneStarted ? (int)NPC.localAI[0] + cutsceneDuration : cutsceneDuration;
                int middleCutsceneTime = middleCutsceneStarted ? (int)NPC.localAI[0] + middleCutsceneDuration : 0;

                bool yeah =
                    (cutsceneTime > 535 && cutsceneTime < 680) ||
                    (middleCutsceneTime > 210 && middleCutsceneTime < 355) ||
                    NPC.ai[0] == Meteors.Id && NPC.ai[1] < 135 && NPC.ai[1] >= 20 ||
                    NPC.ai[0] == Stars.Id && NPC.ai[1] < 135 && NPC.ai[1] >= 20;
                return yeah;
            }
        }

        public bool displayFlap
        {
            get {
                return NPC.ai[0] == Trash.Id;
            }
        }
        public int flapFrameCounter
        {
            get {
                int flapFrameCounter = (int)(NPC.frameCounter * 1.75d);
                flapFrameCounter %= 6;
                return flapFrameCounter;
            }
        }

        public bool displayKnockdown
        {
            get {
                bool yeah =
                    (middleCutsceneStarted && NPC.localAI[0] < -60) ||
                    deadTime > 0;
                return yeah;
            }
        }
        public int FindIntroFrame()
        {
            int counter = (int)NPC.frameCounter;
            switch (counter)
            {
                default:
                    return counter;
                case 12:
                    return talkFrameCounter > 10 ? 12 : 11;
                case 23:
                    return talkFrameCounter > 10 ? 23 : 22;
            }
        }
        public Texture2D FindIntroTransformTexture()
        {
            int counter = (int)NPC.frameCounter;
            if (counter < 1)
                counter = 1;
            else if (counter > 168)
                counter = 168;
            return ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/Boss/Mallet/Transformation Frames/TransformFrame (" + counter.ToString() + ")", AssetRequestMode.ImmediateLoad).Value;
        }
        public Texture2D FindGoopTexture(int frame)
        {
            if (frame < 1)
                frame = 1;
            else if (frame > 54)
                frame = 54;
            return ModContent.Request<Texture2D>("TerRoguelike/NPCs/Enemy/Boss/Mallet/Goop Frames/GoopFrame (" + frame.ToString() + ")", AssetRequestMode.ImmediateLoad).Value;
        }
        public class BackgroundStar
        {
            public static Texture2D tex = null;
            public Vector2 position;
            public int variant;
            public BackgroundStar(Vector2 position, int variant)
            {
                this.position = position;
                this.variant = variant;
                tex ??= TexDict["BGStars"];
            }
            public Rectangle Frame => tex.Frame(1, 4, 0, variant, 0, -2);
        }
        public void SetUpStars()
        {
            Rectangle SpawnRect = new Rectangle(0, 0, 2500, 2500);
            float distanceApart = 200;
            int starCount = 0;
            for (int i = 0; i < 200; i++)
            {
                Vector2 chosenVect = Main.rand.NextVector2FromRectangle(SpawnRect);
                bool allow = true;
                for (int j = 0; j < BackgroundStars.Count; j++)
                {
                    BackgroundStar s = BackgroundStars[j];
                    if (chosenVect.Distance(s.position) < distanceApart)
                    {
                        allow = false;
                        break;
                    }
                    Vector2 checkVect = chosenVect + new Vector2(1250);
                    checkVect.X %= 2500;
                    checkVect.Y %= 2500;
                    Vector2 starVect = s.position + new Vector2(1250);
                    starVect.X %= 2500;
                    starVect.Y %= 2500;
                    if (checkVect.Distance(starVect) < distanceApart)
                    {
                        allow = false;
                        break;
                    }
                }
                if (!allow)
                {
                    continue;
                }
                BackgroundStars.Add(new(chosenVect, Main.rand.Next(4)));
                starCount++;
                if (starCount >= 90)
                    break;
            }
        }
        public void DrawBG()
        {
            int cutsceneTime = !middleCutsceneStarted ? (int)NPC.localAI[0] + cutsceneDuration : cutsceneDuration;

            if (cutsceneTime > 61 && deadTime < 180)
            {
                float bgScale = 1.5f;
                Vector2 basePos = Main.Camera.Center;
                Vector2 backPos = basePos * -0.09f;
                backPos.X %= 44 * bgScale;
                backPos.Y %= 43 * bgScale;
                Vector2 frontPos = basePos * -0.10f;
                frontPos.X %= 45 * bgScale;
                frontPos.Y %= 45 * bgScale;
                Vector2 starAnchor = basePos * -0.08f;

                Main.EntitySpriteDraw(squareTex, basePos - Main.screenPosition, null, Color.Black, 0, squareTex.Size() * 0.5f, new Vector2(2000, 2000), SpriteEffects.None);
                for (int i = 0; i < BackgroundStars.Count; i++)
                {
                    var star = BackgroundStars[i];
                    Vector2 starPos = star.position + starAnchor;
                    starPos.X %= 2500;
                    starPos.Y %= 2500;
                    if (starPos.X < 0)
                        starPos.X += 2500;
                    if (starPos.Y < 0)
                        starPos.Y += 2500;
                    starPos += basePos - Main.screenPosition - new Vector2(1250);
                    var frame = star.Frame;
                    float starScale = 2f;
                    if (star.variant != 2)
                    {
                        starScale += ((float)Math.Cos(i * 1.44f + Main.GlobalTimeWrappedHourly * MathHelper.PiOver2 * (float)(Math.Cos(i) * 0.33f + 1)) - 1) * 0.3f;
                    }
                    Main.EntitySpriteDraw(BGStars, starPos, frame, Color.White, 0, frame.Size() * 0.5f, starScale, i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                }

                Main.EntitySpriteDraw(BGBack, backPos + basePos - Main.screenPosition, null, new Color(17, 80, 207) * 0.3f, 0, BGBack.Size() * 0.5f, bgScale, SpriteEffects.None);
                Main.EntitySpriteDraw(BGFront, frontPos + basePos - Main.screenPosition, null, new Color(66, 187, 255) * 0.286f, 0, BGFront.Size() * 0.5f, bgScale, SpriteEffects.None);


                if (modNPC.isRoomNPC && modNPC.sourceRoomListID >= 0)
                {
                    var room = RoomList[modNPC.sourceRoomListID];
                    var sideRect = new Rectangle(160, 0, 1, 160);
                    var cornerRect = new Rectangle(0, 0, 160, 160);
                    StartNonPremultipliedSpritebatch();
                    int maxXDimensions = (int)room.RoomDimensions.X + room.WallInflateModifier.X * 2;
                    int maxYDimensions = (int)room.RoomDimensions.Y + room.WallInflateModifier.Y * 2;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 drawPos = room.RoomPosition;
                        Vector2 autoAdd = Vector2.Zero;
                        if (i == 0 || i == 3)
                        {
                            drawPos.Y += (int)(room.RoomDimensions.Y + room.WallInflateModifier.Y - 1);
                        }
                        else
                        {
                            drawPos.Y += -room.WallInflateModifier.Y;
                            autoAdd.X = 16;
                        }
                        if (i == 0 || i == 1)
                        {
                            drawPos.X += -room.WallInflateModifier.X;
                        }
                        else
                        {
                            drawPos.X += (int)(room.RoomDimensions.X + room.WallInflateModifier.X - 1);
                            autoAdd.Y = 16;
                        }
                        drawPos = drawPos.ToWorldCoordinates(autoAdd.X, autoAdd.Y) - Main.screenPosition;

                        Vector2 sideScale = Vector2.One;
                        if (i % 2 == 0)
                        {
                            sideScale.X = (maxXDimensions - 2) * 16;
                        }
                        else
                        {
                            sideScale.X = (maxYDimensions - 2) * 16;
                        }

                        float blockRot = i * MathHelper.PiOver2;
                        Main.EntitySpriteDraw(BlueTemporaryBlock, drawPos + new Vector2(-144, 0).RotatedBy(blockRot), cornerRect, Color.White, blockRot, Vector2.Zero, 1f, SpriteEffects.None);

                        Main.EntitySpriteDraw(BlueTemporaryBlock, drawPos + new Vector2(16, 0).RotatedBy(blockRot), sideRect, Color.White, blockRot, Vector2.Zero, sideScale, SpriteEffects.None);
                    }
                }
            }

            
            if (bgParticles != null && bgParticles.Count > 0)
            {
                StartAlphaBlendSpritebatch();
                for (int i = 0; i < bgParticles.Count; i++)
                {
                    var bgparticle = bgParticles[i];
                    if (bgparticle.particle.additive || bgparticle.zDepth >= 1)
                        continue;

                    bgparticle.particle.Draw((bgparticle.particle.position - Main.Camera.Center) * bgParticles[i].zDepth + Main.Camera.Center - bgparticle.particle.position);
                }
                StartAdditiveSpritebatch();
                for (int i = 0; i < bgParticles.Count; i++)
                {
                    var bgparticle = bgParticles[i];
                    if (!bgparticle.particle.additive || bgparticle.zDepth >= 1)
                        continue;

                    bgparticle.particle.Draw((bgparticle.particle.position - Main.Camera.Center) * bgParticles[i].zDepth + Main.Camera.Center - bgparticle.particle.position);
                }
            }
            if (NPC.ai[0] == Dash.Id && NPC.ai[1] > 60)
            {
                Vector2 drawPos = (starPosition - Main.Camera.Center) * starZDepth + Main.Camera.Center;

                StartAlphaBlendSpritebatch();
                float starScale = 1f;
                Color starColor = Color.Yellow;
                starColor.A = 0;
                starScale *= 0.95f + (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) * 0.05f;
                if (NPC.ai[0] == Dash.Id && NPC.ai[1] >= Dash.Duration - 90)
                {
                    starScale *= MathHelper.SmoothStep(1, 0, (NPC.ai[1] - (Dash.Duration - 90)) / 60f);
                }
                Main.EntitySpriteDraw(sparkTex, drawPos - Main.screenPosition, null, starColor, 0, sparkTex.Size() * 0.5f, new Vector2(0.1f, 0.15f) * starScale, SpriteEffects.None);
                Main.EntitySpriteDraw(sparkTex, drawPos - Main.screenPosition, null, starColor, MathHelper.PiOver2, sparkTex.Size() * 0.5f, new Vector2(0.15f, 0.15f) * starScale, SpriteEffects.None);
            }

            if (NPC.localAI[0] >= -30)
            {
                bool sbReset = false;
                for (int i = 0; i < YellowPulses.Count; i++)
                {
                    int pulseTime = YellowPulses[i];
                    float pulseCompletion = 1 - (pulseTime / (float)maxYellowPulseTime);

                    StartAlphaBlendSpritebatch();
                    sbReset = true;

                    float pulseScale = pulseCompletion * 0.75f;
                    float opacity = Math.Min(1, (1 - pulseCompletion) * 6);
                    float drawScale = 4000;
                    float finalMultiplier = 1f;
                    float pixelation = 0.0125f * 0.75f;


                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseOpacity(opacity);
                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseImage0(TextureAssets.Projectile[ModContent.ProjectileType<SandTurret>()]);
                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseColor(Color.Yellow);
                    GameShaders.Misc["TerRoguelike:CircularPulse"].UseShaderSpecificData(new(pulseScale, 2 * finalMultiplier, pixelation * finalMultiplier, 0));

                    GameShaders.Misc["TerRoguelike:CircularPulse"].Apply();

                    Vector2 drawPosition = NPC.Center - Main.screenPosition;
                    Main.EntitySpriteDraw(pulseTex, drawPosition + BodyPartOff(new Vector2(0, -50)), null, Color.White, 0, pulseTex.Size() * 0.5f, drawScale * finalMultiplier, 0, 0);
                }
                if (sbReset)
                    StartVanillaSpritebatch();
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.WriteVector2(spawnPos);
            writer.WriteVector2(starVelocity);
            writer.WriteVector2(starPosition);
            writer.Write(NPC.direction);
            writer.Write(phase2);
            writer.Write(animationCounter);
            writer.WriteVector2(dashStart);
            writer.Write(zDepth);
            writer.Write(starZDepth);
            writer.Write(deadTime);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            spawnPos = reader.ReadVector2();
            starVelocity = reader.ReadVector2();
            starPosition = reader.ReadVector2();
            NPC.direction = reader.ReadInt32();
            phase2 = reader.ReadBoolean();
            animationCounter = reader.ReadDouble();
            dashStart = reader.ReadVector2();
            zDepth = reader.ReadSingle();
            starZDepth = reader.ReadSingle();
            deadTime = reader.ReadInt32();
        }
        public class PatternProjectile
        {
            public virtual int type => 0;
            public int time;
            public int damage;
            public Vector2 position;
            public Vector2 velocity;
            public float ai0;
            public float ai1;
            public float ai2;
            public virtual void SpawnProj(NPC npc)
            {
                if (TerRoguelike.mpClient)
                    return;
                Projectile.NewProjectile(npc.GetSource_FromThis(), position, velocity, type, damage, 0, -1, ai0, ai1, ai2);
            }
        }
        public class YellowWingProj : PatternProjectile
        {
            public override int type => ModContent.ProjectileType<YellowWing>();
            public YellowWingProj(NPC npc, int Time, Vector2 Position, Vector2 Velocity, int wingDir, int projCount = 6)
            {
                time = Time;
                damage = npc.damage;
                position = Position;
                velocity = Velocity;
                ai0 = wingDir;
                ai1 = projCount;
            }
        }

        public class DashingFeatherProj : PatternProjectile
        {
            public override int type => ModContent.ProjectileType<DashingFeather>();
            public DashingFeatherProj(NPC npc, int Time, Vector2 Position, Vector2 featherOffset, Vector2? reticleOffset = null)
            {
                time = Time;
                damage = npc.damage;
                position = Position;
                velocity = reticleOffset == null ? Vector2.Zero : (Vector2)reticleOffset;
                ai0 = featherOffset.X;
                ai1 = featherOffset.Y;
            }
        }
        public class TalonProj : PatternProjectile
        {
            public override int type => ModContent.ProjectileType<Talon>();
            public TalonProj(NPC npc, int Time, Vector2 Position, int slashTime, int dir)
            {
                time = Time;
                damage = npc.damage;
                velocity = Vector2.Zero;
                position = Position;
                ai0 = slashTime;
                ai1 = dir;
            }
        }
        public class SplittingFeatherProj : PatternProjectile
        {
            public override int type => ModContent.ProjectileType<SplittingFeather>();
            public SplittingFeatherProj(NPC npc, int Time, Vector2 Position, Vector2 featherOffset, ReticleType reticleType, Vector2? reticleOffset = null)
            {
                time = Time;
                damage = npc.damage;
                velocity = reticleOffset == null ? featherOffset : (Vector2)reticleOffset;
                position = Position;
                ai0 = featherOffset.X;
                ai1 = featherOffset.Y;
                ai2 = (int)reticleType;
            }
        }
    }
}