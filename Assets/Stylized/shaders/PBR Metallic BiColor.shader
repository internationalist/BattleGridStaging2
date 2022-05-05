// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FlamingSands/Stylized/PBR Metallic BiColor"
{
	Properties
	{
		[Header(Base Textures)]_AlbedoColor("Albedo Color", Color) = (1,1,1,1)
		_AlbedoColorBase("Albedo Color Base", Color) = (1,1,1,1)
		[NoScaleOffset]_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]_RMA("RMA", 2D) = "white" {}
		_MetallicAmount("Metallic Amount", Range( 0 , 1)) = 1
		_SmoothnessAmount("Smoothness Amount", Range( 0 , 1)) = 1
		[Normal][NoScaleOffset]_Normal("Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Float) = 1
		_Offset("Offset", Vector) = (0,0,0,0)
		_Tiling("Tiling", Vector) = (1,1,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _NormalScale;
		uniform sampler2D _Normal;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float4 _AlbedoColorBase;
		uniform sampler2D _MainTex;
		uniform float4 _AlbedoColor;
		uniform float _MetallicAmount;
		uniform sampler2D _RMA;
		uniform float _SmoothnessAmount;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord21_g15 = i.uv_texcoord * _Tiling + _Offset;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, uv_TexCoord21_g15 ), _NormalScale );
			float4 tex2DNode1_g15 = tex2D( _MainTex, uv_TexCoord21_g15 );
			float4 lerpResult29_g15 = lerp( ( _AlbedoColorBase * tex2DNode1_g15 ) , ( _AlbedoColor * tex2DNode1_g15 ) , tex2DNode1_g15.a);
			o.Albedo = lerpResult29_g15.rgb;
			float4 tex2DNode3_g15 = tex2D( _RMA, uv_TexCoord21_g15 );
			o.Metallic = ( _MetallicAmount * tex2DNode3_g15.g );
			o.Smoothness = ( _SmoothnessAmount * tex2DNode3_g15.r );
			o.Occlusion = tex2DNode3_g15.b;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
-1839;41;1802;957;699.37;275.36;1;True;False
Node;AmplifyShaderEditor.FunctionNode;30;-16,32;Inherit;False;PBR_Metallic_bicolor;0;;15;324b80c1d4faca64bb68a8e44aaec0ab;0;0;11;COLOR;12;COLOR;40;COLOR;39;FLOAT;0;FLOAT;31;FLOAT;19;FLOAT;30;FLOAT;23;FLOAT;27;FLOAT3;26;COLOR;33
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;640.6638,36.27554;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FlamingSands/Stylized/PBR Metallic BiColor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;30;12
WireConnection;0;1;30;26
WireConnection;0;3;30;19
WireConnection;0;4;30;0
WireConnection;0;5;30;23
ASEEND*/
//CHKSM=F3F1D01B236DD61041217C541204C5D8F64C7DD0