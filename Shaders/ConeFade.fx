float time;
float fadeCutoff;
float4 tint;
float4 fadeTint;
float halfCone;
float coneFadeStrength;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 pixelColor = tex2D(Texture1Sampler, uv);
    pixelColor *= tint;
    
    float2 origin = float2(0.5, 0.5);
    float2 offsetUV = uv - origin;
    offsetUV = normalize(offsetUV);
    float2 relativeVector;
    if (offsetUV.y < 0)
    {
        relativeVector = float2(cos(-halfCone), sin(-halfCone));
    }
    else
    {
        relativeVector = float2(cos(halfCone), sin(halfCone));
    }
    float DOT = dot(offsetUV, relativeVector);

    float fadeStrength = (DOT - coneFadeStrength) / (1 - coneFadeStrength);

    if (uv.x > fadeCutoff)
    {
        fadeStrength *= 1 - ((uv.x - fadeCutoff) / (1 - fadeCutoff));
    }

    pixelColor = lerp(fadeTint, pixelColor, fadeStrength);

    return pixelColor;
}

technique Technique1
{
    pass ConeFadePass
    {
        PixelShader = compile ps_2_0 main();
    }
}