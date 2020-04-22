// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// ----------------------------------------------------------------------------
// Modified Unity Standard Shader for Procedural Stochastic Textures
// 2019 Unity Labs
// Paper:				https://eheitzresearch.wordpress.com/722-2/
// Technical chapter:	https://eheitzresearch.wordpress.com/738-2/
// Authors: 
// Thomas Deliot		<thomasdeliot@unity3d.com>
// Eric Heitz			<eric@unity3d.com>
// This software is a research prototype adapted for Unity in the hopes that it
// will be useful, but without any warranty of usability or maintenance. The
// comments in the code refer to specific sections of the Technical chapter.
// ----------------------------------------------------------------------------

Shader "StandardStochastic"
{
    Properties
    {
        _FadeDistanceStart("Fade Distance Start",float) =10
		_FadeDistanceEnd("Fade Distance End",float) = 200
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


		// ------------High Performance By-Example Noise Sampling----------------------
		_MainTexT("Albedo", 2D) = "white" {}
		_MetallicGlossMapT("Metallic", 2D) = "white" {}
		_ParallaxMapT("Height Map", 2D) = "black" {}
		_BumpMapT("Normal Map", 2D) = "bump" {}
		_OcclusionMapT("Occlusion", 2D) = "white" {}
		_EmissionMapT("Emission", 2D) = "white" {}
		_DetailMaskT("Detail Mask", 2D) = "white" {}
		_DetailAlbedoMapT("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapT("Normal Map", 2D) = "bump" {}

		_MainTexInvT("Albedo", 2D) = "white" {}
		_MetallicGlossMapInvT("Metallic", 2D) = "white" {}
		_ParallaxMapInvT("Height Map", 2D) = "black" {}
		_BumpMapInvT("Normal Map", 2D) = "bump" {}
		_OcclusionMapInvT("Occlusion", 2D) = "white" {}
		_EmissionMapInvT("Emission", 2D) = "white" {}
		_DetailMaskInvT("Detail Mask", 2D) = "white" {}
		_DetailAlbedoMapInvT("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapInvT("Normal Map", 2D) = "bump" {}

		// Only with DXT compression (Section 1.6)
		_MainTexDXTScalers("_MainTexDXTScalers", Vector) = (0,0,0,0)
		_DetailAlbedoMapDXTScalers("_DetailAlbedoMapDXTScalers", Vector) = (0,0,0,0)
		_BumpMapDXTScalers("_BumpMapDXTScalers", Vector) = (0,0,0,0)
		_DetailNormalMapDXTScalers("_DetailNormalMapDXTScalers", Vector) = (0,0,0,0)
		_EmissionMapDXTScalers("_EmissionMapDXTScalers", Vector) = (0,0,0,0)

		//Decorrelated color space vectors and origins, used on albedo and normal maps
		_MainTexColorSpaceOrigin("_MainTexColorSpaceOrigin", Vector) = (0,0,0,0)
		_MainTexColorSpaceVector1("_MainTexColorSpaceVector1", Vector) = (0,0,0,0)
		_MainTexColorSpaceVector2("_MainTexColorSpaceVector2", Vector) = (0,0,0,0)
		_MainTexColorSpaceVector3("_MainTexColorSpaceVector3", Vector) = (0,0,0,0)
		_DetailAlbedoColorSpaceOrigin("_DetailAlbedoColorSpaceOrigin", Vector) = (0,0,0,0)
		_DetailAlbedoColorSpaceVector1("_DetailAlbedoColorSpaceVector1", Vector) = (0,0,0,0)
		_DetailAlbedoColorSpaceVector2("_DetailAlbedoColorSpaceVector2", Vector) = (0,0,0,0)
		_DetailAlbedoColorSpaceVector3("_DetailAlbedoColorSpaceVector3", Vector) = (0,0,0,0)
		_BumpMapColorSpaceOrigin("_BumpMapColorSpaceOrigin", Vector) = (0,0,0,0)
		_BumpMapColorSpaceVector1("_BumpMapColorSpaceVector1", Vector) = (0,0,0,0)
		_BumpMapColorSpaceVector2("_BumpMapColorSpaceVector2", Vector) = (0,0,0,0)
		_BumpMapColorSpaceVector3("_BumpMapColorSpaceVector3", Vector) = (0,0,0,0)
		_DetailNormalColorSpaceOrigin("_DetailNormalColorSpaceOrigin", Vector) = (0,0,0,0)
		_DetailNormalColorSpaceVector1("_DetailNormalColorSpaceVector1", Vector) = (0,0,0,0)
		_DetailNormalColorSpaceVector2("_DetailNormalColorSpaceVector2", Vector) = (0,0,0,0)
		_DetailNormalColorSpaceVector3("_DetailNormalColorSpaceVector3", Vector) = (0,0,0,0)
		_EmissionColorSpaceOrigin("_EmissionColorSpaceOrigin", Vector) = (0,0,0,0)
		_EmissionColorSpaceVector1("_EmissionColorSpaceVector1", Vector) = (0,0,0,0)
		_EmissionColorSpaceVector2("_EmissionColorSpaceVector2", Vector) = (0,0,0,0)
		_EmissionColorSpaceVector3("_EmissionColorSpaceVector3", Vector) = (0,0,0,0)

		[HideInInspector] _StochasticInputSelected("_StochasticInputSelected", Int) = 0
		 // ----------------------------------------------------------------------------

        // Blending state




		
		[Toggle(_USEDISTANCEFADE)]_UseDistanceFade ("Use Distance Fade",int) = 0
		_FadeDistanceStart("Fade Distance Start",float) =0.1
		_FadeDistanceEnd("Fade Distance End",float) = 0.5
		
		[Toggle(_USESECONDARYCOLOR)] _UseSecondaryColor ("Use Secondary Color",Int) = 0
		_SecondaryColor ("SecondaryColor",Color) = (1,1,1,1)
		_SecondaryEmission ("SecondaryEmissionColor",Color) = (0.65,0.65,0.65,1)
		
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
		[Toggle(_USEVERTEXCOLOR)] _UseVertexColor ("Use Vertex Color",int) = 0
		 [Toggle(_LINEARVERTEXCOLOR)] _LinearVertexColor ("Linear VertexColor",int) = 0
		[Toggle(_SATURATION)] _SetSaturation ("Set Saturation",int) = 0
		_SaturationValue ("saturation value",float) = 1
        
        
        

        
_StencilRef("Stencil Ref ID", Int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", int) = 0 //Disabled=0,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,Always=8
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Pass Operation", int) = 0 //Keep=0,Zero,Replace,IncrementSaturate,DecrementSaturate,Invert,IncrementWrap,DecrementWrap=7
        [Enum(UnityEngine.Rendering.ColorMask)]_ColorMask("Colormask",int) = 15//UnityEngine.Rendering.ColorWriteMask.RGBA
        

		
        [Toggle(_USE_AMBIENT_OVERRIDE)]_UseAmbientOverride ("Use Ambient Override",int) = 0
        _ColorAmbient ("Ambient Color",Color) = (0,0,0,1)
        _ColorShadow ("Shadow Color",Color) = (0,0,0,1)
        [Toggle(_RIMLIGHT)] _Rimlight ("Rimlight",int) = 0
        [HDR]_ColorRimlight ("Rimlight Color",Color) = (1,1,1,1)
        _RimPower ("Rimlight Pow",float) = 12
        _RimGrow ("Rim Grow",float) = 0
        _RimDirection("Rim Light Direction",Vector) = (-0.2,-0.2,1,1)
        [Toggle(_MATCAP)] _Matcap ("Matcap",int) = 0
        _MatcapTex ("Matcap Texture",2D) = "black" {}
                [Toggle(_TRIPLANAR)] _Triplanar ("tiplanar mapping",int) = 0
        
        
        
        
        _SpacingY ("Y spacing",float) = 1
        _BandColor ("BandColor X=tile,OffsetX=Multiply",2D) = "white" {}
        _SinAmplitude ("SinAmplitude",float) = 0.2
        _SinFrequency("_SinFrequency",float) = 0.5
        _SinAmplitude2 ("SinAmplitude",float) = 2
        [Toggle(_STRIATION)]_Striation("Striation",int) = 1
     
        [Toggle(_CONTRAST)]_Contrast("Contrast", int) = 0
        _StandardContrastMul("Non Stochastic Contrast",float) = 1
        _StandardContrastAdd("Non Stochastic Contrast",float) = 0
        
        
        
    }
	CGINCLUDE
	
	  
		#pragma shader_feature _ _TRIPLANAR
		#pragma shader_feature _ _USESECONDARYCOLOR
		#pragma shader_feature _ _DISABLEDIRECTIONAL
		#pragma shader_feature _ _USEDISTANCEFADE
		#pragma shader_feature _ _DISABLEFOG
		#pragma shader_feature _ _SATURATION
		#pragma shader_feature _ _USE_AMBIENT_OVERRIDE
		#pragma shader_feature _ _RIMLIGHT _MATCAP

        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 600


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            			#pragma exclude_renderers gles gles3
            #pragma target 3.0

            // -------------------------------------
            #pragma shader_feature_local _ _STRIATION
            #pragma shader_feature_local _ _STOCHASTIC
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP

			#pragma shader_feature _STOCHASTIC_ALBEDO
			#pragma shader_feature _STOCHASTIC_NORMAL
			#pragma shader_feature _STOCHASTIC_HEIGHT
			#pragma shader_feature _STOCHASTIC_OCCLUSION
			#pragma shader_feature _STOCHASTIC_SPECMETAL
			#pragma shader_feature _STOCHASTIC_EMISSION
			#pragma shader_feature _STOCHASTIC_DETAILMASK
			#pragma shader_feature _STOCHASTIC_DETAILALBEDO
			#pragma shader_feature _STOCHASTIC_DETAILNORMAL

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "UnityStandardStochasticCoreForward.cginc"

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
            			#pragma exclude_renderers gles gles3
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

			#pragma shader_feature _STOCHASTIC_ALBEDO
			#pragma shader_feature _STOCHASTIC_NORMAL
			#pragma shader_feature _STOCHASTIC_HEIGHT
			#pragma shader_feature _STOCHASTIC_OCCLUSION
			#pragma shader_feature _STOCHASTIC_SPECMETAL
			#pragma shader_feature _STOCHASTIC_EMISSION
			#pragma shader_feature _STOCHASTIC_DETAILMASK
			#pragma shader_feature _STOCHASTIC_DETAILALBEDO
			#pragma shader_feature _STOCHASTIC_DETAILNORMAL

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "UnityStandardStochasticCoreForward.cginc"

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

            // -------------------------------------


            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Deferred pass
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            CGPROGRAM
            			#pragma exclude_renderers gles gles3
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

			#pragma shader_feature _STOCHASTIC_ALBEDO
			#pragma shader_feature _STOCHASTIC_NORMAL
			#pragma shader_feature _STOCHASTIC_HEIGHT
			#pragma shader_feature _STOCHASTIC_OCCLUSION
			#pragma shader_feature _STOCHASTIC_SPECMETAL
			#pragma shader_feature _STOCHASTIC_EMISSION
			#pragma shader_feature _STOCHASTIC_DETAILMASK
			#pragma shader_feature _STOCHASTIC_DETAILALBEDO
			#pragma shader_feature _STOCHASTIC_DETAILNORMAL

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferred
            #pragma fragment fragDeferred

            #include "UnityStandardStochasticCore.cginc"

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
			#pragma target 3.0

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

			#pragma shader_feature _STOCHASTIC_ALBEDO
			#pragma shader_feature _STOCHASTIC_NORMAL
			#pragma shader_feature _STOCHASTIC_HEIGHT
			#pragma shader_feature _STOCHASTIC_OCCLUSION
			#pragma shader_feature _STOCHASTIC_SPECMETAL
			#pragma shader_feature _STOCHASTIC_EMISSION
			#pragma shader_feature _STOCHASTIC_DETAILMASK
			#pragma shader_feature _STOCHASTIC_DETAILALBEDO
			#pragma shader_feature _STOCHASTIC_DETAILNORMAL

            #include "UnityStandardStochasticMeta.cginc"
            ENDCG
        }
    }

SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" "Queue"="Geometry" }
		LOD 400

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
		//ColorMask [_ColorMask]
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
			
            #pragma shader_feature_local _ _CONTRAST
            #pragma shader_feature_local _ _STRIATION
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION
			#pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _ _GLOSSYREFLECTIONS_OFF
            
			//#pragma shader_feature _METALLICGLOSSMAP

			#pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature _ _USEVERTEXCOLOR _LINEARVERTEXCOLOR
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
            
            
            #include "../NorthwayStandard.cginc"
          
            float _SpacingY;
            float _SinAmplitude;
            float _SinAmplitude2;
            float _SinFrequency;
            sampler2D _BandColor;
            float4 _BandColor_ST;
            float _SecondaryScale;
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
                    float3 pos = i.posWorld.xyz;
                #endif
        
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

                    float sinOut = abs(sinXY  + (pos.y * heightDivisionMultiplier ));
                
                    float addVal = frac(sinOut);
                    float adjust = tex2D(_BandColor,float2( ceil((sinOut)%_BandColor_ST.x) / _BandColor_ST.x  , 0)).r; // val[)];    
                   
                    
                    float grad = saturate((1-2*abs((addVal)-0.5))*lerp(100,20,distanceLerp) + 0.5);
                    grad = lerp((grad*grad*grad)*0.5,grad,distanceLerp) * 0.8;    
                    adjust = (-grad+adjust);
            
                    adjust *= facingFade;

                       
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
            			#pragma exclude_renderers gles gles3
            #pragma target 3.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

			#pragma shader_feature _STOCHASTIC_ALBEDO
			#pragma shader_feature _STOCHASTIC_NORMAL
			#pragma shader_feature _STOCHASTIC_HEIGHT
			#pragma shader_feature _STOCHASTIC_OCCLUSION
			#pragma shader_feature _STOCHASTIC_SPECMETAL
			#pragma shader_feature _STOCHASTIC_EMISSION
			#pragma shader_feature _STOCHASTIC_DETAILMASK
			#pragma shader_feature _STOCHASTIC_DETAILALBEDO
			#pragma shader_feature _STOCHASTIC_DETAILNORMAL

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "UnityStandardStochasticCoreForward.cginc"

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

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

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
			#pragma target 3.0

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature_local _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

			#pragma shader_feature _STOCHASTIC_ALBEDO
			#pragma shader_feature _STOCHASTIC_NORMAL
			#pragma shader_feature _STOCHASTIC_HEIGHT
			#pragma shader_feature _STOCHASTIC_OCCLUSION
			#pragma shader_feature _STOCHASTIC_SPECMETAL
			#pragma shader_feature _STOCHASTIC_EMISSION
			#pragma shader_feature _STOCHASTIC_DETAILMASK
			#pragma shader_feature _STOCHASTIC_DETAILALBEDO
			#pragma shader_feature _STOCHASTIC_DETAILNORMAL

            #include "UnityStandardStochasticMeta.cginc"
            ENDCG
        }
    }


    FallBack off
    //CustomEditor "StandardStochasticShaderGUI"
    	CustomEditor "StandardStencilGUI"
}
