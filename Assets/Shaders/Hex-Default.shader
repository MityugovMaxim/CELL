Shader "Hex/Default"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vertData
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct fragData
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            sampler2D _MainTex;
            fixed4    _Color;

            fragData vert (vertData IN)
            {
                fragData OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv     = IN.uv;
                OUT.color  = _Color * IN.color;
                return OUT;
            }

            fixed4 frag (fragData IN) : SV_Target
            {
                return tex2D(_MainTex, IN.uv) * IN.color;
            }
            ENDCG
        }
    }
}
