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

namespace TerRoguelike.ILEditing
{
    public class ILEdits : ModSystem
    {
        public override void OnModLoad()
        {
            On_Main.DamageVar_float_int_float += AdjustDamageVariance;
            On_UICharacterCreation.FinishCreatingCharacter += FinishCreatingCharacterEdit;
            On_WorldGen.SaveAndQuit += On_WorldGen_SaveAndQuit;
        }

        private void On_WorldGen_SaveAndQuit(On_WorldGen.orig_SaveAndQuit orig, Action callback)
        {
			if (TerRoguelikeWorld.IsDeletableOnExit && !TerRoguelikeMenu.wipeTempPlayer && !TerRoguelikeMenu.wipeTempWorld)
            {
				TerRoguelikeMenu.wipeTempPlayer = true;
				TerRoguelikeMenu.wipeTempWorld = true;
            }
			ThreadPool.QueueUserWorkItem(WorldGen.SaveAndQuitCallBack, callback);
		}

        private void FinishCreatingCharacterEdit(On_UICharacterCreation.orig_FinishCreatingCharacter orig, UICharacterCreation self)
        {
			//tmod gonna kill me for this
			if (TerRoguelikeMenu.prepareForRoguelikeGeneration)
			{
				Player _player = Main.PendingPlayer;
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

            // Change the default damage variance from +-15% to +-5%.
            // If other mods decide to change the scale, they can override this. We're solely killing the default value.
            if (percent == Main.DefaultDamageVariationPercent && TerRoguelikeWorld.IsTerRoguelikeWorld)
                percent = 0;
            // Remove the ability for luck to affect damage variance by setting it to 0 always.
            return orig(dmg, percent, 0f);
        }
    }
}
