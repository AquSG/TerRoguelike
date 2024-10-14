float4 lightTint;
float4 darkTint;
float contrastThreshold;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 pixelColor = tex2D(Texture1Sampler, uv);
    if (pixelColor.w == 0)
    {
        return float4(0, 0, 0, 0);
    }
    if (pixelColor.x < contrastThreshold && pixelColor.y < contrastThreshold && pixelColor.z < contrastThreshold)
    {
        return float4(darkTint.x, darkTint.y, darkTint.z, pixelColor.w);
    }
    return float4(lightTint.x, lightTint.y, lightTint.z, pixelColor.w);
}

technique Technique1
{
    pass DualContrastPass
    {
        PixelShader = compile ps_2_0 main();
    }
}