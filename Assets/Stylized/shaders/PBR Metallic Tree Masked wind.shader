// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FlamingSands/Stylized/Nature/Tree Masked wind"
{
	Properties
	{
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 4
		_TessMin( "Tess Min Distance", Float ) = 10
		_TessMax( "Tess Max Distance", Float ) = 25
		_AlphaClip("Alpha Clip", Range( 0 , 1)) = 0.5
		[Header(Base Textures)]_AlbedoColor("Albedo Color", Color) = (1,1,1,1)
		[NoScaleOffset]_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]_RMA("RMA", 2D) = "white" {}
		_MetallicAmount("Metallic Amount", Range( 0 , 1)) = 1
		_SmoothnessAmount("Smoothness Amount", Range( 0 , 1)) = 1
		[Normal][NoScaleOffset]_Normal("Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Float) = 1
		_Offset("Offset", Vector) = (0,0,0,0)
		_Tiling("Tiling", Vector) = (1,1,0,0)
		[Header(Wind Settings)]_windmap("wind map", 2D) = "white" {}
		[Toggle]_YMasking("Y Masking", Float) = 0
		_WindStrength("Wind Strength", Range( 0 , 0.4)) = 0.1
		_WindRadius("Wind Radius", Range( 0 , 30)) = 10
		_WindSpeed("Wind Speed", Range( 0 , 0.4)) = 0.1
		[Header(Vertex AO)]_VertexAOColor("Vertex AO Color", Color) = (0,0,0,0)
		_VertexAO("Vertex AO", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _AlphaClip;
		uniform float _WindStrength;
		uniform sampler2D _windmap;
		uniform float _WindSpeed;
		uniform float _WindRadius;
		uniform float _YMasking;
		uniform float _NormalScale;
		uniform sampler2D _Normal;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform sampler2D _MainTex;
		uniform float4 _AlbedoColor;
		uniform float4 _VertexAOColor;
		uniform float _VertexAO;
		uniform float _MetallicAmount;
		uniform sampler2D _RMA;
		uniform float _SmoothnessAmount;
		uniform float _TessValue;
		uniform float _TessMin;
		uniform float _TessMax;

		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _TessMin, _TessMax, _TessValue );
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float dotResult317 = dot( v.color.r , v.color.r );
			float mulTime14_g15 = _Time.y * _WindSpeed;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 panner15_g15 = ( mulTime14_g15 * float2( 1,1 ) + (( ase_worldPos / _WindRadius )).xz);
			float4 temp_cast_0 = (0.0).xxxx;
			float lerpResult24_g15 = lerp( 0.0 , pow( ( 1.0 - (v.texcoord.xy).y ) , 4.0 ) , _YMasking);
			float4 lerpResult10_g15 = lerp( ( _WindStrength * tex2Dlod( _windmap, float4( panner15_g15, 0, 1.0) ) ) , temp_cast_0 , lerpResult24_g15);
			v.vertex.xyz += ( dotResult317 * lerpResult10_g15 ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord21_g11 = i.uv_texcoord * _Tiling + _Offset;
			float2 temp_output_42_0_g11 = uv_TexCoord21_g11;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, temp_output_42_0_g11 ), _NormalScale );
			float4 tex2DNode1_g11 = tex2D( _MainTex, temp_output_42_0_g11 );
			float4 lerpResult41_g11 = lerp( tex2DNode1_g11 , ( _AlbedoColor * tex2DNode1_g11 ) , tex2DNode1_g11.a);
			float4 temp_output_306_12 = lerpResult41_g11;
			float4 temp_cast_0 = (1.0).xxxx;
			float4 lerpResult313 = lerp( _VertexAOColor , temp_cast_0 , i.vertexColor.r);
			float4 lerpResult298 = lerp( temp_output_306_12 , ( temp_output_306_12 * lerpResult313 ) , _VertexAO);
			o.Albedo = lerpResult298.rgb;
			float4 tex2DNode3_g11 = tex2D( _RMA, temp_output_42_0_g11 );
			o.Metallic = ( _MetallicAmount * tex2DNode3_g11.g );
			o.Smoothness = ( _SmoothnessAmount * tex2DNode3_g11.r );
			o.Occlusion = tex2DNode3_g11.b;
			o.Alpha = 1;
			clip( tex2DNode1_g11.a - _AlphaClip );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
0;533;1216;478;1068.606;-227.2228;1.408;True;False
Node;AmplifyShaderEditor.RangedFloatNode;312;-1271.482,228.7869;Inherit;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;290;-1344,304;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;311;-1133.482,411.7869;Inherit;False;Property;_VertexAOColor;Vertex AO Color;24;0;Create;True;0;0;False;1;Header(Vertex AO);0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;306;-1152,-192;Inherit;False;PBR_Metallic;6;;11;5c7e6f5447bf46d409c2c07a21c9b3e8;0;1;42;FLOAT2;0,0;False;12;COLOR;12;COLOR;39;COLOR;40;FLOAT;43;FLOAT3;26;COLOR;33;FLOAT;19;FLOAT;30;FLOAT;0;FLOAT;31;FLOAT;23;FLOAT;27
Node;AmplifyShaderEditor.LerpOp;313;-896,192;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;299;-1104,-400;Float;False;Property;_VertexAO;Vertex AO;25;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;291;-688,-400;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;317;-628.476,446.8122;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;318;-706.5403,636.4227;Inherit;False;Wind Shader Foliage;18;;15;000d47478642950478bb08115e3c5f52;0;0;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;301;-464,192;Float;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;298;-396.6223,-447.276;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;309;-765.3501,-51.65781;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;300;-160,176;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;289;-275.4776,557.7478;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;314;-1152,640;Inherit;False;Property;_AlphaClip;Alpha Clip;5;0;Create;True;0;0;True;0;0.5;0.25;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;128,0;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;FlamingSands/Stylized/Nature/Tree Masked wind;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;0;0;False;0;0;False;-1;-1;0;True;314;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;313;0;311;0
WireConnection;313;1;312;0
WireConnection;313;2;290;1
WireConnection;291;0;306;12
WireConnection;291;1;313;0
WireConnection;317;0;290;1
WireConnection;317;1;290;1
WireConnection;298;0;306;12
WireConnection;298;1;291;0
WireConnection;298;2;299;0
WireConnection;309;0;306;40
WireConnection;300;0;301;0
WireConnection;300;1;290;1
WireConnection;300;2;299;0
WireConnection;289;0;317;0
WireConnection;289;1;318;0
WireConnection;0;0;298;0
WireConnection;0;1;306;26
WireConnection;0;3;306;19
WireConnection;0;4;306;0
WireConnection;0;5;306;23
WireConnection;0;10;306;43
WireConnection;0;11;289;0
ASEEND*/
//CHKSM=AEB4EAFC1DE24ACB74725BF5D4D788585728227F