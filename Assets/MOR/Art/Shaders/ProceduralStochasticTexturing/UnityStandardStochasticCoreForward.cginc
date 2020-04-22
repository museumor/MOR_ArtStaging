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

#ifndef UNITY_STANDARD_CORE_FORWARD_INCLUDED
#define UNITY_STANDARD_CORE_FORWARD_INCLUDED

#if defined(UNITY_NO_FULL_STANDARD_SHADER)
#   define UNITY_STANDARD_SIMPLE 1
#endif

#include "UnityStandardConfig.cginc"

#if UNITY_STANDARD_SIMPLE
    #include "UnityStandardStochasticCoreForwardSimple.cginc"
    VertexOutputBaseSimple vertBase (VertexInput v) { return vertForwardBaseSimple(v); }
    VertexOutputForwardAddSimple vertAdd (VertexInput v) { return vertForwardAddSimple(v); }
    half4 fragBase (VertexOutputBaseSimple i) : SV_Target { return fragForwardBaseSimpleInternal(i); }
    half4 fragAdd (VertexOutputForwardAddSimple i) : SV_Target { return fragForwardAddSimpleInternal(i); }
#else
    #include "UnityStandardStochasticCore.cginc"
    VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
    VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
    
    float _SpacingY;
    float _SinAmplitude;
    float _SinAmplitude2;
    float _SinFrequency;
    sampler2D _BandColor;
    float4 _BandColor_ST;
    float _SecondaryScale;

    half4 fragBase (VertexOutputForwardBase i) : SV_Target { 
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
    
        #if _STRIATION
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
            i.color.g = adjust*_BandColor_ST.w;        
            adjust = (-grad+adjust);
    
            adjust *= facingFade;
            //addVal += adjust;
               
            float colourTint = adjust*_BandColor_ST.z;
            
            
    
            colourTint *= grad;
            
    
            
            
            i.color.r += colourTint;
        #endif
        
        
        
        
        half4 col = fragForwardBaseInternal(i,distanceLerp);
        
        return col; 
    }
    half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }
#endif

#endif // UNITY_STANDARD_CORE_FORWARD_INCLUDED
