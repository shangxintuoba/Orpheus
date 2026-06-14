#ifndef CUSTOMDITHERING_INCLUDED
#define CUSTOMDITHERING_INCLUDED


// If <= 0, scaling is disabled
float _ReferenceHeight;

float GetAnchorSize()
{
    float2 screen = _ScreenParams.xy;
    
    return screen.y;
}

float GetDitherScale()
{
    float reference = _ReferenceHeight;
    if (reference <= 0.0)
        return 1.0;

    float anchor = max(GetAnchorSize(), 1.0);
    return reference / anchor;
}

float GetPixelScale()
{
    float reference = _ReferenceHeight;
    if (reference <= 0.0)
        return 1.0;

    float anchor = max(GetAnchorSize(), 1.0);
    return anchor / reference;
}

void Luminance_float(float3 color, out float luminance)
{
    luminance = Luminance(color);
}

float intensity(float3 color)
{
    //return sqrt((color.x * color.x) + (color.y * color.y) + (color.z * color.z));
    float lum = dot(color, float3(0.2126, 0.7152, 0.0722));
    return pow(lum, 0.5);
}

void ApplyDithering_float(
    float2 screenPosition,
    float ditherSize,
    UnityTexture2D ditherTex,
    UnitySamplerState ditherSampler,
    float2 ditherTextureSize,
    out float3 Out)
{
    float ditherFinalSize = max(ditherSize, 0.0001);
    float2 textureFinalSize = max(ditherTextureSize, float2(1.0, 1.0));

    float scale = GetDitherScale();
    float2 cell = floor(screenPosition * _ScreenParams.xy * scale / ditherFinalSize);

    float2 index = fmod(cell, textureFinalSize);
    float2 ditherUV = (index + 0.5) / textureFinalSize;

    float3 ditherThreshold = SAMPLE_TEXTURE2D(ditherTex, ditherSampler, ditherUV).rgb;
    Out = ditherThreshold;
}

void PixelatedUV_float(float2 screenPosition, int pixelSize, out float2 UV)
{
    float pixelationSize = max((float) pixelSize, 1.0);

    float scale = GetPixelScale();
    float adjustedPixelSize = max(1.0, pixelationSize * scale);

    float2 screenSize = _ScreenParams.xy / adjustedPixelSize;
    UV = (floor(screenPosition * screenSize) + 0.5) / screenSize;
}

Texture2D _BlitTexture;
SamplerState my_point_clamp_sampler;
float4 _BlitTexture_TexelSize;

float3 SampleBlitTexture(float2 uv)
{
    return _BlitTexture.Sample(my_point_clamp_sampler, uv).rgb;
}

void SampleSceneColor_float(
    float2 screenPosition,
    int pixelSize,
    float outlineWidth,
    float outlineThreshold,
    half4 outlineColor,
    out float3 Out)
{
    float2 uv;
    PixelatedUV_float(screenPosition, pixelSize, uv);
    float outline = 0.0;

#if defined(_OUTLINE_DEPTH)
    float2 offset = outlineWidth * _BlitTexture_TexelSize.xy;

    float centerDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
    float d0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv + float2(offset.x, 0));
    float d1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv - float2(offset.x, 0));
    float d2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv + float2(0, offset.y));
    float d3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv - float2(0, offset.y));

    float diff = max(
        max(abs(centerDepth - d0), abs(centerDepth - d1)),
        max(abs(centerDepth - d2), abs(centerDepth - d3))
    );

    outline = smoothstep(0, outlineThreshold, diff);

#elif defined(_OUTLINE_SOBEL)
    float2 texel = _BlitTexture_TexelSize.xy * outlineWidth;

    float tleft = intensity(SampleBlitTexture(uv + texel * float2(-1,  1)).rgb);
    float left  = intensity(SampleBlitTexture(uv + texel * float2(-1,  0)).rgb);
    float bleft = intensity(SampleBlitTexture(uv + texel * float2(-1, -1)).rgb);
    float top   = intensity(SampleBlitTexture(uv + texel * float2( 0,  1)).rgb);
    float bottom= intensity(SampleBlitTexture(uv + texel * float2( 0, -1)).rgb);
    float tright= intensity(SampleBlitTexture(uv + texel * float2( 1,  1)).rgb);
    float right = intensity(SampleBlitTexture(uv + texel * float2( 1,  0)).rgb);
    float bright= intensity(SampleBlitTexture(uv + texel * float2( 1, -1)).rgb);

    float x = tleft + 2.0 * left + bleft - tright - 2.0 * right - bright;
    float y = -tleft - 2.0 * top - tright + bleft + 2.0 * bottom + bright;

    float color = sqrt((x * x) + (y * y));
    color = saturate(smoothstep(0, outlineThreshold, color));
    outline = color;

#else
    outline = 0.0;
#endif

    Out = lerp(SampleBlitTexture(uv), outlineColor.rgb, outline * outlineColor.a);
}

#endif 
