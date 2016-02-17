matrix viewProj;
float lineRadius;

// Per-line instance data:
float4 instanceData[200]; // (x0, y0, rho, theta) 

float texture_x_start = 0;
float texture_x_end = 1;


struct LINE_VS_INPUT
{
	float4 pos : POSITION;
	float2 vertRhoTheta : NORMAL;
	float2 vertScaleTrans : TEXCOORD0;
	float instanceIndex : TEXCOORD1;
};


struct LINE_VS_OUTPUT
{
	float4 position : POSITION;
	float3 polar : TEXCOORD0;
	float2 posModelSpace : TEXCOORD1;
	float2 tex   : TEXCOORD2;
};

LINE_VS_OUTPUT LineVertexShader(LINE_VS_INPUT In)
{
	LINE_VS_OUTPUT Out = (LINE_VS_OUTPUT)0;
	float4 pos = In.pos;
	float distanceToOriginNormalized = In.vertScaleTrans.y;
	float x0 = instanceData[In.instanceIndex].x;
	float y0 = instanceData[In.instanceIndex].y;
	float rho = instanceData[In.instanceIndex].z;
	float theta = instanceData[In.instanceIndex].w;

	// Scale X by lineRadius, and translate X by rho, in worldspace
	// based on what part of the line we're on
	float vertScale = In.vertScaleTrans.x;
	float vertTrans = In.vertScaleTrans.y;
	pos.x *= (vertScale * lineRadius);
	pos.x += (vertTrans * rho);

	// Always scale Y by lineRadius regardless of what part of the line we're on
	pos.y *= lineRadius;

	// Now the vertex is adjusted for the line length and radius, and is 
	// ready for the usual world/view/projection transformation.

	// World matrix is rotate(theta) * translate(p0)
	matrix worldMatrix =
	{
		cos(theta), sin(theta), 0, 0,
		-sin(theta), cos(theta), 0, 0,
		0, 0, 1, 0,
		x0, y0, 0, 1
	};

	Out.position = mul(mul(pos, worldMatrix), viewProj);

	Out.polar = float3(In.vertRhoTheta, vertTrans);
	 
	Out.tex = float2((distanceToOriginNormalized - texture_x_start) / (texture_x_end - texture_x_start),
		(-In.pos[1] + 1) / 2.0);

	Out.posModelSpace.xy = pos.xy;

	return Out;
}