using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Items.Uncommon;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using TerRoguelike.Utilities;
using static log4net.Appender.ColoredConsoleAppender;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class WallOfFlesh : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<WallOfFlesh>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Hell"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public bool CollisionPass = false;
        public List<ExtraHitbox> hitboxes = new List<ExtraHitbox>()
        {
            new ExtraHitbox(new Point(128, 1104), new Vector2(32, 0)),
            new ExtraHitbox(new Point(80, 80), new Vector2(-32, -28)),
            new ExtraHitbox(new Point(80, 80), new Vector2(-32, -331.2f)),
            new ExtraHitbox(new Point(80, 80), new Vector2(-32, 303.6f)),
        };
        public static readonly SoundStyle HellBeamSound = new SoundStyle("TerRoguelike/Sounds/HellBeam");
        public static readonly SoundStyle HellBeamCharge = new SoundStyle("TerRoguelike/Sounds/DeathrayCharge");
        public Texture2D squareTex;
        public Texture2D bodyTex;
        public Texture2D eyeTex;
        public Texture2D mouthTex;
        public SlotId DeathraySlot;
        public float topEyeRotation = MathHelper.Pi;
        public float bottomEyeRotation = MathHelper.Pi;
        public float mouthRotation = MathHelper.Pi;
        public bool positionsinitialized = false;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 180);
        public static Attack Laser = new Attack(1, 30, 180);
        public static Attack Deathray = new Attack(2, 30, 330);
        public int deathrayTrackedProjId1 = -1;
        public int deathrayTrackedProjId2 = -1;
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 80;
            NPC.height = 80;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -32);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.SpecialProjectileCollisionRules = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.behindTiles = true;
            squareTex = TexDict["Square"].Value;
            bodyTex = TexDict["WallOfFleshBody"].Value;
            eyeTex = TexDict["WallOfFleshEye"].Value;
            mouthTex = TexDict["WallOfFleshMouth"].Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            NPC.Center += Vector2.UnitY * NPC.height * 0.5f;
            spawnPos = NPC.Center;
            NPC.ai[2] = None.Id;
            ableToHit = false;
        }
        public override void PostAI()
        {
            if (target != null)
            {
                bool hellbeam = NPC.ai[0] == Deathray.Id && NPC.ai[1] >= 90;
                float angleLerpSpeed = 0.12f;
                float angleBound = MathHelper.PiOver2 * 0.999f;


                float angleToTarget = (target.Center - (NPC.Center + hitboxes[2].offset)).ToRotation();
                angleToTarget = MathHelper.Clamp(AngleSizeBetween(MathHelper.Pi, angleToTarget), -angleBound, angleBound) + MathHelper.Pi;
                AngleCalculation(ref topEyeRotation);

                angleToTarget = (target.Center - (NPC.Center + hitboxes[3].offset)).ToRotation();
                angleToTarget = MathHelper.Clamp(AngleSizeBetween(MathHelper.Pi, angleToTarget), -angleBound, angleBound) + MathHelper.Pi;
                AngleCalculation(ref bottomEyeRotation);

                angleToTarget = (target.Center - (NPC.Center + hitboxes[1].offset)).ToRotation();
                angleToTarget = MathHelper.Clamp(AngleSizeBetween(MathHelper.Pi, angleToTarget), -angleBound, angleBound) + MathHelper.Pi;
                AngleCalculation(ref mouthRotation);

                void AngleCalculation(ref float setAngle)
                {
                    if (hellbeam)
                    {
                        float potentialAngle = setAngle.AngleLerp(angleToTarget, 0.07f);
                        float clamp = 0.01f;
                        float addAngle = MathHelper.Clamp(AngleSizeBetween(setAngle, potentialAngle), -clamp, clamp);
                        setAngle += addAngle;
                    }
                    else
                    {
                        setAngle = setAngle.AngleLerp(angleToTarget, angleLerpSpeed);
                    }
                }
            }
            if (SoundEngine.TryGetActiveSound(DeathraySlot, out var sound) && sound.IsPlaying)
            {
                Vector2 basePos = (NPC.Center + hitboxes[1].offset);
                if (NPC.ai[0] != Deathray.Id)
                {
                    sound.Position += (basePos - (Vector2)sound.Position).SafeNormalize(Vector2.UnitY) * 1.2f;
                    if (sound.Pitch > -0.15f)
                        sound.Pitch -= 0.008f;
                    sound.Volume -= 0.008f;
                    if (sound.Volume <= 0)
                        sound.Stop();
                }
                else
                {
                    float time = (NPC.ai[1] - 90);
                    sound.Position = basePos + (Main.LocalPlayer.Center - basePos) * 0.75f * MathHelper.Clamp(time / 90, 0, 1);
                    if (time < 120)
                    {
                        sound.Pitch += 0.45f / 120;
                    }
                    else
                    {
                        sound.Pitch += 0.12f / 120;
                    }
                }
            }
        }
        public override void AI()
        {
            if (!positionsinitialized)
            {
                positionsinitialized = true;
                if (modNPC.isRoomNPC)
                {
                    Room room = RoomList[modNPC.sourceRoomListID];
                    hitboxes[0].dimensions.Y = (int)room.RoomDimensions16.Y;
                    hitboxes[1].offset.Y = -28;
                    hitboxes[2].offset.Y = hitboxes[0].dimensions.Y * -0.3f;
                    hitboxes[3].offset.Y = hitboxes[0].dimensions.Y * 0.275f;
                }
                Vector2 basePos = hitboxes[0].offset + hitboxes[0].dimensions.ToVector2() * -0.5f;
                for (int i = 0; i < hitboxes[0].dimensions.Y; i += 32)
                {
                    modNPC.ExtraIgniteTargetPoints.Add(basePos + i * Vector2.UnitY);
                }
                for (int i = 1; i < hitboxes.Count; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        modNPC.ExtraIgniteTargetPoints.Add(hitboxes[i].offset + hitboxes[i].dimensions.ToVector2() * new Vector2(-0.5f, -0.5f * j));
                    }
                }
            }
            NPC.frameCounter += 0.1d;

            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(QueenBeeTheme);
            }

            ableToHit = NPC.localAI[0] >= 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(spawnPos, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
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
            NPC.velocity *= 0.98f;

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

            if (NPC.ai[0] == Laser.Id)
            {
                if (NPC.ai[1] >= Laser.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Laser.Id;
                }
            }
            else if (NPC.ai[0] == Deathray.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(HellBeamCharge with { Volume = 0.3f, Pitch = 0.32f }, NPC.Center + hitboxes[1].offset);
                }
                if (NPC.ai[1] < 90)
                {
                    Color outlineColor = Color.Lerp(Color.LightPink, Color.OrangeRed, 0.13f);
                    Color fillColor = Color.Lerp(outlineColor, Color.DarkRed, 0.2f);
                    for (int j = -6; j <= 6; j++)
                    {
                        if (!Main.rand.NextBool(12) || j == 0)
                            continue;
                        Vector2 offset = Main.rand.NextVector2CircularEdge(16, 24);
                        Vector2 particleSpawnPos = NPC.Center + hitboxes[2].offset + topEyeRotation.ToRotationVector2() * 44 + offset.RotatedBy(topEyeRotation);
                        Vector2 particleVel = -offset.RotatedBy(topEyeRotation).SafeNormalize(Vector2.UnitX) * 0.8f;
                        ParticleManager.AddParticle(new BallOutlined(
                            particleSpawnPos, particleVel,
                            20, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.99f, 10));

                        offset = Main.rand.NextVector2CircularEdge(16, 24);
                        particleSpawnPos = NPC.Center + hitboxes[3].offset + bottomEyeRotation.ToRotationVector2() * 44 + offset.RotatedBy(bottomEyeRotation);
                        particleVel = -offset.RotatedBy(bottomEyeRotation).SafeNormalize(Vector2.UnitX) * 0.8f;
                        ParticleManager.AddParticle(new BallOutlined(
                            particleSpawnPos, particleVel,
                            20, outlineColor, fillColor, new Vector2(Main.rand.NextFloat(0.14f, 0.28f)), 4, 0, 0.99f, 10));
                    }
                }
                if (NPC.ai[1] == 90)
                {
                    DeathraySlot = SoundEngine.PlaySound(HellBeamSound with { Volume = 1f }, NPC.Center + hitboxes[1].offset);
                    deathrayTrackedProjId1 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + hitboxes[2].offset + topEyeRotation.ToRotationVector2() * 42, Vector2.Zero, ModContent.ProjectileType<HellBeam>(), NPC.damage, 0);
                    Main.projectile[deathrayTrackedProjId1].rotation = topEyeRotation;
                    deathrayTrackedProjId2 = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + hitboxes[3].offset + bottomEyeRotation.ToRotationVector2() * 42, Vector2.Zero, ModContent.ProjectileType<HellBeam>(), NPC.damage, 0);
                    Main.projectile[deathrayTrackedProjId2].rotation = bottomEyeRotation;
                }
                if (deathrayTrackedProjId1 >= 0)
                {
                    var proj = Main.projectile[deathrayTrackedProjId1];
                    if (proj.type != ModContent.ProjectileType<HellBeam>())
                    {
                        deathrayTrackedProjId1 = -1;
                    }
                    else
                    {
                        proj.Center = NPC.Center + hitboxes[2].offset + topEyeRotation.ToRotationVector2() * 42;
                        proj.rotation = topEyeRotation;
                    }
                }
                if (deathrayTrackedProjId2 >= 0)
                {
                    var proj = Main.projectile[deathrayTrackedProjId2];
                    if (proj.type != ModContent.ProjectileType<HellBeam>())
                    {
                        deathrayTrackedProjId2 = -1;
                    }
                    else
                    {
                        proj.Center = NPC.Center + hitboxes[3].offset + bottomEyeRotation.ToRotationVector2() * 42;
                        proj.rotation = bottomEyeRotation;
                    }
                }
                if (NPC.ai[1] >= Deathray.Duration)
                {
                    deathrayTrackedProjId1 = -1;
                    deathrayTrackedProjId2 = -1;
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Deathray.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Laser };
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
            chosenAttack = Deathray.Id;
            NPC.ai[0] = chosenAttack;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetBox = target.Hitbox;
            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
                if (pass)
                {
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
                if (!hitboxes[i].active)
                    continue;

                bool pass = targetBox.Intersects(hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
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
            if (CollisionPass)
            {
                npcHitbox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
            }
            return CollisionPass;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if ((projectile.hostile && !NPC.friendly) || (projectile.friendly && NPC.friendly))
                return false;

            for (int i = 0; i < hitboxes.Count; i++)
            {
                if (!hitboxes[i].active)
                    continue;

                bool pass = projectile.Colliding(projectile.getRect(), hitboxes[i].GetHitbox(NPC.Center, NPC.rotation));
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


            if (deadTime == 0)
            {
                modNPC.ignitedStacks.Clear();
                if (modNPC.isRoomNPC)
                {
                    ActiveBossTheme.endFlag = true;
                    Room room = RoomList[modNPC.sourceRoomListID];
                    room.bossDead = true;
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
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration, 30, 30, 2.5f);
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
            
        }
        public override void OnKill()
        {

        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            modifiers.HideCombatText();
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            Rectangle rect = hitboxes[0].GetHitbox(NPC.Center, 0);
            rect.Inflate(-16, -100);
            rect.X -= 16;
            rect.Y += (int)(rect.Height * 0.25f);
            CombatText.NewText(rect, hit.Crit ? CombatText.DamagedHostileCrit : CombatText.DamagedHostile, hit.Damage, hit.Crit);
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            for (int i = 0; i < hitboxes.Count; i++)
            {
                Rectangle hitbox = hitboxes[i].GetHitbox(NPC.Center, 0);
                hitbox.Inflate(15, 16);
                bool pass = hitbox.Contains(Main.MouseWorld.ToPoint());
                if (pass)
                {
                    boundingBox = new Rectangle(0, 0, Main.maxTilesX * 16, Main.maxTilesY * 16);
                    return;
                }
            }
            boundingBox = new Rectangle(0, 0, 0, 0);
        }
        public override void FindFrame(int frameHeight)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 bodyDrawStart = NPC.Center + hitboxes[0].offset + new Vector2(-16, hitboxes[0].dimensions.Y * -0.5f) + Vector2.UnitY * 16 + Vector2.UnitX * -8;
            int eyeFrameCount = 2;
            int eyeFrameHeight = eyeTex.Height / eyeFrameCount;
            int currentEyeFrame = (int)(NPC.frameCounter % eyeFrameCount);
            Rectangle eyeFrame = new Rectangle(0, currentEyeFrame * eyeFrameHeight, eyeTex.Width, eyeFrameHeight - 2);
            int mouthFrameCount = 2;
            int mouthFrameHeight = mouthTex.Height / mouthFrameCount;
            int currentMouthFrame = (int)(NPC.frameCounter % eyeFrameCount);
            Rectangle mouthFrame = new Rectangle(0, currentMouthFrame * mouthFrameHeight, mouthTex.Width, mouthFrameHeight - 2);
            SpriteEffects spriteEffects = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color bodyDrawColor;
            float offsetMagnitude = (float)Math.Sin(NPC.localAI[0] * 0.03f);
            int bodyFrameYOff = ((int)(NPC.frameCounter * 2) % 3 * (bodyTex.Height / 3)) + (int)(-10 * offsetMagnitude);

            List<StoredDraw> draws = [];

            for (int i = 0; i < hitboxes[0].dimensions.Y - 32; i += 4)
            {
                Vector2 bodyDrawPos = bodyDrawStart + Vector2.UnitY * i;
                int frameYPos = (i + bodyFrameYOff) % bodyTex.Height;
                Rectangle frame = new Rectangle(0, frameYPos, bodyTex.Width - 16, 4);
                bodyDrawColor = Lighting.GetColor(bodyDrawPos.ToTileCoordinates());
                draws.Add(new StoredDraw(bodyTex, bodyDrawPos, frame, bodyDrawColor, 0, new Vector2(frame.Size().X * 0.5f, 0), 1f, spriteEffects));
            }
            Vector2 drawPos = hitboxes[2].offset + NPC.Center - Vector2.UnitY * 4;
            draws.Add(new StoredDraw(eyeTex, drawPos, eyeFrame, Lighting.GetColor(drawPos.ToTileCoordinates()), topEyeRotation, eyeFrame.Size() * new Vector2(0.6f, 0.5f), 1f, SpriteEffects.FlipHorizontally));
            drawPos = hitboxes[3].offset + NPC.Center - Vector2.UnitY * 4;
            draws.Add(new StoredDraw(eyeTex, drawPos, eyeFrame, Lighting.GetColor(drawPos.ToTileCoordinates()), bottomEyeRotation, eyeFrame.Size() * new Vector2(0.6f, 0.5f), 1f, SpriteEffects.FlipHorizontally));
            drawPos = hitboxes[1].offset + NPC.Center;
            draws.Add(new StoredDraw(mouthTex, drawPos, mouthFrame, Lighting.GetColor(drawPos.ToTileCoordinates()), mouthRotation, mouthFrame.Size() * new Vector2(0.6f, 0.5f), 1f, SpriteEffects.FlipHorizontally));

            if (modNPC.ignitedStacks.Any())
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
                    for (int j = 0; j < 8; j++)
                    {
                        draw.Draw(-Main.screenPosition + Vector2.UnitX.RotatedBy(j * MathHelper.PiOver4 + draw.rotation) * 2);
                    }
                }

                StartVanillaSpritebatch();
            }
            for (int i = 0; i < draws.Count; i++)
            {
                var draw = draws[i];
                draw.Draw(-Main.screenPosition);
            }

            if (false)
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    if (!hitboxes[i].active)
                        continue;

                    Rectangle hitbox = hitboxes[i].GetHitbox(NPC.Center, NPC.rotation);
                    for (int d = 0; d <= 1; d++)
                    {
                        for (int x = 0; x < hitbox.Width; x++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(x, hitbox.Height * d) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                        for (int y = 0; y < hitbox.Height; y++)
                        {
                            Main.EntitySpriteDraw(squareTex, hitbox.Location.ToVector2() + new Vector2(hitbox.Width * d, y) - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                        }
                    }
                }
            }
            if (false)
            {
                for (int i = 0; i < modNPC.ExtraIgniteTargetPoints.Count; i++)
                {
                    Main.EntitySpriteDraw(squareTex, modNPC.ExtraIgniteTargetPoints[i] + NPC.Center - Main.screenPosition, null, Color.Red, 0, squareTex.Size() * 0.5f, 0.5f, SpriteEffects.None);
                }
            }
            return false;
        }
    }
    public class StoredDraw
    {
        public Texture2D texture;
        public Vector2 position;
        public Rectangle? frame;
        public Color color;
        public float rotation;
        public Vector2 origin;
        public Vector2 scale;
        public SpriteEffects spriteEffects;
        public StoredDraw(Texture2D texture, Vector2 position, Rectangle? frame, Color color, float rotation, Vector2 origin, float scale, SpriteEffects spriteEffects)
        {
            Create(texture, position, frame, color, rotation, origin, new Vector2(scale), spriteEffects);
        }
        public StoredDraw(Texture2D texture, Vector2 position, Rectangle? frame, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects)
        {
            Create(texture, position, frame, color, rotation, origin, scale, spriteEffects);
        }
        void Create(Texture2D texture, Vector2 position, Rectangle? frame, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects)
        {
            this.texture = texture;
            this.position = position;
            this.frame = frame;
            this.color = color;
            this.rotation = rotation;
            this.origin = origin;
            this.scale = scale;
            this.spriteEffects = spriteEffects;
        }
        public void Draw()
        {
            Main.EntitySpriteDraw(texture, position, frame, color, rotation, origin, scale, spriteEffects);
        }
        public void Draw(Vector2 offset)
        {
            Main.EntitySpriteDraw(texture, position + offset, frame, color, rotation, origin, scale, spriteEffects);
        }
    }
}
