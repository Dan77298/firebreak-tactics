Shader "Custom/DynamicFog"
{
    Properties
    {
        _MainTex ("Tile Texture", 2D) = "white" {}
        _FogTex ("Fog Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            sampler2D _FogTex;
            float4 _MainTex_ST;
            float4 _FogColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                // Sample the original tile texture
                half4 texColor = tex2D(_MainTex, i.uv);
                
                // Sample and animate the fog texture
                half4 fogColor = tex2D(_FogTex, i.uv + _Time.y * 0.1);
                
                // Calculate fog factor based on depth
                half fogFactor = i.vertex.z / i.vertex.w;
                fogFactor = saturate(1.0f - fogFactor);
                
                // Blend the tile and fog textures based on fog factor
                half4 finalColor = lerp(texColor, fogColor, fogFactor);
                
                return finalColor;
            }
            ENDCG
        }
    }
}