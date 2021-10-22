// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "_MOR/Unlit/Texture" {
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
    Blend [_SrcBlend] [_DstBlend]
    Stencil {
        Ref [_StencilRef]
        Comp [_StencilComp]
        Pass [_StencilOp]
    }
	
	ZWrite [_ZWrite]
	ZTest [_ZTest]
	//Offset -1,-1 
    Pass {
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
                col *= _Color;
                col.rgb *= _Color.a;
                //col.rgb *= _AddColor.rgb
                UNITY_APPLY_FOG(i.fogCoord, col);
                //UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
    
    
         Pass
        {
            Name "META"
            Tags {"LightMode"="Meta"}
            Cull Off
            CGPROGRAM
 
            #include"UnityStandardMeta.cginc"
 
            sampler2D _GIAlbedoTex;
            fixed4 _GIAlbedoColor;
            float _GIAlpha;

    half4 UnityMetaFragmentMOR (UnityMetaInput IN)
    {
        half4 res = 0;
    #if !defined(EDITOR_VISUALIZATION)
        if (unity_MetaFragmentControl.x)
        {
            res = half4(IN.Albedo,1);
    
            // d3d9 shader compiler doesn't like NaNs and infinity.
            unity_OneOverOutputBoost = saturate(unity_OneOverOutputBoost);
    
            // Apply Albedo Boost from LightmapSettings.
            res.rgb = clamp(pow(res.rgb, unity_OneOverOutputBoost), 0, unity_MaxOutputValue);
        }
        if (unity_MetaFragmentControl.y)
        {
            half3 emission;
            if (unity_UseLinearSpace)
                emission = IN.Emission;
            else
                emission = GammaToLinearSpace(IN.Emission);
    
            res = half4(emission, 1.0);
        }
    #else
        if ( unity_VisualizationMode == EDITORVIZ_PBR_VALIDATION_ALBEDO)
        {
            res = UnityMeta_pbrAlbedo(IN);
        }
        else if (unity_VisualizationMode == EDITORVIZ_PBR_VALIDATION_METALSPECULAR)
        {
            res = UnityMeta_pbrMetalspec(IN);
        }
        else if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
        {
            res = tex2D(unity_EditorViz_Texture, IN.VizUV);
    
            if (unity_EditorViz_Decode_HDR.x > 0)
                res = half4(DecodeHDR(res, unity_EditorViz_Decode_HDR), 1);
    
            if (unity_EditorViz_ConvertToLinearSpace)
                res.rgb = LinearToGammaSpace(res.rgb);
    
            res *= unity_EditorViz_ColorMul;
            res += unity_EditorViz_ColorAdd;
        }
        else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
        {
            float result = dot(unity_EditorViz_ChannelSelect, tex2D(unity_EditorViz_Texture, IN.VizUV).rgba);
            if (result == 0)
                discard;
    
            float atten = 1;
            if (unity_EditorViz_LightType == 0)
            {
                // directional:  no attenuation
            }
            else if (unity_EditorViz_LightType == 1)
            {
                // point
                atten = tex2D(unity_EditorViz_LightTexture, dot(IN.LightCoord.xyz, IN.LightCoord.xyz).xx).r;
            }
            else if (unity_EditorViz_LightType == 2)
            {
                // spot
                atten = tex2D(unity_EditorViz_LightTexture, dot(IN.LightCoord.xyz, IN.LightCoord.xyz).xx).r;
                float cookie = tex2D(unity_EditorViz_LightTextureB, IN.LightCoord.xy / IN.LightCoord.w + 0.5).w;
                atten *= (IN.LightCoord.z > 0) * cookie;
            }
            clip(atten - 0.001f);
    
            res = float4(unity_EditorViz_Color.xyz * result, unity_EditorViz_Color.w);
        }
    #endif // EDITOR_VISUALIZATION
           res.a = _GIAlpha;
        return res;
    }
            
            
            float4 frag_meta2 (v2f_meta i): SV_Target
            {
                // We're interested in diffuse & specular colors
                // and surface roughness to produce final albedo.
               
                FragmentCommonData data = UNITY_SETUP_BRDF_INPUT (i.uv);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);
                fixed4 c = tex2D (_GIAlbedoTex, i.uv);
                o.Albedo = fixed3(c.rgb * _GIAlbedoColor.rgb);
                o.Emission = Emission(i.uv.xy);
                return UnityMetaFragmentMOR(o);
            }
           
            #pragma vertex vert_meta
            #pragma fragment frag_meta2
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            ENDCG
        }
}

}
