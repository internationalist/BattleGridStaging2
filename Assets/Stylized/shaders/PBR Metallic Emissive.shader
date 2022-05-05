// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FlamingSands/Stylized/PBR Metallic Emissive"
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
		[NoScaleOffset]_Emissive("Emissive", 2D) = "black" {}
		[HDR]_EmissiveColor("Emissive Color", Color) = (1,1,1,1)
		_Offset("Offset", Vector) = (0,0,0,0)
		_Tiling("Tiling", Vector) = (1,1,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
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
		uniform sampler2D _Emissive;
		uniform float4 _EmissiveColor;
		uniform float _MetallicAmount;
		uniform sampler2D _RMA;
		uniform float _SmoothnessAmount;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord21_g18 = i.uv_texcoord * _Tiling + _Offset;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, uv_TexCoord21_g18 ), _NormalScale );
			float4 tex2DNode1_g18 = tex2D( _MainTex, uv_TexCoord21_g18 );
			float4 lerpResult29_g18 = lerp( ( _AlbedoColorBase * tex2DNode1_g18 ) , ( _AlbedoColor * tex2DNode1_g18 ) , tex2DNode1_g18.a);
			o.Albedo = lerpResult29_g18.rgb;
			o.Emission = ( tex2D( _Emissive, uv_TexCoord21_g18 ) * _EmissiveColor ).rgb;
			float4 tex2DNode3_g18 = tex2D( _RMA, uv_TexCoord21_g18 );
			o.Metallic = ( _MetallicAmount * tex2DNode3_g18.g );
			o.Smoothness = ( _SmoothnessAmount * tex2DNode3_g18.r );
			o.Occlusion = tex2DNode3_g18.b;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17500
-1920;0;1920;1029;611.3699;332.36;1;True;False
Node;AmplifyShaderEditor.FunctionNode;33;-16,32;Inherit;False;PBR_Metallic_Base;0;;18;324b80c1d4faca64bb68a8e44aaec0ab;0;0;9;COLOR;12;FLOAT;0;FLOAT;31;FLOAT;19;FLOAT;30;FLOAT;23;FLOAT;27;FLOAT3;26;COLOR;33
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;640.6638,36.27554;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FlamingSands/Stylized/PBR Metallic Emissive;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;33;12
WireConnection;0;1;33;26
WireConnection;0;2;33;33
WireConnection;0;3;33;19
WireConnection;0;4;33;0
WireConnection;0;5;33;23
ASEEND*/
//CHKSM=809B3F7DDB8FD3EB2E0B415EEF29919F5E27B3F3