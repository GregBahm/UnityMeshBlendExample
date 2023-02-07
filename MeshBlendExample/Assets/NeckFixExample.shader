Shader "Unlit/NeckFixExample"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float4 color : COLOR;
                uint id : SV_VertexID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            StructuredBuffer<float4> _NeckPointsBuffer; // xyz = neckPos, a = alpha
            float4x4 _NeckBoneTransform;

            sampler2D _MainTex;

            float4 GetAdjustedPos(appdata v)
            {
                float4 neckPoint = _NeckPointsBuffer[v.id];
                float4 worldNeck = mul(_NeckBoneTransform, float4(neckPoint.xyz, 1));
                float4 neckScreenPos = mul(UNITY_MATRIX_VP, worldNeck);
                float weight = (1 - v.color.a) * neckPoint.a;
                float4 baseScreenPos = UnityObjectToClipPos(v.vertex);
                return lerp(baseScreenPos, neckScreenPos, weight);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = GetAdjustedPos(v);
                o.uv = v.uv;
                o.color = _NeckPointsBuffer[v.id]; //v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
