Shader "Project Z/Old Television"
{
    Properties
    {
        _MainTex ("Source", 2D) = "black" {}
        [Int] _Density ("Line Density", Range(0, 20)) = 17
        _Distortion ("Distortion", Range(0, 0.5)) = 0.1
        _Brightness ("Brightness", Range(1, 4)) = 3
        _Interval ("Interval", Range(0, 32)) = 0.6
        _Speed ("Speed", Range(-1, 1)) = -0.02
        _RGBOffset ("RGBOffset", Vector) = (0.003, 0, -0.003, 0)
        _WarpTexH ("Warp", 2D) = "white" {}
        _WarpIntenH ("Warp Intensity Horizontal", Range(0, 1)) = 0.02
        _WarpTexV ("Warp", 2D) = "white" {}
        _WarpIntenV ("Warp Intensity Vertical", Range(0, 1)) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                half2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _WarpTexH;
            sampler2D _WarpTexV;
            half4 _RGBOffset;
            half _Distortion;
            half _Brightness;
            half _Density;
            half _Interval;
            half _Speed;
            half _ScreenHeight;
            half _WarpIntenH;
            half _WarpIntenV;

            v2f vert (appdata_img v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed warpH = lerp(0, tex2D(_WarpTexH, i.uv).r * 2 - 1, _WarpIntenH);
                fixed warpV = lerp(0, tex2D(_WarpTexV, half2(i.uv.y, frac(_Time.z) * i.uv.y)).r * 2 - 1, _WarpIntenV);
                half2 uv = i.uv + pow(i.uv.yx * 2 - 1, 2) * _Distortion * (i.uv - 0.5h) + half2(warpH, warpV);

                fixed4 col = fixed4(tex2D(_MainTex, half2(uv.x + _RGBOffset.r, uv.y + _RGBOffset.a)).r, tex2D(_MainTex, half2(uv.x + _RGBOffset.g, uv.y)).g, tex2D(_MainTex, half2(uv.x + _RGBOffset.b, uv.y - _RGBOffset.a)).b, 1);
                uv = 1 - pow(i.uv * 2 - 1, 4);
                half2 screenHeightRates = half2(1080 / _ScreenHeight, _ScreenHeight / 1080);
                return col * _Brightness * uv.x * uv.y * pow(sin((i.uv.y - frac(_Time.x * _Speed * screenHeightRates.x)) * 100 * _Density * screenHeightRates.y), _Interval);
            }
            ENDCG
        }
    }
}
