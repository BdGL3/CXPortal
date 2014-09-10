/// <class>ParametricEdgeDetection</class>

/// <description>Pixel shader: Edge detection using a parametric, symetric, directional convolution kernel</description>
/// 

//   Contributed by Rene Schulte
//   Copyright (c) 2009 Rene Schulte
//   http://kodierer.blogspot.com/2009/07/livin-on-edge-silverlight-parametric_4324.html

// Parameters

/// <summary>The threshold of the edge detection.</summary>
/// <minValue>0</minValue>
/// <maxValue>2</maxValue>
/// <defaultValue>0.5</defaultValue>

//===================================

//sampler2D input : register(s0);

// new HLSL shader

/// <summary>Explain the purpose of this variable.</summary>
/// <minValue>05/minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>3.5</defaultValue>
//int Magnification : register(c0);
//float2 Pos1 : register(c1);
//float2 Pos2 : register(c2); 
//float SampleI : register(c3);
//int Magnification : register(c3);

//float4 main(float2 uv : TEXCOORD) : COLOR 
//{ 
//	float4 Color= tex2D( input , uv.xy);
//}
//===============================



//sampler2D input : register(s0);



float Threshhold : register(C0);

/// <summary>Kernel first column top. Default is the Sobel operator.</summary>
/// <minValue>-10</minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>1</defaultValue>
float K00 : register(C1);

/// <summary>Kernel first column middle. Default is the Sobel operator.</summary>
/// <minValue>-10</minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>2</defaultValue>
float K01 : register(C2);

/// <summary>Kernel first column bottom. Default is the Sobel operator.</summary>
/// <minValue>-10</minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>1</defaultValue>
float K02 : register(C3);

/// <summary>The size of the texture.</summary>
/// <minValue>1,1</minValue>
/// <maxValue>2048,2048</maxValue>
/// <defaultValue>512,512</defaultValue>
float2 TextureSize : register(C4);

float2 StartPoint  : register(C6);

float2 EndPoint  : register(C7);



// Static Vars
static float ThreshholdSq = Threshhold * Threshhold;
static float2 TextureSizeInv = 1.0 / TextureSize;
static float K20 = -K00; // Kernel last column top
static float K21 = -K01; // Kernel last column middle
static float K22 = -K02; // Kernel last column bottom
sampler2D TexSampler : register(S0);

// Shader
float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 Color= tex2D( TexSampler, uv.xy);
	//select image rect
	if((uv.x>=StartPoint.x) && 
	   (uv.y>=StartPoint.y) &&
	   (uv.x<=EndPoint.x)   && 
	   (uv.y<=EndPoint.y) )
	  
   {
	// Calculate pixel offsets
    float2 offX = float2(TextureSizeInv.x, 0);
    float2 offY = float2(0, TextureSizeInv.y);
    
    // Sample texture
	// Top row
	float2 texCoord = uv - offY;
    float4 c00 = tex2D(TexSampler, texCoord - offX);
    float4 c01 = tex2D(TexSampler, texCoord);
    float4 c02 = tex2D(TexSampler, texCoord + offX);
    
	// Middle row
	texCoord = uv;
    float4 c10 = tex2D(TexSampler, texCoord - offX);
    float4 c12 = tex2D(TexSampler, texCoord + offX);
    
	// Bottom row
	texCoord = uv + offY;
    float4 c20 = tex2D(TexSampler, texCoord - offX);
    float4 c21 = tex2D(TexSampler, texCoord);
    float4 c22 = tex2D(TexSampler, texCoord + offX);
    
    // Convolution
    float4 sx = 0;
    float4 sy = 0;
    
	// Convolute X
    sx += c00 * K00;
    sx += c01 * K01;
    sx += c02 * K02;
    sx += c20 * K20;
    sx += c21 * K21;
    sx += c22 * K22; 
    
	// Convolute Y
    sy += c00 * K00;
    sy += c02 * K20;
    sy += c10 * K01;
    sy += c12 * K21;
    sy += c20 * K02;
    sy += c22 * K22;     
    
	// Add and apply Threshold
    float4 s = (sx * sx + sy * sy);
    float4 edge = 1;
    edge = 1-s;//float4(	s.r <= ThreshholdSq,
			//		    s.g <= ThreshholdSq,
			//		    s.b <= ThreshholdSq, 
			//		    0); // Alpha is always 1!
    return edge;
    }//end select
    else
      return Color;
}
