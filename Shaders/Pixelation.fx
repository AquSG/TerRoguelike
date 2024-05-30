float time;
float4 tint;
float2 dimensions;
float offRot;
int pixelation;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float sine = sin(offRot);
    float cosine = sin(offRot + 1.5708);

    float pixelatedWidth = dimensions.x / pixelation;
    float pixelatedHeight = dimensions.y / pixelation;

    uv.x = (int)(uv.x * pixelatedWidth) / pixelatedWidth;
    uv.y = (int)(uv.y * pixelatedHeight) / pixelatedHeight;

    uv -= 0.5;
    float2 olduv = uv;
    uv.x = (olduv.x * cosine) - (olduv.y * sine);
    uv.y = (olduv.x * sine) + (olduv.y * cosine);
    uv += 0.5;

    float4 pixelColor = tex2D(Texture1Sampler, uv);
    pixelColor *= tint;
    

    return pixelColor;
}

technique Technique1
{
    pass PixelationPass
    {
        PixelShader = compile ps_2_0 main();
    }
}