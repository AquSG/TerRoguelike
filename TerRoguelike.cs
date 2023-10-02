using Terraria.ModLoader;
using TerRoguelike.Schematics;
using static TerRoguelike.Managers.SpawnManager;
using TerRoguelike.Managers;
using static TerRoguelike.Systems.RoomSystem;
using TerRoguelike.UI;

namespace TerRoguelike
{
	public class TerRoguelike : Mod
	{
		internal static TerRoguelike Instance;
        public override void Load()
        {
            Instance = this;
            SchematicManager.Load();
            BarrierUI.Load();
            ItemManager.Load();
        }
        public override void Unload()
        {
            Instance = null;
            SchematicManager.Unload();
            RoomList = null;
            pendingEnemies = null;
            pendingItems = null;
            BarrierUI.Unload();
            ItemManager.Unload();
        }
    }
}