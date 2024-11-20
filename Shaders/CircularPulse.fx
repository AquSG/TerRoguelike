sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor; //Color of the circle
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uShaderSpecificData;

float4 Recolor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float radius = uShaderSpecificData.x;
    float pixelation = uShaderSpecificData.y;
    float thickness = uShaderSpecificData.z;

    float2 uv = coords;
    if (pixelation > 0)
    {
        float pixelatedWidth = uImageSize0.x / pixelation;
        float pixelatedHeight = uImageSize0.y / pixelation;

        uv.x = (int)(uv.x * pixelatedWidth) / pixelatedWidth;
        uv.y = (int)(uv.y * pixelatedHeight) / pixelatedHeight;
        
        uv.x += (1 / pixelatedWidth) * 0.5;
        uv.y += (1 / pixelatedHeight) * 0.5;
    }
    float3 color = uColor;
    float distanceFromCenter = length(uv - float2(0.5, 0.5)) * 2;
    
    //Circle limitation
    if (distanceFromCenter > radius)
        return float4(0, 0, 0, 0);

    if (distanceFromCenter < radius - thickness)
        return float4(0, 0, 0, 0);

    color = color * uOpacity;
    
    return float4(color, uOpacity);
}


technique Technique1
{
    pass CircularPulsePass
    {
        PixelShader = compile ps_2_0 Recolor();
    }
}