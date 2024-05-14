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
    public class TerRoguelikeShaders
    {
        //Shader loading and registering lifted from the Calamity Mod
        private const string ShaderPath = "Shaders/";
        internal const string CalamityShaderPrefix = "TerRoguelike:";

        internal static Effect BasicTintShader;
        internal static Effect CircularGradientWithEdge;
        internal static Effect CircularGradientOuter;
        internal static Effect ProtectiveBubbleShield;
        internal static Effect MaskOverlay;
        internal static Effect SideFade;

        public static void LoadShaders()
        {
            AssetRepository terAss = TerRoguelike.Instance.Assets;

            // Shorthand to load shaders immediately.
            // Strings provided to LoadShader are the .xnb file paths.
            Effect LoadShader(string path) => terAss.Request<Effect>($"{ShaderPath}{path}", AssetRequestMode.ImmediateLoad).Value;


            BasicTintShader = LoadShader("BasicTint"); //lifted from the Calamity Mod
            RegisterMiscShader(BasicTintShader, "TintPass", "BasicTint");

            CircularGradientWithEdge = LoadShader("CircularGradientWithEdge"); //lifted from the Calamity Mod. Though I technically helped make the edited version of this one.
            RegisterMiscShader(CircularGradientWithEdge, "CircularGradientWithEdgePass", "CircularGradientWithEdge");

            CircularGradientOuter = LoadShader("CircularGradientOuter"); //Edited version of other circle gradient
            RegisterMiscShader(CircularGradientOuter, "CircularGradientOuterPass", "CircularGradientOuter");

            ProtectiveBubbleShield = LoadShader("ProtectiveBubbleShield"); //lifted from the Calamity Mod
            RegisterScreenShader(ProtectiveBubbleShield, "ShieldPass", "ProtectiveBubbleShield");

            SideFade = LoadShader("SideFade");
            RegisterScreenShader(SideFade, "SideFadePass", "SideFade");

            MaskOverlay = LoadShader("MaskOverlay");
            RegisterScreenShader(MaskOverlay, "MaskOverlayPass", "MaskOverlay");
            //This was some code I wrote to get it working on the ice queen boss I made
            /*
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Texture2D starTex = TexDict["StarrySky"];

            Main.spriteBatch.End();
            Effect maskEffect = Filters.Scene["TerRoguelike:MaskOverlay"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, maskEffect, Main.GameViewMatrix.TransformationMatrix);

            Vector2 screenOff = (NPC.Center);
            screenOff.X %= tex.Width;
            screenOff.Y %= tex.Height;
            screenOff.X /= tex.Width;
            screenOff.Y /= tex.Height;
            screenOff.Y -= currentFrame * (1f / Main.npcFrameCount[Type]);

            maskEffect.Parameters["screenOffset"].SetValue(screenOff);
            maskEffect.Parameters["stretch"].SetValue(new Vector2(1f, Main.npcFrameCount[Type]));
            maskEffect.Parameters["replacementTexture"].SetValue(starTex);

            Main.EntitySpriteDraw(tex, NPC.Center - Main.screenPosition, NPC.frame, Color.White * NPC.Opacity, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            StartVanillaSpritebatch();
            */

        }
        private static void RegisterMiscShader(Effect shader, string passName, string registrationName)
        {
            Ref<Effect> shaderPointer = new(shader);
            MiscShaderData passParamRegistration = new(shaderPointer, passName);
            GameShaders.Misc[$"{CalamityShaderPrefix}{registrationName}"] = passParamRegistration;
        }
        private static void RegisterScreenShader(Effect shader, string passName, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            Ref<Effect> shaderPointer = new(shader);
            ScreenShaderData passParamRegistration = new(shaderPointer, passName);
            RegisterSceneFilter(passParamRegistration, registrationName, priority);
        }
        private static void RegisterSceneFilter(ScreenShaderData passReg, string registrationName, EffectPriority priority = EffectPriority.High)
        {
            string prefixedRegistrationName = $"{CalamityShaderPrefix}{registrationName}";
            Filters.Scene[prefixedRegistrationName] = new Filter(passReg, priority);
            Filters.Scene[prefixedRegistrationName].Load();
        }
    }
}
