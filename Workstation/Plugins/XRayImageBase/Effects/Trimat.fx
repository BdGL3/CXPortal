sampler2D input : register(s0);
sampler2D ColorMapping : register(s1);

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

float CompHLImageDecoding(float4 Color)
{
	return clamp(Color.b, 0, 1);
}

float4 AlphaImageDecoding(float4 Color)
{
	return clamp(Color.g, 0, 1);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{ 
	float4 Color = tex2D( input , uv.xy);
	float compHL = CompHLImageDecoding(Color);
	float alpha  = AlphaImageDecoding(Color);
	float compare = 1.0 - compHL;
	
 	float2 lookup = float2(compare, alpha);
 	
	Color = tex2D(ColorMapping, lookup.xy);
	
	if (Color.r == 1.0 && Color.g == 1.0 && Color.b == 1.0)
	{
		Color = sRGBColorEncoding(float4(compHL,compHL,compHL,1.0));
	}
	
	return Color;
}
