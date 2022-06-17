// https://forum.unity3d.com/threads/standard-shader-with-stencil-mask.308433/
// Standard Unity shader with Stencil Ref so that other shaders can mask it
Shader "_MOR/Standard Stencil"
{
	Properties
	{
	

		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}


		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Factor", Range(0.0, 1.0)) = 1.0
        [Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        _SpecColor("Specular", Color) = (0.2,0.2,0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0
        
		_BumpScale("Scale", Float) = 1.0
		[Normal]_BumpMap("Normal Map", 2D) = "bump" {}

		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
		_ParallaxMap ("Height Map", 2D) = "black" {}

		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		[HDR]_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}

		_DetailMask("Detail Mask", 2D) = "white" {}

		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
		[Normal]_DetailNormalMap("Normal Map", 2D) = "bump" {}

		[Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0
		
		

		[HideInInspector] _EmissionScaleUI("Scale", Float) = 0.0
		[HideInInspector] _EmissionColorUI("Color", Color) = (1,1,1)


		// Blending state




		
		[Toggle(_USEDISTANCEFADE)]_UseDistanceFade ("Use Distance Fade",int) = 0
		_FadeDistanceStart("Fade Distance Start",float) =0.1
		_FadeDistanceEnd("Fade Distance End",float) = 0.5
		
		
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Blend Source", Float) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Blend Destination", Float) = 0.0
		[Enum(OFF,0,ON,1)]_ZWrite ("Z Write", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTestCompare("ZTest Compare", int) = 4 //Disabled=0,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,LessEqual=8
		[Enum(UnityEngine.Rendering.CullMode)] _CullOp("Face Culling", int) = 2 //Back
		
		
		// #MOR !!!!!***** Add any new properties under this line to have them added to the Inspector automagically.
		// UI-only data
		[Toggle(_DISABLEFOG)] _DisableFor ("DisableFog",int)= 0
		[Toggle(_DISABLEDIRECTIONAL)] _DisableDirectional ("Disable Directional Light",int)=0
		[Toggle(_USEVERTEXCOLOR)] _UseVertexColor ("Use Vertex Colour",int) = 0
		 [Toggle(_LINEARVERTEXCOLOR)] _LinearVertexColor ("Linear VertexColour",int) = 0
		[Toggle(_SATURATION)] _SetSaturation ("Set Saturation",int) = 0
		_SaturationValue ("Saturation Value",float) = 1
			// sarah difference from Standard Unity shader
		_StencilRef("Stencil Ref ID", Int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", int) = 0 //Disabled=0,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,Always=8
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Pass Operation", int) = 0 //Keep=0,Zero,Replace,IncrementSaturate,DecrementSaturate,Invert,IncrementWrap,DecrementWrap=7
        [Enum(UnityEngine.Rendering.ColorMask)]_ColorMask("Colormask",int) = 15//UnityEngine.Rendering.ColorWriteMask.RGBA
        

		
        [Toggle(_USE_AMBIENT_OVERRIDE)]_UseAmbientOverride ("Use Ambient Override",int) = 0
        _ColorAmbient ("Ambient Colour",Color) = (0,0,0,1)
        _ColorShadow ("Shadow Colour",Color) = (0,0,0,1)
        [Toggle(_RIMLIGHT)] _Rimlight ("Rimlight",int) = 0
        [HDR]_ColorRimlight ("Rimlight Colour",Color) = (1,1,1,1)
        _RimPower ("Rimlight Pow",float) = 12
        _RimGrow ("Rim Grow",float) = 0
        _RimDirection("Rim Light Direction",Vector) = (-0.2,-0.2,1,1)
        [Toggle(_MATCAP)] _Matcap ("Matcap",int) = 0
        _MatcapTex ("Matcap Texture",2D) = "black" {}
        [Toggle(_TRIPLANAR)] _Triplanar ("tiplanar mapping",int) = 0

	}
    
	CGINCLUDE
		#pragma shader_feature _ _DISABLEDIRECTIONAL
		#pragma shader_feature _ _USEDISTANCEFADE

		#pragma shader_feature _ _USE_AMBIENT_OVERRIDE
        #if !SHADER_API_MOBILE
 		    #pragma shader_feature _ _TRIPLANAR       
 		    #pragma shader_feature _ _DISABLEFOG       
 		    #pragma shader_feature _ _RIMLIGHT _MATCAP   
 		    #pragma shader_feature _ _SATURATION	
		    #pragma shader_feature _ _USESECONDARYCOLOR 		
		    #pragma skip_variants SHADOWS_CUBE SHADOWS_DEPTH SHADOWS_SOFT DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE	        
        #endif

	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" "Queue"="Geometry" }
		LOD 300

		// sarah difference from Standard Unity shader
		Stencil {
			Ref [_StencilRef]
			Comp [_StencilComp]
			Pass [_StencilOp]
			//Comp Equal
			//Pass Keep
			//Fail Keep
		}

		Cull [_CullOp]
		ZWrite [_ZWrite]
		ZTest [_ZTestCompare]
		ColorMask [_ColorMask]
    CGINCLUDE
        
        #define UNITY_SETUP_BRDF_INPUT SpecularSetup
		#define MOR_SETUP_BRDF_INPUT SpecularSetup_MOR
    ENDCG
		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma target 3.0


			// -------------------------------------
#if !SHADER_API_MOBILE
            #pragma shader_feature_local ___ _DETAIL_MULX2
			#pragma shader_feature_local _PARALLAXMAP       
			#pragma shader_feature _NORMALMAP		
			#pragma shader_feature_local _SPECGLOSSMAP	
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _ _USEVERTEXCOLOR _LINEARVERTEXCOLOR            		
#else
            #define _SPECULARHIGHLIGHTS_OFF
            #define _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _ _USEVERTEXCOLOR
#endif

			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION


            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            
			//#pragma shader_feature _METALLICGLOSSMAP



			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
            #pragma multi_compile_instancing
            
			#pragma vertex vertForwardBaseMOR
			#pragma fragment fragForwardBase_MOR

			#include "UnityStandardCore.cginc"
			
            #if _USESECONDARYCOLOR
                uniform float4 _SecondaryColor;
                uniform float4 _SecondaryEmission;
            #endif
            #if _USE_AMBIENT_OVERRIDE
                uniform float4 _ColorAmbient;
            #endif
            #if _RIMLIGHT
                uniform float4 _ColorRimlight;
                uniform float _RimPower;
                uniform float _RimGrow;
                uniform float4 _RimDirection;
            #endif
            #if _MATCAP
                uniform sampler2D _MatcapTex;
            #endif
            uniform float _FadeDistanceStart;
            uniform float _FadeDistanceEnd;
            uniform float _SaturationValue;
            uniform float4 _ColorShadow;
            
            
            #include "NorthwayStandard.cginc"
            
             #if SHADER_API_MOBILE
           half4 fragForwardBase_MOR (VertexOutputForwardBaseMOR i) : SV_Target   // backward compatibility (this used to be the fragment entry function)
             #else
           half4 fragForwardBase_MOR (VertexOutputForwardBaseMOR i,fixed vface : VFACE) : SV_Target   // backward compatibility (this used to be the fragment entry function)
            #endif
            {
            #if SHADER_API_MOBILE
                float vface=1;
            #endif
             #if _STRIATION        
                
                 float screenDistance =  length(i.pos.z/i.pos.w);	//this is a 0-1 value of screen depth.
                 screenDistance=(1.0 - screenDistance * _ZBufferParams.w) / (screenDistance * _ZBufferParams.z);
                 float distanceLerp = saturate( (screenDistance - _FadeDistanceStart) * (1.0 / _FadeDistanceEnd ) );    
                
                #if UNITY_PACK_WORLDPOS_WITH_TANGENT
                    float3 pos = float3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w);
                #else
                    //o.posWorld = posWorld.xyz;
                    float3 pos = i.posWorld.xyz;
                #endif
                //float3 pos = i.posWorld.xyz;
                
                //pos.y = pos.y + ()
            
        
                    float adjustY = (abs(frac(pos.y/8)-0.5)*2);
                    adjustY = adjustY * adjustY * (3-(2*adjustY));
                    pos.y *= _SpacingY + (adjustY*0.1);
            
                    float gapSpread = 0;//frac(pos.y/_SpacingY) * 0.2 - 0.1;
                    float heightDivisionMultiplier = 0.4 + (gapSpread);
                    
                        
                    float3 normalWorld = PerPixelWorldNormal(i.tex,  i.tangentToWorldAndPackedData);
                    float facingFade = abs(normalWorld.z);
                    float2 tilingXY = float2(0.5,0.5);
                    
                    float sinSecondaryScale = frac((dot(pos.xz,float2(.1,.04))*0.5)+0.5)*UNITY_PI*2;
            
                    
                    float ySpacing = pos.y+_SpacingY;
                    float smallSin = sin(dot(tilingXY,pos.xy) * _SinFrequency) * _SinAmplitude2;
                    float sinXY = sin(smallSin  * UNITY_PI + sinSecondaryScale) * _SinAmplitude;
                            //float sinXY = sin((sin(dot(tilingXY,pos.xy)) * 0.5 )*(UNITY_PI) + sinSecondaryScale) * 0.2;
                    float sinOut = abs(sinXY  + (pos.y * heightDivisionMultiplier ));
                
                    float addVal = frac(sinOut);
                    //float val[5] = {-0.1,0.05,-0.18,0.2,0};
                    float adjust = tex2D(_BandColor,float2( ceil((sinOut)%_BandColor_ST.x) / _BandColor_ST.x  , 0)).r; // val[)];    
                   
                    
                    float grad = saturate((1-2*abs((addVal)-0.5))*lerp(100,20,distanceLerp) + 0.5);
                    grad = lerp((grad*grad*grad)*0.5,grad,distanceLerp) * 0.8;
                    //i.color.g = adjust*_BandColor_ST.w;        
                    adjust = (-grad+adjust);
            
                    adjust *= facingFade;
                    //addVal += adjust;
                       
                    float colourTint = adjust*_BandColor_ST.z;
                    
                    
            
                    colourTint *= grad;
                    
            
                    
                    
                    i.color.rgb += colourTint;
                #endif
            
                #if SHADER_API_MOBILE
                    return fragForwardBase_MOR_Internal(i);
                #else
                    return fragForwardBase_MOR_Internal(i,vface);
                #endif
            }
            
			ENDCG
		}
		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
			#pragma target 3.0
			// GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
			#pragma exclude_renderers gles gles3

			// -------------------------------------


             #pragma shader_feature _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP
            
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog

			#pragma vertex vertForwardAdd
			#pragma fragment fragForwardAdd

			#include "UnityStandardCore.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
		    #pragma exclude_renderers gles gles3
			#pragma target 3.0
			// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT


			// -------------------------------------
            #if !SHADER_API_MOBILE
 		     #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
             #pragma shader_feature_local _SPECGLOSSMAP
             #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
             #pragma shader_feature_local _PARALLAXMAP
             #pragma multi_compile_shadowcaster
             #pragma multi_compile_instancing       
			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"                 
            #endif





			ENDCG
		}
		

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
		{
			Name "META"
			Tags { "LightMode"="Meta" }

			Cull Off

			CGPROGRAM
			#pragma exclude_renderers gles gles3
			#pragma vertex vert_meta
			#pragma fragment frag_meta

		    #pragma shader_feature _EMISSION
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION


			#include "UnityStandardMeta.cginc"
			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" "Queue"="Geometry" }
		LOD 400

		// sarah difference from Standard Unity shader
		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass[_StencilOp]
			//Comp Equal
			//Pass Keep
			//Fail Keep
		}

	// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma target 3.0
		
			//#pragma shader_feature_local _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

			//#pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #define _SPECULARHIGHLIGHTS_OFF
            #define _GLOSSYREFLECTIONS_OFF
            
            #pragma shader_feature _ _USEVERTEXCOLOR
			#pragma multi_compile_fwdbase
            
			#pragma vertex vertForwardBaseMOR
			#pragma fragment fragForwardBase_MOR

			#include "UnityStandardCore.cginc"



            #if _USE_AMBIENT_OVERRIDE
                uniform float4 _ColorAmbient;
            #endif
            uniform float _FadeDistanceStart;
            uniform float _FadeDistanceEnd;
            uniform float _SaturationValue;
            uniform float4 _ColorShadow;

            #if _RIMLIGHT
                uniform float4 _ColorRimlight;
                uniform float _RimPower;
                uniform float _RimGrow;
                uniform float4 _RimDirection;
            #endif         
        
            #include "NorthwayStandard.cginc"
             #if SHADER_API_MOBILE
           half4 fragForwardBase_MOR (VertexOutputForwardBaseMOR i) : SV_Target   // backward compatibility (this used to be the fragment entry function)
             #else
           half4 fragForwardBase_MOR (VertexOutputForwardBaseMOR i,fixed vface : VFACE) : SV_Target   // backward compatibility (this used to be the fragment entry function)
            #endif
            {
            #if SHADER_API_MOBILE
                float vface=1;
            #endif
                  #if SHADER_API_MOBILE
                return fragForwardBase_MOR_InternalAndroid(i);
                #else
                return fragForwardBase_MOR_InternalAndroid(i,vface);
                #endif
            }
            
			ENDCG
        }


	}
	Fallback off
	CustomEditor "StandardStencilGUI"
}
