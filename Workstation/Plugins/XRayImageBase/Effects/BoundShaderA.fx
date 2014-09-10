sampler2D input : register(s0);

// new HLSL shader

/// <summary>Explain the purpose of this variable.</summary>
/// <minValue>05/minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>3.5</defaultValue>
float LowBound : register(c0);
float UprBound : register(c1);
float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	float4 Color= tex2D( input , uv.xy);

	if((Color[0] < LowBound) || (Color[0] > UprBound ))  //or
	   {
	     Color=0; 
	    }
	   else
	     Color=Color;

   return Color;
}
