// Copyright 2017 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "Brush/Special/Comet" {
Properties {
  _MainTex ("Texture", 2D) = "white" {}
  _AlphaMask("Alpha Mask", 2D) = "white" {}
  _Speed ("Animation Speed", Range (0,1)) = 1
  _EmissionGain ("Emission Gain", Range(0, 1)) = 0.5
  [Toggle(_GAIN)]_UseGain("Use Gain", int) = 0
      _MORMultiplier ("MOR vcolor multiply",float) = 1
  [Toggle(_USEDISTANCEFADE)]_UseDistanceFade ("Use Distance Fade",int) = 0
  _FadeDistanceStart("Fade Distance Start",float) =0.1
    _FadeDistanceEnd("Fade Distance Size",float) = 2
      
}

Category {
  Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
  Blend One One // SrcAlpha One
  BlendOp Add, Min
  ColorMask RGBA
  Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

  SubShader {
    Pass {

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile __ AUDIO_REACTIVE
      #pragma multi_compile __ TBT_LINEAR_TARGET
                  		#pragma shader_feature_local _ _USEDISTANCEFADE
      #pragma shader_feature_local _ _GAIN
      #include "UnityCG.cginc"
      #include "../../../Shaders/Include/Brush.cginc"

      sampler2D _MainTex;
      sampler2D _AlphaMask;
      float4 _MainTex_ST;
      float4 _AlphaMask_ST;
      float _Speed;
      half _EmissionGain;
      float _MORMultiplier;
      struct appdata_t {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
        float3 normal : NORMAL;
        float3 texcoord : TEXCOORD0;
      };

      struct v2f {
        //float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
      };


      v2f vert (appdata_t v, out float4 vertex : SV_POSITION)
      {
        v2f o;

        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
        #if _GAIN
            o.color = (bloomColor(TbVertToNative(v.color), _EmissionGain)*_MORMultiplier;
            
        #else
            o.color = TbVertToNative(v.color)*_MORMultiplier;
            
        #endif

#ifdef AUDIO_REACTIVE
        float3 displacement = _BeatOutput.y * v.normal *
            saturate((1.0 - smoothstep(0, .3, v.texcoord.x)) * v.texcoord.z);
        v.vertex.xyz += displacement;
#endif
        vertex = UnityObjectToClipPos(v.vertex);

        return o;
      }

float _FadeDistanceStart;
float _FadeDistanceEnd;

      fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
      {
        // Set up some staggered scrolling for "fire" effect
#ifdef AUDIO_REACTIVE
        float time = (_Time.x * 2 + _BeatOutputAccum.w) * -_Speed;
#else
        float time = _Time.y * -_Speed;
#endif
        fixed2 scrollUV = i.texcoord;
        fixed2 scrollUV2 = i.texcoord;
        fixed2 scrollUV3 = i.texcoord;
        scrollUV.y += time; // a little twisting motion
        scrollUV.x += time;
        scrollUV2.x += time * 1.5;
        scrollUV3.x += time * 0.5;

        // Each channel has its own tileable pattern which we want to scroll against one another
        // at different rates. We pack 'em into channels because it's more performant than
        // using 3 different texture lookups.
        float r = tex2D(_MainTex, scrollUV).r;
        float g = tex2D(_MainTex, scrollUV2).g;
        float b = tex2D(_MainTex, scrollUV3).b;

        // Combine all channels
        float gradient_lookup_value = (r + g + b) / 3.0;
        gradient_lookup_value *= (1 - i.texcoord.x); // rescales the lookup value from start to finish
        gradient_lookup_value = (pow(gradient_lookup_value, 2) + 0.125) * 3;

        float falloff = max((0.2 - i.texcoord.x) * 5, 0);
        float tex = tex2D(_AlphaMask, saturate(gradient_lookup_value + falloff)).r;
         
 #if _USEDISTANCEFADE
         float screenDistance =  length(screenPos.z/screenPos.w);	//this is a 0-1 value of screen depth.
        screenDistance=(1.0 - screenDistance * _ZBufferParams.w) / (screenDistance * _ZBufferParams.z);
    
        float distanceLerp = saturate( (screenDistance - _FadeDistanceStart*10) * (1.0 / (_FadeDistanceEnd*10) ) );
        if(distanceLerp <= 0){
            discard;
        }
    #else
        float distanceLerp = 1;
    #endif
        float4 c= float4((tex * i.color).rgb , 1.0);
        c.a *= distanceLerp;
        return c;
      }
      ENDCG
    }
  }
}
}
