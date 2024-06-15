using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerRoguelike.World;
using Terraria.GameContent.UI.States;
using TerRoguelike.MainMenu;
using Terraria.IO;
using Terraria.GameContent.Creative;
using System.Threading;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using TerRoguelike.TerPlayer;
using Terraria.Audio;
using TerRoguelike.Systems;
using ReLogic.Utilities;
using Terraria.ID;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ModLoader.Core;
using static Terraria.Collision;
using Terraria.GameContent.Drawing;
using static Terraria.WorldGen;
using ReLogic.Threading;
using Terraria.GameContent.Events;
using TerRoguelike.Utilities;
using Terraria.GameContent.Shaders;
using TerRoguelike.NPCs.Enemy.Boss;
using System.Reflection;
using Terraria.Graphics.Effects;
using TerRoguelike.Managers;

namespace TerRoguelike.ILEditing
{
    public class ILEdits : ModSystem
    {
		public int passInNPC = -1;
		public int passInPlayer = -1;
        public override void OnModLoad()
        {
            On_Main.DamageVar_float_int_float += AdjustDamageVariance;
            On_UICharacterCreation.FinishCreatingCharacter += FinishCreatingCharacterEdit;
            On_UICharacterCreation.Click_GoBack += ExitCreatingCharacter;
            On_WorldGen.SaveAndQuit += On_WorldGen_SaveAndQuit;
            On_PlayerDrawLayers.DrawPlayer_04_ElectrifiedDebuffBack += EditElectrifiedDisplayCondition1;
            On_PlayerDrawLayers.DrawPlayer_34_ElectrifiedDebuffFront += EditElectrifiedDisplayCondition2;
            On_WorldGen.UpdateWorld_UndergroundTile += FuckUnderGroundUpdating;
            IL_Main.DrawMenu += DrawTerRogulikeMenuInjection;
            On_Main.DrawMenu += On_Main_DrawMenu;
            On_Collision.SlopeCollision += On_Collision_SlopeCollision;
			On_Main.DoDraw_DrawNPCsBehindTiles += PreDrawTilesInjection;
            On_NPC.NPCLoot_DropCommonLifeAndMana += StopOnKillHeartsAndMana;
            On_ScreenObstruction.Draw += PostDrawBasicallyEverything;
            IL_Main.DoDraw += IL_Main_DoDraw;
            IL_Player.Update += RopeMovementILEdit;
            On_Player.SlopingCollision += On_Player_SlopingCollision;
            On_NPC.Collision_MoveSlopesAndStairFall += On_NPC_Collision_MoveSlopesAndStairFall;
        }

        private void RopeMovementILEdit(ILContext il)
        {
            ILCursor cursor = new(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(0.02f)))
            {
				TerRoguelike.Instance.Logger.Warn("Failed to find stealth subtraction");
                return;
            }
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(0.02f)))
            {
                TerRoguelike.Instance.Logger.Warn("Failed to find rope slow upwards acceleration");
                return;
            }
			cursor.Remove();
            cursor.EmitDelegate<Func<float>>(() => TerRoguelikeWorld.IsTerRoguelikeWorld ? 0.04f : 0.02f);

            if (!cursor.TryGotoPrev(MoveType.Before, i => i.MatchLdcR4(0.7f)))
            {
                TerRoguelike.Instance.Logger.Warn("Failed to find rope upwards deceleration");
                return;
            }
            cursor.Remove();
            cursor.EmitDelegate<Func<float>>(() => TerRoguelikeWorld.IsTerRoguelikeWorld ? 0.5f : 0.7f);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(0.2f)))
            {
                TerRoguelike.Instance.Logger.Warn("Failed to find rope upwards acceleration");
                return;
            }
            cursor.Remove();
            cursor.EmitDelegate<Func<float>>(() => TerRoguelikeWorld.IsTerRoguelikeWorld ? 0.4f : 0.2f);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(0.2f)))
            {
                TerRoguelike.Instance.Logger.Warn("Failed to find rope downwards acceleration");
                return;
            }
            cursor.Remove();
            cursor.EmitDelegate<Func<float>>(() => TerRoguelikeWorld.IsTerRoguelikeWorld ? 0.4f : 0.2f);

            if (!cursor.TryGotoPrev(MoveType.Before, i => i.MatchLdcR4(0.7f)))
            {
                TerRoguelike.Instance.Logger.Warn("Failed to find rope downwards deceleration");
                return;
            }
            cursor.Remove();
            cursor.EmitDelegate<Func<float>>(() => TerRoguelikeWorld.IsTerRoguelikeWorld ? 0.5f : 0.7f);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(0.1f)))
            {
                TerRoguelike.Instance.Logger.Warn("Failed to find rope slow downwards acceleration");
                return;
            }
            cursor.Remove();
            cursor.EmitDelegate<Func<float>>(() => TerRoguelikeWorld.IsTerRoguelikeWorld ? 0.2f : 0.1f);
        }

        private void IL_Main_DoDraw(ILContext il)
        {
			ILCursor cursor = new(il);
			if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<Main>("DrawProjectiles")))
			{
				TerRoguelike.Instance.Logger.Warn("Failed to find DrawProjectiles in Main.DoDraw");
				return;
			}
			cursor.EmitDelegate<Action>(() =>
			{
				ParticleManager.DrawParticles_AfterProjectiles();
			});
        }

        private void PostDrawBasicallyEverything(On_ScreenObstruction.orig_Draw orig, SpriteBatch spriteBatch)
        {
			orig.Invoke(spriteBatch);
			spriteBatch.End();
			RoomSystem.PostDrawEverything(spriteBatch);
			TerRoguelikeUtils.StartVanillaSpritebatch(false);
        }

        private void PreDrawTilesInjection(On_Main.orig_DoDraw_DrawNPCsBehindTiles orig, Main self)
        {
            RoomSystem.PostDrawWalls_PreNPCsBehindTiles(Main.spriteBatch);
            orig.Invoke(self);
            RoomSystem.PostDrawWalls(Main.spriteBatch);
        }

        private void StopOnKillHeartsAndMana(On_NPC.orig_NPCLoot_DropCommonLifeAndMana orig, NPC self, Player closestPlayer)
        {
			if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
				orig.Invoke(self, closestPlayer);
        }
        private void On_Player_SlopingCollision(On_Player.orig_SlopingCollision orig, Player self, bool fallThrough, bool ignorePlats)
        {
			if (TerRoguelikeWorld.IsTerRoguelikeWorld)
				passInPlayer = self.whoAmI;

			orig.Invoke(self, fallThrough, ignorePlats);
        }
        private void On_NPC_Collision_MoveSlopesAndStairFall(On_NPC.orig_Collision_MoveSlopesAndStairFall orig, NPC self, bool fall)
        {
			if (TerRoguelikeWorld.IsTerRoguelikeWorld)
				passInNPC = self.whoAmI;

            orig.Invoke(self, fall);
        }

        private Vector4 On_Collision_SlopeCollision(On_Collision.orig_SlopeCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, float gravity, bool fall)
        {
			if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
				return orig.Invoke(Position, Velocity, Width, Height, gravity, fall);

			NPC npc = passInNPC == -1 ? null : Main.npc[passInNPC];
			Player player = passInPlayer == -1 ? null : Main.player[passInPlayer];
			stair = false;
			stairFall = false;
			bool[] array = new bool[5];
			float y = Position.Y;
			float y2 = Position.Y;
			sloping = false;
			Vector2 savedPosition = Position;
			Vector2 savedVelocity = Velocity;
			int leftTile = (int)(Position.X / 16f) - 1;
			int rightTile = (int)((Position.X + (float)Width) / 16f) + 2;
			int topTile = (int)(Position.Y / 16f) - 1;
			int bottomTile = (int)((Position.Y + (float)Height) / 16f) + 2;
			int safeLeftTile = Utils.Clamp(leftTile, 0, Main.maxTilesX - 1);
			rightTile = Utils.Clamp(rightTile, 0, Main.maxTilesX - 1);
			topTile = Utils.Clamp(topTile, 0, Main.maxTilesY - 1);
			bottomTile = Utils.Clamp(bottomTile, 0, Main.maxTilesY - 1);
			Vector2 checkTilePos = default(Vector2);
			for (int i = safeLeftTile; i < rightTile; i++)
			{
				for (int j = topTile; j < bottomTile; j++)
				{
					if (Main.tile[i, j] == null || !Main.tile[i, j].HasTile || Main.tile[i, j].IsActuated || (!Main.tileSolid[Main.tile[i, j].TileType] && (!Main.tileSolidTop[Main.tile[i, j].TileType] || Main.tile[i, j].TileFrameY != 0)))
					{
						continue;
					}
					checkTilePos.X = i * 16;
					checkTilePos.Y = j * 16;
					int num11 = 16;
					if (Main.tile[i, j].IsHalfBlock)
					{
						checkTilePos.Y += 8f;
						num11 -= 8;
					}
					if (!(Position.X + (float)Width > checkTilePos.X) || !(Position.X < checkTilePos.X + 16f) || !(Position.Y + (float)Height > checkTilePos.Y) || !(Position.Y < checkTilePos.Y + (float)num11))
					{
						continue;
					}
					bool flag = true;
					if (TileID.Sets.Platforms[Main.tile[i, j].TileType])
					{
						if (Velocity.Y < 0f && (player == null || (Main.tile[i, j].Slope == SlopeType.Solid)))
						{
							flag = false;
						}
						if (player != null && Velocity.Y < 0 && Main.tile[i, j].Slope != SlopeType.Solid)
						{
							var platSlopeType = Main.tile[i, j].Slope;
							if (platSlopeType == SlopeType.SlopeDownLeft && Velocity.X > 0)
							{
								Vector2 relativePos = player.BottomLeft - checkTilePos;
								if (relativePos.X < relativePos.Y)
									flag = false;
                            }
							if (platSlopeType == SlopeType.SlopeDownRight && Velocity.X < 0)
							{
                                Vector2 relativePos = player.BottomRight - checkTilePos + Vector2.UnitX * 16;
								if (relativePos.Y < relativePos.X)
									flag = false;
                            }	
						}
						if (Position.Y + (float)Height < (float)(j * 16) || Position.Y + (float)Height - (1f + Math.Abs(Velocity.X)) > (float)(j * 16 + 16))
						{
							flag = false;
						}
						if (((Main.tile[i, j].Slope == SlopeType.SlopeDownLeft && Velocity.X >= 0f) || (Main.tile[i, j].Slope == SlopeType.SlopeDownRight && Velocity.X <= 0f)) && (Position.Y + (float)Height) / 16f - 1f == (float)j)
						{
							flag = false;
						}
					}
					if (!flag)
					{
						continue;
					}
					bool flag2 = false;
					if (fall && TileID.Sets.Platforms[Main.tile[i, j].TileType])
					{
						flag2 = true;
					}
					SlopeType slopeType = Main.tile[i, j].Slope;
					checkTilePos.X = i * 16;
					checkTilePos.Y = j * 16;
					if (!(Position.X + (float)Width > checkTilePos.X) || !(Position.X < checkTilePos.X + 16f) || !(Position.Y + (float)Height > checkTilePos.Y) || !(Position.Y < checkTilePos.Y + 16f))
					{
						continue;
					}
					float num13 = 0f;
					if (slopeType == SlopeType.SlopeUpLeft || slopeType == SlopeType.SlopeUpRight)
					{
						if (slopeType == SlopeType.SlopeUpLeft)
						{
							num13 = Position.X - checkTilePos.X;
						}
						if (slopeType == SlopeType.SlopeUpRight)
						{
							num13 = checkTilePos.X + 16f - (Position.X + (float)Width);
						}
						if (num13 >= 0f)
						{
							if (Position.Y <= checkTilePos.Y + 16f - num13)
							{
								float num14 = checkTilePos.Y + 16f - Position.Y - num13;
								if (Position.Y + num14 > y2)
								{
									if (npc != null)
									{
										if (npc.velocity.Y < 0)
											npc.collideY = true;
									}
									savedPosition.Y = Position.Y + num14;
									y2 = savedPosition.Y;
									if (savedVelocity.Y < 0.0101f)
									{
										savedVelocity.Y = 0.0101f;
									}
									array[(int)slopeType] = true;
								}
							}
						}
						else if (Position.Y > checkTilePos.Y)
						{
							if (npc != null)
							{
								if (npc.velocity.Y < 0)
									npc.collideY = true;
							}
							float num15 = checkTilePos.Y + 16f;
							if (savedPosition.Y < num15)
							{
								savedPosition.Y = num15;
								if (savedVelocity.Y < 0.0101f)
								{
									savedVelocity.Y = 0.0101f;
								}
							}
						}
					}
					if (slopeType != SlopeType.SlopeDownLeft && slopeType != SlopeType.SlopeDownRight)
					{
						continue;
					}
					if (slopeType == SlopeType.SlopeDownLeft)
					{
						num13 = Position.X - checkTilePos.X;
					}
					if (slopeType == SlopeType.SlopeDownRight)
					{
						num13 = checkTilePos.X + 16f - (Position.X + (float)Width);
					}
					if (num13 >= 0f)
					{
						if (!(Position.Y + (float)Height >= checkTilePos.Y + num13))
						{
							continue;
						}
						float num16 = checkTilePos.Y - (Position.Y + (float)Height) + num13;
						if (!(Position.Y + num16 < y))
						{
							continue;
						}
						if (flag2)
						{
							stairFall = true;
							continue;
						}
						if (TileID.Sets.Platforms[Main.tile[i, j].TileType])
						{
							stair = true;
						}
						else
						{
							stair = false;
						}
						if (npc != null)
                        {
							if (npc.velocity.Y > 0)
								npc.collideY = true;
						}
							
						savedPosition.Y = Position.Y + num16;
						y = savedPosition.Y;
						if (savedVelocity.Y > 0f)
						{
							savedVelocity.Y = 0f;
						}
						array[(int)slopeType] = true;
						continue;
					}
					if (TileID.Sets.Platforms[Main.tile[i, j].TileType] && !(Position.Y + (float)Height - 4f - Math.Abs(Velocity.X) <= checkTilePos.Y))
					{
						if (flag2)
						{
							stairFall = true;
						}
						continue;
					}
					float num17 = checkTilePos.Y - (float)Height;
					if (!(savedPosition.Y > num17))
					{
						continue;
					}
					if (flag2)
					{
						stairFall = true;
						continue;
					}
					if (TileID.Sets.Platforms[Main.tile[i, j].TileType])
					{
						stair = true;
					}
					else
					{
						stair = false;
					}
					if (npc != null)
						npc.collideY = true;
					savedPosition.Y = num17;
					if (savedVelocity.Y > 0f)
					{
						savedVelocity.Y = 0f;
					}
				}
			}
			Vector2 velocity = savedPosition - Position;
			Vector2 vector5 = TileCollision(Position, velocity, Width, Height);
			if (vector5.Y > velocity.Y)
			{
				if (npc != null)
					npc.collideY = true;

				float num18 = velocity.Y - vector5.Y;
				savedPosition.Y = Position.Y + vector5.Y;
				if (array[1])
				{
					savedPosition.X = Position.X - num18;
				}
				if (array[2])
				{
					savedPosition.X = Position.X + num18;
				}
				savedVelocity.X = 0f;
				savedVelocity.Y = 0f;
				up = false;
			}
			else if (vector5.Y < velocity.Y)
			{
				if (npc != null)
					npc.collideY = true;

				float num10 = vector5.Y - velocity.Y;
				savedPosition.Y = Position.Y + vector5.Y;
				if (array[3])
				{
					savedPosition.X = Position.X - num10;
				}
				if (array[4])
				{
					savedPosition.X = Position.X + num10;
				}
				savedVelocity.X = 0f;
				savedVelocity.Y = 0f;
			}

			passInNPC = -1;
			passInPlayer = -1;
			return new Vector4(savedPosition, savedVelocity.X, savedVelocity.Y);
		}

        private void On_Main_DrawMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
		{
			TerRoguelikeMenu.TerRoguelikeMenuInteractionLogic();
			orig.Invoke(self, gameTime);
        }
        private void DrawTerRogulikeMenuInjection(ILContext il)
        {
            ILCursor cursor = new(il);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<Main>("DrawThickCursor")))
            {
                TerRoguelike.Instance.Logger.Warn("Failed to find DrawThickCursor in Main.DrawMenu");
                return;
            }

            cursor.EmitDelegate<Action>(() =>
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

                TerRoguelikeMenu.DrawTerRoguelikeMenu();

                Main.spriteBatch.End();
                Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            });
        }

        private void FuckUnderGroundUpdating(On_WorldGen.orig_UpdateWorld_UndergroundTile orig, int i, int j, bool checkNPCSpawns, int wallDist)
        {
			if (!TerRoguelikeWorld.IsTerRoguelikeWorld)
				orig.Invoke(i, j, checkNPCSpawns, wallDist);
        }

        private void EditElectrifiedDisplayCondition1(On_PlayerDrawLayers.orig_DrawPlayer_04_ElectrifiedDebuffBack orig, ref PlayerDrawSet drawinfo)
		{
			if ((!drawinfo.drawPlayer.electrified && drawinfo.drawPlayer.GetModPlayer<TerRoguelikePlayer>().portableGeneratorImmuneTime <= 0) || drawinfo.shadow != 0f)
			{
				return;
			}
			Texture2D value = TextureAssets.GlowMask[25].Value;
			int num = drawinfo.drawPlayer.miscCounter / 5;
			for (int i = 0; i < 2; i++)
			{
				num %= 7;
				if (num <= 1 || num >= 5)
				{
					DrawData item = new DrawData(value, new Vector2((float)(int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (float)(int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2((float)(drawinfo.drawPlayer.bodyFrame.Width / 2), (float)(drawinfo.drawPlayer.bodyFrame.Height / 2)), (Rectangle?)new Rectangle(0, num * value.Height / 7, value.Width, value.Height / 7), drawinfo.colorElectricity, drawinfo.drawPlayer.bodyRotation, new Vector2((float)(value.Width / 2), (float)(value.Height / 14)), 1f, drawinfo.playerEffect, 0f);
					drawinfo.DrawDataCache.Add(item);
				}
				num += 3;
			}
		}
		private void EditElectrifiedDisplayCondition2(On_PlayerDrawLayers.orig_DrawPlayer_34_ElectrifiedDebuffFront orig, ref PlayerDrawSet drawinfo)
        {
			if ((!drawinfo.drawPlayer.electrified && drawinfo.drawPlayer.GetModPlayer<TerRoguelikePlayer>().portableGeneratorImmuneTime <= 0) || drawinfo.shadow != 0f)
			{
				return;
			}
			Texture2D value = TextureAssets.GlowMask[25].Value;
			int num = drawinfo.drawPlayer.miscCounter / 5;
			for (int i = 0; i < 2; i++)
			{
				num %= 7;
				if (num > 1 && num < 5)
				{
					DrawData item = new DrawData(value, new Vector2((float)(int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (float)(int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2((float)(drawinfo.drawPlayer.bodyFrame.Width / 2), (float)(drawinfo.drawPlayer.bodyFrame.Height / 2)), (Rectangle?)new Rectangle(0, num * value.Height / 7, value.Width, value.Height / 7), drawinfo.colorElectricity, drawinfo.drawPlayer.bodyRotation, new Vector2((float)(value.Width / 2), (float)(value.Height / 14)), 1f, drawinfo.playerEffect, 0f);
					drawinfo.DrawDataCache.Add(item);
				}
				num += 3;
			}
		}

        private void On_WorldGen_SaveAndQuit(On_WorldGen.orig_SaveAndQuit orig, Action callback)
        {
            SkyManager.Instance.Deactivate("TerRoguelike:MoonLordSkyClone");
            MusicSystem.ClearMusic();
			if (TerRoguelikeWorld.IsDeletableOnExit && !TerRoguelikeMenu.wipeTempPlayer && !TerRoguelikeMenu.wipeTempWorld)
            {
				TerRoguelikeMenu.wipeTempPlayer = true;
				TerRoguelikeMenu.wipeTempWorld = true;
            }
			ThreadPool.QueueUserWorkItem(WorldGen.SaveAndQuitCallBack, callback);
		}
		private void ExitCreatingCharacter(On_UICharacterCreation.orig_Click_GoBack orig, UICharacterCreation self, Terraria.UI.UIMouseEvent evt, Terraria.UI.UIElement listeningElement)
		{
			if (TerRoguelikeMenu.prepareForRoguelikeGeneration)
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				Main.menuMode = 0;
			}
			else
				orig.Invoke(self, evt, listeningElement);

		}
		private void FinishCreatingCharacterEdit(On_UICharacterCreation.orig_FinishCreatingCharacter orig, UICharacterCreation self)
        {
			//tmod gonna kill me for this
			if (TerRoguelikeMenu.prepareForRoguelikeGeneration)
			{
				Player _player = Main.PendingPlayer;
				if (_player.ModPlayer() != null)
				{
					_player.ModPlayer().isDeletableOnExit = true;
				}
				int num = 0;
				if (_player.difficulty == 3)
				{
					PlayerLoader.ModifyMaxStats(_player);
					_player.statLife = _player.statLifeMax;
					_player.statMana = _player.statManaMax;
					_player.inventory[num].SetDefaults(6);
					_player.inventory[num++].Prefix(-1);
					_player.inventory[num].SetDefaults(1);
					_player.inventory[num++].Prefix(-1);
					_player.inventory[num].SetDefaults(10);
					_player.inventory[num++].Prefix(-1);
					_player.inventory[num].SetDefaults(7);
					_player.inventory[num++].Prefix(-1);
					_player.inventory[num].SetDefaults(4281);
					_player.inventory[num++].Prefix(-1);
					_player.inventory[num].SetDefaults(8);
					_player.inventory[num++].stack = 100;
					_player.inventory[num].SetDefaults(965);
					_player.inventory[num++].stack = 100;
					_player.inventory[num++].SetDefaults(50);
					_player.inventory[num++].SetDefaults(84);
					_player.armor[3].SetDefaults(4978);
					_player.armor[3].Prefix(-1);
					if (_player.name == "Wolf Pet" || _player.name == "Wolfpet")
					{
						_player.miscEquips[3].SetDefaults(5130);
					}
					_player.AddBuff(216, 3600);
				}
				else
				{
					_player.inventory[num].SetDefaults(3507);
					_player.inventory[num++].Prefix(-1);
					_player.inventory[num].SetDefaults(3509);
					_player.inventory[num++].Prefix(-1);
					_player.inventory[num].SetDefaults(3506);
					_player.inventory[num++].Prefix(-1);
				}
				if (Main.runningCollectorsEdition)
				{
					_player.inventory[num++].SetDefaults(603);
				}
				_player.savedPerPlayerFieldsThatArentInThePlayerClass = new Player.SavedPlayerDataWithAnnoyingRules();
				CreativePowerManager.Instance.ResetDataForNewPlayer(_player);
				IEnumerable<Item> vanillaItems = from item in _player.inventory
												 where !item.IsAir
												 select item into x
												 select x.Clone();
				List<Item> startingItems = PlayerLoader.GetStartingItems(_player, vanillaItems);
				PlayerLoader.SetStartInventory(_player, startingItems);
				TerRoguelikeMenu.desiredPlayer = PlayerFileData.CreateAndSave(_player);
				Main.LoadPlayers();
				Main.menuMode = 1;
				return;
			}
			else
				orig.Invoke(self);
        }

        private int AdjustDamageVariance(On_Main.orig_DamageVar_float_int_float orig, float dmg, int percent, float luck)
        {
            //IL edit lifted from the Calamity Mod. Only active if in a TerRoguelike world.

            if (percent == Main.DefaultDamageVariationPercent && TerRoguelikeWorld.IsTerRoguelikeWorld)
                percent = 0;
            // Remove the ability for luck to affect damage variance by setting it to 0 always.
            return orig(dmg, percent, 0f);
        }

		public static void LogFailure(string name, string reason) => TerRoguelike.Instance.Logger.Warn($"IL edit \"{name}\" failed! {reason}");
	}
}
