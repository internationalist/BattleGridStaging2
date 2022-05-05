// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FlamingSands/Stylized/PBR Metallic Three Layers Triplanner Puddles"
{
	Properties
	{
		[Header(Base Textures)]_AlbedoColor("Albedo Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_MetallicAmount("Metallic Amount", Range( 0 , 1)) = 0
		_SmoothnessAmount("Smoothness Amount", Range( 0 , 1)) = 1
		_RMA("RMA", 2D) = "white" {}
		_NormalScale("Normal Scale", Float) = 1
		_Normal("Normal", 2D) = "bump" {}
		_Tiling("Tiling", Vector) = (1,1,0,0)
		_Falloff("Falloff", Float) = 1
		[Header(Secondary Textures)]_SecondAlbedoColor("Second Albedo Color", Color) = (1,1,1,1)
		_SecondAlbedo("Second Albedo", 2D) = "white" {}
		_SecondNormal("Second Normal", 2D) = "white" {}
		_SecondNormalScale("Second Normal Scale", Float) = 1
		_SecondTiling("Second Tiling", Vector) = (1,1,0,0)
		_Gloss("Second RMA", 2D) = "white" {}
		[Header(Trinary Textures)]_ThirdAlbedoColor("Third Albedo Color", Color) = (1,1,1,1)
		_ThirdAlbedo("Third Albedo", 2D) = "white" {}
		_ThirdNormal("Third Normal", 2D) = "white" {}
		_ThirdNormalScale("Third Normal Scale", Float) = 1
		_ThirdRMA("Third RMA", 2D) = "white" {}
		[Header(Puddles)]_WetnessColor("Wetness Color", Color) = (0.9607843,0.9607843,0.9607843,1)
		_PuddlesColorDepth("Puddles Color (Depth)", Color) = (0.2705882,0.254902,0.2078431,0.9019608)
		[Toggle(_INVERTDEPTH_ON)] _InvertDepth("Invert Depth", Float) = 1
		_PuddlesSmoothness("Puddles Smoothness", Range( 0 , 1)) = 0.95
		[HideInInspector]_WaterNormal("Water Normal", 2D) = "bump" {}
		_WetnessSmoothness("Wetness Smoothness", Range( 0 , 1)) = 0.75
		[Toggle(_PUDDLESUSEALLHEIGHTMAPS_ON)] _PuddlesUseAllHeightMaps("Puddles Use All Height Maps", Float) = 0
		[Header(Blending Properties)]_HeightContrast("Height Contrast", Float) = 20
		_HeightOffset("Height Offset", Float) = 0
		[Toggle]_InvertMask("Invert Mask", Float) = 0
		[Toggle]_InvertFactor("Invert Factor", Float) = 0
		_NormalDistance("Normal Distance", Float) = 0.25
		[Toggle][Enum(Base Layer,0,Secondary Layer,1)]_HeightFrom("Height From", Float) = 0
		[Toggle][Enum(Bellow Layer,0,Trinary Layer,1)]_Layer3HeightFrom("Layer 3 Height From", Float) = 0
		[Enum(Custom Mask,0,Vertex Colors,1)]_BlendingFactor("Blending Factor", Float) = 0
		[NoScaleOffset]_BlendingMask("Blending Mask", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _PUDDLESUSEALLHEIGHTMAPS_ON
		#pragma shader_feature_local _INVERTDEPTH_ON
		#define ASE_TEXTURE_PARAMS(textureName) textureName

		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _WaterNormal;
		uniform sampler2D _Normal;
		uniform float2 _Tiling;
		uniform float _Falloff;
		uniform float _NormalScale;
		uniform sampler2D _SecondNormal;
		uniform float2 _SecondTiling;
		uniform float _SecondNormalScale;
		uniform sampler2D _BlendingMask;
		uniform float _BlendingFactor;
		uniform float _NormalDistance;
		uniform float _HeightOffset;
		uniform sampler2D _ThirdNormal;
		uniform float _ThirdNormalScale;
		uniform float _HeightContrast;
		uniform sampler2D _RMA;
		uniform sampler2D _Gloss;
		uniform float _HeightFrom;
		uniform float _InvertMask;
		uniform float _InvertFactor;
		uniform sampler2D _ThirdRMA;
		uniform float _Layer3HeightFrom;
		uniform sampler2D _MainTex;
		uniform float4 _AlbedoColor;
		uniform sampler2D _SecondAlbedo;
		uniform float4 _SecondAlbedoColor;
		uniform sampler2D _ThirdAlbedo;
		uniform float4 _ThirdAlbedoColor;
		uniform float4 _PuddlesColorDepth;
		uniform float4 _WetnessColor;
		uniform float _MetallicAmount;
		uniform float _PuddlesSmoothness;
		uniform float _SmoothnessAmount;
		uniform float _WetnessSmoothness;


		inline float3 TriplanarSamplingSNF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			xNorm.xyz = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2( nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		inline float4 TriplanarSamplingSF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 appendResult32_g63 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 temp_output_33_0_g63 = ( appendResult32_g63 / 2.0 );
			float2 panner30_g63 = ( 1.0 * _Time.y * float2( 0.05,0.05 ) + temp_output_33_0_g63);
			float2 panner41_g63 = ( 1.0 * _Time.y * float2( -0.05,0 ) + temp_output_33_0_g63);
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 triplanar48_g41 = TriplanarSamplingSNF( _Normal, ase_worldPos, ase_worldNormal, _Falloff, _Tiling, _NormalScale, 0 );
			float3 tanTriplanarNormal48_g41 = mul( ase_worldToTangent, triplanar48_g41 );
			float temp_output_187_52 = _Falloff;
			float temp_output_36_0_g45 = temp_output_187_52;
			float3 triplanar35_g45 = TriplanarSamplingSNF( _SecondNormal, ase_worldPos, ase_worldNormal, temp_output_36_0_g45, _SecondTiling, _SecondNormalScale, 0 );
			float3 tanTriplanarNormal35_g45 = mul( ase_worldToTangent, triplanar35_g45 );
			float2 uv_BlendingMask147 = i.uv_texcoord;
			float lerpResult150 = lerp( tex2D( _BlendingMask, uv_BlendingMask147 ).r , i.vertexColor.r , _BlendingFactor);
			float temp_output_16_0_g61 = lerpResult150;
			float temp_output_48_0_g61 = ( _NormalDistance + _HeightOffset );
			float clampResult41_g61 = clamp( temp_output_16_0_g61 , 0.0 , temp_output_48_0_g61 );
			float3 lerpResult155 = lerp( tanTriplanarNormal48_g41 , tanTriplanarNormal35_g45 , saturate( (0.0 + (clampResult41_g61 - 0.0) * (1.0 - 0.0) / (temp_output_48_0_g61 - 0.0)) ));
			float temp_output_36_0_g60 = temp_output_187_52;
			float3 triplanar35_g60 = TriplanarSamplingSNF( _ThirdNormal, ase_worldPos, ase_worldNormal, temp_output_36_0_g60, _SecondTiling, _ThirdNormalScale, 0 );
			float3 tanTriplanarNormal35_g60 = mul( ase_worldToTangent, triplanar35_g60 );
			float temp_output_16_0_g62 = i.vertexColor.g;
			float temp_output_48_0_g62 = ( _NormalDistance + _HeightOffset );
			float clampResult41_g62 = clamp( temp_output_16_0_g62 , 0.0 , temp_output_48_0_g62 );
			float3 lerpResult195 = lerp( lerpResult155 , tanTriplanarNormal35_g60 , saturate( (0.0 + (clampResult41_g62 - 0.0) * (1.0 - 0.0) / (temp_output_48_0_g62 - 0.0)) ));
			float4 triplanar47_g41 = TriplanarSamplingSF( _RMA, ase_worldPos, ase_worldNormal, _Falloff, _Tiling, 1.0, 0 );
			float temp_output_187_27 = triplanar47_g41.w;
			float4 triplanar34_g45 = TriplanarSamplingSF( _Gloss, ase_worldPos, ase_worldNormal, temp_output_36_0_g45, _SecondTiling, 1.0, 0 );
			float temp_output_189_27 = triplanar34_g45.w;
			float lerpResult144 = lerp( temp_output_187_27 , temp_output_189_27 , _HeightFrom);
			float temp_output_15_0_g61 = lerpResult144;
			float lerpResult29_g61 = lerp( temp_output_15_0_g61 , ( 1.0 - temp_output_15_0_g61 ) , _InvertMask);
			float lerpResult32_g61 = lerp( temp_output_16_0_g61 , ( 1.0 - temp_output_16_0_g61 ) , _InvertFactor);
			float clampResult8_g61 = clamp( ( ( ( lerpResult29_g61 - 1.0 ) + ( lerpResult32_g61 * 2.0 ) ) + _HeightOffset ) , 0.0 , 1.0 );
			float4 temp_cast_0 = (clampResult8_g61).xxxx;
			float4 clampResult10_g61 = clamp( CalculateContrast(_HeightContrast,temp_cast_0) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			float temp_output_159_0 = saturate( (clampResult10_g61).r );
			float lerpResult203 = lerp( temp_output_187_27 , temp_output_189_27 , temp_output_159_0);
			float4 triplanar34_g60 = TriplanarSamplingSF( _ThirdRMA, ase_worldPos, ase_worldNormal, temp_output_36_0_g60, _SecondTiling, 1.0, 0 );
			float temp_output_208_27 = triplanar34_g60.w;
			float lerpResult198 = lerp( lerpResult144 , temp_output_208_27 , _Layer3HeightFrom);
			float temp_output_15_0_g62 = lerpResult198;
			float lerpResult29_g62 = lerp( temp_output_15_0_g62 , ( 1.0 - temp_output_15_0_g62 ) , _InvertMask);
			float lerpResult32_g62 = lerp( temp_output_16_0_g62 , ( 1.0 - temp_output_16_0_g62 ) , _InvertFactor);
			float clampResult8_g62 = clamp( ( ( ( lerpResult29_g62 - 1.0 ) + ( lerpResult32_g62 * 2.0 ) ) + _HeightOffset ) , 0.0 , 1.0 );
			float4 temp_cast_1 = (clampResult8_g62).xxxx;
			float4 clampResult10_g62 = clamp( CalculateContrast(_HeightContrast,temp_cast_1) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			float temp_output_197_0 = saturate( (clampResult10_g62).r );
			float lerpResult204 = lerp( lerpResult203 , temp_output_208_27 , temp_output_197_0);
			#ifdef _PUDDLESUSEALLHEIGHTMAPS_ON
				float staticSwitch209 = lerpResult204;
			#else
				float staticSwitch209 = lerpResult203;
			#endif
			float temp_output_17_0_g63 = staticSwitch209;
			float temp_output_15_0_g65 = temp_output_17_0_g63;
			float lerpResult29_g65 = lerp( temp_output_15_0_g65 , ( 1.0 - temp_output_15_0_g65 ) , _InvertMask);
			float temp_output_16_0_g65 = i.vertexColor.a;
			float lerpResult32_g65 = lerp( temp_output_16_0_g65 , ( 1.0 - temp_output_16_0_g65 ) , _InvertFactor);
			float clampResult8_g65 = clamp( ( ( ( lerpResult29_g65 - 1.0 ) + ( lerpResult32_g65 * 2.0 ) ) + _HeightOffset ) , 0.0 , 1.0 );
			float4 temp_cast_2 = (clampResult8_g65).xxxx;
			float4 clampResult10_g65 = clamp( CalculateContrast(_HeightContrast,temp_cast_2) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			float temp_output_82_0_g63 = ( 1.0 - (clampResult10_g65).r );
			float3 lerpResult8_g63 = lerp( BlendNormals( UnpackScaleNormal( tex2D( _WaterNormal, panner30_g63 ), 0.25 ) , UnpackScaleNormal( tex2D( _WaterNormal, panner41_g63 ), 0.25 ) ) , lerpResult195 , temp_output_82_0_g63);
			o.Normal = lerpResult8_g63;
			float4 triplanar43_g41 = TriplanarSamplingSF( _MainTex, ase_worldPos, ase_worldNormal, _Falloff, _Tiling, 1.0, 0 );
			float4 lerpResult41_g41 = lerp( triplanar43_g41 , ( _AlbedoColor * triplanar43_g41 ) , triplanar43_g41.w);
			float4 triplanar33_g45 = TriplanarSamplingSF( _SecondAlbedo, ase_worldPos, ase_worldNormal, temp_output_36_0_g45, _SecondTiling, 1.0, 0 );
			float4 lerpResult29_g45 = lerp( triplanar33_g45 , ( _SecondAlbedoColor * triplanar33_g45 ) , triplanar33_g45.w);
			float4 lerpResult151 = lerp( lerpResult41_g41 , lerpResult29_g45 , temp_output_159_0);
			float4 triplanar33_g60 = TriplanarSamplingSF( _ThirdAlbedo, ase_worldPos, ase_worldNormal, temp_output_36_0_g60, _SecondTiling, 1.0, 0 );
			float4 lerpResult29_g60 = lerp( triplanar33_g60 , ( _ThirdAlbedoColor * triplanar33_g60 ) , triplanar33_g60.w);
			float4 lerpResult191 = lerp( lerpResult151 , lerpResult29_g60 , temp_output_197_0);
			float4 temp_output_20_0_g63 = lerpResult191;
			#ifdef _INVERTDEPTH_ON
				float staticSwitch90_g63 = ( 1.0 - temp_output_17_0_g63 );
			#else
				float staticSwitch90_g63 = temp_output_17_0_g63;
			#endif
			float clampResult75_g63 = clamp( ( staticSwitch90_g63 + _PuddlesColorDepth.a ) , 0.0 , 1.0 );
			float4 lerpResult4_g63 = lerp( temp_output_20_0_g63 , _PuddlesColorDepth , clampResult75_g63);
			float4 lerpResult6_g63 = lerp( lerpResult4_g63 , temp_output_20_0_g63 , temp_output_82_0_g63);
			float temp_output_15_0_g64 = temp_output_17_0_g63;
			float lerpResult29_g64 = lerp( temp_output_15_0_g64 , ( 1.0 - temp_output_15_0_g64 ) , _InvertMask);
			float temp_output_16_0_g64 = i.vertexColor.b;
			float lerpResult32_g64 = lerp( temp_output_16_0_g64 , ( 1.0 - temp_output_16_0_g64 ) , _InvertFactor);
			float clampResult8_g64 = clamp( ( ( ( lerpResult29_g64 - 1.0 ) + ( lerpResult32_g64 * 2.0 ) ) + _HeightOffset ) , 0.0 , 1.0 );
			float4 temp_cast_14 = (clampResult8_g64).xxxx;
			float4 clampResult10_g64 = clamp( CalculateContrast(_HeightContrast,temp_cast_14) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			float clampResult88_g63 = clamp( ( temp_output_82_0_g63 - ( 1.0 - (clampResult10_g64).r ) ) , 0.0 , 1.0 );
			float4 lerpResult65_g63 = lerp( lerpResult6_g63 , ( _WetnessColor * temp_output_20_0_g63 ) , clampResult88_g63);
			o.Albedo = lerpResult65_g63.rgb;
			float temp_output_187_30 = _MetallicAmount;
			float lerpResult153 = lerp( ( _MetallicAmount * triplanar47_g41.y ) , ( temp_output_187_30 * triplanar34_g45.y ) , temp_output_159_0);
			float lerpResult193 = lerp( lerpResult153 , ( temp_output_187_30 * triplanar34_g60.y ) , temp_output_197_0);
			o.Metallic = lerpResult193;
			float temp_output_187_31 = _SmoothnessAmount;
			float lerpResult152 = lerp( ( _SmoothnessAmount * triplanar47_g41.x ) , ( temp_output_187_31 * triplanar34_g45.x ) , temp_output_159_0);
			float lerpResult192 = lerp( lerpResult152 , ( temp_output_187_31 * triplanar34_g60.x ) , temp_output_197_0);
			float lerpResult7_g63 = lerp( _PuddlesSmoothness , lerpResult192 , temp_output_82_0_g63);
			float clampResult58_g63 = clamp( lerpResult7_g63 , 0.0 , 1.0 );
			float lerpResult54_g63 = lerp( clampResult58_g63 , _WetnessSmoothness , clampResult88_g63);
			o.Smoothness = lerpResult54_g63;
			float lerpResult154 = lerp( triplanar47_g41.z , triplanar34_g45.z , temp_output_159_0);
			float lerpResult194 = lerp( lerpResult154 , triplanar34_g60.z , temp_output_197_0);
			float lerpResult45_g63 = lerp( 1.0 , lerpResult194 , temp_output_82_0_g63);
			o.Occlusion = lerpResult45_g63;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
1920;0;1440;849;1152.162;1023.877;2.87706;True;False
Node;AmplifyShaderEditor.FunctionNode;187;-1408,0;Inherit;False;PBR_Metallic triplanner;0;;41;f0f998ae4fe6f7044b8612e4c48c06da;0;0;12;COLOR;12;FLOAT4;40;COLOR;39;FLOAT3;26;FLOAT4;33;FLOAT;0;FLOAT;31;FLOAT;19;FLOAT;30;FLOAT;23;FLOAT;27;FLOAT;52
Node;AmplifyShaderEditor.RangedFloatNode;145;-1104,-176;Inherit;False;Property;_HeightFrom;Height From;56;2;[Toggle];[Enum];Create;True;2;Base Layer;0;Secondary Layer;1;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;-1424,-560;Inherit;False;Property;_BlendingFactor;Blending Factor;58;1;[Enum];Create;True;2;Custom Mask;0;Vertex Colors;1;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;37;-1209.764,-432.6649;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;147;-1536,-768;Inherit;True;Property;_BlendingMask;Blending Mask;59;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;959294060da18394091cf64038034145;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;189;-1024,256;Inherit;False;PBR_Metallic_Secondary_triplanner;13;;45;3af3ab9864add6e439e6f6802f952060;0;3;36;FLOAT;0;False;31;FLOAT;0;False;30;FLOAT;0;False;6;COLOR;12;FLOAT;0;FLOAT;19;FLOAT;23;FLOAT;27;FLOAT3;26
Node;AmplifyShaderEditor.LerpOp;144;-752,-224;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-912,-48;Inherit;False;Property;_Layer3HeightFrom;Layer 3 Height From;57;2;[Toggle];[Enum];Create;True;2;Bellow Layer;0;Trinary Layer;1;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;208;-368,448;Inherit;False;PBR_Metallic_Trinary_triplanner;20;;60;49d374d184d644548a9710cff58fca14;0;3;36;FLOAT;0;False;31;FLOAT;0;False;30;FLOAT;0;False;6;COLOR;12;FLOAT;0;FLOAT;19;FLOAT;23;FLOAT;27;FLOAT3;26
Node;AmplifyShaderEditor.LerpOp;150;-758,-552;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;198;-576,-80;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;180;-418.3846,-579.7354;Inherit;False;Height Blend;49;;61;432689a0d44ae5a47ab782d727c43695;0;2;15;FLOAT;1;False;16;FLOAT;1;False;4;FLOAT;0;FLOAT;38;FLOAT;33;FLOAT;34
Node;AmplifyShaderEditor.FunctionNode;196;-400,-192;Inherit;False;Height Blend;49;;62;432689a0d44ae5a47ab782d727c43695;0;2;15;FLOAT;1;False;16;FLOAT;1;False;4;FLOAT;0;FLOAT;38;FLOAT;33;FLOAT;34
Node;AmplifyShaderEditor.SaturateNode;159;-48,-304;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;203;720,-384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;197;-32,-160;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;154;256,160;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;155;256,304;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;151;256,-256;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;152;256,-112;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;153;256,32;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;204;912,-384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;193;464,64;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;191;464,-224;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;194;464,192;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;192;464,-80;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;195;464,336;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;209;1031.863,-228.4061;Inherit;False;Property;_PuddlesUseAllHeightMaps;Puddles Use All Height Maps;48;0;Create;True;0;0;False;0;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;206;1264,0;Inherit;False;Water Puddles;27;;63;df7c5e0e9061f4c41b65f98bf48c3fde;0;8;17;FLOAT;0;False;38;FLOAT;0;False;20;FLOAT4;0,0,0,0;False;21;FLOAT3;0,0,0;False;22;FLOAT;0;False;23;FLOAT;0;False;24;FLOAT;0;False;47;FLOAT;0;False;6;COLOR;0;FLOAT3;13;FLOAT;14;FLOAT;16;FLOAT;25;FLOAT;91
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1904,-96;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FlamingSands/Stylized/PBR Metallic Three Layers Triplanner Puddles;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;189;36;187;52
WireConnection;189;31;187;31
WireConnection;189;30;187;30
WireConnection;144;0;187;27
WireConnection;144;1;189;27
WireConnection;144;2;145;0
WireConnection;208;36;187;52
WireConnection;208;31;187;31
WireConnection;208;30;187;30
WireConnection;150;0;147;1
WireConnection;150;1;37;1
WireConnection;150;2;148;0
WireConnection;198;0;144;0
WireConnection;198;1;208;27
WireConnection;198;2;200;0
WireConnection;180;15;144;0
WireConnection;180;16;150;0
WireConnection;196;15;198;0
WireConnection;196;16;37;2
WireConnection;159;0;180;0
WireConnection;203;0;187;27
WireConnection;203;1;189;27
WireConnection;203;2;159;0
WireConnection;197;0;196;0
WireConnection;154;0;187;23
WireConnection;154;1;189;23
WireConnection;154;2;159;0
WireConnection;155;0;187;26
WireConnection;155;1;189;26
WireConnection;155;2;180;38
WireConnection;151;0;187;12
WireConnection;151;1;189;12
WireConnection;151;2;159;0
WireConnection;152;0;187;0
WireConnection;152;1;189;0
WireConnection;152;2;159;0
WireConnection;153;0;187;19
WireConnection;153;1;189;19
WireConnection;153;2;159;0
WireConnection;204;0;203;0
WireConnection;204;1;208;27
WireConnection;204;2;197;0
WireConnection;193;0;153;0
WireConnection;193;1;208;19
WireConnection;193;2;197;0
WireConnection;191;0;151;0
WireConnection;191;1;208;12
WireConnection;191;2;197;0
WireConnection;194;0;154;0
WireConnection;194;1;208;23
WireConnection;194;2;197;0
WireConnection;192;0;152;0
WireConnection;192;1;208;0
WireConnection;192;2;197;0
WireConnection;195;0;155;0
WireConnection;195;1;208;26
WireConnection;195;2;196;38
WireConnection;209;1;203;0
WireConnection;209;0;204;0
WireConnection;206;17;209;0
WireConnection;206;38;37;4
WireConnection;206;20;191;0
WireConnection;206;21;195;0
WireConnection;206;22;192;0
WireConnection;206;23;193;0
WireConnection;206;24;194;0
WireConnection;206;47;37;3
WireConnection;0;0;206;0
WireConnection;0;1;206;13
WireConnection;0;3;206;16
WireConnection;0;4;206;14
WireConnection;0;5;206;25
ASEEND*/
//CHKSM=921AFA8B4DF0CFCE1ABAE27C6F756185FFE031FE