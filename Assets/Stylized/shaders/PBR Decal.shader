// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FlamingSands/Stylized/PBR Decal"
{
	Properties
	{
		[Header(Base Textures)]_AlbedoColor("Albedo Color", Color) = (1,1,1,1)
		_AlbedoColorBase("Albedo Color Base", Color) = (1,1,1,1)
		[NoScaleOffset]_MainTex("Albedo", 2D) = "white" {}
		_MetallicAmount("Metallic Amount", Range( 0 , 1)) = 1
		_SmoothnessAmount("Smoothness Amount", Range( 0 , 1)) = 1
		_Offset("Offset", Vector) = (0,0,0,0)
		_Tiling("Tiling", Vector) = (1,1,0,0)
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _AlbedoColorBase;
		uniform sampler2D _MainTex;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float4 _AlbedoColor;
		uniform float _MetallicAmount;
		uniform float _SmoothnessAmount;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord21_g4 = i.uv_texcoord * _Tiling + _Offset;
			float4 tex2DNode1_g4 = tex2D( _MainTex, uv_TexCoord21_g4 );
			float4 lerpResult29_g4 = lerp( ( _AlbedoColorBase * tex2DNode1_g4 ) , ( _AlbedoColor * tex2DNode1_g4 ) , tex2DNode1_g4.a);
			o.Albedo = lerpResult29_g4.rgb;
			float4 temp_output_5_40 = tex2DNode1_g4;
			o.Metallic = ( temp_output_5_40 * _MetallicAmount ).r;
			o.Smoothness = ( temp_output_5_40 * _SmoothnessAmount ).r;
			o.Alpha = 1;
			clip( temp_output_5_40.r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
-1839;41;1802;957;1390;275;1;True;False
Node;AmplifyShaderEditor.FunctionNode;5;-640,0;Inherit;False;PBR_Metallic_Base;0;;4;324b80c1d4faca64bb68a8e44aaec0ab;0;0;11;COLOR;12;COLOR;40;COLOR;39;FLOAT;0;FLOAT;31;FLOAT;19;FLOAT;30;FLOAT;23;FLOAT;27;FLOAT3;26;COLOR;33
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-368,64;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-368,160;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-727,372;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RoundOpNode;10;-468,375;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FlamingSands/Stylized/PBR Decal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;True;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;5;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;13;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;5;40
WireConnection;6;1;5;31
WireConnection;7;0;5;40
WireConnection;7;1;5;30
WireConnection;10;0;9;2
WireConnection;0;0;5;12
WireConnection;0;3;7;0
WireConnection;0;4;6;0
WireConnection;0;9;5;40
WireConnection;0;10;5;40
ASEEND*/
//CHKSM=32DC35252D2B48E1AC4A946545B8D9D6FC773442