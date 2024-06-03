using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria;
using Terraria.Graphics.Effects;

namespace TerRoguelike.Shaders
{
    public class ModifiedMoonLordScreenShaderData : ScreenShaderData
    {
        public ModifiedMoonLordScreenShaderData(string passName)
        : base(passName)
        {

        }
        public override void Apply()
        {
            UseTargetPosition(Main.Camera.Center); // focuses the shader on the camera center instead of on a specific entity
            base.Apply();
        }
    }
}
