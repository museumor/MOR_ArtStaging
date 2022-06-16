//UnityStandardInput.cginc line 57
struct VertexInputMOR // : VertexInput
{
    //struct VertexInput
    //{
    float4 vertex : POSITION;
    half3 normal : NORMAL;
    #if !defined(LIGHTMAP_ON) && !defined(DIRLIGHTMAP_COMBINED)
    float4 uv0 : TEXCOORD0; //MOR: Change to float4
    #else
        float2 uv0      : TEXCOORD0;//MOR: Change to float4
    #endif
    float2 uv1 : TEXCOORD1;
    #if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
    #endif
    #ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    //};

    float4 color: COLOR;
};

//See UnityStandardCore.cs ~Line:357
struct VertexOutputForwardBaseMOR : VertexOutputForwardBase
{
    #if _MATCAP || _RIMLIGHT
		    float4 viewNorm : NORMAL2;
    #endif
    float4 color : COLOR;

    #if _TRIPLANAR
		    float3 vPos : COLOR2;
    #endif
    #if _LIGHTMAP
		    float2 uv2 : TEXCOORD9;
    #endif
    float4 emissionCol : COLOR3;
   
};
#if _SPECULARMULTIPLIER
    float4 _SpecularColor;
#endif
float _BlendAmount;

//upcast from normal struct to custom. Watch on upgrade.
inline VertexOutputForwardBaseMOR vOutToRadialVout(VertexOutputForwardBase i, VertexInputMOR v)
{
    //Lame not good function.
    VertexOutputForwardBaseMOR o = (VertexOutputForwardBaseMOR)0;
    o.pos = i.pos;

    #if defined(_WORLD_UVS)
    #if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
                o.tex.xy = (i.posWorld.xz * _MainTex_ST.xy);
    #else
                o.tex.xy = half2(i.tangentToWorldAndPackedData[0].w, i.tangentToWorldAndPackedData[2].w);
                o.tex.xy *= _MainTex_ST.xy;
    #endif
    #elif !defined(_DISABLE_TEXTURES)

    o.tex = i.tex;
    #endif

    o.eyeVec = i.eyeVec;
    o.tangentToWorldAndPackedData = i.tangentToWorldAndPackedData;

    o.ambientOrLightmapUV = i.ambientOrLightmapUV;

    #if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
			o.posWorld.xyz = i.posWorld.xyz;
    #endif
    //o._ShadowCoord = (i.uv.xy)//i._ShadowCoord;
    UNITY_TRANSFER_SHADOW(o, v.uv1);
    #if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
			o.instanceID = i.instanceID;
    #endif
    #ifdef UNITY_STEREO_INSTANCING_ENABLED
			o.stereoTargetEyeIndex = i.stereoTargetEyeIndex;
    #endif

    return o;
}

VertexOutputForwardBaseMOR vertForwardBaseMOR(VertexInputMOR v)
{
    VertexOutputForwardBase o1 = vertForwardBase((VertexInput)v);

    //VertexOutputForwardBase_Radial o = (VertexOutputForwardBase_Radial)o1; //!!!I WANT THIS TO WORK!
    VertexOutputForwardBaseMOR o = vOutToRadialVout(o1, v); //This is lame.
    #if !defined(LIGHTMAP_ON) && !defined(DIRLIGHTMAP_COMBINED)
    o.tex.zw = (v.uv1.xy - float2(1, 2)) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw; //detail mapping tied into same channel as face colour flag as can't set uv zw in 3d editor programs
    #else
            o.tex.zw = o.tex.xy;
    #endif
    //o.uv2.zw = v.uv1.xy;
    float4 i_color;
    #if _USEVERTEXCOLOR | _LINEARVERTEXCOLOR
        i_color = (v.color);
        //o.color.a = 1;//vertex alpha is making dresses get black 
    #ifndef UNITY_COLORSPACE_GAMMA
    #ifndef _LINEARVERTEXCOLOR
        i_color.rgb = GammaToLinearSpace(i_color.rgb);
    #endif
    #endif
    #else
        i_color = float4(1, 1, 1, 1);
    #endif
    float4 col;
    float4 colEmiss;
    #if _USESECONDARYCOLOR
        col = lerp( _Color,_SecondaryColor,saturate(v.uv1.x));
        colEmiss = lerp(_EmissionColor,_SecondaryEmission,saturate(v.uv1.x));
    #else
        col = _Color;
        colEmiss = _EmissionColor;
    #endif
    /*#if _VTXBLEND_OVERLAY
        //float3 texCol = lerp(fixed3(1,1,1),i_color.rgb,_BlendAmount); 
        //col = col * i_color.rgb;
        col.rgb = 1.0 - (1.0 - col.rgb) / lerp(fixed3(1,1,1),i_color.rgb,_BlendAmount);//ColorDodge
        //col = lerp(   1 - 2 * (1 - col) * (1 - texCol),    2 * col * texCol,    step( col, 0.5 ));//Overlay
    #elif _VTXBLEND_ADD
        col.rgb += i_color.rgb;
    #elif _VTXBLEND_REPLACE
        col.rgb = i_color.rgb;
    #else*/
    col.rgb *= i_color.rgb;
    // #endif
    o.color = col;
    //#if !_LIGHTMAP
    o.emissionCol = colEmiss * i_color.rgba;
    //#endif
    //https://forum.unity.com/threads/view-space-normals-affected-by-camera-rotation.465570/
    #if _MATCAP || _RIMLIGHT
            //float3 viewNorm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, o.tangentToWorldAndPackedData[2].xyz));        // get view space position of vertex
            float3 viewNorm = mul((float3x3)UNITY_MATRIX_V, o.tangentToWorldAndPackedData[2].xyz);
            
            float3 viewPos = UnityObjectToViewPos(v.vertex);
            float3 viewDir = normalize(viewPos);
        
            // get vector perpendicular to both view direction and view normal
            float3 viewCross = cross(viewDir, viewNorm);
            
            //viewNorm = float3(-viewCross.y, viewCross.x, dot(viewNorm, -viewDir));
            viewNorm = float3(-viewCross.y, viewCross.x, 0);
            viewNorm.z = sqrt(1 - saturate(dot(viewNorm.xy, viewNorm.xy)));
            o.viewNorm.xyz = viewNorm.xyz;
            o.viewNorm.w = o.tex.z<1;//1 or 0 mask
    #endif
    //o.tex.z = frac(o.tex.z);//Trim off any other values from masking
    #if _TRIPLANAR
		    o.vPos = v.vertex *_MainTex_ST.x;
    #endif
    #if _LIGHTMAP
            o.uv2.xy = v.uv1.xy * _LightMap_ST.xy + _LightMap_ST.zw;
    #endif
    #if _USEVERTEXCOLOR | _LINEARVERTEXCOLOR
       o.color.a *= v.color.a;
    #endif
    return o;
}


#if _CONTRAST
    float _StandardContrastMul;
    float _StandardContrastAdd;
    
    
#endif


#if _TRIPLANAR
half3 Albedo_MOR(float4 texcoords,float3 blend,float3 i_color)
#else
half3 Albedo_MOR(float4 texcoords, float3 i_color)
#endif
{
    float3 col = i_color; //_Color.rgb;

    #if _TRIPLANAR
        //project mapping
        fixed4 cx = tex2D(_MainTex, texcoords.yz);
        fixed4 cy = tex2D(_MainTex, texcoords.xz);
        fixed4 cz = tex2D(_MainTex, texcoords.xy);
        // blend the textures based on weights
        float3 texCol = cx * blend.x + cy * blend.y + cz * blend.z;
    #else
    //Normal sampling
    float3 texCol = tex2D(_MainTex, texcoords.xy).rgb;
    #endif
   
    #if _CONTRAST
        texCol = texCol * _StandardContrastMul + (_StandardContrastAdd);
    #endif
    
    #if _VTXBLEND_OVERLAY
        half3 albedo = lerp(   1 - 2 * (1 - col) * (1 - texCol),    2 * col * texCol,    step( col, 0.5 ));
    #elif _VTXBLEND_ADD
        half3 albedo =  texCol + col;
    #elif _VTXBLEND_REPLACE
        half3 albedo = col;
    #else
    half3 albedo = col * texCol.rgb;
    #endif
    
    //half3 albedo = col *texCol.rgb;
    #if _DETAIL
    #if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
        // SM20: no detail mask
        half mask = 1;
    #else
        half mask = DetailMask(texcoords.zw);
    #endif
    half3 detailAlbedo = tex2D (_DetailAlbedoMap, texcoords.zw).rgb;
    #if _DETAIL_MULX2
        albedo -= 1-detailAlbedo;//lerp( ((1-detailAlbedo)*0.1),1-detailAlbedo,Luminance(col));//LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
    #elif _DETAIL_MUL
        albedo *= LerpWhiteTo (detailAlbedo, mask);
    #elif _DETAIL_ADD
        albedo += detailAlbedo * mask;
    #elif _DETAIL_LERP
        albedo = lerp (albedo, detailAlbedo, mask);
    #endif
    #endif
    return albedo;
}

half3 Emission_MOR(float4 uv)
{
    #ifndef _EMISSION
    return 0;
    #else
    //half3 col = _EmissionColor.rgb;
    //#if _USESECONDARYCOLOR
    //   col = lerp(col,_SecondaryEmission,(uv.z));
       // #endif
        half3 texCol = tex2D(_EmissionMap, uv).rgb;
        /*#if _VTXBLEND_OVERLAY
            //return texCol * col;
           //return 0;//col;
            return lerp(   1 - 2 * (1 - col) * (1 - texCol),    2 * col * texCol,    step( col, 0.5 ));
        #elif _VTXBLEND_ADD
            return texCol + col;
        #elif _VTXBLEND_REPLACE
            return col;
        #else
             return texCol * col;
        #endif*/
        return texCol;
        //return tex2D(_EmissionMap, uv).rgb * col;
    #endif
}


#if _TRIPLANAR
inline FragmentCommonData SpecularSetup_MOR (float4 i_tex,float4 i_color,float3 blend)
#else
inline FragmentCommonData SpecularSetup_MOR(float4 i_tex, float4 i_color)
#endif
{
    half4 specGloss = SpecularGloss(i_tex.xy);
    half3 specColor = specGloss.rgb;

    half smoothness = specGloss.a;

    half oneMinusReflectivity;
    #if _TRIPLANAR
        float3 col = Albedo_MOR(i_tex,blend,i_color.rgb);
    #else
    float3 col = Albedo_MOR(i_tex, i_color.rgb);
    #endif


    #if _SATURATION
       col = lerp(Luminance(col),col,_SaturationValue);
    #endif
    half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(col, specColor, /*out*/ oneMinusReflectivity);
    #if _SPECULARMULTIPLIER
        specColor = _SpecularColor;
    #endif
    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && (_USEVERTEXCOLOR)
        o.smoothness = i_color.a;
    #else
    o.smoothness = smoothness;
    #endif
    return o;
}

#if _TRIPLANAR
inline FragmentCommonData RoughnessSetup_MOR(float4 i_tex,float4 i_color,float3 blend)
#else
inline FragmentCommonData RoughnessSetup_MOR(float4 i_tex, float4 i_color)
#endif
{
    half2 metallicGloss = MetallicRough(i_tex.xy);
    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    #if _TRIPLANAR
        float3 col = Albedo_MOR(i_tex,blend,i_color.rgb);
    #else
    half3 col = Albedo_MOR(i_tex, i_color.rgb);
    #endif
    half3 diffColor = DiffuseAndSpecularFromMetallic(col, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    #if _SPECULARMULTIPLIER
        specColor *= _SpecularColor;
    #endif
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;

    o.smoothness = smoothness;

    return o;
}
#if _TRIPLANAR
inline FragmentCommonData MetallicSetup_MOR (float4 i_tex,float4 i_color, float3 blend)
#else
inline FragmentCommonData MetallicSetup_MOR(float4 i_tex, float4 i_color)
#endif
{
    half2 metallicGloss = MetallicGloss(i_tex.xy); //!Metalic 'Gloss' here.
    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    #if _TRIPLANAR
        float3 col = Albedo_MOR(i_tex,blend,i_color.rgb);
    #else
    float3 col = Albedo_MOR(i_tex, i_color.rgb);
    #endif
    half3 diffColor = DiffuseAndSpecularFromMetallic(col, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    return o;
}


/// <summary>
/// Calculate the normal for the fragment. Modified to factor in fragment to eye distance.
/// Original at : UnityStandardCore.cs ~ Line : 115
/// </summary>
/// <returns></returns>
float3 PerPixelWorldNormal_MOR(float4 i_tex, float4 tangentToWorld[3], float distanceFade, float vface)
{
    #ifdef _NORMALMAP
    half3 tangent = tangentToWorld[0].xyz;
    half3 binormal = tangentToWorld[1].xyz;
    half3 normal = tangentToWorld[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        normal = NormalizePerPixelNormal(normal);

        // ortho-normalize Tangent
        tangent = normalize (tangent - normal * dot(tangent, normal));

        // recalculate Binormal
        half3 newB = cross(normal, tangent);
        binormal = newB * sign (dot (newB, binormal));
    #endif

    half3 normalTangent = NormalInTangentSpace(i_tex);

    float3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z * vface); // @TODO: see if we can squeeze this normalize on SM2.0 as well
    normalWorld = lerp(normalWorld,normal,distanceFade);
   
    #else
    float3 normalWorld = normalize(tangentToWorld[2].xyz);
    normalWorld.z *= vface;
    #endif
    return normalWorld;
}
#if _TRIPLANAR
inline FragmentCommonData FragmentSetup_MOR (inout float4 i_tex, float3 i_eyeVec, half3 i_viewDirForParallax, float4 tangentToWorld[3], float3 i_posWorld,float4 i_color,float screenDistance,float3 vPos,float vface)
#else
inline FragmentCommonData FragmentSetup_MOR(inout float4 i_tex, float3 i_eyeVec, half3 i_viewDirForParallax, float4 tangentToWorld[3], float3 i_posWorld, float4 i_color, float screenDistance,
                                            float vface)
#endif
{
    i_tex =Parallax(i_tex, i_viewDirForParallax);
    half texAlpha = Alpha(i_tex.xy);
    half alpha = texAlpha * i_color.a;
    #if defined(_ALPHATEST_ON)
        clip (alpha - _Cutoff);
    #endif

    //FragmentCommonData o = RoughnessSetup_MOR (i_tex,i_color);

    #if _TRIPLANAR
    //https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
        // use absolute value of normal as texture weights
        half3 blend = abs(tangentToWorld[2].xyz);
        // make sure the weights sum up to 1 (divide by sum of x+y+z)
        blend /= dot(blend,1.0);
        FragmentCommonData o = MOR_SETUP_BRDF_INPUT(i_tex.rgbr,i_color,blend);
    
        //if _METALLICGLOSSMAP
        //    FragmentCommonData o = MetallicSetup_MOR (i_tex.rgbr,i_color,blend);
        //#else
        //   FragmentCommonData o = SpecularSetup_MOR (i_tex.rgbr,i_color,blend);
        //#endif
    #else
        //Shader defines which function to use here, metalic or specular etc
        FragmentCommonData o = MOR_SETUP_BRDF_INPUT(i_tex.rgbr,i_color);
        
        //#if _METALLICGLOSSMAP
         //   FragmentCommonData o = MetallicSetup_MOR (i_tex,i_color);
        //#else
        //    FragmentCommonData o = SpecularSetup_MOR(i_tex, i_color);
        //#endif
    #endif
    //o.oneMinusReflectivity*- vface;
    o.normalWorld = PerPixelWorldNormal_MOR(i_tex, tangentToWorld, screenDistance, vface);
    o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
    o.eyeVec.z *= vface;
    o.posWorld = i_posWorld;

    // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    o.diffColor =  PreMultiplyAlpha(o.diffColor, texAlpha, o.oneMinusReflectivity, /*out*/ o.alpha);
    return o;
}


/*
//https://docs.unity3d.com/Manual/SL-ShaderSemantics.html Make screenspace checker 'dither' fx. Maybe needs VR adjustment
half4 fragForwardBase_MOR (VertexOutputForwardBaseMOR i, UNITY_VPOS_TYPE screenPos : SV_POSITION) : SV_Target
{
    screenPos.xy = floor(screenPos.xy * 0.1) * 0.5;
    float checker = -frac(screenPos.r + screenPos.g);
    // clip HLSL instruction stops rendering a pixel if value is negative
    clip(checker);*/


uniform samplerCUBE _Cube;
#if SHADER_API_MOBILE
       half4 fragForwardBase_MOR_Internal (VertexOutputForwardBaseMOR i) : SV_Target   // backward compatibility (this used to be the fragment entry function)
#else
half4 fragForwardBase_MOR_Internal(VertexOutputForwardBaseMOR i,fixed vface : VFACE) : SV_Target // backward compatibility (this used to be the fragment entry function)
#endif
{

    #if SHADER_API_MOBILE
                float vface=1;
    #endif
    //https://forum.unity3d.com/threads/writing-depth-value-in-fragment-program.66153/
    float screenDistance = length(i.pos.z / i.pos.w); //this is a 0-1 value of screen depth.
    #if _USEDISTANCEFADE
        screenDistance=(1.0 - screenDistance * _ZBufferParams.w) / (screenDistance * _ZBufferParams.z);
    
        float distanceLerp = saturate( (screenDistance - _FadeDistanceStart) * (1.0 / _FadeDistanceEnd ) );
    #else
    float distanceLerp = 0;
    #endif

    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    //FRAGMENT_SETUP(s)
    #if _TRIPLANAR
        FragmentCommonData s = FragmentSetup_MOR(i.tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i),i.color,distanceLerp, i.vPos.xyzz,vface);
    #else
    FragmentCommonData s = FragmentSetup_MOR(i.tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i), i.color, distanceLerp, vface);
    //float4 tex = i.tex;
    #endif
    //FragmentCommonData s = FragmentSetup_MOR(tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i),i.color,distanceLerp);

    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    UnityLight mainLight = MainLight();

    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);
    #if _DISABLEDIRECTIONAL
        mainLight.color =half3(0,0,0);
        //return half4(1,0,0,1);
    #endif
    half occlusion = Occlusion(i.tex.zw);
    float4 fragAmbIn = i.ambientOrLightmapUV;
    #if _LIGHTMAP
        half4 fragAmbInRaw = tex2D(_LightMap,i.uv2.xy);
        fragAmbIn.rgb = DecodeLightmap(fragAmbInRaw);

    //fragAmbIn.rgb = SubtractMainLightWithRealtimeAttenuationFromLightmap(fragAmbIn, atten, fragAmbInRaw, s.normalWorld);

    
    #elif LIGHTMAP_ON || DIRLIGHTMAP_COMBINED
        fragAmbIn.rgb = 0;
    #endif
    //return fragAmbIn; //lightmap Color.
    
    UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight, true);

    
    //return half4(gi.indirect.specular,1) * _Metallic;
    #if _DISABLEDIRECTIONAL
        gi.light.color = half3(0,0,0);
    
        #if !_LIGHTMAP
            //s.specColor =  gi.indirect.specular.rgb;
        #endif
    
    #endif
    
    #if _USE_AMBIENT_OVERRIDE
        #if LIGHTMAP_ON || DIRLIGHTMAP_COMBINED
            //fragAmbIn.rgb = 0; //this will be lightmap coords, not an ambient value in this situation.
            gi.indirect.diffuse.rgb += _ColorAmbient.rgb;
            gi.indirect.specular.rgb +=  _ColorAmbient.rgb;
        #else
            gi.indirect.diffuse.rgb = _ColorAmbient.rgb + fragAmbIn.rgb;
    //gi.indirect.diffuse = SubtractMainLightWithRealtimeAttenuationFromLightmap(gi.indirect.diffuse, atten, fragAmbInRaw, s.normalWorld);
    
        #endif
        //gi.indirect.diffuse.rgb = _ColorAmbient.rgb + fragAmbIn.rgb;
        
    #endif

    half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
    
    #if _DISABLEDIRECTIONAL
        float zDist = dot(_WorldSpaceCameraPos - s.posWorld, UNITY_MATRIX_V[2].xyz);
        float fade = UnityComputeShadowFadeDistance(s.posWorld, zDist);
        //data.atten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
        float shadow = lerp(atten,1, UnityComputeShadowFade(fade) );
        c.rgb = min(c.rgb, lerp(_ColorShadow.rgb,c.rgb ,shadow) );
        //c.rgb -= saturate(1-atten);
    #endif

    #if !_LIGHTMAP
    float3 emissionCol = i.emissionCol.rgb; //(i.color.rgb);//*i.color.a; //The alpha multiply here is messing up dresses.
    #else
        float3 emissionCol = i.emissionCol.rgb;
    #endif
    #if _USESECONDARYCOLOR
        emissionCol *= i.color.a;//Do only for heads or things that have the boundry between primary and secondary
    #endif
    #if _SATURATION
       emissionCol = lerp(Luminance(emissionCol),emissionCol,_SaturationValue);
    #endif
    c.rgb += Emission_MOR(i.tex.xyzw) * emissionCol;

    UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
    #if _DISABLEFOG

    #else
    UNITY_APPLY_FOG(_unity_fogCoord, c.rgb);
    #endif


    #if _RIMLIGHT
        float3 normalView = i.viewNorm.xyz;//ScreenspaceNormal
        normalView.z *- vface;
        float3 viewAdjusted = normalize(_RimDirection.xyz);//LightDirection
        float ndotv = saturate(dot(1-normalView, viewAdjusted)-_RimGrow);
        float rimAmount =  pow( ndotv, _RimPower);
        
         float minus = pow(ndotv,_RimPower*4);
         rimAmount = saturate(rimAmount);//-(minus*4);
         float x = abs(rimAmount*2-1);
         
         //x = x*x*x*x;
         x = 1-x;
         
         //return x;
       
        float3 rim = (_ColorRimlight.rgb * x) * _ColorRimlight.a;//(rimAmount-minus) ;
        
        
        
        c.rgb += (rim*i.viewNorm.w) *c.a ;
    #endif

    #if _MATCAP
        //return float4(i.viewNorm.xyz,1);
        float3 matCapColor =  (tex2D(_MatcapTex,i.viewNorm.xy*0.5+0.5).rgb-0.01) *occlusion;
        c.rgb = (matCapColor*i.color.rgb * c.rgb)*2;
    #endif



    float4 outColor = OutputForward(c, s.alpha);
    outColor.a *= _Color.a *  i.color.a;
    return outColor;
}


half4 LightingWrapLambert(SurfaceOutputStandard s, half NdotL, UnityGI gi)
{
    half diff = NdotL; //* 0.5 + 0.5;
    half4 c;
    c.rgb = s.Albedo * _LightColor0.rgb * (diff);
    c.a = s.Alpha;

    return c;
}


/*
//https://docs.unity3d.com/Manual/SL-ShaderSemantics.html Make screenspace checker 'dither' fx. Maybe needs VR adjustment
half4 fragForwardBase_MOR (VertexOutputForwardBaseMOR i, UNITY_VPOS_TYPE screenPos : SV_POSITION) : SV_Target
{
    screenPos.xy = floor(screenPos.xy * 0.1) * 0.5;
    float checker = -frac(screenPos.r + screenPos.g);
    // clip HLSL instruction stops rendering a pixel if value is negative
    clip(checker);*/
#if SHADER_API_MOBILE
half4 fragForwardBase_MOR_InternalAndroid (VertexOutputForwardBaseMOR i) : SV_Target   // backward compatibility (this used to be the fragment entry function)
#else
half4 fragForwardBase_MOR_InternalAndroid(VertexOutputForwardBaseMOR i,fixed vface : VFACE) : SV_Target // backward compatibility (this used to be the fragment entry function)
#endif
{
    #if SHADER_API_MOBILE
        float vface=1;
    #endif

    //https://forum.unity3d.com/threads/writing-depth-value-in-fragment-program.66153/
    #if _TRIPLANAR
        FragmentCommonData s = FragmentSetup_MOR(i.tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i),i.color,1, i.vPos.xyzz,vface);
    #else
    FragmentCommonData s = FragmentSetup_MOR(i.tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i), i.color, 1, vface);
    //float4 tex = i.tex;
    #endif

    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    UnityLight mainLight = MainLight();
    #if _DISABLEDIRECTIONAL
        mainLight.color =half3(0,0,0);
        //return half4(1,0,0,1);
    #endif

    float4 fragAmbIn = i.ambientOrLightmapUV;
    #if _LIGHTMAP
        fragAmbIn = tex2D(_LightMap,i.uv2.xy);
    #elif LIGHTMAP_ON || DIRLIGHTMAP_COMBINED
        fragAmbIn.rgb = 0;
    #endif
    //return fragAmbIn; //lightmap Color.
    UnityGI gi = FragmentGI(s, 1, i.ambientOrLightmapUV, 1, mainLight, true);
    //return half4(gi.indirect.specular,1);

    #if _USE_AMBIENT_OVERRIDE
        #if LIGHTMAP_ON || DIRLIGHTMAP_COMBINED
            //fragAmbIn.rgb = 0; //this will be lightmap coords, not an ambient value in this situation.
            gi.indirect.diffuse.rgb += _ColorAmbient.rgb ;
            gi.indirect.specular.rgb +=  _ColorAmbient.rgb;
        #else
             gi.indirect.diffuse.rgb = _ColorAmbient.rgb + fragAmbIn.rgb;
             gi.indirect.specular.rgb +=  _ColorAmbient.rgb +fragAmbIn.rgb;       
        #endif

    #endif

    //half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
    #ifdef UNITY_COMPILER_HLSL
    SurfaceOutputStandard o = (SurfaceOutputStandard)0;
    #else
        SurfaceOutputStandard o;
    #endif

    o.Albedo = s.diffColor;
    o.Emission = 0.0;
    o.Alpha = 0.0;
    o.Occlusion = 1.0;
    o.Normal = s.normalWorld;

    #if !_LIGHTMAP
    float3 emissionCol = i.emissionCol.rgb; //(i.color.rgb);//*i.color.a; //The alpha multiply here is messing up dresses.
    #else
        float3 emissionCol = 0;
    #endif


    half ndotl = saturate(dot(s.normalWorld, mainLight.dir));

    //half3 attenuatedLightColor = gi.light.color * ndotl;

    half4 c;
    c.rgb = BRDF3_Indirect(s.diffColor, s.specColor, gi.indirect, 0, 0);
    //c.rgb = s.diffColor;//BRDF3DirectSimple(s.diffColor, s.specColor, s.smoothness, rl) * attenuatedLightColor;
    //c.rgb = lerp((LightingWrapLambert (o, ndotl, gi)).rgb, c.rgb,1- ndotl);

    c += LightingWrapLambert(o, ndotl, gi);
    c.rgb += Emission_MOR(i.tex.xyzw) * emissionCol;
    c.a = 1;
    //c.rgb *= 1/s.alpha;
    #if _RIMLIGHT
        float3 normalView = i.viewNorm.xyz;//ScreenspaceNormal
        normalView.z *- vface;
        float3 viewAdjusted = _RimDirection.xyz;//normalize(_RimDirection.xyz);//LightDirection
        float ndotv = saturate(dot(1-normalView, viewAdjusted)-_RimGrow);
        float rimAmount =  pow( ndotv, _RimPower);
                                                      
        //float minus = pow(ndotv,_RimPower*4);
        rimAmount = saturate(rimAmount);//-(minus*4);
        //
        float x = abs(rimAmount*2-1);
        x = 1-x;
        //float x = rimAmount;
        //return x;
           
        float3 rim = (_ColorRimlight.rgb * x) * _ColorRimlight.a;//(rimAmount-minus) ;
            
            
            
        c.rgb += (rim*i.viewNorm.w) *c.a ;
    #endif

    return OutputForward(c, s.alpha);
}
