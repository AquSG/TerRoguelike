﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerRoguelike;
using TerRoguelike.TerPlayer;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.GameContent;
using TerRoguelike.NPCs;
using TerRoguelike.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static TerRoguelike.Schematics.SchematicManager;
using Terraria.DataStructures;
using static TerRoguelike.Utilities.TerRoguelikeUtils;
using Terraria.Graphics.Shaders;
using static TerRoguelike.Managers.TextureManager;
using TerRoguelike.Managers;
using TerRoguelike.Particles;
using System.IO;

namespace TerRoguelike.NPCs.Enemy
{
    public class Clinger : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<Clinger>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Corrupt"] };
        public override int CombatStyle => 2;
        public Vector2 AnchorPos = Vector2.Zero;
        public Texture2D segment1Tex;
        public Texture2D segment2Tex;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 30;
            NPC.height = 30;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            modNPC.drawCenter = new Vector2(0, -2);
            NPC.noGravity = true;
            NPC.behindTiles = true;
            NPC.noTileCollide = true;
            segment1Tex = TexDict["ClingerSegment1"];
            segment2Tex = TexDict["ClingerSegment2"];
        }
        public override void OnSpawn(IEntitySource source)
        {
            Point spawnTile = new Point((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f));
            if (!ParanoidTileRetrieval(spawnTile.X, spawnTile.Y).IsTileSolidGround(true))
            {
                for (int i = 0; i < 100; i++)
                {
                    int direction = i % 4 > 1 ? (i % 2 == 0 ? 2 : -2) : (i % 2 == 0 ? 1 : -1);
                    if (Math.Abs(direction) == 2)
                    {
                        direction = Math.Sign(direction);
                        if (!ParanoidTileRetrieval(spawnTile.X, spawnTile.Y + ((i / 4) * direction)).IsTileSolidGround(true))
                            continue;

                        NPC.Center = new Vector2(spawnTile.X, spawnTile.Y + ((i / 4) * direction)) * 16f + new Vector2(8, 8);
                        break;
                    }
                    else
                    {
                        if (!ParanoidTileRetrieval(spawnTile.X + ((i / 4) * direction), spawnTile.Y).IsTileSolidGround(true))
                            continue;

                        NPC.Center = new Vector2(spawnTile.X + ((i / 4) * direction), spawnTile.Y) * 16f + new Vector2(8, 8);
                        break;
                    }
                }
            }


            AnchorPos = NPC.Center;
        }
        public override void AI()
        {
            int attackTelegraph = 30;
            modNPC.RogueClingerAI(NPC, 4f, 0.09f, AnchorPos, 240f, attackTelegraph, 90, ModContent.ProjectileType<CursedFlame>(), 8f, NPC.damage);

            NPC.frameCounter += 0.1d;
            float direction = (NPC.Center - AnchorPos).ToRotation();
            if (modNPC.targetPlayer != -1)
                direction = (Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation();
            else if (modNPC.targetNPC != -1)
                direction = (Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation();
            NPC.rotation = direction;

            if (NPC.ai[0] < attackTelegraph && NPC.ai[0] > 0)
            {
                Vector2 offset = (Main.rand.Next(16, 21) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
                ParticleManager.AddParticle(new Square(NPC.Center + offset + NPC.velocity + (Vector2.UnitX * 12).RotatedBy(NPC.rotation), -offset.SafeNormalize(Vector2.UnitX) + NPC.velocity, 20, Color.Lerp(Color.LimeGreen, Color.Yellow, Main.rand.NextFloat(0.75f)), new Vector2(Main.rand.NextFloat(0.9f, 1f)), Main.rand.NextFloat(MathHelper.TwoPi), 0.96f, 20, false));
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -1f, NPC.alpha, NPC.color, NPC.scale);
                }
                return;
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 18, hit.HitDirection, -2f, NPC.alpha, NPC.color, NPC.scale);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 110, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 114, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 114, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 115, NPC.scale);
            }
        }
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)NPC.frameCounter % 2 == 1 ? 2 : 0;
            NPC.frame = new Rectangle(0, currentFrame * frameHeight, NpcTexWidth(Type), frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AnchorPos != Vector2.Zero)
            {
                int segmentLength = (int)(20 * NPC.scale);
                int tetherLength = (int)(NPC.Center - AnchorPos).Length();
                if (tetherLength < 1)
                    tetherLength = 1;
                float direction = (AnchorPos - NPC.Center).ToRotation();
                Vector2 start = NPC.Center + (Vector2.UnitX * 10).RotatedBy(direction);
                if (modNPC.ignitedStacks.Count > 0)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    for (int j = 0; j < tetherLength; j += segmentLength)
                    {
                        bool end = false;
                        if (j + segmentLength > tetherLength)
                            end = true;

                        Vector2 drawPos = start + (Vector2.UnitX * j).RotatedBy(direction);
                        Texture2D tex = (j / segmentLength) % 2 == 0 ? segment1Tex : segment2Tex;
                        SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
                        Vector2 position = drawPos + (Vector2.UnitY * NPC.gfxOffY);

                        Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                        Vector3 colorHSL = Main.rgbToHsl(color);
                        float outlineThickness = 1f;
                        GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                        GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                        GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                        for (float k = 0; k < 1; k += 0.125f)
                        {
                            spriteBatch.Draw(tex, position + (k * MathHelper.TwoPi + direction).ToRotationVector2() * outlineThickness - Main.screenPosition, !end ? null : new Rectangle(tex.Width - (tetherLength % segmentLength), 0, (tetherLength % segmentLength), tex.Height), color, direction, tex.Size() * 0.5f, NPC.scale, spriteEffects, 0f);
                        }
                    }
                }
                for (int i = 0; i < tetherLength; i += segmentLength)
                {
                    bool end = false;
                    if (i + segmentLength > tetherLength)
                        end = true;

                    Vector2 drawPos = start + (Vector2.UnitX * i).RotatedBy(direction);
                    Texture2D tex = (i / segmentLength) % 2 == 0 ? segment1Tex : segment2Tex;
                    modNPC.EliteEffectSpritebatch(NPC, new(1, 1, tex.Size(), tex.Frame()));
                    spriteBatch.Draw(tex, drawPos - Main.screenPosition, !end ? null : new Rectangle(tex.Width - (tetherLength % segmentLength), 0, (tetherLength % segmentLength), tex.Height), modNPC.ignitedStacks.Count > 0 ? Color.Lerp(Color.White, Color.OrangeRed, 0.4f) : Lighting.GetColor((int)(drawPos.X / 16), (int)(drawPos.Y / 16)), direction, tex.Size() * 0.5f, NPC.scale, SpriteEffects.FlipHorizontally, 0);
                }
                modNPC.EliteEffectSpritebatch(NPC, new());
            }
            return true;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(AnchorPos);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AnchorPos = reader.ReadVector2();
        }
    }
}
