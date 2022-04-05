/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1


Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

Shader "_MOR/Unlit/ColumnGlowVideoTrigger"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_Thickness("Thickness", Range(0, 1)) = 0.5
		_FadeStart("Fade Start", Range(0, 1)) = 0.5
		_FadeEnd("Fade End", Range(-1, 1)) = 0.5
		_Intensity("Intensity", Range(0, 1)) = 0.5
		
		_FadeDistanceStart("Fade Distance Start",float) =0.1
		_FadeDistanceEnd("Fade Distance End",float) = 0.5
		//_HeightFadePow ("Height Fade Pow",float) = 2
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True"  "Queue"="Transparent+1" "DisableBatching"="true"}
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off
		
		
		LOD 0

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				//float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 origPosition : POSITION1;
				float3 eyeDir : TEXCOORD1;
			};

			fixed4 _TintColor;
			float _Thickness; 
			float _FadeStart;  
			float _FadeEnd;  
			float _Intensity;  
			
	
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = normalize(mul(UNITY_MATRIX_IT_MV,v.normal).xyz);
				o.origPosition = v.vertex;
				o.eyeDir = -normalize(mul(UNITY_MATRIX_MV, v.vertex).xyz);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			float _FadeDistanceStart = 2;
			float _FadeDistanceEnd = 1;
			float _PlayerScale; //Global parameter set by MORPlayer.
			float _HeightFadePow;
			fixed4 frag (v2f i) : SV_Target
			{
                float screenDistance =  length(i.vertex.z/i.vertex.w);	//this is a 0-1 value of screen depth.
                screenDistance=(1.0 - screenDistance * _ZBufferParams.w) / (screenDistance * _ZBufferParams.z);
                float distanceLerp = saturate( (screenDistance - _FadeDistanceStart*_PlayerScale) * (1.0 / (_FadeDistanceEnd*_PlayerScale)) );
                float heightFade = 1-saturate((i.origPosition.y*0.5+0.5));
                //heightFade = pow(heightFade,_HeightFadePow);
                heightFade = heightFade *heightFade *heightFade;
				float d = 	dot(i.normal,i.eyeDir);
				//float p = smoothstep (0,_Thickness,dot(i.normal,i.eyeDir)) * distanceLerp;
				float ndote = abs(dot(i.normal,i.eyeDir));
				float p = smoothstep (0,_Thickness,ndote) * smoothstep(_FadeStart,_FadeEnd,i.origPosition.y) + (1-distanceLerp);    
				return float4((p * _TintColor * (_Intensity*heightFade)).xyz,(p*heightFade+ (1-distanceLerp))*ndote) ;
			}
			ENDCG
		}
	}
}
