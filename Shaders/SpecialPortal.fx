float noiseScale;
float2 uvOff;
float outerRing;
float innerRing;
float invisThreshold;
float edgeBlend;
float4 tint;
float4 edgeTint;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float distance = length(uv - float2(0.5, 0.5)) * 2;
    if ((distance > innerRing && distance < outerRing) || distance > 1)
    {
        return float4(0, 0, 0, 0);
    }

    float2 uv2 = (uv - float2(0.5, 0.5)) * noiseScale;
    uv2 += uvOff;
    float4 pixelColor = tex2D(Texture1Sampler, uv2);

    float4 finalColor = tint;
    if (pixelColor.x < invisThreshold && pixelColor.y < invisThreshold && pixelColor.z < invisThreshold)
    {
        finalColor *= pow(1 - distance, 0.5);
    }

    float interpolant = 0;
    float edgeThreshold = 1 - edgeBlend;
    float outerThreshold = outerRing + edgeBlend;
    float innerThreshold = innerRing - edgeBlend;
    if (distance > edgeThreshold)
    {
        interpolant = (distance - edgeThreshold) / edgeBlend;
    }
    else if (distance > outerRing && distance < outerThreshold)
    {
        interpolant = 1 - ((distance - outerRing) / edgeBlend);
    }
    else if (distance < innerRing && distance > innerThreshold)
    {
        interpolant = (distance - innerThreshold) / edgeBlend;
    }

    return lerp(finalColor, edgeTint, interpolant);
}

technique Technique1
{
    pass SpecialPortalPass
    {
        PixelShader = compile ps_2_0 main();
    }
}