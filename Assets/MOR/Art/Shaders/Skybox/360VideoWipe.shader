Shader "_MOR/360 Video Wipe" {
    Properties {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [NoScaleOffset]_MainTex ("MainTex", 2D) = "gray" {}
        [Gamma] _Exposure ("Exposure (Multiply)",float) = 1
        _Alpha ("Alpha (Transparency)",float) = 1
        _Mask ("MaskTex",2D) = "white" {}
        _Subtract ("Subtract",Float) = 0.501
        _Wipe ("Wipe",Range(0,1)) = 1
        _WipePow ("Wipe Pow",float) = 1
        [Toggle(_MONO)] _MonoMode("Mono",int) = 0
        [Toggle(_HORIZONTAL)] _HorizontalMode ("Horizontal",int) = 0
        _Rotation ("Rotation (Deg)",Range(0,360))=0
        [Toggle()]_UpsideDown ("Upside down",int) = 0
        [Toggle()]_ClipAlpha ("Clip alpha",int) = 0
        [Toggle()]_DoClip("Do alphaClip, like, for real",int) = 1
        [Toggle(_LINEAR_CONVERT)] _LinearConvertMode("Convert To Linear",int) = 0
        
        _StencilRef("Stencil Ref ID", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", int) = 8 //Disabled=0,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,Always=8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Pass Operation", int) = 2 //Keep=0,Zero,Replace,IncrementSaturate,DecrementSaturate,Invert,IncrementWrap,DecrementWrap=7
        [Space(10)]
        _ViewScaler ("View Scaler",Range(-1,2)) = 1
        _FlipX("Flip X",float) = 1
        _EyeLerp ("Eye lerp",Range(0,1)) = 0
        [Enum(OFF,0,ON,1)]_ZWrite ("Z Write", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTestCompare("ZTest Compare", int) = 4 //Disabled=0,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,LessEqual=8
    [Enum(UnityEngine.Rendering.CullMode)] _CullOp("Face Culling", int) = 1 //Back
        
    }
    SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue" = "Geometry+1"
        }
        LOD 100
        Pass {
            Cull [_CullOp]
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            ZTest [_ZTestCompare]
            Stencil {
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilOp]
		    }
		
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ _MONO _HORIZONTAL
            #pragma multi_compile_local _LINEAR_CONVERT
            #include "UnityCG.cginc"
            #pragma target 3.0
            uniform sampler2D _MainTex; 
            float _UpsideDown;
            float _FlipX;
            inline float StereoEyeIndex( float V ){
                float eyeIndex = _FlipX > 0  ? unity_StereoEyeIndex : 1-unity_StereoEyeIndex;
                return (eyeIndex ==  0) ? (V*0.5+0.5) : (V*0.5);
            }
            
            struct VertexInput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float2 tex : TEXCOORD1;
                float3 vertex : TEXCOORD2;
            };
            
            inline float2 ToRadialCoords(float3 coords)
            {
                float3 normalizedCoords = normalize(coords);
                float latitude = acos(normalizedCoords.y);
                float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
                float2 sphereCoords = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
                return float2(0.5,1.0) - sphereCoords;
            }

            
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3x3 matrixRot = unity_ObjectToWorld;
                
                float3 vert = mul(matrixRot,v.vertex.xyz);
                vert.x *= _FlipX;
                o.vertex = vert;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.tex = v.uv;
                return o;
            }
            float4 _Tint;
            float _Alpha;
            float _Wipe;
            float _WipePow;
            sampler2D _Mask;
            float4 _Mask_ST;
            float _Rotation;
            
            float _LinearConvert;
            float _ClipAlpha;
            float _DoClip;
            float _ViewScaler;
            float _EyeLerp;
            float _Subtract;
            float _Exposure;
            
            float4x4  _SpaceMatrix = {{1,0,0,0}  ,{0,1,0,0} ,{0,0,1,0}, {0,0,0,1}};//Identity as default.
            
            float4 frag(VertexOutput i) : SV_TARGET {
                //Mask
                float mask = (tex2D(_Mask,(_Mask_ST.xy*i.tex.xy) + _Mask_ST.zw).r ); // 0-1 value for the mask.
                mask = mask-_Subtract;
                mask = saturate(mask + (_Wipe*2));
                float alpha = pow(mask,_WipePow);
                //clip(c.a-0.5);
                float alphaClip = _ClipAlpha > 0 ? .999999 : 0.5;
                
                if(_DoClip > 0)
                    clip(alpha-alphaClip);

                
                //Actual Movie Rendering
                float3 viewDirection = normalize(i.posWorld.xyz-_WorldSpaceCameraPos.xyz);
               
                
                float radians = _Rotation * UNITY_PI / 180;
                float angleSin = sin( radians );
                float angleCos = cos( radians );
                viewDirection = mul(_SpaceMatrix,viewDirection);
                viewDirection = lerp(i.vertex,viewDirection*float3(_FlipX,1,1),_ViewScaler);
                viewDirection = float3(viewDirection.x*angleCos + viewDirection.z*angleSin , viewDirection.y , -viewDirection.x*angleSin +viewDirection.z*angleCos);
                
                
                viewDirection.xy  *= _UpsideDown > 0 ? -1 : 1;
                float u = atan2(viewDirection.r,viewDirection.b)/(2*UNITY_PI);
                float2 uvLeft;
                #if _MONO
                    float2 tc = ToRadialCoords(viewDirection);
                    float2 uv = tc;// float2 (u+0.5, acos(viewDirection.g)/UNITY_PI);
                    uvLeft = uv;
                #elif _HORIZONTAL
                    viewDirection = normalize(i.posWorld.xyz-_WorldSpaceCameraPos.xyz);
                    viewDirection = mul(_SpaceMatrix,viewDirection);
                    //viewDirection = normalize(_WorldSpaceCameraPos.xyz-i.posWorld.xyz);
                    viewDirection = lerp(i.vertex,viewDirection*float3(_FlipX,1,1),_ViewScaler);
                //rotate
                    viewDirection = float3(viewDirection.x*angleCos + viewDirection.z*angleSin , viewDirection.y , -viewDirection.x*angleSin +viewDirection.z*angleCos);
                    
                    
                    viewDirection.xy  *= _UpsideDown > 0 ? -1 : 1;
                    float2 tc = ToRadialCoords(viewDirection);
                    bool bak = false;
                    if(tc.x>0.5)
                    {
                        return float4(0,0,0,1);
                    }
                    tc.x = fmod(tc.x*2.0, 1);
                    uvLeft = tc;
                
                    tc.x += unity_StereoEyeIndex;
                    tc.x *= 0.5;
                    uvLeft.x *= 0.5;
                    float2 uv = tc;

                #else
                    float2 uv = float2 (u+0.5, StereoEyeIndex(acos(viewDirection.g)/UNITY_PI));
                    float2 tc = ToRadialCoords(viewDirection);
                    tc.x = fmod(tc.x, 1);
                    uvLeft = tc;
                    float eyeIndex = _FlipX > 0 ? unity_StereoEyeIndex : 1-unity_StereoEyeIndex;
                    tc.y += 1-eyeIndex;
                    tc.y *= 0.5;
                    uvLeft.y *= 0.5;
                    uv = tc;
                #endif
                float4 c = tex2D(_MainTex, uv);
                float4 cLeft = tex2D(_MainTex,uvLeft);
                
                c = lerp(c,cLeft,_EyeLerp);
                
                c.rgb *= _Exposure;
                c.a = _Alpha;
                if(_DoClip==0)
                    c.a *= alpha;
                
                
                #if _LINEAR_CONVERT
                    c = fixed4(GammaToLinearSpace(c.rgb), c.a);
                #endif
                c.rgb *= _Tint.rgb * unity_ColorSpaceDouble.rgb;
                //c.rgb = _LinearConvert > 0 ? pow(c.rgb, 2.2) : c.rgb;
                return c;
            }
            ENDCG
        }
 
    
    }
    FallBack off
}
