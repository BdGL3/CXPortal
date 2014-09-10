sampler2D input : register(s0);

// new HLSL shader
float LowerBound : register(c0);
float UpperBound : register(c1);

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	float4 Color= tex2D( input , uv.xy);
	
	float Min = LowerBound;
	float Max = UpperBound;

	if (Min < Max)
	{
		if (Min > 1.0)
		{
			Min = 1.0;
		}
		else if (Min < 0.0)
		{
			Min = 0.0;
		}
		
		if (Max > 1.0)
		{
			Max = 1.0;
		}
		else if (Max < 0.0)
		{
			Max = 0.0;
		}
		
		float Diff = Max - Min;
	
		Color = ((Color - Min) / Diff);
	}


   return Color;
}
