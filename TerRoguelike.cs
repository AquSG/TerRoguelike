using Terraria.ModLoader;
using TerRoguelike.Schematics;

namespace TerRoguelike
{
	public class TerRoguelike : Mod
	{
		internal static TerRoguelike Instance;
        public override void Load()
        {
            Instance = this;
            SchematicManager.Load();
        }
        public override void Unload()
        {
            Instance = null;
            SchematicManager.Unload();
        }
    }
}