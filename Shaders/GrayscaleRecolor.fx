float4 recolor;
float4 intensity;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 pixelColor = tex2D(Texture1Sampler, uv);
    if (pixelColor.w == 0)
    {
        return float4(0, 0, 0, 0);
    }

    pixelColor.x = pixelColor.y = pixelColor.z = (pixelColor.x + pixelColor.y + pixelColor.z) / 3;

    return float4(lerp(float3(pixelColor.x, pixelColor.y, pixelColor.z), float3(recolor.x, recolor.y, recolor.z), intensity), recolor.w);
}

technique Technique1
{
    pass GrayscaleRecolorPass
    {
        PixelShader = compile ps_2_0 main();
    }
}