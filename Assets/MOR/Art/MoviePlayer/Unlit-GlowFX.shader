// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "_MOR/Unlit/Texture-GlowFX" {
Properties {
    [HDR]_Color ("Color",color) = (1,1,1,1)

    _MainTex ("Base (RGB)", 2D) = "black" {}
    _GIAlpha("GI alpha",float) = 1
    [Enum(UnityEngine.Rendering.CullMode)] _CullOp("Face Culling", int) = 2 //Back
    
    _StencilRef("Stencil Ref ID", Int) = 1
    [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", int) = 0 //Disabled=0,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,Always=8
    [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Pass Operation", int) = 0 //Keep=0,Zero,Replace,IncrementSaturate,DecrementSaturate,Invert,IncrementWrap,DecrementWrap=7
    [Enum(UnityEngine.Rendering.ColorMask)]_ColorMask("Colormask",int) = 15//UnityEngine.Rendering.ColorWriteMask.RGBA
    
    _ZWrite ("ZWrite", Float) = 1.0
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", int) = 4 //Disabled=0,Never,Less,Equal,LessEqual,Greater,NotEqual,GreaterEqual,LessEqual=8
    [Toggle(_VERTEXCOLOR)] _VertexColor ("Use Vertex Color",int) = 0
    [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend ("__src", Float) = 1.0
    [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("__dst", Float) = 0.0
}

SubShader {
    Tags { "RenderType"="Transparent" }
    LOD 100
    Cull [_CullOp]
  
	
	
    ZTest [_ZTest]
    ZWrite off
    Pass {
        Tags { "LightMode" = "ForwardBase" }
        Blend [_SrcBlend] [_DstBlend]
        BlendOp RevSub
        Offset 0,1
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature_local _VERTEXCOLOR
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex) + float4((1.0/_ScreenParams.xy)*float2(4,8),0,0);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord );
                #if _VERTEXCOLOR
                    col.rgb = saturate(col.rgb + i.color.rgb);
                    col.a = i.color.a;
                #endif
                col *= _Color;
                col.rgb *= _Color.a;
                //col.rgb *= _AddColor.rgb
                UNITY_APPLY_FOG(i.fogCoord, col);
                //UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
    Pass {
        ZWrite [_ZWrite]
        Tags { "LightMode" = "Always" }
        Blend [_SrcBlend] [_DstBlend]
        Offset 0,-1
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
            #pragma shader_feature_local _VERTEXCOLOR
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                #if _VERTEXCOLOR
                    col.rgb = saturate(col.rgb + i.color.rgb);
                    col.a = i.color.a;
                #endif
                col *= _Color * 2;
                col.rgb *= _Color.a;
                //col.rgb *= _AddColor.rgb
                UNITY_APPLY_FOG(i.fogCoord, col);
                //UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
    
}

}
