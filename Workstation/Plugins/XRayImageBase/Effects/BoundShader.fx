sampler2D input : register(s0);

// new HLSL shader

/// <summary>Explain the purpose of this variable.</summary>
/// <minValue>05/minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>3.5</defaultValue>
float LowBound : register(c0);
float UprBound : register(c1);
//float2 Pos1 : register(c1);
//float2 Pos2 : register(c2); 
//float SampleI : register(c3);

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	float4 Color= tex2D( input , uv.xy);
	//float2 samplePoint = uv;
//	float x=samplePoint.x;
//	float y=samplePoint.y;
	
	if((Color[0] < LowBound) || (Color[0] > UprBound ))  //or
	   {
	     //samplePoint.x=x/Magnification;
	     //samplePoint.y=y/Magnification;
	     Color=0; 
	     //Color.g=0;
	     //Color.b=0;
	     
	    }
	   else
	     Color=Color;

   return Color;
}
