sampler2D input;

// new HLSL shader

/// <summary>Explain the purpose of this variable.</summary>
/// <minValue>05/minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>3.5</defaultValue>
float SampleI : register(C0);

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	return sqrt(tex2D( input , uv.xy));
}