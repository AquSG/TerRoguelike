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

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class Being : BaseRoguelikeNPC
    {
        Vector2 spawnPos;
        public bool SkipCutscene = false;
        public override int modNPCID => ModContent.NPCType<Being>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Surface"] };
        public override string Texture => "TerRoguelike/NPCs/Enemy/Boss/TalkBubble2";
        public static SoundStyle BeingTalk = new("TerRoguelike/Sounds/BeingTalk");
        public Texture2D squareTex, TalkBubble = null;
        public double animationCounter = 0;
        public override int CombatStyle => -1;
        public int currentFrame = 0;

        public int cutsceneDuration = 120;

        public float textSpeed = 0.5f;
        public float textProgress = -0.5f;
        public int textProgressPause = 0;
        public bool DrawTalkBubble = false;
        public List<StringBundle> DialogueKillAll = [];
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
            SoundEngine.PlaySound(BeingTalk with { Volume = 1f, MaxInstances = 2 });
        }

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
        public int fadeBlackTime = 60;
        public int moveDownTime = 240;

        public static DynamicSpriteFont DotumChePixel = null;
        public static bool TalkFont
        {
            get {
                return !(DotumChePixel == null || Environment.OSVersion.Platform != PlatformID.Win32NT || !GameCulture.FromCultureName(GameCulture.CultureName.English).IsActive);
            }
        }

        public override void SetStaticDefaults()
        {
            if (DotumChePixel is null && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                DotumChePixel = ModContent.Request<DynamicSpriteFont>("TerRoguelike/Fonts/DotumChePixel", AssetRequestMode.ImmediateLoad).Value;

            }
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
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
            modNPC.drawCenter = new Vector2(0, 0);
            NPC.hide = true;
            modNPC.drawAfterEverything = true;

            TalkBubble = TexDict["TalkBubble2"];
            squareTex = TexDict["Square"];
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

            SetMusicMode(MusicStyle.Silent);
            
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;

            string dialogueString = Language.GetOrRegister("Mods.TerRoguelike.DialogueKillAll").Value;
            string username = Environment.UserName;

            StringToBundleList(dialogueString, ref DialogueKillAll);

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
        public void UpdateSpeech(List<StringBundle> jstcSpeech, bool allowControl = true, bool forceControl = false, bool keepBubbleThroughEvent = false)
        {
            var speech = jstcSpeech[(int)NPC.ai[3]];
            bool control = allowControl && (forceControl || TextControl);

            int speechLength = speech.TotalLength;
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

                                PlayTalkSound();
                            }
                        }
                    }
                }
            }


            if (control)
            {
                textProgressPause = 0;
                if (textProgress < speechLength)
                {
                    textProgress = speechLength;
                }
                else
                {
                    textProgress = -0.5f;
                    if (jstcSpeech[(int)NPC.ai[3]].Event)
                    {
                        DrawTalkBubble = keepBubbleThroughEvent;
                        eventCounter++;
                        eventTimer = 0;
                    }
                    NPC.ai[3]++;
                    if (NPC.ai[3] >= jstcSpeech.Count)
                    {
                        NPC.ai[3] = 100;
                    }
                }
            }
        }
        public override void AI()
        {
            if (textProgressPause > 0)
                textProgressPause--;
            eventTimer++;


            if (NPC.localAI[0] < -30)
            {
                for (int i = 0; i < Main.combatText.Length; i++)
                {
                    var text = Main.combatText[i];
                    text.active = false;
                }

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 60, 30, 1.25f, CutsceneSystem.CutsceneSource.Boss);
                    NPC.localAI[0]++;
                    CutsceneSystem.cutsceneDuration = cutsceneDuration + 120;
                    CutsceneSystem.cutsceneTimer = CutsceneSystem.cutsceneDuration - 61;
                }

                bool intro = NPC.ai[3] < 200;
                if (intro)
                {
                    if (CutsceneSystem.cutsceneTimer < CutsceneSystem.cutsceneDuration - 62)
                        CutsceneSystem.cutsceneTimer++;
                    introTime++;

                    if (introTime <= fadeBlackTime)
                    {
                        NPC.frameCounter = (int)(introTime / 6d) % 6;
                        CutsceneSystem.cameraTargetCenter = spawnPos;
                        CutsceneSystem.cameraTargetCenter += Vector2.UnitY * 500 * (float)Math.Pow(introTime / 60f, 1.5f);

                    }
                    else if (introTime < fadeBlackTime + moveDownTime)
                    {
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
                        if (eventCounter == 0 || eventTimer > 180)
                            UpdateSpeech(DialogueKillAll);

                        if (NPC.ai[3] == 100)
                        {
                            NPC.ai[3] = 200;
                            DrawTalkBubble = false;
                        }       
                    }
                }
                else
                {
                    NPC.localAI[0]++;
                }

                if (NPC.localAI[0] == -91)
                {
                    foreach (Player player in Main.ActivePlayers)
                    {
                        var modPlayer = player.ModPlayer();
                        if (modPlayer != null)
                            modPlayer.escapeFail = true;
                    }
                }
            }
            else
            {
                NPC.localAI[0]++;
                if (NPC.localAI[0] >= 0)
                    NPC.active = false;
            }
        }
        
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return false;
        }
        
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return false;
        }
        public override bool CanHitNPC(NPC target)
        {
            return false;
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = new Rectangle(0, 0, 1, 1);
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;

            var tex = TextureAssets.Npc[Type].Value;
            var font = TalkFont ? DotumChePixel : FontAssets.DeathText.Value;
            Color npcColor = Color.White;
            float scale = NPC.scale;
            Vector2 drawPos = NPC.Center;

            Vector2 drawOff = -Main.screenPosition;

            int cutsceneTime = (int)NPC.localAI[0] + cutsceneDuration + 120;

            float blackOpacity = 1f;
            if (introTime < fadeBlackTime)
                blackOpacity *= introTime / (float)fadeBlackTime;

            if (NPC.localAI[0] > -90)
            {
                blackOpacity *= (Math.Abs(NPC.localAI[0]) - 30) / 60f;
                if (blackOpacity < 0)
                    blackOpacity = 0;
            }


            Color squareColor = Color.Black;
            Main.EntitySpriteDraw(squareTex, Main.Camera.Center - Main.screenPosition, null, squareColor * blackOpacity, 0, squareTex.Size() * 0.5f, new Vector2(500, 300), SpriteEffects.None);


            if (DrawTalkBubble)
            {
                void DrawDialogue(List<StringBundle> list, Vector2? offset = null)
                {
                    Vector2 vector = offset ?? Vector2.Zero;
                    Main.EntitySpriteDraw(TalkBubble, drawPos + new Vector2(55, -90) + vector - Main.screenPosition, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None);

                    if ((int)NPC.ai[3] < 0 || (int)NPC.ai[3] >= list.Count)
                        return;

                    var strings = list[(int)NPC.ai[3]].StringDisplay(textProgress < 0 ? -1 : (int)textProgress);
                    int verticalStep = TalkFont ? 15 : 18;
                    for (int i = 0; i < strings.Count; i++)
                    {
                        Vector2 textScale = TalkFont ? new Vector2(0.081f) : new Vector2(0.38f);
                        float stringSize = (font.MeasureString(list[(int)NPC.ai[3]].strings[i]) * textScale).X;
                        float shrinkThreshold = 195;
                        if (stringSize > shrinkThreshold)
                        {
                            textScale.X *= shrinkThreshold / stringSize;
                        }
                        ChatManager.DrawColorCodedString(Main.spriteBatch, font, strings[i], drawPos + new Vector2(77, -82) + Vector2.UnitY * verticalStep * i + vector - Main.screenPosition, Color.Black, 0, Vector2.Zero, textScale);
                    }
                }
                DrawDialogue(DialogueKillAll, new Vector2(-160, 60));
            }

            return false;
        }
    }
}