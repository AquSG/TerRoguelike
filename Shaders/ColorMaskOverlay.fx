float2 position;
float2 stretch;
float4 tint;
float4 maskColor;
float4 maskTint;
float rotation;
float2 texSize;
float2 frameSize;
float2 framePos;
float2 replacementTexSize;

texture sampleTexture;
texture replacementTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = clamp; AddressV = clamp; };
sampler2D Texture2Sampler = sampler_state { texture = <replacementTexture>; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = wrap; AddressV = wrap; };

float4 main(float2 uv : TEXCOORD) : COLOR
{

    float4 pixelColor = tex2D(Texture1Sampler, uv);
    if (!any(pixelColor))
        return (pixelColor);
    if (!(pixelColor.x == maskColor.x && pixelColor.y == maskColor.y && pixelColor.z == maskColor.z && pixelColor.w == maskColor.w))
    {
        return pixelColor * tint;
    }

    float sine = sin(rotation);
    float cosine = sin(rotation + 1.5708);

    float2 uvPx = uv * texSize;
    uvPx -= framePos;
    float2 origin = float2(0.5, 0.5);
    float2 originFrameSize = frameSize * 0.5;
    uvPx -= originFrameSize;

    float2 oldUvPx = uvPx;
    uvPx.x = (oldUvPx.x * cosine) - (oldUvPx.y * sine);
    uvPx.y = (oldUvPx.x * sine) + (oldUvPx.y * cosine);
    uvPx *= stretch;
    //uvPx += originFrameSize * stretch;

    uvPx += position;
    uvPx /= replacementTexSize;

    float4 replacementColor = tex2D(Texture2Sampler, uvPx);
    replacementColor *= maskTint;
    return replacementColor;
}

technique Technique1
{
    pass ColorMaskOverlayPass
    {
        PixelShader = compile ps_2_0 main();
    }
}