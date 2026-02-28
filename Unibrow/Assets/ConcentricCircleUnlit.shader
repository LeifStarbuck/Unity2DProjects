Shader "Unlit/ConcentricCircle"
{
    Properties
    {
        _LightColor ("Light Color", Color) = (1,0.33,0.33,1)
        _DarkColor  ("Dark Color",  Color) = (0.66,0,0,1)
        _InnerRadius("Inner Radius (0-0.5)", Range(0,0.5)) = 0.22
        _Softness   ("Edge Softness", Range(0.0001,0.05)) = 0.01
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
            float _InnerRadius;
            float _Softness;

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
                // UV 0..1 => centered -0.5..0.5
                float2 p = i.uv - 0.5;
                float r = length(p);

                // Outer circle alpha (soft edge)
                float outer = 1.0 - smoothstep(0.5 - _Softness, 0.5, r);

                // Inner circle mask (soft edge)
                float inner = 1.0 - smoothstep(_InnerRadius - _Softness, _InnerRadius, r);

                fixed4 col = lerp(_DarkColor, _LightColor, inner);
                col.a *= outer;

                return col;
            }
            ENDCG
        }
    }
}