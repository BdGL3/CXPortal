sampler2D input : register(s0);

// new HLSL shader

/// <summary>Explain the purpose of this variable.</summary>
/// <minValue>0/minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>0.05</defaultValue>
float SampleI : register(C0);

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	

	float4 Color; 
	Color= tex2D( input , uv.xy); 
	
	if ( Color.r < SampleI)
		Color.r = 1;
	if ( Color.g < SampleI)
	{
		Color.r = 1;
		Color.g = 0;
	}
	if ( Color.g < SampleI)
	{
		Color.r = 1;
		Color.b = 0;
	}


	return Color; 
}