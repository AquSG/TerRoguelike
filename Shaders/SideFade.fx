float time;
float fadeCutoff;
float4 tint;
float4 fadeTint;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 pixelColor = tex2D(Texture1Sampler, uv);
    pixelColor *= tint;
    float fadeStrength;
    if (uv.x < 0.5)
    {
        if (uv.x >= fadeCutoff)
        {
            return pixelColor;
        }
        fadeStrength = uv.x / fadeCutoff;
    }
    else
    {
        if (uv.x <= 1 - fadeCutoff)
        {
            return pixelColor;
        }
        fadeStrength = (1 - uv.x) / fadeCutoff;
    }

    pixelColor = lerp(fadeTint, pixelColor, fadeStrength);

    return pixelColor;
}

technique Technique1
{
    pass SideFadePass
    {
        PixelShader = compile ps_2_0 main();
    }
}