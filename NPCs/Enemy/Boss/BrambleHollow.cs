using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using TerRoguelike.Projectiles;
using TerRoguelike.Systems;
using static TerRoguelike.Managers.TextureManager;
using static TerRoguelike.Schematics.SchematicManager;
using static TerRoguelike.Systems.MusicSystem;
using static TerRoguelike.Systems.RoomSystem;
using static TerRoguelike.Utilities.TerRoguelikeUtils;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class BrambleHollow : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<BrambleHollow>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Forest"] };
        public override int CombatStyle => -1;
        public int currentFrame;
        public int horizontalFrame;
        public Texture2D lightTex;
        public Texture2D ballTex;
        public int deadTime = 0;
        public int cutsceneDuration = 180;
        public int deathCutsceneDuration = 150;
        public SoundStyle BramblePunch = new SoundStyle("TerRoguelike/Sounds/BramblePunch");

        public Attack None = new Attack(0, 0, 180);
        public Attack Burrow = new Attack(1, 40, 360);
        public Attack VineWall = new Attack(2, 30, 240);
        public Attack RootLift = new Attack(3, 40, 180);
        public Attack SeedBarrage = new Attack(4, 40, 150);
        public Attack Summon = new Attack(5, 20, 150);
        public int ballAttack1;
        public int ballAttack2;
        public Vector2 ballAttack1Pos;
        public Vector2 ballAttack2Pos;
        public int rootAttack1;
        public int rootAttack2;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[modNPCID] = 10;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 128;
            NPC.height = 128;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            lightTex = TexDict["BrambleHollowGlow"];
            ballTex = TexDict["LeafBall"];
            NPC.behindTiles = true;
            NPC.noGravity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //NPC ai 3 is used for the current cardinal direction the npc is.
            NPC.ai[3] = 0;

            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            currentFrame = 0;
            horizontalFrame = 0;
            NPC.localAI[0] = -(cutsceneDuration + 30);
            NPC.direction = -1;
            NPC.spriteDirection = -1;
            spawnPos = NPC.Center;
            NPC.ai[2] = Burrow.Id;
        }
        public override void AI()
        {
            if (deadTime > 0)
            {
                CheckDead();
                return;
            }
            if (modNPC.isRoomNPC && NPC.localAI[0] == -(cutsceneDuration + 30))
            {
                SetBossTrack(PaladinTheme);
            }

            ableToHit = !(NPC.ai[0] == Burrow.Id && NPC.ai[1] > Burrow.Duration * 0.5f);
            canBeHit = !(NPC.ai[0] == Burrow.Id && Math.Abs(NPC.ai[1] - (Burrow.Duration * 0.5f)) < 60);

            NPC.velocity += new Vector2(0, 0.1f).RotatedBy(MathHelper.PiOver2 * NPC.ai[3]);

            if (NPC.localAI[0] < 0)
            {
                spawnPos = NPC.Center;
                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;
                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
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
            target = modNPC.GetTarget(NPC, false, false);

            NPC.ai[1]++;
            NPC.frameCounter += 0.18d;

            ballAttack1 = 60;
            ballAttack2 = 120;
            ballAttack1Pos = NPC.Center + modNPC.drawCenter + new Vector2(100, 0).RotatedBy(NPC.rotation);
            ballAttack2Pos = NPC.Center + modNPC.drawCenter + new Vector2(-100, 0).RotatedBy(NPC.rotation);
            rootAttack1 = 90;
            rootAttack2 = 150;


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

            if (NPC.ai[0] == Burrow.Id)
            {
                if (NPC.ai[1] < 80 || NPC.ai[1] > (int)(Burrow.Duration * 0.5f) + 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 pos = new Vector2(Main.rand.NextFloat(-NPC.width * 0.5f, NPC.width * 0.5f), NPC.height * 0.5f).RotatedBy(NPC.rotation) + NPC.Center;
                        Dust d = Dust.NewDustPerfect(pos, DustID.WoodFurniture, null, 0, default, 1);
                    }
                }
                if (NPC.ai[1] == (int)(Burrow.Duration * 0.5f))
                {
                    NPC.velocity = Vector2.Zero;
                    List<int> directions = new List<int>() { -1, 0, 1, 2} ;
                    directions.RemoveAll(x => x == (int)NPC.ai[3]);
                    int newDir = directions[Main.rand.Next(directions.Count)];
                    NPC.ai[3] = newDir;
                    if (newDir % 2 != 0)
                    {
                        newDir *= -1;
                        Room room = RoomList[modNPC.sourceRoomListID];
                        Vector2 checkPos = room.RoomPosition16 + room.RoomCenter16 + (new Vector2(((room.RoomDimensions16.X * 0.5f) - 16) * newDir, room.RoomDimensions16.Y * 0.17f));
                        Point tilePos = checkPos.ToTileCoordinates();
                        if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true))
                        {
                            for (int i = 1; i < 25; i++)
                            {
                                if (!ParanoidTileRetrieval(tilePos.X + (i  * -newDir), tilePos.Y).IsTileSolidGround(true))
                                {
                                    tilePos.X += (i * -newDir);
                                    break;
                                }
                            }
                        }
                        checkPos = tilePos.ToVector2() * 16f;
                        if (newDir == 1)
                        {
                            checkPos.X += 16;
                            NPC.Right = checkPos;
                        }
                        else
                        {
                            NPC.Left = checkPos;
                        }
                    }
                    else
                    {
                        newDir -= 1;
                        newDir *= -1;
                        Room room = RoomList[modNPC.sourceRoomListID];
                        Vector2 checkPos = room.RoomPosition16 + room.RoomCenter16 + ((new Vector2(0, (room.RoomDimensions16.Y * 0.5f) - 16) * newDir));
                        Point tilePos = checkPos.ToTileCoordinates();
                        if (ParanoidTileRetrieval(tilePos.X, tilePos.Y).IsTileSolidGround(true))
                        {
                            for (int i = 1; i < 25; i++)
                            {
                                if (!ParanoidTileRetrieval(tilePos.X, tilePos.Y + (i * -newDir)).IsTileSolidGround(true))
                                {
                                    tilePos.Y += (i * -newDir);
                                    break;
                                }
                            }
                        }
                        checkPos = tilePos.ToVector2() * 16f;
                        if (newDir == 1)
                        {
                            checkPos.Y += 16;
                            NPC.Bottom = checkPos;
                        }
                        else
                        {
                            NPC.Top = checkPos;
                        }
                            
                    }
                }
                if (NPC.ai[1] >= Burrow.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = None.Duration - 60;
                    NPC.ai[2] = Burrow.Id;
                }
            }
            else if (NPC.ai[0] == VineWall.Id)
            {
                float attackHorizSpeedMulti = (int)NPC.ai[3] % 2 == 0 ? 3f : 0.6f;
                if (NPC.ai[1] == 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDust(ballAttack1Pos, 1, 1, DustID.Grass);
                        Dust.NewDust(ballAttack2Pos, 1, 1, DustID.Grass);
                    }
                    SoundEngine.PlaySound(SoundID.Item76 with { Volume = 1f }, NPC.Center);
                }
                else if (NPC.ai[1] == ballAttack1)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), ballAttack1Pos, new Vector2(attackHorizSpeedMulti, -6).RotatedBy(NPC.rotation), ModContent.ProjectileType<LeafBall>(), NPC.damage, 0f, -1, NPC.ai[3]);
                }
                else if (NPC.ai[1] == ballAttack2)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), ballAttack2Pos, new Vector2(-attackHorizSpeedMulti, -6).RotatedBy(NPC.rotation), ModContent.ProjectileType<LeafBall>(), NPC.damage, 0f, -1, NPC.ai[3]);
                }
                else if (NPC.ai[1] >= VineWall.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = VineWall.Id;
                }
            }
            else if (NPC.ai[0] == RootLift.Id)
            {
                if (NPC.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.DeerclopsScream with { Volume = 0.4f, Pitch = -0.7f }, NPC.Center);
                }
                if (NPC.ai[1] == rootAttack1 - 35 || NPC.ai[1] == rootAttack2 - 35)
                {
                    SoundEngine.PlaySound(BramblePunch with { Volume = 0.7f, MaxInstances = 2 }, NPC.Center);
                }
                if (NPC.ai[1] == rootAttack1 || NPC.ai[1] == rootAttack2)
                {
                    float rotateBy = (NPC.ai[3] * MathHelper.PiOver2) + MathHelper.Pi;
                    Vector2 checkDirection = new Vector2(0, -16).RotatedBy(rotateBy);
                    Vector2 targetPos = target != null ? target.Center : NPC.Center + new Vector2(NPC.ai[1] == rootAttack1 ? 320f : -320f, 0);
                    for (int i = 0; i < 100; i++)
                    {
                        Vector2 offsetTargetPos = (targetPos + (checkDirection * i));
                        Point targetTilePos = offsetTargetPos.ToTileCoordinates();
                        if (ParanoidTileRetrieval(targetTilePos.X, targetTilePos.Y).IsTileSolidGround(true))
                        {
                            float ai0 = (int)NPC.ai[3] % 2 != 0 ? -NPC.ai[3] : (NPC.ai[3] == 2 ? 0 : 2);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), targetTilePos.ToWorldCoordinates() + new Vector2(0, -32).RotatedBy(rotateBy), new Vector2(0, 12).RotatedBy(rotateBy), ModContent.ProjectileType<RootPillar>(), NPC.damage, 0f, -1, ai0);
                            break;
                        }
                    }
                }
                else if (NPC.ai[1] >= RootLift.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = RootLift.Id;
                }
            }
            else if (NPC.ai[0] == SeedBarrage.Id)
            {
                if (NPC.ai[1] >= SeedBarrage.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = SeedBarrage.Id;
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
            //List<Attack> potentialAttacks = new List<Attack>() { Burrow, VineWall, RootLift, SeedBarrage, Summon };
            List<Attack> potentialAttacks = new List<Attack>() { Burrow, RootLift };
            potentialAttacks.RemoveAll(x => x.Id == (int)NPC.ai[2]);
            if (!modNPC.isRoomNPC)
            {
                potentialAttacks.RemoveAll(x => x.Id == Burrow.Id);
            }

            int totalWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
            {
                totalWeight += potentialAttacks[i].Weight;
            }
            int chosenRandom = Main.rand.Next(totalWeight);
            int chosenAttack = 0;
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
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return projectile.Colliding(projectile.getRect(), ModifiedHitbox()) && canBeHit ? null : false;
        }
        public override bool? CanBeHitByItem(Player player, Item item)
        {
            return item.Hitbox.Intersects(ModifiedHitbox()) && canBeHit ? null : false;
        }
        public override bool CanBeHitByNPC(NPC attacker)
        {
            return true;
        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            npcHitbox = ModifiedHitbox();
            
            return false;
        }
        public Rectangle ModifiedHitbox()
        {
            if (NPC.ai[0] != Burrow.Id)
                return NPC.Hitbox;
            Rectangle npcHitbox = NPC.Hitbox;
            int lesser = npcHitbox.Width < npcHitbox.Height ? npcHitbox.Width : npcHitbox.Height;
            int offset = (int)GetOffset();
            if (offset > lesser)
                offset = lesser;

            if (NPC.ai[3] == 0)
            {
                npcHitbox.Y += offset;
                npcHitbox.Height -= offset;
            }
            else if (NPC.ai[3] == 2)
            {
                npcHitbox.Height -= offset;
            }
            else if (NPC.ai[3] == 1)
            {
                npcHitbox.Width -= offset;
            }
            else if (NPC.ai[3] == -1)
            {
                npcHitbox.Width -= -offset;
                npcHitbox.X += offset;
            }
            return npcHitbox;
        }
        public override bool CheckDead()
        {
            if (deadTime >= deathCutsceneDuration)
            {
                return true;
            }
            NPC.ai[0] = None.Id;
            NPC.ai[1] = 1;

            modNPC.OverrideIgniteVisual = true;
            NPC.velocity *= 0;
            NPC.life = 1;
            NPC.immortal = true;
            NPC.dontTakeDamage = true;
            NPC.active = true;
            if (deadTime == 0)
            {
                CutsceneSystem.SetCutscene(NPC.Center, deathCutsceneDuration + 30, 30, 30, 2.5f);
                if (modNPC.isRoomNPC)
                {
                    ActiveBossTheme.endFlag = true;
                    RoomList[modNPC.sourceRoomListID].bossDead = true;
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
            }
            deadTime++;

            if (deadTime >= deathCutsceneDuration)
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
                NPC.StrikeInstantKill();
            }
                
            return deadTime >= cutsceneDuration;
        }
        public override void OnKill()
        {
            //SoundEngine.PlaySound(SoundID.NPCDeath12 with { Volume = 0.8f, Pitch = -0.5f }, NPC.Center);
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.rotation = MathHelper.PiOver2 * NPC.ai[3];
            modNPC.drawCenter = (NPC.Bottom - NPC.Center + new Vector2(0, (-frameHeight * 0.5f) + 2)).RotatedBy(NPC.rotation);

            int frameCount = Main.npcFrameCount[Type];
            int frameWidth = TextureAssets.Npc[Type].Value.Width / 2;
            currentFrame = (int)NPC.frameCounter % (frameCount - 5);
            horizontalFrame = 0;

            if (NPC.ai[0] == Burrow.Id)
                currentFrame += 5;
            else if (NPC.ai[0] == VineWall.Id && NPC.ai[1] < ballAttack2 + 48)
            {
                horizontalFrame = 1;
                if (NPC.ai[1] < ballAttack1)
                    currentFrame = 0;
                else if (NPC.ai[1] < ballAttack1 + 12)
                    currentFrame = 1;
                else if (NPC.ai[1] < ballAttack1 + 24)
                    currentFrame = 2;
                else if (NPC.ai[1] < ballAttack2)
                    currentFrame = 0;
                else if (NPC.ai[1] < ballAttack2 + 12)
                    currentFrame = 3;
                else if (NPC.ai[1] < ballAttack2 + 24)
                    currentFrame = 4;
                else
                    currentFrame = 0;

                NPC.frameCounter = 3;
            }

            NPC.frame = new Rectangle(horizontalFrame * frameWidth, currentFrame * frameHeight, frameWidth, frameHeight);

            if (NPC.ai[0] == Burrow.Id)
            {
                float offset = GetOffset();
                int rectCutoff = (int)offset - 40;
                if (rectCutoff > NPC.frame.Height)
                    rectCutoff = NPC.frame.Height;
                if (rectCutoff > 0)
                {
                    NPC.frame.Height -= rectCutoff;
                }

                
                modNPC.drawCenter += (Vector2.UnitY * offset * 0.5f).RotatedBy(NPC.rotation);
            }
        }
        public float GetOffset()
        {
            int halfTime = (int)(Burrow.Duration * 0.5f);
            float maxOffset = 240;
            float offset;
            if (NPC.ai[1] < halfTime)
            {
                offset = MathHelper.SmoothStep(0, maxOffset, NPC.ai[1] / halfTime);
            }
            else
            {
                offset = MathHelper.SmoothStep(maxOffset, 0, (NPC.ai[1] - halfTime) / halfTime);
            }
            return offset;
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position += modNPC.drawCenter + new Vector2(0, 28);
            if (NPC.ai[0] == Burrow.Id)
            {
                float halfTime = (Burrow.Duration * 0.5f);
                scale = MathHelper.Clamp(MathHelper.SmoothStep(0, 1f, Math.Abs((NPC.ai[1] - halfTime) / (halfTime))), 0, 1f);
            }
            return true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            if ((int)NPC.ai[0] == VineWall.Id && NPC.ai[1] < 120)
            {
                float interpolant = MathHelper.Clamp(MathHelper.SmoothStep(0, 1f, NPC.ai[1] / ballAttack1), 0, 1f);
                float offset = -32f * (1f - interpolant);
                if (NPC.ai[1] < ballAttack1)
                {
                    Main.EntitySpriteDraw(ballTex, ballAttack1Pos + new Vector2(0, offset).RotatedBy(NPC.rotation) - Main.screenPosition, null, Color.White * interpolant, NPC.rotation, ballTex.Size() * 0.5f, 1f, SpriteEffects.None);
                }
                Main.EntitySpriteDraw(ballTex, ballAttack2Pos + new Vector2(0, offset).RotatedBy(NPC.rotation) - Main.screenPosition, null, Color.White * interpolant, NPC.rotation, ballTex.Size() * 0.5f, 1f, SpriteEffects.None);
            }

            Vector2 drawPos = NPC.Center + modNPC.drawCenter;
            float glowOpacity = 0.8f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly) * 0.2f);
            Main.EntitySpriteDraw(tex, drawPos - Main.screenPosition, NPC.frame, Lighting.GetColor(drawPos.ToTileCoordinates()), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            Main.EntitySpriteDraw(lightTex, drawPos - Main.screenPosition, NPC.frame, Color.White * glowOpacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection > 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
