//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including Texture1)
//--------------------------------------------------------------------------------------
sampler2D  Texture1Sampler : register(S0);

sampler2D  MappingTextureSampler : register(s1);

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 locationInSource : TEXCOORD) : COLOR
{
	return tex2D( MappingTextureSampler , tex2D( Texture1Sampler , locationInSource.xy));
}
    