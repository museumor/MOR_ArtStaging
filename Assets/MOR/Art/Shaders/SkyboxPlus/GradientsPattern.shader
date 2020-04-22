Shader "SkyboxPlus/GradientsPattern"
{
    Properties
    {
        _MainTex ("Pattern Texture",2D)  = "white" {}
        _WaveTex ("Wave Texture",2D) = "black" {}
        [Toggle(_SOLID)]_Solid ("SolidColor",int) = 0
         _Color ("SolidColor",Color) = (1,1,1,1)
        [HDR] _BaseColor("Base Color", Color) = (0.1, 0.1, 0.1)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1

        [Toggle] _Switch2("Gradient 2", Float) = 1
        [Toggle] _Switch3("Gradient 3", Float) = 1
        [Toggle] _Switch4("Gradient 4", Float) = 1

        _Direction1("Direction 1", Vector) = (0, 1, 0)
        _Direction2("Direction 2", Vector) = (0.5, 1, 0)
        _Direction3("Direction 3", Vector) = (-0.3, -1, -0.2)
        _Direction4("Direction 4", Vector) = (0, 1, 0)

        [HDR] _Color1("Color 1", Color) = (0.16, 0.18, 0.19)
        [HDR] _Color2("Color 2", Color) = (0.26, 0.28, 0.21)
        [HDR] _Color3("Color 3", Color) = (0.15, 0.15, 0.12)
        [HDR] _Color4("Color 4", Color) = (1.00, 0.99, 0.95)

        _Exponent1("Exponent 1", Range(1, 20)) = 1
        _Exponent2("Exponent 2", Range(1, 20)) = 1
        _Exponent3("Exponent 3", Range(1, 20)) = 1
        _Exponent4("Exponent 4", Range(1, 20)) = 20

        [HideInInspector] _NormalizedVector1("-", Vector) = (0, 1, 0)
        [HideInInspector] _NormalizedVector2("-", Vector) = (0, 1, 0)
        [HideInInspector] _NormalizedVector3("-", Vector) = (0, 1, 0)
        [HideInInspector] _NormalizedVector4("-", Vector) = (0, 1, 0)
    }

    CGINCLUDE


    #pragma shader_feature _ _SOLID _SWITCH2_ON _SWITCH3_ON _SWITCH4_ON

    #include "UnityCG.cginc"

    half3 _BaseColor;
    half _Exposure;

    half3 _NormalizedVector1;
    half3 _NormalizedVector2;
    half3 _NormalizedVector3;
    half3 _NormalizedVector4;

    half3 _Color1;
    half3 _Color2;
    half3 _Color3;
    half3 _Color4;

    half _Exponent1;
    half _Exponent2;
    half _Exponent3;
    half _Exponent4;

    struct appdata_t {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        float3 texcoord : TEXCOORD0;
        float2 uv : TEXCOORD1;
    };
float4 _WaveTex_ST;
    v2f vert(appdata_t v)
    {
        v2f o;
        float4x4 cam = UNITY_MATRIX_MVP;
       // cam[0][0]*=1000;
        //cam[1][1]*=1000;
        //cam[2][2]*=1000;
        o.vertex = UnityObjectToClipPos(v.vertex);
        float2 uv = v.uv ;
        //uv.y = uv.y + sin((uv.x*40*UNITY_PI+_Time[2]))*0.01;
        o.uv = uv;
        //(v.vertex*100000).xyz;
#if defined(UNITY_REVERSED_Z)
		// when using reversed-Z, make the Z be just a tiny
		// bit above 0.0
		o.vertex.z = 1.0e-9f;
#else
		// when not using reversed-Z, make Z/W be just a tiny
		// bit below 1.0
		o.vertex.z = o.vertex.w - 1.0e-6f;
#endif
        o.texcoord = v.vertex;

        return o;
    }
half4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _WaveTex;
    float offsets[9] = {0.0,0.2,0.8,0.6,0.3,0.05,0.2,0.1,0.4};
    half4 frag(v2f i) : SV_Target
    {
        #if _SOLID
            return _Color;
        #endif
        
        half3 d = normalize(i.texcoord.xyz);
        half3 c = _BaseColor;
        half v = dot(d, _NormalizedVector1) * 0.5 + 0.5;
        c += _Color1 * v*v;//pow( v , _Exponent1);//#MOR : usig pretty close to ^2 so do it the cheap way
        
        #ifdef _SWITCH2_ON
            c += _Color2 * pow((dot(d, _NormalizedVector2) + 1) * 0.5, _Exponent2);

            #ifdef _SWITCH3_ON
                c += _Color3 * pow((dot(d, _NormalizedVector3) + 1) * 0.5, _Exponent3);

                #ifdef _SWITCH4_ON
                    c += _Color4 * pow((dot(d, _NormalizedVector4) + 1) * 0.5, _Exponent4);
                #endif
            #endif 
        #endif

        //Add pattern to the sky
        
        fixed2 tex = i.uv.xy;
        float sinVal =  sin((tex.y*20*UNITY_PI+_SinTime[1]*0.75));
        sinVal =sinVal*sinVal*sinVal;// (sinVal * sinVal + 3 - (2*sinVal));
        sinVal *= 0.005;
        tex.y = tex.y + sinVal;
        tex = tex *_MainTex_ST.xy;
        fixed2 wholepart = floor(abs(tex));
        float2 move =  _Time[0]*0.33 * (wholepart%2>0?1:-1);
        c = lerp(c,c +  1- tex2D(_MainTex,tex + move).rgb,0.05);
        
        
        //Wave
        fixed2 coords01 = (i.texcoord.xz+0.5); // Sphere is mapped -0.5 to +0.5 so shift for 0-1
        fixed2 pole = (1-abs(i.texcoord.xz * 2)); // 1 is at middle, 0 at polar extremes.

        fixed2 waveUV =  coords01 *_WaveTex_ST.xy + _WaveTex_ST.zw;//Tiling
        float offsetX = tex2D(_WaveTex,fixed2(_Time[0]*0.2 + coords01.x *0.8,0)).r;
         waveUV.x += offsetX;
         //return frac(offsetX.x);
        float2 hX = coords01;
        hX = (hX * hX) * (3 - (2*hX)); //hermite curve
        waveUV.x += coords01.x + coords01.x - hX.x;//mirror hermite.
        
        //Pull back on far edge when behind, pull forwards when ahead.
        float xval = (coords01.x+coords01.x-hX.x)-0.5;
        xval = xval *xval;
        float pull = (pole.y*((xval)*20));
        waveUV.x += pull;//this is to pull in the far Y towards the middle.
        
        //Scroll
        waveUV.x += frac(_Time[0]); //Scroll speed;

        //Wave along length
        fixed sinWave = sin( frac(lerp(waveUV.y,waveUV.y*waveUV.y,pole.y) + lerp(waveUV.x*0.1,waveUV.x,pole.x) + _Time[0] * 5 ) *2* UNITY_PI) + 1;
        sinWave = lerp(0,sinWave,saturate(pole.x));
        //return sinWave;
        fixed2 waveUVFinal = waveUV + (sinWave*0.02);
         //waveUVFinal.x += offsetX;
        fixed waveTex  =  lerp(0.01,0.05,(pole.x*(pole.y*pole.y)));//tex2D(_WaveTex,waveUVFinal).r;
        waveTex =  1-saturate(abs(frac(waveUVFinal) - 0.5)*20);// < waveTex;
        waveTex = waveTex*waveTex*waveTex;
        float addVal = waveTex;
        float2 fade = 1-abs(hX-0.5)*2;
        addVal = lerp(0,addVal,fade.x*fade.y);
        addVal = addVal*addVal;
        //return 1-(abs(hX-0.5)*2);
        c.rgb += 0.36*addVal;
        
        
        return half4(c * _Exposure, 1);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
    Fallback Off
}
