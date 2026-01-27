#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// s0 is the default texture passed by SpriteBatch.Draw
sampler s0 : register(s0);

// We explicitly put the Mask in register s1
Texture2D Mask;
sampler MaskSample : register(s1)
{
    Texture = (Mask);
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
    float4 tex = tex2D(s0, TexCoords);
    float4 bitMask = tex2D(MaskSample, TexCoords);

    // Calculate mask strength (0.0 to 1.0)
    float howSolid = (bitMask.r + bitMask.g + bitMask.b) / 3.0;

    // Apply the mask
    // We multiply everything by howSolid to keep Premultiplied Alpha happy
    tex.rgb *= howSolid;
    tex.a *= howSolid;

    // IMPORTANT: Multiply by 'Color' so that the color parameter 
    // in spriteBatch.Draw(texture, position, COLOR) still works!
    return tex * Color;
}

technique Masking
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}