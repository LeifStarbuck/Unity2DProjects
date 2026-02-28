Shader "Unlit/SplatterRectTwoTone"
{
    Properties
    {
        _LightColor ("Light Color", Color) = (1,0.33,0.33,1)
        _DarkColor  ("Dark Color",  Color) = (0.66,0,0,1)
        _TopBand    ("Top Band Height (UV)", Range(0,0.2)) = 0.06
        _Alpha      ("Alpha", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _LightColor;
            fixed4 _DarkColor;
            float  _TopBand;
            float  _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UV.y: 0 bottom -> 1 top
                fixed4 col = (i.uv.y >= (1.0 - _TopBand)) ? _LightColor : _DarkColor;
                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}