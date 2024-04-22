float time;
float2 screenOffset;
float2 stretch;
float4 tint;

texture sampleTexture;
texture replacementTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };
sampler2D Texture2Sampler = sampler_state { texture = <replacementTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 main(float2 uv : TEXCOORD) : COLOR
{
    //Get the original color
    float4 pixelColor = tex2D(Texture1Sampler, uv);
    if (pixelColor.w == 0)
    {
        return float4(0, 0, 0, 0);
    }
    float2 totalUV = screenOffset + uv;
    float4 replacementColor = tex2D(Texture2Sampler, totalUV * stretch);
    replacementColor *= tint;
    return replacementColor;
}

technique Technique1
{
    pass MaskOverlayPass
    {
        PixelShader = compile ps_2_0 main();
    }
}