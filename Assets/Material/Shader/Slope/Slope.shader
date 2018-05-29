Shader "Unlit/Slope"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SteepSlopeTex("Texture", 2D) = "red" {}
		_cutoff("_cutoff", Range(-1, 1)) = 0.0
		_transitionUp("_transitionUp", Range(0, 2)) = 0.2
		_transitionDown("_transitionDown", Range(0, 2)) = 0.2
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			#define EXPAND_NORMAL 1000

			struct appdata
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				fixed4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float2 rands : TEXCOORD2;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 normal : COLOR;				
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _SteepSlopeTex;
			float4 _SteepSlopeTex_ST;
			fixed _cutoff;
			fixed _transitionUp;
			fixed _transitionDown;
			

			fixed rand01(float seed) {
				return frac(sin(seed) * 10000.0) > .5;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _SteepSlopeTex);
				o.rands = fixed2(rand01( v.normal.x * v.normal.z), rand01(v.normal.y));
				o.normal = float4(v.normal * EXPAND_NORMAL, 1);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 scol = tex2D(_SteepSlopeTex, i.uv2);
				fixed rand = i.rands.x; // rand01(i.normal.x * i.normal.z);
				fixed testUA = saturate(floor(i.normal.y) > (_cutoff + _transitionUp) * EXPAND_NORMAL);
				fixed testLA = saturate(floor(i.normal.y) < (_cutoff - _transitionDown) * EXPAND_NORMAL);

				fixed testU = testUA + (1 - testUA) * rand;

				fixed testL = testLA + (1 - testLA) * (1 - testU) * (1 - rand);

				col = testU * col + (testL) * scol;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
