// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FlamingSands/Stylized/PBR Metallic Height Blend"
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
		[Header(Secondary Textures)]_SecondAlbedoColor("Second Albedo Color", Color) = (1,1,1,1)
		[NoScaleOffset]_SecondAlbedo("Second Albedo", 2D) = "white" {}
		[NoScaleOffset]_Gloss("Second RMA", 2D) = "white" {}
		[Normal][NoScaleOffset]_SecondNormal("Second Normal", 2D) = "bump" {}
		_SecondNormalScale("Second Normal Scale", Float) = 1
		_SecondOffset("Second Offset", Vector) = (0,0,0,0)
		_SecondTiling("Second Tiling", Vector) = (1,1,0,0)
		[Header(Blending Properties)]_HeightContrast("Height Contrast", Float) = 20
		_HeightOffset("Height Offset", Float) = 0
		[Toggle]_InvertMask("Invert Mask", Float) = 0
		[Toggle]_InvertFactor("Invert Factor", Float) = 0
		_NormalDistance("Normal Distance", Float) = 0.25
		[Toggle][Enum(Base Layer,0,Secondary Layer,1)]_HeightFrom("Height From", Float) = 0
		[Enum(Custom Mask,0,Vertex Colors,1)]_BlendingFactor("Blending Factor", Float) = 0
		[NoScaleOffset]_BlendingMask("Blending Mask", 2D) = "white" {}
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
			float4 vertexColor : COLOR;
		};

		uniform float _NormalScale;
		uniform sampler2D _Normal;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float _SecondNormalScale;
		uniform sampler2D _SecondNormal;
		uniform float2 _SecondTiling;
		uniform float2 _SecondOffset;
		uniform sampler2D _BlendingMask;
		uniform float _BlendingFactor;
		uniform float _NormalDistance;
		uniform float _HeightOffset;
		uniform float4 _AlbedoColorBase;
		uniform sampler2D _MainTex;
		uniform float4 _AlbedoColor;
		uniform sampler2D _SecondAlbedo;
		uniform float4 _SecondAlbedoColor;
		uniform float _HeightContrast;
		uniform sampler2D _RMA;
		uniform sampler2D _Gloss;
		uniform float _HeightFrom;
		uniform float _InvertMask;
		uniform float _InvertFactor;
		uniform float _MetallicAmount;
		uniform float _SmoothnessAmount;


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord21_g21 = i.uv_texcoord * _Tiling + _Offset;
			float2 uv_TexCoord21_g22 = i.uv_texcoord * _SecondTiling + _SecondOffset;
			float2 uv_BlendingMask147 = i.uv_texcoord;
			float lerpResult150 = lerp( tex2D( _BlendingMask, uv_BlendingMask147 ).r , i.vertexColor.r , _BlendingFactor);
			float temp_output_16_0_g34 = lerpResult150;
			float temp_output_48_0_g34 = ( _NormalDistance + _HeightOffset );
			float clampResult41_g34 = clamp( temp_output_16_0_g34 , 0.0 , temp_output_48_0_g34 );
			float3 lerpResult155 = lerp( UnpackScaleNormal( tex2D( _Normal, uv_TexCoord21_g21 ), _NormalScale ) , UnpackScaleNormal( tex2D( _SecondNormal, uv_TexCoord21_g22 ), _SecondNormalScale ) , saturate( (0.0 + (clampResult41_g34 - 0.0) * (1.0 - 0.0) / (temp_output_48_0_g34 - 0.0)) ));
			o.Normal = lerpResult155;
			float4 tex2DNode1_g21 = tex2D( _MainTex, uv_TexCoord21_g21 );
			float4 lerpResult29_g21 = lerp( ( _AlbedoColorBase * tex2DNode1_g21 ) , ( _AlbedoColor * tex2DNode1_g21 ) , tex2DNode1_g21.a);
			float4 tex2DNode1_g22 = tex2D( _SecondAlbedo, uv_TexCoord21_g22 );
			float4 lerpResult29_g22 = lerp( tex2DNode1_g22 , ( _SecondAlbedoColor * tex2DNode1_g22 ) , tex2DNode1_g22.a);
			float4 tex2DNode3_g21 = tex2D( _RMA, uv_TexCoord21_g21 );
			float4 tex2DNode3_g22 = tex2D( _Gloss, uv_TexCoord21_g22 );
			float lerpResult144 = lerp( tex2DNode3_g21.a , tex2DNode3_g22.a , _HeightFrom);
			float temp_output_15_0_g34 = lerpResult144;
			float lerpResult29_g34 = lerp( temp_output_15_0_g34 , ( 1.0 - temp_output_15_0_g34 ) , _InvertMask);
			float lerpResult32_g34 = lerp( temp_output_16_0_g34 , ( 1.0 - temp_output_16_0_g34 ) , _InvertFactor);
			float clampResult8_g34 = clamp( ( ( ( lerpResult29_g34 - 1.0 ) + ( lerpResult32_g34 * 2.0 ) ) + _HeightOffset ) , 0.0 , 1.0 );
			float4 temp_cast_0 = (clampResult8_g34).xxxx;
			float4 clampResult10_g34 = clamp( CalculateContrast(_HeightContrast,temp_cast_0) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			float temp_output_159_0 = saturate( (clampResult10_g34).r );
			float4 lerpResult151 = lerp( lerpResult29_g21 , lerpResult29_g22 , temp_output_159_0);
			o.Albedo = lerpResult151.rgb;
			float lerpResult153 = lerp( ( _MetallicAmount * tex2DNode3_g21.g ) , ( _MetallicAmount * tex2DNode3_g22.g ) , temp_output_159_0);
			o.Metallic = lerpResult153;
			float lerpResult152 = lerp( ( _SmoothnessAmount * tex2DNode3_g21.r ) , ( _SmoothnessAmount * tex2DNode3_g22.r ) , temp_output_159_0);
			o.Smoothness = lerpResult152;
			float lerpResult154 = lerp( tex2DNode3_g21.b , tex2DNode3_g22.b , temp_output_159_0);
			o.Occlusion = lerpResult154;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
-1913;1;1906;1021;1769.481;894.4299;1.3;True;False
Node;AmplifyShaderEditor.FunctionNode;169;-1360,16;Inherit;False;PBR_Metallic_bicolor;0;;21;324b80c1d4faca64bb68a8e44aaec0ab;0;0;11;COLOR;12;COLOR;40;COLOR;39;FLOAT;0;FLOAT;31;FLOAT;19;FLOAT;30;FLOAT;23;FLOAT;27;FLOAT3;26;COLOR;33
Node;AmplifyShaderEditor.VertexColorNode;37;-1184,-496;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;147;-1536,-768;Inherit;True;Property;_BlendingMask;Blending Mask;30;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;85d8e55bf09580547a72cdbeeb101cda;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;145;-1056,-176;Inherit;False;Property;_HeightFrom;Height From;28;2;[Toggle];[Enum];Create;True;2;Base Layer;0;Secondary Layer;1;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;-1424,-560;Inherit;False;Property;_BlendingFactor;Blending Factor;29;1;[Enum];Create;True;2;Custom Mask;0;Vertex Colors;1;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;165;-1040,304;Inherit;False;PBR_Metallic_Secondary;13;;22;38e03d1762c08cf449ebe773617d6389;0;2;31;FLOAT;0;False;30;FLOAT;0;False;6;COLOR;12;FLOAT;0;FLOAT;19;FLOAT;23;FLOAT;27;FLOAT3;26
Node;AmplifyShaderEditor.LerpOp;144;-640,-224;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;150;-758,-552;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;180;-419.3861,-579.7354;Inherit;False;Height Blend;21;;34;432689a0d44ae5a47ab782d727c43695;0;2;15;FLOAT;1;False;16;FLOAT;1;False;4;FLOAT;0;FLOAT;38;FLOAT;33;FLOAT;34
Node;AmplifyShaderEditor.SaturateNode;159;-48,-304;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;151;256,-256;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;154;256,176;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;155;256,304;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;152;256,-112;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;153;256,32;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;871.0997,-181.5999;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FlamingSands/Stylized/PBR Metallic Height Blend;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;165;31;169;31
WireConnection;165;30;169;30
WireConnection;144;0;169;27
WireConnection;144;1;165;27
WireConnection;144;2;145;0
WireConnection;150;0;147;1
WireConnection;150;1;37;1
WireConnection;150;2;148;0
WireConnection;180;15;144;0
WireConnection;180;16;150;0
WireConnection;159;0;180;0
WireConnection;151;0;169;12
WireConnection;151;1;165;12
WireConnection;151;2;159;0
WireConnection;154;0;169;23
WireConnection;154;1;165;23
WireConnection;154;2;159;0
WireConnection;155;0;169;26
WireConnection;155;1;165;26
WireConnection;155;2;180;38
WireConnection;152;0;169;0
WireConnection;152;1;165;0
WireConnection;152;2;159;0
WireConnection;153;0;169;19
WireConnection;153;1;165;19
WireConnection;153;2;159;0
WireConnection;0;0;151;0
WireConnection;0;1;155;0
WireConnection;0;3;153;0
WireConnection;0;4;152;0
WireConnection;0;5;154;0
ASEEND*/
//CHKSM=D97984E0BB30F5381540B8EA620487CB3FEA9D6D