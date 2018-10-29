//Code written by Przemyslaw Zaworski
//https://github.com/przemyslawzaworski

Shader "Point Cloud"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Pass
	{
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off
		LOD 200
		CGPROGRAM
#pragma vertex vertex_shader
#pragma fragment pixel_shader
#pragma target 5.0

		struct Point
	{
		float3 position;
		float4 colour;
	};
	StructuredBuffer<Point> cloud;
	sampler2D _MainTex;


	struct type
	{
		float4 vertex : SV_POSITION;
		float4 variable : TEXCOORD1;
	};

	type vertex_shader(uint id : SV_VertexID)
	{
		type vs;
		Point T = cloud[id];
		vs.variable = T.colour;
		vs.vertex = UnityObjectToClipPos(float4(T.position,1.0));
		return vs;
	}

	float4 pixel_shader(type ps) : SV_TARGET
	{
	return ps.variable;
	}

		ENDCG
	}
	}
}