Shader "_MOR/Solid Color" {

Properties {
    [HDR]_Color ("Color", Color) = (0.700000,0.500000,1.000000,0.500000)
    _StencilLayer ("Stencil Layer", Float) = 1.000000
    [Toggle(_USEVERTEXCOLORS)]_UseVertexColors ("Use VertexColors",int) =1
    [Toggle(_USEGAMMAVERTEXCOLORS)]_UseRawVertexColors ("Don't adjust to linear",int) = 0
    _VertexColorContribution ("VertexColorAmount",Range(0,1)) = 1
    [Enum(UnityEngine.Rendering.ColorMask)] _ColorMask ("ColorMask",int) = 15
    [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull",int) = 2
    [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Blend Source", Float) = 1.0
    [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Blend Destination", Float) = 0.0
    [Enum(OFF,0,ON,1)]_ZWrite ("Z Write", Float) = 1.0
}
SubShader { 
    Tags { "RenderType"="Geometry" "IgnoreProjector"="True"}
    Pass {
        Stencil {
            Ref [_StencilLayer]
            Pass Replace
        }
        ColorMask [_ColorMask]
        Cull[_Cull]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #include "UnityCG.cginc"
        #pragma multi_compile_fog
        #define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))
        #pragma shader_feature _ _USEVERTEXCOLORS _USEGAMMAVERTEXCOLORS
        #if !SHADER_API_MOBILE
         #pragma multi_compile_instancing
        #endif
        // uniforms

        uniform float _VertexColorContribution;
        // vertex shader input data
        struct appdata {
            float3 pos : POSITION;
            float4 color : COLOR;            
            UNITY_VERTEX_INPUT_INSTANCE_ID

        };

        // vertex-to-fragment interpolators
        struct v2f {
             float4 pos : SV_POSITION;       
            fixed4 color : COLOR;
            #if USING_FOG
                fixed fog : TEXCOORD0;
            #endif
            UNITY_VERTEX_OUTPUT_STEREO
        #if !SHADER_API_MOBILE
            UNITY_VERTEX_INPUT_INSTANCE_ID
        #endif
        };
        #if !SHADER_API_MOBILE
            UNITY_INSTANCING_BUFFER_START(Props)
               UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)
        #else
            float4 _Color;
        #endif
        // vertex shader
        v2f vert (appdata i) {
            v2f o=(v2f)0;
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float4 color = float4(1,1,1,1);// = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            #if defined(_USEVERTEXCOLORS) || defined(_USEGAMMAVERTEXCOLORS)
                float4 vColors = i.color;
                vColors = lerp(float4(1,1,1,1),vColors,_VertexColorContribution);
               #if defined( UNITY_COLORSPACE_GAMMA) || defined(_USEGAMMAVERTEXCOLORS)
                   color = vColors;
                #else
                    color = fixed4(GammaToLinearSpace(vColors.rgb), i.color.a);
                #endif
            #endif
            #if SHADER_API_MOBILE
                color *= ( _Color);
            #else
                color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            #endif
            o.color = (color);
            // compute texture coordinates
            // fog
            #if USING_FOG
                float3 eyePos = UnityObjectToViewPos(float4(i.pos,1));//mul (UNITY_MATRIX_MV, float4(i.pos,1)).xyz;
                float fogCoord = length(eyePos.xyz); // radial fog distance
                UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
                o.fog = saturate(unityFogFactor);
            #endif
            o.pos = UnityObjectToClipPos(i.pos);
            return o;
        }

        // fragment shader
        float4 frag (v2f i) : SV_Target {
            float4 col;
            col = i.color;
            return col;
        }
    ENDCG
    }
}
}