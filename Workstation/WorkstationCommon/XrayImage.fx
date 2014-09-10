/// <summary>The first input texture.</summary>
/// <defaultValue>C:\Users\L3_Admin\Desktop\input.bmp</defaultValue>
sampler2D input : register(s0);

/// <summary>The color mapping used when the pixel value is
///          less than the LowerBound Value.</summary>
/// <defaultValue>Blue</defaultValue>
float4 LowerBoundColor : register(C0);

/// <summary>The color used when the pixel value is
///          greater than the UpperBound Value.</summary>
/// <defaultValue>Green</defaultValue>
float4 UpperBoundColor : register(C1);

/// <summary>The lowest pixel value allowed to be displayed.</summary>
/// <minValue>0.0</minValue>
/// <maxValue>1.0</maxValue>
/// <defaultValue>0.0</defaultValue>
float LowerBound : register(c2);

/// <summary>The highest pixel value allowed to be displayed.</summary>
/// <minValue>0.0</minValue>
/// <maxValue>1.0</maxValue>
/// <defaultValue>1.0</defaultValue>
float UpperBound : register(c3);

/// <summary>Enable of Disable Log curve.</summary>
/// <minValue>0.0</minValue>
/// <maxValue>1.0</maxValue>
/// <defaultValue>0.0</defaultValue>
float EnableLog : register(c4);

/// <summary>Enable of Disable Squareroot curve.</summary>
/// <minValue>0.0</minValue>
/// <maxValue>1.0</maxValue>
/// <defaultValue>0.0</defaultValue>
float EnableSquareroot : register(c5);

/// <summary>Enable of Disable Square curve.</summary>
/// <minValue>0.0</minValue>
/// <maxValue>1.0</maxValue>
/// <defaultValue>0.0</defaultValue>
float EnableSquare : register(c6);


float4 sRGBColorDecoding(float4 Color)
{
	if (Color.r <= 0.04045)
 		Color.r = Color.r/12.92;
 	else
 		Color.r = pow(((Color.r + 0.055)/1.055),2.4);
 		
 	if (Color.g <= 0.04045)
 		Color.g = Color.g/12.92;
 	else
 		Color.g = pow(((Color.g + 0.055)/1.055),2.4);
 		
 	if (Color.b <= 0.04045)
 		Color.b = Color.b/12.92;
 	else
 		Color.b = pow(((Color.b + 0.055)/1.055),2.4);

	return Color;
}

float4 sRGBColorEncoding(float4 Color)
{
	if (Color.r <= (0.04045 / 12.92))
 		Color.r = Color.r * 12.92;
 	else
 		Color.r = pow(Color.r, (1/2.4)) * 1.055 - 0.055;

	if (Color.g <= (0.04045 / 12.92))
 		Color.g = Color.g * 12.92;
 	else
 		Color.g = pow(Color.g, (1/2.4)) * 1.055 - 0.055;
 		
 	if (Color.b <= (0.04045 / 12.92))
 		Color.b = Color.b * 12.92;
 	else
 		Color.b = pow(Color.b, (1/2.4)) * 1.055 - 0.055;

	return Color;
}


float4 CargoImageDecoding(float4 Color)
{
	float realColor = clamp((Color.r / 65536.0) + (Color.g / 256.0) + Color.b, 0, 1);
	
	return float4(realColor, realColor, realColor, 1.0);
}


float4 main(float2 uv : TEXCOORD) : COLOR
{ 
	float4 Color = sRGBColorDecoding(tex2D( input , uv.xy));
	
	Color = CargoImageDecoding(Color);
	
	float Min = clamp(LowerBound, 0, 1);
	float Max = clamp(UpperBound, 0, 1);
	float Diff = Max - Min;

	if(Color.r < LowerBound)
	{
		Color *= LowerBoundColor;
	}
	else if(Color.r > UpperBound)
	{
		if (UpperBoundColor.r == 1.0 &&
		    UpperBoundColor.g == 1.0 &&
		    UpperBoundColor.b == 1.0)
		{
			Color = float4(1.0, 1.0, 1.0, 1.0);
		}
		else
		{
			Color *= UpperBoundColor;
		}
	}
	else if (EnableLog > 0)
	{
		Color.r = log(16777216 * ((Color.r - Min) / Diff)) / log(16777216);
		Color.g = Color.r;
		Color.b = Color.r;
	}
	else if (EnableSquareroot > 0)
	{
		Color.r = sqrt(16777216 * ((Color.r - Min) / Diff)) / sqrt(16777216);
		Color.g = Color.r;
		Color.b = Color.r;
	}
	else if (EnableSquare > 0)
	{
		Color.r = pow(((Color.r - Min) / Diff), 2);
		Color.g = Color.r;
		Color.b = Color.r;
	}
	else
	{
		Color.r = (Color.r - Min) / Diff;
		Color.g = Color.r;
		Color.b = Color.r;
	}
	
	return sRGBColorEncoding(Color);
}