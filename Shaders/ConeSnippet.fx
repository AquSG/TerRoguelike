float4 tint;
float minDOT;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 pixelColor = tex2D(Texture1Sampler, uv);
    pixelColor *= tint;
    
    float2 origin = float2(0.5, 0.5);
    float2 offsetUV = uv - origin;
    offsetUV = normalize(offsetUV);
    float2 relativeVector = float2(1, 0);

    float DOT = dot(offsetUV, relativeVector);

    if (DOT < minDOT)
    {
        return float4(0, 0, 0, 0);
    }

    return pixelColor;
}

technique Technique1
{
    pass ConeSnippetPass
    {
        PixelShader = compile ps_2_0 main();
    }
}