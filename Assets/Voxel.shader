Shader "Custom/Voxel"
{
  Properties
  {
    _MainTex ("Texture", 3D) = "white" {}
    _Tex ("Tile", 2D) = "white" {}
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing
      #pragma target 3.5

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        fixed4 color : COlOR0;
        float2 uv : TEXCOORD0;
      };

      sampler2D _Tex;
      float4 _Tex_ST;

      sampler3D _MainTex;
      float4 _MainTex_ST;

      v2f vert (appdata v)
      {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);

        o.vertex = UnityObjectToClipPos(v.vertex);
        float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);

        o.color = tex3Dlod(_MainTex, worldVertex);

        o.uv = TRANSFORM_TEX(v.uv, _Tex);

        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        if (i.uv.x > 0.99 || i.uv.y > 0.99 ||
            i.uv.x < 0.01 || i.uv.y < 0.01) {
          i.color = lerp(i.color, fixed4(
            1 - i.color.r,
            1 - i.color.g,
            1 - i.color.b,
            1
          ), 0.1);
        }
        return i.color;
      }
      ENDCG
    }
  }
}
