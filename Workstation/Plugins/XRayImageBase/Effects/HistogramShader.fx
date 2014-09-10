/// <summary>The current pixel of the displayed image.</summary>
sampler2D input : register(s0);

/// <summary>The color mapping used when the pixel value is
///          greater than the UpperBound Value.</summary>
sampler2D UpperBoundTexture : register(s1);

/// <summary>The color mapping used when the pixel value is
///          less than the LowerBound Value.</summary>
sampler2D LowerBoundTexture : register(s2);

/// <summary>The lowest pixel value allowed to be displayed.</summary>
/// <minValue>0.0</minValue>
/// <maxValue>1.0</maxValue>
/// <defaultValue>0.0</defaultValue>
float LowerBound : register(c0);

/// <summary>The highest pixel value allowed to be displayed.</summary>
/// <minValue>0.0</minValue>
/// <maxValue>1.0</maxValue>
/// <defaultValue>1.0</defaultValue>
float UpperBound : register(c1);

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	float4 Color= tex2D( input , uv.xy);
	float compareColor = Color.r;
	
	float Min = clamp(LowerBound, 0, 1);
	float Max = clamp(UpperBound, 0, 1);
	float Diff = Max - Min;
	
	if (compareColor < 0.0)
	{
		compareColor = 0.0;
	}
	else if (compareColor < 0.04045)
	{
		compareColor /= 12.92;
	}
	else if (compareColor < 1.0)
	{
		compareColor = pow(((compareColor + 0.055) / 1.055), 2.4);
	}
	else
	{
		compareColor = 1.0;
	}

	if(compareColor <= LowerBound)
	{
		Color = tex2D( LowerBoundTexture , tex2D( input , uv.xy));
	}
	else if(compareColor >= UpperBound )
	{
		Color = tex2D( UpperBoundTexture , tex2D( input , uv.xy));
	}	
	else if (Min < Max)
	{
		Color = ((Color - Min) / Diff);
	}

   return Color;
}
