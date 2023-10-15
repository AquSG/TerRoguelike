using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Items.Weapons;
using TerRoguelike.Managers;
using TerRoguelike.NPCs;
using TerRoguelike.Projectiles;
using TerRoguelike.Utilities;
using TerRoguelike.World;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using ReLogic.Content;
using TerRoguelike.Systems;

namespace TerRoguelike.TerPlayer
{
    public class TerRoguelikePlayer : ModPlayer
    {
        #region Item Variables
        public int coolantCanister;
        public int clingyGrenade;
        public int pocketSpotter;
        public int antiqueLens;
        public int instigatorsBrace;
        public int hotPepper;
        public int brazenNunchucks;
        public int attackPlan;
        public int sanguineOrb;
        public int livingCrystal;
        public int soulstealCoating;
        public int bottleOfVigor;
        public int benignFungus;
        public int sentientPutty;
        public int memoryFoam;
        public int runningShoe;
        public int bunnyHopper;
        public int timesHaveBeenTougher;
        public int rustedShield;
        public int amberBead;
        public int flimsyPauldron;
        public int protectiveBubble;
        public int lockOnMissile;
        public int evilEye;
        public int spentShell;
        public int heatSeekingChip;
        public int backupDagger;
        public int clusterBombSatchel;
        public int retaliatoryFist;
        public int bloodSiphon;
        public int enchantingEye;
        public int automaticDefibrillator;
        public int stimPack;
        public int barbedLasso;
        public int steamEngine;
        public int bouncyBall;
        public int airCanister;
        public int unencumberingStone;
        public int ballAndChain;
        public int soulOfLena;
        public int emeraldRing;
        public int amberRing;
        public int thrillOfTheHunt;
        public int giftBox;
        public int volatileRocket;
        public int theDreamsoul;
        public int droneBuddy;
        public int cornucopia;
        public int nutritiousSlime;
        public int itemPotentiometer;
        public int barrierSynthesizer;
        public int jetLeg;
        public List<int> evilEyeStacks = new List<int>();
        public List<int> thrillOfTheHuntStacks = new List<int>();
        public int benignFungusCooldown = 0;
        public int storedDaggers = 0;
        public int soulOfLenaUses = 0;
        public bool soulOfLenaHurtVisual = false;
        public List<int> barbedLassoTargets = new List<int>();
        public int barbedLassoHitCooldown = 0;
        public List<int> steamEngineStacks = new List<int>();
        public int droneBuddyState = 0; // 0 - passive, 1 - aggressive, 2 - healing
        public float droneBuddyAttackCooldown = 0;
        public int droneTarget = -1;
        public int droneBuddyHealTime = 0;
        #endregion

        #region Misc Variables
        public Floor currentFloor;
        public int shotsToFire = 1;
        public int extraDoubleJumps = 0;
        public int timesDoubleJumped = 0;
        public float jumpSpeedMultiplier;
        public float scaleMultiplier;
        public int procLuck = 0;
        public float swingAnimCompletion = 0;
        public int bladeFlashTime = 0;
        public Vector2 playerToCursor = Vector2.Zero;
        public float barrierHealth = 0;
        public bool barrierFullAbsorbHit = false;
        public int barrierInHurt = 0;
        public float barrierFloor = 0;
        public int outOfDangerTime = 600;
        public bool dodgeAttack = false;
        public float healMultiplier = 1f;
        public float diminishingDR = 0f;
        public bool onGround = false;
        public float previousBonusDamageMulti = 1f;
        public int timeAttacking = 0;
        public Vector2 lenaVisualPosition = Vector2.Zero;
        public Vector2 droneBuddyVisualPosition = Vector2.Zero;
        public float droneBuddyRotation = 0f;
        public float droneSeenRot = 0f;
        public int currentRoom = -1;
        public int DashDir = 0;
        public int DashDelay = 0;
        public int DashTime = -6;
        public int DashDirCache = 0;
        #endregion

        #region Reset Variables
        public override void PreUpdate()
        {
            coolantCanister = 0;
            clingyGrenade = 0;
            pocketSpotter = 0;
            antiqueLens = 0;
            instigatorsBrace = 0;
            hotPepper = 0;
            brazenNunchucks = 0;
            attackPlan = 0;
            sanguineOrb = 0;
            livingCrystal = 0;
            soulstealCoating = 0;
            bottleOfVigor = 0;
            benignFungus = 0;
            sentientPutty = 0;
            memoryFoam = 0;
            runningShoe = 0;
            bunnyHopper = 0;
            timesHaveBeenTougher = 0;
            rustedShield = 0;
            amberBead = 0;
            flimsyPauldron = 0;
            protectiveBubble = 0;
            lockOnMissile = 0;
            evilEye = 0;
            spentShell = 0;
            heatSeekingChip = 0;
            backupDagger = 0;
            clusterBombSatchel = 0;
            retaliatoryFist = 0;
            bloodSiphon = 0;
            enchantingEye = 0;
            automaticDefibrillator = 0;
            stimPack = 0;
            barbedLasso = 0;
            steamEngine = 0;
            bouncyBall = 0;
            airCanister = 0;
            unencumberingStone = 0;
            ballAndChain = 0;
            soulOfLena = 0;
            emeraldRing = 0;
            amberRing = 0;
            thrillOfTheHunt = 0;
            giftBox = 0;
            volatileRocket = 0;
            theDreamsoul = 0;
            droneBuddy = 0;
            cornucopia = 0;
            nutritiousSlime = 0;
            itemPotentiometer = 0;
            barrierSynthesizer = 0;
            jetLeg = 0;
            shotsToFire = 1;
            jumpSpeedMultiplier = 0f;
            extraDoubleJumps = 0;
            procLuck = 0;
            scaleMultiplier = 1f;
            healMultiplier = 1f;
            diminishingDR = 0f;
            previousBonusDamageMulti = 1f;

            onGround = (ParanoidTileRetrieval((int)(Player.Bottom.X / 16f), (int)((Player.Bottom.Y) / 16f)).IsTileSolidGround() && Math.Abs(Player.velocity.Y) <= 0.1f);
            barrierFloor = 0;
            barrierFullAbsorbHit = false;
            barrierInHurt = 0;
        }
        public override void ResetEffects()
        {
            if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[2] < 15)
            {
                DashDir = 1;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[3] < 15)
            {
                DashDir = -1;
            }
            else
            {
                DashDir = 0;
            }

        }
        #endregion

        #region Always Active Effects
        public override void UpdateEquips()
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                Player.noFallDmg = true;
                Player.noKnockback = true;
                Player.GetCritChance(DamageClass.Generic) -= 3f;
                Player.hasMagiluminescence = true;
                Player.GetJumpState(ExtraJump.CloudInABottle).Enable();
                Player.lifeRegen += 4;
                Player.lifeRegenTime = 0;
                if (Player.controlDown)
                {
                    Player.gravity *= 1.5f;
                }
            }
            outOfDangerTime++;

            if (Player.itemAnimation > 0 && Player.HeldItem.damage > 0)
            {
                if (timeAttacking < 0)
                    timeAttacking = 0;

                timeAttacking++;
            }
            else
            {
                timeAttacking--;

                if (timeAttacking > 0)
                    timeAttacking = 0;
            }
                
            //max life effects happen before barrier calculations
            if (bottleOfVigor > 0)
            {
                int maxLifeIncrease = bottleOfVigor * 10;
                Player.statLifeMax2 += maxLifeIncrease;
            }
            if (steamEngine > 0)
            {
                int maxLifeIncrease = steamEngine * steamEngineStacks.Count;
                Player.statLifeMax2 += maxLifeIncrease;
                for (int i = 0; i < steamEngineStacks.Count; i++)
                {
                    steamEngineStacks[i]--;
                }
                steamEngineStacks.RemoveAll(x => x <= 0);
            }
            else if (steamEngineStacks.Any())
            {
                for (int i = 0; i < steamEngineStacks.Count; i++)
                {
                    steamEngineStacks[i]--;
                }
                steamEngineStacks.RemoveAll(x => x <= 0);
            }

            //barrier calculations happen now
            if (rustedShield > 0)
            {
                float addedBarrier = 0.08f * Player.statLifeMax2 * rustedShield;
                barrierFloor += addedBarrier;
            }
            if (barrierFloor > Player.statLifeMax2)
                barrierFloor = Player.statLifeMax2;

            if (barrierHealth < barrierFloor && outOfDangerTime >= 420 && barrierFloor != 0)
            {
                barrierHealth += barrierFloor * 0.0083334f;
            }
            if (barrierHealth > barrierFloor)
            {
                barrierHealth -= Player.statLifeMax2 * (0.04f * 0.0166f);
                if (barrierHealth < barrierFloor)
                    barrierHealth = barrierFloor;
            }

            //everything else
            if (coolantCanister > 0)
            {
                float attackSpeedIncrease = coolantCanister * 0.10f;
                Player.GetAttackSpeed(DamageClass.Generic) += attackSpeedIncrease;
            }
            if (pocketSpotter > 0)
            {
                float critIncrease = pocketSpotter * 10f;
                Player.GetCritChance(DamageClass.Generic) += critIncrease;
            }
            if (antiqueLens > 0)
            {
                float scaleIncrease = antiqueLens * 0.1f;
                scaleMultiplier += scaleIncrease;
            }
            if (livingCrystal > 0)
            {
                int regenIncrease = livingCrystal * 4;
                Player.lifeRegen += regenIncrease;
            }
            if (benignFungusCooldown > 0)
                benignFungusCooldown--;
            if (benignFungus > 0 && benignFungusCooldown == 0 && Math.Abs(Player.velocity.X) > 2.5f && onGround)
            {
                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Bottom + new Vector2(0, -5f), Vector2.Zero, ModContent.ProjectileType<HealingFungus>(), 0, 0f, Player.whoAmI);
                benignFungusCooldown += Main.rand.Next(13, 16);
            }
            if (sentientPutty > 0 && outOfDangerTime == 120)
            {
                int healAmt = sentientPutty * 10;
                SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/OrbHeal", 5) { Volume = 0.12f }, Player.Center);
                ScaleableHeal(healAmt);
            }
            if (memoryFoam > 0 && outOfDangerTime >= 420)
            {
                int regenIncrease = memoryFoam * 8;
                Player.lifeRegen += regenIncrease;
            }
            if (runningShoe > 0)
            {
                float speedIncrease = runningShoe * 0.08f;
                Player.moveSpeed += speedIncrease;
            }
            if (bunnyHopper > 0)
            {
                float jumpSpeedIncrease = bunnyHopper * 0.12f;
                jumpSpeedMultiplier += jumpSpeedIncrease;
            }
            if (protectiveBubble > 0 && outOfDangerTime >= 420)
            {
                float drIncrease = protectiveBubble * 50f;
                diminishingDR += drIncrease;
            }

            if (evilEye > 0)
            {
                Player.GetCritChance(DamageClass.Generic) += 5;
                if (evilEyeStacks.Any())
                {
                    for (int i = 0; i < evilEyeStacks.Count; i++)
                    {
                        evilEyeStacks[i]--;
                    }

                    evilEyeStacks.RemoveAll(time => time <= 0);

                    Player.GetAttackSpeed(DamageClass.Generic) += MathHelper.Clamp(evilEyeStacks.Count(), 1, 4 + evilEye) * 0.1f * (float)evilEye;
                }
            }
            else if (evilEyeStacks.Any())
                evilEyeStacks.Clear();

            if (enchantingEye > 0)
            {
                Player.GetCritChance(DamageClass.Generic) += 5;
            }
            if (spentShell > 0)
            {
                shotsToFire += spentShell;
            }
            if (backupDagger > 0)
            {
                int maxStocks = 3 + backupDagger;
                int stockRate = (int)(360f / (float)maxStocks);
                if (storedDaggers > maxStocks)
                {
                    storedDaggers = maxStocks;
                    SoundEngine.PlaySound(SoundID.Item7, Player.Center);
                }
                    
                int releaseCooldown = (int)(stockRate / 2f);
                if (releaseCooldown > 8)
                    releaseCooldown = 8;
                if (releaseCooldown < 1)
                    releaseCooldown = 1;

                if (timeAttacking > 0)
                {
                    if (stockRate < 1)
                        stockRate = 1;

                    if (timeAttacking % stockRate == 0 && storedDaggers < maxStocks)
                    {
                        storedDaggers++;
                        if (storedDaggers == maxStocks)
                            SoundEngine.PlaySound(SoundID.Item63 with { Volume = 0.8f }, Player.Center);
                        else
                            SoundEngine.PlaySound(SoundID.Item64 with { Volume = 0.4f }, Player.Center);
                    }
                }
                else if (Math.Abs(timeAttacking) % releaseCooldown == 0 && storedDaggers > 0)
                {
                    Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX) * 2.2f, ModContent.ProjectileType<ThrownBackupDagger>(), 100, 1f, Player.whoAmI, -1f);
                    storedDaggers--;
                }
            }
            else
                storedDaggers = 0;

            if (stimPack > 0)
            {
                if (currentRoom != -1)
                {
                    if (RoomSystem.RoomList[currentRoom].roomTime <= 600)
                    {
                        int lifeRegenIncrease = stimPack * 16;
                        Player.lifeRegen += lifeRegenIncrease;
                    }
                }
            }

            if (barbedLasso > 0)
            {
                barbedLassoTargets.Clear();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.life <= 0 || npc.immortal || npc.dontTakeDamage)
                        continue;

                    float requiredDistance = 128f;
                    float npcDistance = Player.Center.Distance(npc.getRect().ClosestPointInRect(Player.Center));
                    if (npcDistance <= requiredDistance)
                    {
                        barbedLassoTargets.Add(i);
                        if (barbedLassoTargets.Count == barbedLasso)
                            break;
                    }
                }

                if (barbedLassoHitCooldown <= 0 && barbedLassoTargets.Any())
                {
                    int totalHealAmt = 0;
                    for (int i = 0; i < barbedLassoTargets.Count; i++)
                    {
                        NPC target = Main.npc[barbedLassoTargets[i]];
                        int hitDamage = (int)(Player.statLifeMax2 * 0.02f);
                        hitDamage = (int)(hitDamage * GetBonusDamageMulti(target, target.getRect().ClosestPointInRect(Player.Center)));

                        if (target.life - hitDamage <= 0)
                        {
                            OnKillEffects(target);
                        }
                        NPC.HitInfo info = new NPC.HitInfo();
                        info.HideCombatText = true;
                        info.Damage = hitDamage;
                        info.InstantKill = false;
                        info.HitDirection = 1;
                        info.Knockback = 0f;
                        info.Crit = false;

                        target.StrikeNPC(info);
                        NetMessage.SendStrikeNPC(target, info);
                        CombatText.NewText(target.getRect(), Color.DarkGreen, hitDamage);

                        totalHealAmt += hitDamage;
                    }

                    ScaleableHeal(totalHealAmt);

                    barbedLassoHitCooldown += 30;
                }
                if (barbedLassoHitCooldown > 0)
                    barbedLassoHitCooldown--;
            }
            else
            {
                barbedLassoTargets.Clear();
                if (barbedLassoHitCooldown > 0)
                    barbedLassoHitCooldown--;
            }

            if (airCanister > 0)
            {
                extraDoubleJumps += airCanister;
            }
            if (unencumberingStone > 0 && timeAttacking <= -300)
            {
                float speedIncrease = unencumberingStone * 0.25f;
                Player.moveSpeed += speedIncrease;
            }
            if (emeraldRing > 0 && timeAttacking > 0)
            {
                if (Player.HeldItem.CountsAsClass(DamageClass.Melee))
                {
                    int drIncrease = emeraldRing * 20;
                    diminishingDR += drIncrease;
                }
            }
            if (thrillOfTheHunt > 0)
            {
                if (thrillOfTheHuntStacks.Any())
                {
                    for (int i = 0; i < thrillOfTheHuntStacks.Count; i++)
                    {
                        thrillOfTheHuntStacks[i]--;
                    }

                    thrillOfTheHuntStacks.RemoveAll(time => time <= 0);

                    Player.GetAttackSpeed(DamageClass.Generic) += MathHelper.Clamp(thrillOfTheHuntStacks.Count(), 1, 4 + thrillOfTheHunt) * 0.15f * (float)thrillOfTheHunt;
                    Player.moveSpeed += MathHelper.Clamp(thrillOfTheHuntStacks.Count(), 1, 4 + thrillOfTheHunt) * 0.15f;
                }
            }
            else if (thrillOfTheHuntStacks.Any())
                thrillOfTheHuntStacks.Clear();

            if (theDreamsoul > 0)
            {
                int luckIncrease = theDreamsoul;
                procLuck += luckIncrease;
            }
            if (droneBuddy > 0)
            {
                if (droneBuddyState != 2 && droneBuddyHealTime < 600)
                    droneBuddyHealTime++;

                if (droneTarget != -1 && droneBuddyAttackCooldown <= 0)
                {
                    droneBuddyRotation = droneBuddyRotation % MathHelper.TwoPi;
                    NPC npc = Main.npc[droneTarget];
                    if (!npc.active || npc.life <= 0 || npc.Center.Distance(Player.Center) > 960f || !Collision.CanHit(droneBuddyVisualPosition, 1, 1, Main.npc[droneTarget].Center, 1, 1))
                        droneTarget = -1;
                    else
                        droneBuddyAttackCooldown += 20f / (1f + ((droneBuddy - 1) * 0.5f));
                }
                if (droneTarget == -1)
                {
                    float bestTargetDistance = 960f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || npc.life <= 0 || !npc.CanBeChasedBy())
                            continue;

                        if (!Collision.CanHit(droneBuddyVisualPosition, 1, 1, Main.npc[i].Center, 1, 1))
                            continue;

                        float distance = npc.Center.Distance(Player.Center);
                        if (distance <= bestTargetDistance)
                        {
                            bestTargetDistance = distance;
                            droneTarget = i;
                            droneBuddyAttackCooldown = 20;
                        }
                    }
                }
                if (droneTarget != -1)
                {
                    droneBuddyState = 1;
                    droneBuddyAttackCooldown--;
                    if (droneBuddyAttackCooldown <= 0)
                    {
                        Vector2 projSpawnPos = droneBuddyVisualPosition + (Vector2.UnitY.RotatedBy(droneSeenRot) * 11f * new Vector2(1.2f, 1));
                        droneBuddyAttackCooldown = 0;
                        int spawnedProjectile = Projectile.NewProjectile(Player.GetSource_FromThis(), projSpawnPos, (Main.npc[droneTarget].Center - projSpawnPos).SafeNormalize(Vector2.UnitY) * 1.5f, ModContent.ProjectileType<AdaptiveGunBullet>(), 100, 1f, Player.whoAmI);
                        Main.projectile[spawnedProjectile].scale = scaleMultiplier;
                        SoundEngine.PlaySound(SoundID.Item108 with { Volume = 0.3f }, droneBuddyVisualPosition);
                        Dust dust = Dust.NewDustDirect(projSpawnPos - new Vector2(2), 4, 4, DustID.YellowTorch);
                        dust.noLight = true;
                        dust.noLightEmittence = true;
                    }
                }
                else
                {
                    if (droneBuddyState != 2)
                        droneBuddyState = 0;
                    if (droneBuddyHealTime >= 600)
                    {
                        droneBuddyState = 2;
                    }
                    if (droneBuddyState == 2)
                    {
                        if (droneBuddyHealTime % 15 == 0)
                        {
                            int healAmt = 2 * droneBuddy;
                            ScaleableHeal(healAmt);
                        }
                        droneBuddyHealTime -= 2;
                        if (droneBuddyHealTime <= 0)
                            droneBuddyState = 0;
                    }
                }
                    
            }
            else
            {
                droneBuddyHealTime = 0;
            }
        }
        public override void PostUpdateEquips()
        {
            if (spentShell > 0)
            {
                float finalAttackSpeedMultiplier = 1f;
                for (int i = 0; i < spentShell; i++)
                {
                    finalAttackSpeedMultiplier *= 1 - (1f / (2f + (float)spentShell));
                }
                Player.GetAttackSpeed(DamageClass.Generic) *= finalAttackSpeedMultiplier;
            }
            if (!Player.GetJumpState(ExtraJump.CloudInABottle).Available && timesDoubleJumped < extraDoubleJumps)
            {
                Player.GetJumpState(ExtraJump.CloudInABottle).Available = true;
                timesDoubleJumped++;
            }
            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                Player.moveSpeed *= 1.30f;
                Player.maxRunSpeed *= 1.30f;
            }
            if (cornucopia > 0)
            {
                float healMultiIncrease = (float)cornucopia;
                healMultiplier += healMultiIncrease;
                Player.lifeRegen *= (int)(1f + healMultiIncrease);
            }

            Player.jumpSpeedBoost += 5f * jumpSpeedMultiplier;

            if (barrierHealth > 0)
            {
                if (barrierHealth > Player.statLifeMax2)
                    barrierHealth = Player.statLifeMax2;
            }
            if (barrierHealth < 0)
                barrierHealth = 0;
        }
        public override void PreUpdateMovement()
        {
            if (jetLeg > 0)
            {
                if (DashDir != 0 && DashDelay == 0)
                {
                    DashDirCache = DashDir;
                    SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot with { Volume = 0.7f });
                    Player.immuneTime += 7;
                    DashTime += 15;
                    int dashDelayAdd = (int)(600 * 4 / (float)(jetLeg + 3));
                    if (dashDelayAdd < 30)
                        dashDelayAdd = 30;

                    DashDelay += dashDelayAdd;
                }
                if (DashTime > 0)
                {
                    JetLegDashMovement();
                    DashDelay++;
                }
            }

            if (DashTime > -6)
                DashTime--;
            if (DashDelay > 0)
            {
                DashDelay--;
                if (DashDelay == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.42f });
                }
            }

            if (DashTime <= -6)
            {
                DashDirCache = 0;
            }
        }
        #endregion

        #region On Hit Enemy
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TerRoguelikeGlobalProjectile modProj = proj.GetGlobalProjectile<TerRoguelikeGlobalProjectile>();
            TerRoguelikeGlobalNPC modNPC = target.GetGlobalNPC<TerRoguelikeGlobalNPC>();

            if (previousBonusDamageMulti != 1f)
                hit.Damage = (int)(hit.Damage / previousBonusDamageMulti);

            if (target.life <= 0)
                OnKillEffects(target);

            if (clingyGrenade > 0 && !modProj.procChainBools.clinglyGrenadePreviously)
            {
                float chance;
                chance = clingyGrenade * 0.05f;
                if (ChanceRollWithLuck(chance, procLuck))
                {
                    float radius;
                    if (target.width < target.height)
                        radius = (float)target.width;
                    else
                        radius = (float)target.height;

                    radius *= 0.4f;

                    Vector2 direction = (proj.Center - target.Center).SafeNormalize(Vector2.UnitY);
                    Vector2 spawnPosition = (direction * radius) + target.Center;
                    int damage = (int)(hit.Damage * 1.5f);
                    if (hit.Crit)
                        damage /= 2;

                    int spawnedProjectile = Projectile.NewProjectile(proj.GetSource_FromThis(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<StuckClingyGrenade>(), damage, 0f, proj.owner, target.whoAmI);
                    TerRoguelikeGlobalProjectile spawnedModProj = Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>();

                    spawnedModProj.procChainBools = new ProcChainBools(modProj.procChainBools);
                    spawnedModProj.procChainBools.originalHit = false;
                    spawnedModProj.procChainBools.clinglyGrenadePreviously = true;
                    if (hit.Crit)
                        spawnedModProj.procChainBools.critPreviously = true;
                }
            }
            if (sanguineOrb > 0)
            {
                float chance = 0.08f * sanguineOrb;
                if (ChanceRollWithLuck(chance, procLuck))
                {
                    modNPC.AddBleedingStackWithRefresh(new BleedingStack(240, Player.whoAmI));
                }
            }
            if (lockOnMissile > 0 && !modProj.procChainBools.lockOnMissilePreviously)
            {
                float chance = 0.1f;
                if (ChanceRollWithLuck(chance, procLuck))
                {
                    Vector2 spawnPosition = Main.player[proj.owner].Top;
                    Vector2 direction = -Vector2.UnitY;
                    int damage = (int)(hit.Damage * 3f * lockOnMissile);
                    if (hit.Crit)
                        damage /= 2;

                    int spawnedProjectile = Projectile.NewProjectile(proj.GetSource_FromThis(), spawnPosition, direction * 2.2f, ModContent.ProjectileType<Missile>(), damage, 0f, proj.owner, target.whoAmI);
                    TerRoguelikeGlobalProjectile spawnedModProj = Main.projectile[spawnedProjectile].GetGlobalProjectile<TerRoguelikeGlobalProjectile>();

                    spawnedModProj.procChainBools = new ProcChainBools(modProj.procChainBools);
                    spawnedModProj.procChainBools.originalHit = false;
                    spawnedModProj.procChainBools.lockOnMissilePreviously = true;
                    if (hit.Crit)
                        spawnedModProj.procChainBools.critPreviously = true;
                }
            }
            if (evilEye > 0 && hit.Crit)
            {
                if (evilEyeStacks == null)
                    evilEyeStacks = new List<int>();

                evilEyeStacks.Add(180);
            }
            if (bloodSiphon > 0)
            {
                int healAmt = bloodSiphon;
                ScaleableHeal(healAmt);
            }
            if (enchantingEye > 0 && hit.Crit)
            {
                int healAmt = enchantingEye * 8;
                ScaleableHeal(healAmt);
            }
            if (ballAndChain > 0)
            {
                int slowTime = 120 * ballAndChain;
                if (modNPC.ballAndChainSlow < slowTime)
                    modNPC.ballAndChainSlow = slowTime;
            }
            if (amberRing > 0 && proj.DamageType == DamageClass.Melee)
            {
                int barrierGainAmt = 5 * amberRing;
                AddBarrierHealth(barrierGainAmt);
            }

            if (proj.type == ModContent.ProjectileType<ThrownBackupDagger>())
            {
                if (ChanceRollWithLuck(0.5f, procLuck))
                {
                    modNPC.AddBleedingStackWithRefresh(new BleedingStack(120, Player.whoAmI));
                }
                
            }
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= GetBonusDamageMulti(target, target.getRect().ClosestPointInRect(proj.Center), proj);
        }
        public float GetBonusDamageMulti(NPC npc, Vector2 hitPosition, Projectile? projectile = null)
        {
            float bonusDamageMultiplier = 1f;
            if (instigatorsBrace > 0 && (npc.life / (float)npc.lifeMax) >= 0.9f)
            {
                float bonusDamage = 0.75f * instigatorsBrace;
                bonusDamageMultiplier *= 1 + bonusDamage;
            }
            if (brazenNunchucks > 0 && Vector2.Distance(Player.Center, hitPosition) <= 128f)
            {
                float bonusDamage = 0.2f * brazenNunchucks;
                bonusDamageMultiplier *= 1 + bonusDamage;
            }
            previousBonusDamageMulti = bonusDamageMultiplier;
            return bonusDamageMultiplier;
        }
        #endregion

        #region On Kill Enemy
        public void OnKillEffects(NPC target)
        {
            TerRoguelikeGlobalNPC modTarget = target.GetGlobalNPC<TerRoguelikeGlobalNPC>();

            if (hotPepper > 0 && !modTarget.activatedHotPepper)
            {
                float radius = 128f + (48f * (hotPepper - 1));
                int damageToDeal = 300 + (75 * (hotPepper - 1));
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc == null || !npc.active)
                        continue;

                    if (npc.Center.Distance(target.Center) <= radius)
                    {
                        npc.GetGlobalNPC<TerRoguelikeGlobalNPC>().ignitedStacks.Add(new IgnitedStack(damageToDeal, Player.whoAmI));
                    }
                }
                modTarget.activatedHotPepper = true;
            }
            if (soulstealCoating > 0 && !modTarget.activatedSoulstealCoating)
            {
                Projectile.NewProjectile(Projectile.GetSource_None(), target.Center, Vector2.Zero, ModContent.ProjectileType<SoulstealHealingOrb>(), 0, 0f, Player.whoAmI);
                modTarget.activatedSoulstealCoating = true;
            }
            if (amberBead > 0 && !modTarget.activatedAmberBead)
            {
                int barrierGainAmt = amberBead * 15;
                modTarget.activatedAmberBead = true;
                AddBarrierHealth(barrierGainAmt);
            }
            if (thrillOfTheHunt > 0 && !modTarget.activatedThrillOfTheHunt)
            {
                if (thrillOfTheHuntStacks == null)
                    thrillOfTheHuntStacks = new List<int>();

                thrillOfTheHuntStacks.Add(480);
                modTarget.activatedThrillOfTheHunt = true;
            }
            if (clusterBombSatchel > 0 && !modTarget.activatedClusterBombSatchel)
            {
                int damage = 350 + (150 * clusterBombSatchel);
                float scale = 1f + (0.12f * (clusterBombSatchel - 1));
                if (scale > 15f)
                    scale = 15f;
                int spawnedProj = Projectile.NewProjectile(Projectile.GetSource_None(), target.Center, Vector2.Zero, ModContent.ProjectileType<ClusterHandler>(), damage, 0f, Player.whoAmI);
                Main.projectile[spawnedProj].scale = scale;
                modTarget.activatedClusterBombSatchel = true;
            }
            if (steamEngine > 0 && !modTarget.activatedSteamEngine)
            {
                steamEngineStacks.Add(3600);
                modTarget.activatedSteamEngine = true;
            }
            if (nutritiousSlime > 0 && !modTarget.activatedNutritiousSlime)
            {
                int slimeCount = 4;
                for (int i = 0; i < slimeCount; i++)
                {
                    Vector2 velocity = (-Vector2.UnitY * 3f).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 3f, (MathHelper.Pi / 3f) + float.Epsilon));
                    Projectile.NewProjectile(Player.GetSource_FromThis(), target.Center, velocity, ModContent.ProjectileType<SlimeGlob>(), 0, 0f, Player.whoAmI);
                }
                modTarget.activatedNutritiousSlime = true;
            }
            if (itemPotentiometer > 0 && !modTarget.activatedItemPotentiometer)
            {
                float chance = 0.02f * itemPotentiometer;
                if (ChanceRollWithLuck(chance, procLuck))
                {
                    SpawnRoguelikeItem(target.Center);
                }
                modTarget.activatedItemPotentiometer = true;
            }
        }
        #endregion

        #region Player Hurt
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (dodgeAttack)
            {
                dodgeAttack = false;
                return true;
            }

            if (barrierHealth > 1 && info.Damage <= (int)barrierHealth && !barrierFullAbsorbHit)
            {
                BarrierHitEffect(info.Damage, info.Damage);
                return true;
            }

            return false;
        }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (timesHaveBeenTougher > 0)
            {
                float chance = (0.15f * timesHaveBeenTougher) / (0.15f * timesHaveBeenTougher + 1);

                if (ChanceRollWithLuck(chance, procLuck))
                {
                    SoundEngine.PlaySound(new SoundStyle("TerRoguelike/Sounds/Squeak", 3) with { Volume = 0.075f }, Player.Center);
                    CombatText.NewText(Player.getRect(), Color.LightGray, "blocked!");
                    Player.immuneTime += 40;
                    Player.immune = true;
                    dodgeAttack = true;
                    return;
                }
            }

            if (diminishingDR != 0f)
            {
                if (diminishingDR > 0)
                    modifiers.SourceDamage *= (100f / (100f + diminishingDR));
                else
                    modifiers.SourceDamage *= 2 - (100f / (100f - diminishingDR));
            }

            modifiers.ModifyHurtInfo += ModifyHurtInfo_TerRoguelike;
        }
        private void ModifyHurtInfo_TerRoguelike(ref Player.HurtInfo info)
        {
            if (flimsyPauldron > 0)
            {
                int reductedDamage = 5 * flimsyPauldron;
                info.Damage -= reductedDamage;
            }
            if (barrierHealth > 1 && info.Damage > (int)barrierHealth)
            {
                int preBarrierDamage = info.Damage;
                info.Damage -= (int)barrierHealth;
                barrierFullAbsorbHit = true;
                BarrierHitEffect(info.Damage + (int)barrierHealth, preBarrierDamage);
            }
        }
        public void BarrierHitEffect(int damageToBarrier, int fullHitDamage)
        {
            barrierInHurt = damageToBarrier;
            CombatText.NewText(Player.getRect(), Color.Gold, damageToBarrier > (int)barrierHealth ? -(int)barrierHealth : -damageToBarrier);
            SoundStyle soundStyle = damageToBarrier < (int)barrierHealth ? SoundID.NPCHit53 with { Volume = 0.5f } : SoundID.NPCDeath56 with { Volume = 0.3f };
            SoundEngine.PlaySound(soundStyle, Player.Center);
            barrierHealth -= damageToBarrier;
            Player.immuneTime += fullHitDamage == 1 ? 20 : 40;
            Player.immune = true;
            HurtEffects(fullHitDamage);
            
        }
        public void HurtEffects(int damage)
        {
            if (protectiveBubble > 0 && outOfDangerTime >= 420)
            {
                SoundEngine.PlaySound(SoundID.Item87 with { Volume = 0.75f }, Player.Center);
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(Player.MountedCenter + new Vector2(-16, -16), 32, 32, DustID.BlueTorch);
                }
            }
            if (retaliatoryFist > 0)
            {
                bool playSound = false;
                float requiredDistance = 160f + (32f * retaliatoryFist);
                int projSpawnCount = 0;
                int projMaxSpawnCount = 1 + (retaliatoryFist * 2);
                int fistDamage = 125 + (retaliatoryFist * 125);
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (projSpawnCount >= projMaxSpawnCount)
                        break;

                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.life <= 0 || npc.friendly)
                        continue;

                    if (Player.Center.Distance(npc.getRect().ClosestPointInRect(Player.Center)) <= requiredDistance)
                    {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), npc.Center + new Vector2(0, -96), Vector2.Zero, ModContent.ProjectileType<MagicFist>(), fistDamage, 0f, Player.whoAmI, i);
                        playSound = true;
                        projSpawnCount++;
                    }
                }
                if (playSound)
                    SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.5f }, Player.Center);
            }
            outOfDangerTime = 0;
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            if (barrierInHurt <= 0)
                HurtEffects(info.Damage);
        }
        public override void PostHurt(Player.HurtInfo info)
        {
            if (soulOfLena > 0)
            {
                if (soulOfLenaUses < soulOfLena && Player.statLife / (float)Player.statLifeMax2 <= 0.25f)
                {
                    Player.immuneTime += 300;
                    Player.immuneNoBlink = true;
                    Player.immune = true;
                    soulOfLenaUses++;
                    soulOfLenaHurtVisual = true;
                    SoundEngine.PlaySound(SoundID.NPCHit36 with { Volume = 0.25f }, Player.Center);
                }
            }
        }
        #endregion

        #region Mechanical Functions
        public void ScaleableHeal(int healAmt)
        {
            healAmt = (int)(healAmt * healMultiplier);
            if (barrierSynthesizer > 0)
            {
                if (Player.statLife + healAmt > Player.statLifeMax2)
                {
                    int barrierHealAmt = (int)((Player.statLife + healAmt - Player.statLifeMax2) * barrierSynthesizer * 0.5f);
                    AddBarrierHealth(barrierHealAmt);
                }
            }
            Player.Heal(healAmt);
        }
        public void AddBarrierHealth(int barrierGainAmt)
        {
            barrierHealth += barrierGainAmt;
            if (barrierHealth > Player.statLifeMax2)
                barrierHealth = Player.statLifeMax2;

            CombatText.NewText(Player.getRect(), Color.LightGoldenrodYellow, barrierGainAmt);
        }
        public void GiftBoxLogic(Vector2 position)
        {
            float commonWeight = 0.8f;
            float uncommonWeight = 0.18f * (float)giftBox;
            float rareWeight = 0.02f * (float)Math.Pow(giftBox, 2);
            float chance = Main.rand.NextFloat(commonWeight + uncommonWeight + rareWeight + float.Epsilon);
            int itemType;
            int itemTier;
            if (chance <= commonWeight)
            {
                itemType = ItemManager.GiveCommon(false);
                itemTier = 0;
            }
            else if (chance <= commonWeight + uncommonWeight)
            {
                itemType = ItemManager.GiveUncommon(false);
                itemTier = 1;
            }
            else
            {
                itemType = ItemManager.GiveRare(false);
                itemTier = 2;
            }

            SpawnManager.SpawnItem(itemType, position, itemTier, 105, 0.5f);
        }
        public void SpawnRoguelikeItem(Vector2 position)
        {
            int chance = Main.rand.Next(1, 101);
            int itemType;
            int itemTier;

            if (chance <= 80)
            {
                itemType = ItemManager.GiveCommon(false);
                itemTier = 0;
            }
            else if (chance <= 98)
            {
                itemType = ItemManager.GiveUncommon(false);
                itemTier = 1;
            }
            else
            {
                itemType = ItemManager.GiveRare(false);
                itemTier = 2;
            }

            position = FindAirToPlayer(position);

            SpawnManager.SpawnItem(itemType, position, itemTier, 75, 0.5f);
        }
        public Vector2 FindAirToPlayer(Vector2 position)
        {
            Point tilePos = position.ToTileCoordinates();
            Tile tile = ParanoidTileRetrieval(tilePos.X, tilePos.Y);
            if (tile != null)
            {
                if (!tile.HasTile)
                {
                    return position;
                }
            }

            float distanceReCheckMove = 22.627f;
            Vector2 direction = Player.Center - position;
            if (direction.Length() >= distanceReCheckMove)
            {
                position += direction.SafeNormalize(Vector2.UnitY) * distanceReCheckMove;
                return FindAirToPlayer(position);
            }

            return Player.Center;
        }

        public override void OnExtraJumpRefreshed(ExtraJump jump)
        {
            timesDoubleJumped = 0;
        }
        public override void OnEnterWorld()
        {
            if (TerRoguelikeWorld.IsTerRoguelikeWorld)
            {
                if (Player.armor[3].type == ItemID.CreativeWings)
                    Player.armor[3] = new Item();
            }
        }
        #endregion

        #region Edit Starting Items
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            static Item createItem(int type)
            {
                Item i = new Item();
                i.SetDefaults(type);
                return i;
            }

            IEnumerable<Item> items = new List<Item>()
            {
                createItem(ModContent.ItemType<AdaptiveGun>()),
                createItem(ModContent.ItemType<AdaptiveBlade>())
            };

            return items;
        }

        public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
        {
            itemsByMod["Terraria"].Clear();
        }
        #endregion

        #region Draw Effects
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            float closestNPCDistance = -1f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc == null || !npc.active)
                    continue;

                if (closestNPCDistance == -1f)
                {
                    closestNPCDistance = Vector2.Distance(Player.Center, npc.Center);
                }
                else if (Vector2.Distance(Player.Center, npc.Center) < closestNPCDistance)
                {
                    closestNPCDistance = Vector2.Distance(Player.Center, npc.Center);
                }
            }
            if (closestNPCDistance == -1)
            {
                closestNPCDistance = 10000f;
            }

            if (bladeFlashTime > 0)
            {
                drawInfo.heldItem.color = Color.Lerp(Color.White, Color.Cyan, (float)bladeFlashTime / 23f);
                bladeFlashTime--;
            }

            if (brazenNunchucks > 0)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D telegraphBase = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/InvisibleProj").Value;

                float scale = 1.64f;
                float opacity = MathHelper.Lerp(0.08f, 0f, (closestNPCDistance - 128f) / 128f);

                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseOpacity(opacity);
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseColor(Color.Lerp(Color.Orange, Color.SandyBrown, 0.5f) * 0.85f);
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseSecondaryColor(Color.SandyBrown * 0.75f);
                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].UseSaturation(scale);

                GameShaders.Misc["TerRoguelike:CircularGradientWithEdge"].Apply();

                Vector2 drawPosition = Player.Center - Main.screenPosition;
                Main.EntitySpriteDraw(telegraphBase, drawPosition, null, Color.White, 0, telegraphBase.Size() / 2f, scale * 156f, 0, 0);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }
            
            if (protectiveBubble > 0 && outOfDangerTime >= 400)
            {
                Main.spriteBatch.End();
                Effect shieldEffect = Filters.Scene["TerRoguelike:ProtectiveBubbleShield"].GetShader().Shader;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);

                // If in vanity, the shield is always projected as if it's at full strength.
                float shieldStrength = 1f;

                // Noise scale also grows and shrinks, although out of sync with the shield
                float noiseScale = MathHelper.Lerp(0.4f, 0.8f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.3f) * 0.5f + 0.5f);

                // Define shader parameters
                
                shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.24f);
                shieldEffect.Parameters["blowUpPower"].SetValue(2.5f);
                shieldEffect.Parameters["blowUpSize"].SetValue(0.5f);
                shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

                // Shield opacity multiplier slightly changes, this is independent of current shield strength
                float baseShieldOpacity = 0.2f + 0.05f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f);
                shieldEffect.Parameters["shieldOpacity"].SetValue(baseShieldOpacity * (0.5f + 0.5f * shieldStrength));
                shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

                Color edgeColor;
                Color shieldColor;

                float opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, (float)Math.Log2(Math.Pow((outOfDangerTime - 400) * 0.9f, 2)) + 6f) / 20f, 0f, 1f);
                Color blueTint = new Color(51, 102, 255);
                Color cyanTint = new Color(71, 202, 255);
                Color wulfGreen = new Color(100, 180, 220) * 0.8f;
                edgeColor = MulticolorLerp(Main.GlobalTimeWrappedHourly * 0.2f, blueTint, cyanTint, wulfGreen) * 0.5f * opacity;
                shieldColor = blueTint * 0.125f * opacity;

                // Define shader parameters for shield color
                shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
                shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

                // Fetch shield noise overlay texture (this is the techy overlay fed to the shader)
                Asset<Texture2D> NoiseTex = ModContent.Request<Texture2D>("TerRoguelike/Shaders/OverheadWaves");
                Vector2 pos = Player.MountedCenter + Player.gfxOffY * Vector2.UnitY - Main.screenPosition;
                Texture2D tex = NoiseTex.Value;

                float scale = 0.115f * opacity;
                Main.spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }

            if (soulOfLena > 0)
            {
                bool hurtCheck = Player.immuneTime > 0 && soulOfLenaHurtVisual;
                if (hurtCheck)
                {
                    r = 0.4f;
                    g = 0.8f;
                    b = 1f;
                    a = 0.5f;
                    if (Player.immuneTime <= 1)
                    {
                        soulOfLenaHurtVisual = false;
                    }
                    Lighting.AddLight(Player.Center, 0f, 0.24f, 0.36f);
                }
                if (soulOfLenaUses < soulOfLena || soulOfLenaHurtVisual)
                {
                    Vector2 desiredPos = Player.Top - new Vector2(32 * Player.direction, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 6f) * 8f + 12f);
                    Texture2D lenaTex = ModContent.Request<Texture2D>("TerRoguelike/TerPlayer/Lena").Value;
                    Texture2D circleTex = ModContent.Request<Texture2D>("TerRoguelike/TerPlayer/LenaGlow").Value;
                    int lenaFrame = hurtCheck ? 2 : (Main.GlobalTimeWrappedHourly % 1.1f >= 0.55f ? 1 : 0);
                    int frameHeight = lenaTex.Height / 3;
                    SpriteEffects spriteEffects = Player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    Color color = soulOfLenaHurtVisual && soulOfLenaUses == soulOfLena ? Color.White * MathHelper.Lerp(0, 1f, Player.immuneTime / 300f) : Color.White;
                    if (lenaVisualPosition == Vector2.Zero)
                    {
                        lenaVisualPosition = desiredPos;
                    }
                    else
                    {
                        lenaVisualPosition = Vector2.Lerp(lenaVisualPosition, desiredPos, 0.02f);
                    }
                    Main.EntitySpriteDraw(circleTex, lenaVisualPosition - Main.screenPosition, null, color.MultiplyRGB(Color.Cyan) * 0.15f, 0f, circleTex.Size() * 0.5f, 0.75f, SpriteEffects.None);
                    Main.EntitySpriteDraw(lenaTex, lenaVisualPosition - Main.screenPosition, new Rectangle(0, frameHeight * lenaFrame, lenaTex.Width, frameHeight), color, 0f, new Vector2(lenaTex.Width, frameHeight) * 0.5f, 1f, spriteEffects);
                }
                else
                    lenaVisualPosition = Vector2.Zero;
            }
            else
                lenaVisualPosition = Vector2.Zero;

            if (barbedLasso > 0)
            {
                if (barbedLassoTargets.Any())
                {
                    for (int i = 0; i < barbedLassoTargets.Count; i++)
                    {
                        NPC npc = Main.npc[barbedLassoTargets[i]];

                        Vector2 distanceVector = npc.Center - Player.MountedCenter;
                        for (int d = 0; d < 1; d++)
                        {
                            Vector2 dustPosition = distanceVector.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1f, distanceVector.Length());
                            Dust dust = Dust.NewDustPerfect(Player.MountedCenter + dustPosition, DustID.GreenTorch);
                            dust.noGravity = true;
                            dust.noLight = true;
                            dust.noLightEmittence = true;
                        }
                    }
                }
            }

            if (jetLeg > 0)
            {
                if (DashDirCache != 0 && DashTime > -6)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Dust.NewDustDirect(Player.Bottom + new Vector2((-8 - (i * Player.velocity.X * 0.5f)) * DashDirCache, -10), 4, 8, DustID.Torch, SpeedX: (-4f - i) * DashDirCache);
                    }
                }
            }

            if (droneBuddy > 0)
            {
                Vector2 desiredPos = droneBuddyState != 1 ? Player.Center - new Vector2(48 * Player.direction, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 6f) * 8f) : Player.Center + ((Main.npc[droneTarget].Center - Player.Center).SafeNormalize(Vector2.UnitY) * 48);
                Texture2D droneTex = ModContent.Request<Texture2D>("TerRoguelike/TerPlayer/DroneBuddyMinion").Value;
                float fullAnimTime = Main.GlobalTimeWrappedHourly % 0.5f;
                int droneFrame = fullAnimTime % 0.25f <= 0.125f ? 1 : (fullAnimTime <= 0.25f ? 0 : 2);
                int faceFrame = droneBuddyState == 1 ? 5 : (Main.GlobalTimeWrappedHourly % 9f <= 0.4f ? 4 : 3);
                int frameHeight = droneTex.Height / 6;
                SpriteEffects spriteEffects = droneBuddyState != 1 ? (droneBuddyVisualPosition.X > Player.Center.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None) : (Main.npc[droneTarget].Center.X >= droneBuddyVisualPosition.X ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                float idleRotation = MathHelper.Clamp(droneBuddyVisualPosition.Distance(desiredPos) / 500f, -MathHelper.PiOver4, MathHelper.PiOver4) * (spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1);
                float desiredRot = droneBuddyState != 1 ? idleRotation + (spriteEffects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0) : (Main.npc[droneTarget].Center - droneBuddyVisualPosition).ToRotation();
                Color color = Color.White;
                
                if (Math.Abs(droneBuddyRotation - desiredRot) > MathHelper.PiOver2 * 3f)
                {
                    if (droneBuddyRotation < desiredRot)
                        droneBuddyRotation += MathHelper.TwoPi;
                    else if (desiredRot < droneBuddyRotation)
                        desiredRot += MathHelper.TwoPi;
                }
                if (droneBuddyVisualPosition == Vector2.Zero)
                {
                    droneBuddyVisualPosition = desiredPos;
                    droneBuddyRotation = 0f;
                }
                else
                {
                    droneBuddyVisualPosition = Vector2.Lerp(droneBuddyVisualPosition, desiredPos, 0.03f);
                    droneBuddyRotation = MathHelper.Lerp(droneBuddyRotation, desiredRot, 0.08f);
                    
                }
                droneSeenRot = spriteEffects == SpriteEffects.FlipHorizontally ? droneBuddyRotation - MathHelper.Pi : droneBuddyRotation;
                Main.EntitySpriteDraw(droneTex, droneBuddyVisualPosition - Main.screenPosition, new Rectangle(0, frameHeight * droneFrame, droneTex.Width, frameHeight), color, droneSeenRot, new Vector2(droneTex.Width, frameHeight) * 0.5f, 1f, spriteEffects);
                Main.EntitySpriteDraw(droneTex, droneBuddyVisualPosition - Main.screenPosition, new Rectangle(0, frameHeight * faceFrame, droneTex.Width, frameHeight), color, droneSeenRot, new Vector2(droneTex.Width, frameHeight) * 0.5f, 1f, spriteEffects);

                if (droneBuddyState == 2)
                {
                    float opacity = MathHelper.Clamp(MathHelper.Lerp(0f, 1f, (droneBuddyHealTime) / 30f), 0f, 1f);
                    Texture2D squareTex = ModContent.Request<Texture2D>("TerRoguelike/Projectiles/AdaptiveGunBullet").Value;
                    Vector2 projSpawnPos = droneBuddyVisualPosition + (new Vector2(0.4f * (droneBuddyVisualPosition.X > Player.Center.X ? -1 : 1), 1f).RotatedBy(droneSeenRot) * 11f * new Vector2(1.2f, 1));
                    for (int i = 0; i < 15; i++)
                    {
                        float randFloat = (i / 15f) + (Main.GlobalTimeWrappedHourly % 0.1f);
                        Vector2 pointOnLine = (Player.MountedCenter + new Vector2(0, Player.gfxOffY) - projSpawnPos) * randFloat;
                        pointOnLine.Y += (-1 * (float)Math.Pow(randFloat * 2f - 1, 2) + 1) * 32f;
                        pointOnLine += projSpawnPos;
                        if (pointOnLine.Distance(Player.MountedCenter) < 20f)
                            continue;

                        Main.EntitySpriteDraw(squareTex, pointOnLine - Main.screenPosition, null, Color.LimeGreen * opacity, Main.rand.NextFloatDirection(), squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }
            else
                droneBuddyVisualPosition = Vector2.Zero;

            return;

            if (evilEye > 0)
                EvilEyePlayerEffect();

            if (enchantingEye > 0)
                EnchantingEyePlayerEffect();
        }
        #endregion

        #region Item Drawing on Player
        public void EvilEyePlayerEffect()
        {
            int num = 0;
            num += Player.bodyFrame.Y / 56;
            if (num >= Main.OffsetsPlayerHeadgear.Length)
            {
                num = 0;
            }
            Vector2 vector = Main.OffsetsPlayerHeadgear[num];
            vector *= Player.Directions;
            Vector2 vector2 = new Vector2((float)(Player.width / 2), (float)(Player.height / 2)) + vector + (Player.MountedCenter - base.Player.Center);
            Player.sitting.GetSittingOffsetInfo(Player, out var posOffset, out var seatAdjustment);
            vector2 += posOffset + new Vector2(0f, seatAdjustment);
            if (Player.face == 19)
            {
                vector2.Y -= 5f * Player.gravDir;
            }
            if (Player.head == 276)
            {
                vector2.X += 2.5f * (float)Player.direction;
            }
            if (Player.mount.Active && Player.mount.Type == 52)
            {
                vector2.X += 14f * (float)Player.direction;
                vector2.Y -= 2f * Player.gravDir;
            }
            float y = -11.5f * Player.gravDir;
            int eyeDistance = Player.direction == 1 ? 7 : 3;
            Vector2 vector3 = new Vector2((float)(eyeDistance * Player.direction - ((Player.direction == 1) ? 1 : 0)), y) + Vector2.UnitY * Player.gfxOffY + vector2;
            Vector2 vector4 = new Vector2((float)(eyeDistance * Player.shadowDirection[1] - ((Player.direction == 1) ? 1 : 0)), y) + vector2;
            Vector2 vector5 = Vector2.Zero;
            if (Player.mount.Active && Player.mount.Cart)
            {
                int num2 = Math.Sign(Player.velocity.X);
                if (num2 == 0)
                {
                    num2 = Player.direction;
                }
                vector5 = Utils.RotatedBy(new Vector2(MathHelper.Lerp(0f, -8f, Player.fullRotation / ((float)Math.PI / 4f)), MathHelper.Lerp(0f, 2f, Math.Abs(Player.fullRotation / ((float)Math.PI / 4f)))), (double)Player.fullRotation, default(Vector2));
                if (num2 == Math.Sign(Player.fullRotation))
                {
                    vector5 *= MathHelper.Lerp(1f, 0.6f, Math.Abs(Player.fullRotation / ((float)Math.PI / 4f)));
                }
            }
            if (Player.fullRotation != 0f)
            {
                vector3 = vector3.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
                vector4 = vector4.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
            }
            float num3 = 0f;
            Vector2 vector6 = Player.position + vector3 + vector5;
            Vector2 vector7 = Player.oldPosition + vector4 + vector5;
            vector7.Y -= num3 / 2f;
            vector6.Y -= num3 / 2f;
            float num4 = 0.58f;
            int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
            if (Vector2.Distance(vector6, vector7) % 3f != 0f)
            {
                num5++;
            }
            for (float num6 = 1f; num6 <= (float)num5; num6 += 1f)
            {
                Dust[] dust = Main.dust;
                Vector2 center = base.Player.Center;
                Color newColor = default(Color);
                Dust obj = dust[Dust.NewDust(center, 0, 0, 182, 0f, 0f, 0, newColor)];
                obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5);
                obj.noGravity = true;
                obj.velocity = Vector2.Zero;
                obj.scale = num4;
            }
        }
        public void EnchantingEyePlayerEffect()
        {
            int num = 0;
            num += Player.bodyFrame.Y / 56;
            if (num >= Main.OffsetsPlayerHeadgear.Length)
            {
                num = 0;
            }
            Vector2 vector = Main.OffsetsPlayerHeadgear[num];
            vector *= Player.Directions;
            Vector2 vector2 = new Vector2((float)(Player.width / 2), (float)(Player.height / 2)) + vector + (Player.MountedCenter - base.Player.Center);
            Player.sitting.GetSittingOffsetInfo(Player, out var posOffset, out var seatAdjustment);
            vector2 += posOffset + new Vector2(0f, seatAdjustment);
            if (Player.face == 19)
            {
                vector2.Y -= 5f * Player.gravDir;
            }
            if (Player.head == 276)
            {
                vector2.X += 2.5f * (float)Player.direction;
            }
            if (Player.mount.Active && Player.mount.Type == 52)
            {
                vector2.X += 14f * (float)Player.direction;
                vector2.Y -= 2f * Player.gravDir;
            }
            float y = -11.5f * Player.gravDir;
            int eyeDistance = Player.direction == 1 ? 3 : 7;
            Vector2 vector3 = new Vector2((float)(eyeDistance * Player.direction - ((Player.direction == 1) ? 1 : 0)), y) + Vector2.UnitY * Player.gfxOffY + vector2;
            Vector2 vector4 = new Vector2((float)(eyeDistance * Player.shadowDirection[1] - ((Player.direction == 1) ? 1 : 0)), y) + vector2;
            Vector2 vector5 = Vector2.Zero;
            if (Player.mount.Active && Player.mount.Cart)
            {
                int num2 = Math.Sign(Player.velocity.X);
                if (num2 == 0)
                {
                    num2 = Player.direction;
                }
                vector5 = Utils.RotatedBy(new Vector2(MathHelper.Lerp(0f, -8f, Player.fullRotation / ((float)Math.PI / 4f)), MathHelper.Lerp(0f, 2f, Math.Abs(Player.fullRotation / ((float)Math.PI / 4f)))), (double)Player.fullRotation, default(Vector2));
                if (num2 == Math.Sign(Player.fullRotation))
                {
                    vector5 *= MathHelper.Lerp(1f, 0.6f, Math.Abs(Player.fullRotation / ((float)Math.PI / 4f)));
                }
            }
            if (Player.fullRotation != 0f)
            {
                vector3 = vector3.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
                vector4 = vector4.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
            }
            float num3 = 0f;
            Vector2 vector6 = Player.position + vector3 + vector5;
            Vector2 vector7 = Player.oldPosition + vector4 + vector5;
            vector7.Y -= num3 / 2f;
            vector6.Y -= num3 / 2f;
            float num4 = 0.6f;
            int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
            if (Vector2.Distance(vector6, vector7) % 3f != 0f)
            {
                num5++;
            }
            for (float num6 = 1f; num6 <= (float)num5; num6 += 1f)
            {
                Dust[] dust = Main.dust;
                Vector2 center = base.Player.Center;
                Color newColor = default(Color);
                Dust obj = dust[Dust.NewDust(center, 0, 0, 180, 0f, 0f, 0, newColor)];
                obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5);
                obj.noGravity = true;
                obj.velocity = Vector2.Zero;
                obj.scale = num4;
            }
        }
        #endregion

        #region Barrier Effect Drawing
        public class BarrierDrawLayer : PlayerDrawLayer
        {
            public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

            public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
            {
                Player drawPlayer = drawInfo.drawPlayer;
                if (drawInfo.shadow != 0f || drawPlayer.dead || (drawPlayer.immuneAlpha > 120))
                    return false;

                return drawInfo.drawPlayer.GetModPlayer<TerRoguelikePlayer>().barrierHealth > 1f;
            }

            protected override void Draw(ref PlayerDrawSet drawInfo)
            {
                Player drawPlayer = drawInfo.drawPlayer;
                TerRoguelikePlayer modPLayer = drawPlayer.GetModPlayer<TerRoguelikePlayer>();

                List<DrawData> existingDrawData = drawInfo.DrawDataCache;
                for (int i = 0; i < 4; i++)
                {
                    float scale = 0.8f + 0.08f * ((float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi));
                    float opacity = (0.13f + (0.23f * (0.5f + (modPLayer.barrierHealth / drawPlayer.statLifeMax2)))) * 0.3f;
                    List<DrawData> afterimages = new List<DrawData>();
                    for (int j = 0; j < existingDrawData.Count; j++)
                    {
                        var drawData = existingDrawData[j];
                        drawData.position = existingDrawData[j].position + (Vector2.UnitY * 6f).RotatedBy(MathHelper.PiOver2 * i);
                        drawData.color = Color.Yellow * opacity;
                        drawData.scale = new Vector2(scale);
                        afterimages.Add(drawData);
                        drawData.shader = 1;
                    }
                    drawInfo.DrawDataCache.InsertRange(0, afterimages);
                }
            }
        }
        #endregion

        #region Jet Leg Dash
        public void JetLegDashMovement()
        {
            Player.velocity.X = Player.maxRunSpeed * DashDirCache * 1.5f;
            Player.immuneTime++;
            Player.immune = true;
            Player.immuneNoBlink = true;
        }
        #endregion
    }
}
