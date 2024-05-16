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

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class MoonLord : BaseRoguelikeNPC
    {
        public Entity target;
        public Vector2 spawnPos;
        public bool ableToHit = true;
        public bool canBeHit = true;
        public override int modNPCID => ModContent.NPCType<MoonLord>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Lunar"] };
        public override int CombatStyle => -1;
        public int coreCurrentFrame = 0;
        public int mouthCurrentFrame = 0;
        public int headEyeCurrentFrame = 0;
        public int leftHandCurrentFrame = 0;
        public int rightHandCurrentFrame = 0;
        public Rectangle coreFrame;
        public Rectangle mouthFrame;
        public Rectangle headEyeFrame;
        public Rectangle leftHandFrame;
        public Rectangle rightHandFrame;
        Vector2 headPos;
        Vector2 leftHandPos;
        Vector2 rightHandPos;

        public Texture2D coreTex, coreCrackTex, emptyEyeTex, innerEyeTex, lowerArmTex, upperArmTex, mouthTex, sideEyeTex, topEyeTex, topEyeOverlayTex, headTex, handTex, bodyTex;
        public int leftHandWho = -1; // yes, technically moon lord's "Left" is not the same as the left for the viewer, and vice versa for right hand. I do not care. internally it will be based on viewer perspective.
        public int rightHandWho = -1;
        public int headWho = -1;

        public int deadTime = 0;
        public int cutsceneDuration = 120;
        public int deathCutsceneDuration = 120;

        public static Attack None = new Attack(0, 0, 180);
        public static Attack Attack1 = new Attack(1, 30, 180);
        public static Attack Attack2 = new Attack(2, 30, 180);
        public static Attack Attack3 = new Attack(3, 30, 180);
        public static Attack Attack4 = new Attack(4, 30, 180);
        public static Attack Attack5 = new Attack(5, 30, 180);
        public static Attack Attack6 = new Attack(6, 30, 180);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 70;
            NPC.height = 70;
            NPC.aiStyle = -1;
            NPC.damage = 36;
            NPC.lifeMax = 25000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, 0);
            modNPC.IgnoreRoomWallCollision = true;
            modNPC.OverrideIgniteVisual = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            coreTex = TexDict["MoonLordCore"];
            coreCrackTex = TexDict["MoonLordCoreCracks"];
            emptyEyeTex = TexDict["MoonLordEmptyEye"];
            innerEyeTex = TexDict["MoonLordInnerEye"];
            lowerArmTex = TexDict["MoonLordLowerArm"];
            upperArmTex = TexDict["MoonLordUpperArm"];
            mouthTex = TexDict["MoonLordMouth"];
            sideEyeTex = TexDict["MoonLordSideEye"];
            topEyeTex = TexDict["MoonLordTopEye"];
            topEyeOverlayTex = TexDict["MoonLordTopEyeOverlay"];
            headTex = TexDict["MoonLordHead"];
            handTex = TexDict["MoonLordHand"];
            bodyTex = TexDict["MoonLordBodyHalf"];
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

            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 handSpawnPos = new Vector2(800 * i, -40) + NPC.Center;
                int whoAmI = NPC.NewNPC(NPC.GetSource_FromThis(), (int)handSpawnPos.X, (int)handSpawnPos.Y, ModContent.NPCType<MoonLordHand>());
                NPC hand = Main.npc[whoAmI];
                hand.direction = hand.spriteDirection = i;
                if (i == -1)
                {
                    leftHandWho = whoAmI;
                    leftHandPos = handSpawnPos;
                }
                else
                {
                    rightHandWho = whoAmI;
                    rightHandPos = handSpawnPos;
                }
                    
            }
            Vector2 headSpawnPos = new Vector2(0, -700) + NPC.Center;
            headWho = NPC.NewNPC(NPC.GetSource_FromThis(), (int)headSpawnPos.X, (int)headSpawnPos.Y, ModContent.NPCType<MoonLordHead>());
            headPos = headSpawnPos;
        }
        public override void PostAI()
        {
            leftHandPos = NPC.Center + new Vector2(-500, -40);
            rightHandPos = NPC.Center + new Vector2(500, -40);
            rightHandPos = Main.MouseWorld;
            headPos = NPC.Center + new Vector2(0, -400);

            NPC leftHand = Main.npc[leftHandWho];
            NPC rightHand = Main.npc[rightHandWho];
            NPC head = Main.npc[headWho];
            if (leftHand.life > 1)
            {
                leftHand.Center = leftHandPos;
            }
            if (rightHand.life > 1)
            {
                rightHand.Center = rightHandPos;
            }
            if (head.life > 1)
            {
                head.Center = headPos;
            }
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
                SetBossTrack(TempleGolemTheme);
            }

            ableToHit = NPC.localAI[0] >= 0 && deadTime == 0;
            canBeHit = true;

            if (NPC.localAI[0] < 0)
            {
                target = modNPC.GetTarget(NPC);

                if (NPC.localAI[0] == -cutsceneDuration)
                {
                    CutsceneSystem.SetCutscene(NPC.Center, cutsceneDuration, 30, 30, 2.5f);
                }
                NPC.localAI[0]++;

                if (NPC.localAI[0] == -30)
                {
                    NPC.immortal = false;
                    NPC.dontTakeDamage = false;
                    NPC.ai[1] = 0;
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
            else if (NPC.ai[0] == Attack6.Id)
            {
                if (NPC.ai[1] >= Attack6.Duration)
                {
                    NPC.ai[0] = None.Id;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = Attack6.Id;
                }
            }
        }
        public void ChooseAttack()
        {
            NPC.ai[1] = 0;
            int chosenAttack = 0;

            List<Attack> potentialAttacks = new List<Attack>() { Attack1, Attack2, Attack3, Attack4, Attack5, Attack6 };
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
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return ableToHit;
        }
        public override bool CanHitNPC(NPC target)
        {
            return ableToHit;
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

            if (deadTime == 0)
            {
                enemyHealthBar.ForceEnd(0);
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0;
                modNPC.ignitedStacks.Clear();

                if (modNPC.isRoomNPC)
                {
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
                for (int i = 0; (double)i < hit.Damage * 0.04d; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Lihzahrd, hit.HitDirection, -1f);
                }
            }
        }
        public override void OnKill()
        {
            
        }
        public override void FindFrame(int frameHeight)
        {
            frameHeight = coreTex.Height / 5;
            coreFrame = new Rectangle(0, coreCurrentFrame * frameHeight, coreTex.Width, frameHeight - 2);

            frameHeight = mouthTex.Height / 3;
            mouthFrame = new Rectangle(0, mouthCurrentFrame * frameHeight, mouthTex.Width, frameHeight - 2);

            frameHeight = topEyeOverlayTex.Height / 4;
            headEyeFrame = new Rectangle(0, headEyeCurrentFrame * frameHeight, topEyeOverlayTex.Width, frameHeight - 2);

            frameHeight = handTex.Height / 4;
            leftHandFrame = new Rectangle(0, leftHandCurrentFrame * frameHeight, handTex.Width, frameHeight - 2);

            rightHandFrame = new Rectangle(0, rightHandCurrentFrame * frameHeight, handTex.Width, frameHeight - 2);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 bodyDrawPos = NPC.Center + new Vector2(0, 43);
            Vector2 shoulderAnchor = new Vector2(208, -64);
            float upperArmLength = upperArmTex.Height * 0.9f;
            float lowerArmLength = lowerArmTex.Height * 0.9f;
            float upperArmLengthRatio = 1 / ((upperArmLength + lowerArmLength) / upperArmLength);

            Vector2 upperArmOrigin = upperArmTex.Size() * new Vector2(0.5f, 0.15f);
            Vector2 lowerArmOrigin = lowerArmTex.Size() * new Vector2(0.5f, 0.9f);

            Vector2 leftShoulderPos = NPC.Center + shoulderAnchor * new Vector2(-1, 1);
            Vector2 rightShoulderPos = NPC.Center + shoulderAnchor * new Vector2(1, 1);

            Vector2 leftShoulderHandVect = leftHandPos - leftShoulderPos;
            Vector2 rightShoulderHandVect = rightHandPos - rightShoulderPos;

            float leftUpperArmRot = (float)Math.Asin((leftShoulderHandVect * upperArmLengthRatio).Length() / upperArmLength) + leftShoulderHandVect.ToRotation() - MathHelper.Pi;
            Vector2 leftElbowPos = (leftUpperArmRot + MathHelper.PiOver2).ToRotationVector2() * upperArmLength + leftShoulderPos;
            float rightUpperArmRot = (float)Math.Asin((rightShoulderHandVect * upperArmLengthRatio).Length() / upperArmLength) * -1 + rightShoulderHandVect.ToRotation();
            Vector2 rightElbowPos = (rightUpperArmRot + MathHelper.PiOver2).ToRotationVector2() * upperArmLength + rightShoulderPos;

            float leftLowerArmRot = (leftHandPos - leftElbowPos).ToRotation() + MathHelper.PiOver2;
            float rightLowerArmRot = (rightHandPos - rightElbowPos).ToRotation() + MathHelper.PiOver2;

            Main.EntitySpriteDraw(upperArmTex, leftShoulderPos - Main.screenPosition, null, Color.White, leftUpperArmRot, upperArmOrigin, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(upperArmTex, rightShoulderPos - Main.screenPosition, null, Color.White, rightUpperArmRot, upperArmOrigin, NPC.scale, SpriteEffects.FlipHorizontally);

            Main.EntitySpriteDraw(lowerArmTex, leftElbowPos - Main.screenPosition, null, Color.White, leftLowerArmRot, lowerArmOrigin, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(lowerArmTex, rightElbowPos - Main.screenPosition, null, Color.White, rightLowerArmRot, lowerArmOrigin, NPC.scale, SpriteEffects.FlipHorizontally);

            Main.EntitySpriteDraw(topEyeTex, leftHandPos - Main.screenPosition, null, Color.White, 0, topEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(topEyeTex, rightHandPos - Main.screenPosition, null, Color.White, 0, topEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally);

            Main.EntitySpriteDraw(innerEyeTex, leftHandPos - Main.screenPosition, null, Color.White, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(innerEyeTex, rightHandPos - Main.screenPosition, null, Color.White, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);

            Main.EntitySpriteDraw(handTex, leftHandPos + new Vector2(2, -49) - Main.screenPosition, leftHandFrame, Color.White, 0, leftHandFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(handTex, rightHandPos + new Vector2(-2, -49) - Main.screenPosition, rightHandFrame, Color.White, 0, rightHandFrame.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally);

            for (int i = -1; i <= 1; i += 2)
            {

                Main.EntitySpriteDraw(bodyTex, bodyDrawPos - Main.screenPosition, null, Color.White, 0, bodyTex.Size() * new Vector2(i == -1 ? 1 : 0, 0.5f), NPC.scale, i == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            Main.EntitySpriteDraw(coreCrackTex, NPC.Center + new Vector2(2, -11) - Main.screenPosition, null, Color.White, 0, coreCrackTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(coreTex, NPC.Center + new Vector2(-1, 0) - Main.screenPosition, coreFrame, Color.White, 0, coreFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None);

            Main.EntitySpriteDraw(headTex, headPos - Main.screenPosition, null, Color.White, 0, headTex.Size() * new Vector2(0.5f, 0.25f), NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(mouthTex, headPos + new Vector2(1, 208) - Main.screenPosition, mouthFrame, Color.White, 0, mouthFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None);

            Main.EntitySpriteDraw(topEyeTex, headPos - Main.screenPosition, null, Color.White, 0, topEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(innerEyeTex, headPos - Main.screenPosition, null, Color.White, 0, innerEyeTex.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(topEyeOverlayTex, headPos - Main.screenPosition, headEyeFrame, Color.White, 0, headEyeFrame.Size() * 0.5f, NPC.scale, SpriteEffects.None);
            return false;
        }
    }
}
