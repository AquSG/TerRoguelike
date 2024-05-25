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
using static Terraria.GameContent.PlayerEyeHelper;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using static TerRoguelike.Systems.EnemyHealthBarSystem;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class TrueBrain : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<TrueBrain>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => -1;
        public int currentFrame = 0;
        public Texture2D eyeTex, innerEyeTex;
        public Vector2 eyeVector = Vector2.Zero;
        public Vector2 eyePosition { get { return new Vector2(0, -18) + modNPC.drawCenter; } }
        public Vector2 innerEyePosition { get { return new Vector2(0, -20) + modNPC.drawCenter; } }

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 120);
        public static Attack Attack1 = new Attack(1, 30, 180);
        public static Attack Attack2 = new Attack(2, 30, 180);
        public static Attack Attack3 = new Attack(3, 30, 180);
        public static Attack Attack4 = new Attack(4, 30, 180);
        public static Attack Attack5 = new Attack(5, 30, 180);
        public static Attack Summon = new Attack(6, 18, 180);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 240;
            NPC.height = 150;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 60000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            modNPC.AdaptiveArmorEnabled = true;
            modNPC.AdaptiveArmorAddRate = 50;
            innerEyeTex = TexDict["MoonLordInnerEye"];
            eyeTex = TexDict["TrueBrainEye"];
            modNPC.drawCenter = new Vector2(0, 32);
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            ableToHit = false;
        }
        public override void PostAI()
        {
            if (NPC.localAI[0] >= -(cutsceneDuration + 30))
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active)
                        continue;
                    var modPlayer = player.ModPlayer();
                    if (modPlayer == null)
                        continue;

                    modPlayer.moonLordVisualEffect = true;
                }
            }

            bool eyeCenter = false;
            float rate = 0.15f;
            if (eyeCenter)
            {
                eyeVector = Vector2.Lerp(eyeVector, Vector2.Zero, rate);
            }
            else
            {
                Vector2 targetPos = target != null ? target.Center : spawnPos + new Vector2(0, -80);
                float maxEyeOffset = 12;

                Vector2 targetVect = targetPos - (NPC.Center + innerEyePosition.RotatedBy(NPC.rotation));
                if (targetVect.Length() > maxEyeOffset)
                    targetVect = targetVect.SafeNormalize(Vector2.UnitY) * maxEyeOffset;
                eyeVector = Vector2.Lerp(eyeVector, targetVect, rate);
            }
        }
        public override void AI()
        {
            NPC.rotation = 0f;
            NPC.frameCounter += 0.12d;
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }

            ableToHit = NPC.localAI[0] >= 0 && deadTime == 0;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center + new Vector2(0, -80), cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;


                if (NPC.localAI[0] == -30)
                {
                    NPC.localAI[1] = 0;
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    enemyHealthBar = new EnemyHealthBar([NPC.whoAmI], NPC.FullName);
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
            target = modNPC.GetTarget(NPC);
            NPC.ai[1]++;

            if (NPC.ai[0] == None.Id)
            {
                if (NPC.ai[1] >= None.Duration)
                {
                    ChooseAttack();
                }
                else
                {

                }
            }

            if (NPC.ai[0] == Attack1.Id)
            {
                if (NPC.ai[1] >= Attack1.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Attack1.Id;
                }
            }
            else if (NPC.ai[0] == Attack2.Id)
            {
                if (NPC.ai[1] >= Attack2.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Attack2.Id;
                }
            }
            else if (NPC.ai[0] == Attack3.Id)
            {
                if (NPC.ai[1] >= Attack3.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Attack3.Id;
                }
            }
            else if (NPC.ai[0] == Attack4.Id)
            {
                if (NPC.ai[1] >= Attack4.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Attack4.Id;
                }
            }
            else if (NPC.ai[0] == Attack5.Id)
            {
                if (NPC.ai[1] >= Attack5.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Attack5.Id;
                }
            }
            else if (NPC.ai[0] == Summon.Id)
            {
                if (NPC.ai[1] >= Summon.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Summon.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Attack1, Attack2, Attack3, Attack4, Attack5, Summon };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);

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
            return canBeHit ? null : false;
        }
        public override bool CheckDead()
        {
            if (deadTime >= deathCutsceneDuration - 30)
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
                ExtraSoundSystem.ForceStopAllExtraSounds();
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();

                if (modNPC.isRoomNPC)
                {
                    if (ActiveBossTheme != null)
                        ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
                    ClearChildren();
                }
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
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

            if (deadTime >= deathCutsceneDuration - 30)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }

            return deadTime >= cutsceneDuration - 30;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage * 0.01d; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Vortex, hit.HitDirection, -1f, 0, default, 0.9f);
                    Main.dust[d].noLight = true;
                    Main.dust[d].noLightEmittence = true;
                }
            }
        }
        public override void OnKill()
        {
            
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override void FindFrame(int frameHeight)
        {
            var tex = TextureAssets.Npc[Type].Value;

            currentFrame = (int)NPC.frameCounter % (Main.npcFrameCount[Type] - 1) + 1;
            NPC.frame = new Rectangle(0, frameHeight * currentFrame, tex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var tex = TextureAssets.Npc[Type].Value;
            Color npcColor = Color.White;
            Vector2 scale = new Vector2(NPC.scale);

            List<StoredDraw> draws = [];

            draws.Add(new(tex, NPC.Center + modNPC.drawCenter.RotatedBy(NPC.rotation), NPC.frame, npcColor, NPC.rotation, NPC.frame.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(eyeTex, NPC.Center + eyePosition.RotatedBy(NPC.rotation), null, npcColor, NPC.rotation, eyeTex.Size() * 0.5f, scale, SpriteEffects.None));
            draws.Add(new(innerEyeTex, NPC.Center + innerEyePosition.RotatedBy(NPC.rotation) + eyeVector * new Vector2(0.35f, 1f), null, npcColor, 0, innerEyeTex.Size() * 0.5f, scale, SpriteEffects.None));

            Vector2 drawOff = -Main.screenPosition;

            if (modNPC.ignitedStacks.Count > 0)
            {
                StartAlphaBlendSpritebatch();

                Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                Vector3 colorHSL = Main.rgbToHsl(color);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                for (int i = 0; i < draws.Count; i++)
                {
                    var draw = draws[i];
                    if (draw.texture.Width < 100) // every small texture here is covered up by a bigger texture. no point in wasting time drawing ignite textures for things that would have no effect
                        continue;
                    for (int j = 0; j < 8; j++)
                    {
                        draw.Draw(drawOff + Vector2.UnitX.RotatedBy(j * MathHelper.PiOver4 + draw.rotation) * 2);
                    }
                }
                StartVanillaSpritebatch();

            }
            
            for (int i = 0; i < draws.Count; i++)
            {
                draws[i].Draw(drawOff);
            }

            return false;
        }
    }
}
