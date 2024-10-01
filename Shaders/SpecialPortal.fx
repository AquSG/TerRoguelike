float noiseScale;
float2 uvOff;
float outerRing;
float innerRing;
float invisThreshold;
float edgeBlend;
float4 tint;
float4 edgeTint;
float finalFadeExponent;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float distance = length(uv - float2(0.5, 0.5)) * 2;
    //if ((distance > innerRing && distance < outerRing) || distance > 1)
    if (distance > 1)
    {
        return float4(0, 0, 0, 0);
    }

    float2 uv2 = (uv - float2(0.5, 0.5)) * noiseScale;
    uv2 += uvOff;
    float4 pixelColor = tex2D(Texture1Sampler, uv2);

    float4 finalColor = tint;
    float brightness = (pixelColor.x + pixelColor.y + pixelColor.z) / 3;

    float edgeInterpolant = 1;
    float edgeThreshold = 1 - edgeBlend;
    float outerThreshold = outerRing + edgeBlend;
    float innerThreshold = innerRing - edgeBlend;

    if (distance > innerRing && distance < outerRing)
    {
        edgeInterpolant = 0;
    }
    else
    {
        if (distance > edgeThreshold)
        {
            edgeInterpolant *= 1 - ((distance - edgeThreshold) / edgeBlend);
        }
        if (distance > outerRing && distance < outerThreshold)
        {
            edgeInterpolant *= ((distance - outerRing) / edgeBlend);
        }
        else if (distance < innerRing && distance > innerThreshold)
        {
            edgeInterpolant *= 1 - ((distance - innerThreshold) / edgeBlend);
        }
    }
    edgeInterpolant = lerp(invisThreshold * 1.25, 1, edgeInterpolant);
    edgeInterpolant *= 1 - brightness;

    if (edgeInterpolant < invisThreshold)
    {
        return float4(0, 0, 0, 0);
    }
    edgeInterpolant = pow((edgeInterpolant - invisThreshold) / (1 - invisThreshold), finalFadeExponent);

    return lerp(edgeTint, finalColor, edgeInterpolant);
}

technique Technique1
{
    pass SpecialPortalPass
    {
        PixelShader = compile ps_2_0 main();
    }
}