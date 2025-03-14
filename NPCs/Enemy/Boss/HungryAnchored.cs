using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
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
using static TerRoguelike.NPCs.Enemy.Boss.CrimsonVessel;

namespace TerRoguelike.NPCs.Enemy.Boss
{
    public class HungryAnchored : BaseRoguelikeNPC
    {
        public override int modNPCID => ModContent.NPCType<HungryAnchored>();
        public override List<int> associatedFloors => new List<int>() { FloorDict["Hell"] };
        public override int CombatStyle => -1;
        public int currentFrame = 0;
        public Vector2 AnchorPos = Vector2.Zero;
        public Texture2D segmentTex;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 24;
            NPC.height = 24;
            NPC.aiStyle = -1;
            NPC.damage = 35;
            NPC.lifeMax = 360;
            NPC.HitSound = SoundID.NPCHit9;
            NPC.DeathSound = SoundID.NPCDeath11;
            NPC.knockBackResist = 0f;
            modNPC.drawCenter = new Vector2(0, -3);
            modNPC.IgnoreRoomWallCollision = true;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.behindTiles = true;
            segmentTex = TexDict["HungryTether"];
            NPC.spriteDirection = 1;
            NPC.gfxOffY = 2;
            modNPC.activatedPuppeteersHand = true;
        }
        public override void DrawBehind(int index)
        {

        }
        public override void OnSpawn(IEntitySource source)
        {
            AnchorPos = NPC.Center;
            NPC.rotation = MathHelper.Pi;

            NPC.ai[1] = -1;
            if (source is EntitySource_Parent parentSource)
            {
                if (parentSource.Entity is NPC)
                {
                    NPC.ai[1] = parentSource.Entity.whoAmI;
                    NPC npc = Main.npc[(int)NPC.ai[1]];
                    if (!npc.active || npc.type != ModContent.NPCType<WallOfFlesh>())
                    {
                        NPC.ai[1] = -1;
                        NPC.StrikeInstantKill();
                        return;
                    }
                        
                }
            }

            if (NPC.ai[1] == -1)
                NPC.StrikeInstantKill();
        }
        
        public override void AI()
        {
            modNPC.RogueClingerAI(NPC, 4f, 0.09f, AnchorPos, 240f, 60, 60, ProjectileID.None, 8f, NPC.damage);

            NPC.frameCounter += 0.2d;
            float direction = (NPC.Center - AnchorPos).ToRotation();
            if (modNPC.targetPlayer != -1)
                direction = (Main.player[modNPC.targetPlayer].Center - NPC.Center).ToRotation();
            else if (modNPC.targetNPC != -1)
                direction = (Main.npc[modNPC.targetNPC].Center - NPC.Center).ToRotation();
            NPC.rotation = direction;

            NPC parent = Main.npc[(int)NPC.ai[1]];
            if (!parent.active || parent.type != ModContent.NPCType<WallOfFlesh>())
                NPC.StrikeInstantKill();
            else
                parent.ai[3]++;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 20.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
                }
            }
            
        }
        public override void OnKill()
        {
            if (NPC.ai[1] >= 0)
            {
                NPC npc = NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<Hungry>(), 0, NPC.ai[1]);
                npc.rotation = NPC.rotation;
                npc.Center = NPC.Center;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (Main.dedServ)
                return;

            Texture2D tex = TextureAssets.Npc[Type].Value;
            currentFrame = (int)NPC.frameCounter % Main.npcFrameCount[Type];

            NPC.frame = new Rectangle(0, currentFrame * frameHeight, tex.Width, frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AnchorPos != Vector2.Zero)
            {
                SpriteEffects chainEffects = (int)NPC.frameCounter % 4 < 2 ? SpriteEffects.None : SpriteEffects.FlipVertically;
                int segmentLength = segmentTex.Width;
                Vector2 start = NPC.Center + NPC.rotation.ToRotationVector2() * -8;
                int tetherLength = (int)(start - AnchorPos).Length();
                if (tetherLength < 1)
                    tetherLength = 1;
                float direction = (AnchorPos - start).ToRotation();

                if (modNPC.ignitedStacks.Count > 0)
                {
                    StartAlphaBlendSpritebatch();
                    Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, Main.rand.NextFloat(0.4f, 0.6f + float.Epsilon) + 0.2f + (0.2f * (float)Math.Cos((Main.GlobalTimeWrappedHourly * 20f)))) * 0.8f;
                    Vector3 colorHSL = Main.rgbToHsl(color);
                    float outlineThickness = 1f;
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseOpacity(1f);
                    GameShaders.Misc["TerRoguelike:BasicTint"].UseColor(Main.hslToRgb(1 - colorHSL.X, colorHSL.Y, colorHSL.Z));
                    GameShaders.Misc["TerRoguelike:BasicTint"].Apply();

                    for (int i = 0; i < tetherLength; i += segmentLength)
                    {
                        bool end = false;
                        if (i + segmentLength > tetherLength)
                            end = true;

                        Vector2 drawPos = start + direction.ToRotationVector2() * i;
                        for (float k = 0; k < 1; k += 0.125f)
                        {
                            spriteBatch.Draw(segmentTex, drawPos - Main.screenPosition + (k * MathHelper.TwoPi + direction).ToRotationVector2() * outlineThickness, !end ? null : new Rectangle(segmentTex.Width - (tetherLength % segmentLength), 0, (tetherLength % segmentLength), segmentTex.Height), Color.White, direction, new Vector2(0, segmentTex.Size().Y * 0.5f), NPC.scale, chainEffects, 0);
                        }
                    }
                    StartVanillaSpritebatch();
                }
                for (int i = 0; i < tetherLength; i += segmentLength)
                {
                    bool end = false;
                    if (i + segmentLength > tetherLength)
                        end = true;

                    Vector2 drawPos = start + direction.ToRotationVector2() * i;
                    spriteBatch.Draw(segmentTex, drawPos - Main.screenPosition, !end ? null : new Rectangle(segmentTex.Width - (tetherLength % segmentLength), 0, (tetherLength % segmentLength), segmentTex.Height), modNPC.ignitedStacks.Count > 0 ? Color.Lerp(Color.White, Color.OrangeRed, 0.4f) : Lighting.GetColor((int)(drawPos.X / 16), (int)(drawPos.Y / 16)), direction, new Vector2(0, segmentTex.Size().Y * 0.5f), NPC.scale, chainEffects, 0);
                }
            }
            return true;
        }
    }
}
