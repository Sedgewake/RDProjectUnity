Shader "Custom/RetroWaterShader"
{
    Properties
    {
        _MainTex ("Base Water Texture", 2D) = "white" { }
        _BumpMap ("Bump Map", 2D) = "bump" { }
        _WaterColor ("Water Color", Color) = (.3, .4, .7, 1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0.05, 0.05, 0, 0)
        _TimeMultiplier ("Time Multiplier", float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            float4 _WaterColor;
            float2 _ScrollSpeed;
            float _TimeMultiplier;

            // Simple vertex shader with some sine wave displacement
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Displace vertex position to simulate water ripples
                float2 uv = v.uv;
                uv += _ScrollSpeed * _TimeMultiplier * _Time.y; // Scroll the texture
                float height = sin(uv.x * 10.0 + _Time.y * 0.5) * 0.1 + cos(uv.y * 10.0 + _Time.y * 0.3) * 0.1; // Basic sine wave displacement for ripples
                o.vertex.y += height;

                o.uv = uv;
                return o;
            }

            // Fragment shader to add color and water texture
            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv) * _WaterColor;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
