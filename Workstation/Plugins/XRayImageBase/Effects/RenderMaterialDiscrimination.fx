//sampler2D Input : register(s0);

/// <summary>The first input texture.</summary>
/// <defaultValue>C:\Users\L3_Admin\Desktop\FinalCompHL.bmp</defaultValue>
sampler2D compHLInput : register(s0);

/// <summary>The first input texture.</summary>
/// <defaultValue>C:\Users\L3_Admin\Desktop\FinalAlpha.bmp</defaultValue>
sampler2D alphaInput : register(s1);

/// <summary>The second input texture.</summary>
/// <defaultValue>C:\Projects\ClearView Workstation\Workstation\Workstation-sazmi\Workstation\Workstation\bin\x86\Debug\Plugins\PseudoColor\Trimat_76.bmp</defaultValue>
sampler2D matDisc2DColorTable : register(s2);

/// <summary>Change the ratio between the two Textures.  0 is 100% input source, 1 is 100</summary>
/// <minValue>1</minValue>
/// <maxValue>16</maxValue>
/// <defaultValue>1</defaultValue>
float2 Ratio : register(C0);

/// <summary>Change the ratio between the two Textures.  0 is 100% input source, 1 is 100</summary>
/// <minValue>0,0</minValue>
/// <maxValue>1,1</maxValue>
/// <defaultValue>0,0</defaultValue>
float2 TopLeft : register(C1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 outputPixel = 0.0;

	float2 uv1;
	uv1.x = ((uv.x / Ratio.x) + (TopLeft.x));
	uv1.y = ((uv.y / Ratio.y) + (TopLeft.y));

 	float alphaInputImage = tex2D(alphaInput, uv1);
 	float compHLInputImage = tex2D(compHLInput, uv);
 	
 	float adjAlpha = 0.0;
 	float adjCompHL = 0.0;
 	
 	if (alphaInputImage <= 0.04045)
 		adjAlpha = alphaInputImage/12.92;
 	else
 		adjAlpha = pow(((alphaInputImage + 0.055)/1.055),2.4);
 	
 	if (compHLInputImage <= 0.04045)
 		adjCompHL = compHLInputImage/12.92;
 	else
 		adjCompHL = pow(((compHLInputImage + 0.055)/1.055),2.4);
 	
 	float2 lookup2D = float2((1.0 - adjCompHL), adjAlpha);
 	
 	if((1.0 - adjCompHL) < 0.01)
 	{
 		outputPixel = compHLInputImage;  // texture
 	}
 	else if((1.0 - adjCompHL) < 0.15)
 	{
 		outputPixel = compHLInputImage/1.05;  // texture
 	}
 	else if((1.0 - adjCompHL) > 0.9)
 	{
 		//outputPixel = compHLInputImage*1.0;  // texture
 	}
 	else
 	{
		outputPixel = tex2D(matDisc2DColorTable, lookup2D);  // Lookup
 	}
 			
	outputPixel.a = 1.0;
 	return outputPixel;
}