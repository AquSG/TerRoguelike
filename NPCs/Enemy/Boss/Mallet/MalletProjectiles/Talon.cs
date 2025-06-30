using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerRoguelike.Managers;
using TerRoguelike.Systems;
using TerRoguelike.TerPlayer;
using Microsoft.Xna.Framework.Graphics;
using TerRoguelike.Items.Common;
using TerRoguelike.Utilities;
using Terraria.DataStructures;
using TerRoguelike.Particles;
using static TerRoguelike.Managers.TextureManager;
using Steamworks;
using ReLogic.Utilities;
using Microsoft.Build.Construction;
using System.IO;

namespace TerRoguelike.NPCs.Enemy.Boss.Mallet.MalletProjectiles
{
    public class Talon : ModProjectile, ILocalizedModType
    {
        public int maxTimeLeft;
        public int targetPlayer = -1;
        public int targetNPC = -1;
        public Vector2 spawnPos = Vector2.Zero;
        Entity target
        {
            get
            {
                if (targetPlayer >= 0)
                {
                    return Main.player[targetPlayer];
                }
                else if (targetNPC >= 0)
                {
                    return Main.npc[targetNPC];
                }
                else
                    return null;
            }
        }
        public override string Texture => "TerRoguelike/Projectiles/InvisibleProj";
        public Texture2D TalonTex, TalonScratch;
        public int swipeTime = 60;
        public Vector2 shakeVect = Vector2.Zero;
        public override void SetDefaults()
        {
            Projectile.width = 256;
            Projectile.height = 256;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = maxTimeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.netImportant = true; //makes projectile not able to be replaced if at the projectile cap 
            TalonTex = TexDict["Talon"];
            TalonScratch = TexDict["TalonScratch"];
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            Projectile.hide = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC parent = Main.npc[parentSource.Entity.whoAmI];
                    var modparent = parent.ModNPC();
                    if (modparent != null)
                    {
                        targetNPC = modparent.targetNPC;
                        targetPlayer = modparent.targetPlayer;
                    }
                }
            }
        }
        public override bool PreAI()
        {
            if (target != null)
            {
                if (targetPlayer >= 0)
                {
                    var player = Main.player[targetPlayer];
                    if (!player.active || player.dead)
                        targetPlayer = -1;
                }
                else if (targetNPC >= 0)
                {
                    var npc = Main.npc[targetNPC];
                    if (!npc.active)
                        targetNPC = -1;
                }
            }
            return true;
        }
        public override void AI()
        {
            if (Projectile.ai[0] > 0)
            {
                Projectile.timeLeft = maxTimeLeft = (int)Projectile.ai[0] + swipeTime + 8;
                Projectile.ai[0] = 0;
            }
            if (Projectile.ai[1] == 0)
                Projectile.ai[1] = Main.rand.NextBool() ? -1 : 1;

            Projectile.localAI[0]++;

            if (target != null && Projectile.timeLeft > swipeTime - 16)
            {
                bool alt = Projectile.timeLeft > swipeTime;
                Vector2 targetVect = target.Center - Projectile.Center;
                float optimalRot = -MathHelper.PiOver2 + MathHelper.PiOver4 * -Projectile.ai[1];
                float sizeBetween = Math.Abs(TerRoguelikeUtils.AngleSizeBetween(optimalRot, targetVect.ToRotation()));
                float closeness = sizeBetween / (alt ? MathHelper.PiOver2 : MathHelper.Pi);
                if (closeness > 1)
                    closeness = 1;
                Projectile.Center += targetVect * MathHelper.Lerp(alt ? 0.025f : 0, 1f, closeness);
            }

            if (Projectile.timeLeft == swipeTime || Projectile.timeLeft == swipeTime + 8)
            {
                SoundEngine.PlaySound(Mallet.Warning with { Volume = 0.5f }, Projectile.Center);
            }
            if (Projectile.timeLeft <= swipeTime)
            {
                shakeVect = Vector2.Zero;
                Projectile.frameCounter++;
                if (Projectile.timeLeft == swipeTime - 8)
                {
                    SoundEngine.PlaySound(Mallet.TalonSwipe with { Volume = 0.7f }, Projectile.Center);
                }
            }
            else
            {
                if (Projectile.timeLeft % 2 == 0)
                {
                    shakeVect = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int count = (Projectile.frameCounter / 4) >= 5 ? 5 : 4;

            Vector2 increment = Projectile.ai[1] > 0 ? new Vector2(-32, 32) : new Vector2(32, 32);
            Vector2 start = Projectile.ai[1] > 0 ? Projectile.TopRight + increment * 2.5f : Projectile.TopLeft + increment * 2.5f;
            for (int i = 0; i < count; i++)
            {
                Vector2 checkPos = start + increment * i;

                if (targetHitbox.ClosestPointInRect(checkPos).Distance(checkPos) < 32)
                    return true;
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            int talonFrameCounter = (Projectile.frameCounter / 4) + 1;
            if (talonFrameCounter >= 4)
            {
                int scratchFrameCounter = talonFrameCounter - 4;
                if (scratchFrameCounter > 2)
                    scratchFrameCounter = 2;

                float opacity = Projectile.timeLeft / 24f;
                if (opacity > 1)
                    opacity = 1;
                var scratchFrame = TalonScratch.Frame(1, 3, 0, scratchFrameCounter);
                Main.EntitySpriteDraw(TalonScratch, drawPos, scratchFrame, Color.White * opacity, Projectile.rotation, scratchFrame.Size() * 0.5f, Projectile.scale, Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
            if (talonFrameCounter < 8)
            {
                float opacity = (maxTimeLeft - Projectile.timeLeft) / 60f;
                if (opacity > 1)
                    opacity = 1;
                if (Projectile.frameCounter >= 20)
                    opacity *= 1 - ((Projectile.frameCounter - 20) / 8f);

                bool red = (Projectile.timeLeft <= swipeTime && Projectile.timeLeft > swipeTime - 4) || (Projectile.timeLeft <= swipeTime + 8 && Projectile.timeLeft > swipeTime + 4);
                var talonFrame = TalonTex.Frame(1, 8, 0, talonFrameCounter);
                Main.EntitySpriteDraw(TalonTex, drawPos + shakeVect, talonFrame, (red ? Color.Red : Color.White) * opacity, Projectile.rotation, talonFrame.Size() * 0.5f, Projectile.scale, Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

                if (Projectile.localAI[0] >= 0)
                {
                    float completion = 1 - (Projectile.localAI[0] / 30f);
                    float imageOpacity = 0.3f;
                    imageOpacity *= completion;
                    if (Projectile.localAI[0] < 5)
                        imageOpacity *= Projectile.localAI[0] / 5f;
                    Main.EntitySpriteDraw(TalonTex, drawPos + shakeVect, talonFrame, (red ? Color.Red : Color.White) * imageOpacity, Projectile.rotation, talonFrame.Size() * 0.5f, Projectile.scale * 6 * MathHelper.SmoothStep(0, 1, completion), Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                }
                
            }

            bool drawHitboxes = false;
            if (drawHitboxes && CanDamage() != false)
            {
                var squareTex = TexDict["Square"];
                int count = (Projectile.frameCounter / 4) >= 5 ? 5 : 4;

                Vector2 increment = Projectile.ai[1] > 0 ? new Vector2(-32, 32) : new Vector2(32, 32);
                Vector2 start = Projectile.ai[1] > 0 ? Projectile.TopRight + increment * 2.5f : Projectile.TopLeft + increment * 2.5f;
                for (int i = 0; i < count; i++)
                {
                    Vector2 checkPos = start + increment * i;

                    for (int j = 0; j < 60; j++)
                    {
                        Main.EntitySpriteDraw(squareTex, checkPos - Main.screenPosition + (j * MathHelper.TwoPi / 60f).ToRotationVector2() * 32, null, Color.Red, 0, squareTex.Size() * 0.5f, 1f, SpriteEffects.None);
                    }
                }
            }
            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            Mallet.InflictRetribution(target);
        }
        public override bool? CanDamage() => Projectile.timeLeft > 21 && Projectile.timeLeft < swipeTime - 14 ? null : false;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(targetNPC);
            writer.Write(targetPlayer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            targetNPC = reader.ReadInt32();
            targetPlayer = reader.ReadInt32();
        }
    }
}
