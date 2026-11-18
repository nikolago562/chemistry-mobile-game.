Shader "Chemistry/PixelCharacter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _HairColor ("Hair", Color) = (0.26,0.18,0.18,1)
        _SkinColor ("Skin", Color) = (1,0.75,0.45,1)
        _ShirtColor ("Shirt", Color) = (0.26,0.45,0.67,1)
        _CoatColor ("Lab coat", Color) = (1,1,1,1)
        _HasCoat ("Coat equipped", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color, _HairColor, _ShirtColor, _CoatColor, _SkinColor;
            float _HasCoat;

            struct appdata { float4 vertex : POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv);
                float2 pixel = floor(input.uv * _MainTex_TexelSize.zw);
                float x = fmod(pixel.x, 32);
                float y = 31 - fmod(pixel.y, 32);
                float row = floor((_MainTex_TexelSize.w - 1 - pixel.y) / 32);
                bool outline = max(color.r, max(color.g, color.b)) < 0.08;
                if (color.a > 0.5 && !outline)
                {
                    float shade = lerp(0.65, 1.0, color.r);
                    bool back = row >= 3 && row <= 5;
                    bool hair = y <= 11 || (back && y <= 16) ||
                        (y <= 13 && (x < 13 || x > 18));
                    if (hair)
                        color.rgb = _HairColor.rgb * shade;
                    else if (y >= 21)
                        color.rgb = y >= 22 ? fixed3(0.16,0.19,0.28) : fixed3(0.25,0.31,0.44);
                    else if (y >= 17 && y <= 20 && x >= 12 && x <= 19)
                    {
                        color.rgb = lerp(_ShirtColor.rgb, _CoatColor.rgb, _HasCoat) * shade;
                        if (_HasCoat > 0.5 && (x == 15 || x == 16) && row == 0)
                            color.rgb *= 0.7;
                    }
                    else if (!(color.r > 0.9 && color.g > 0.9 && color.b > 0.9))
                        color.rgb = _SkinColor.rgb * shade;
                }
                color *= input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
