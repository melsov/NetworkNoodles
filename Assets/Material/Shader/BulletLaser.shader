Shader "Unlit/BulletLaser"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SteepSlopeTex("Texture", 2D) = "red" {}
		_InnerColor("_InnerColor", Color) = (1.0, 0.0, 0.0, 1.0)
		_xOffset("_xOffset", Range(0, 1)) = 0.25
		_transitionUp("_transitionUp", Range(0, 2)) = 0.2
		_transitionDown("_transitionDown", Range(0, 2)) = 0.2
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _InnerColor;
			fixed _xOffset;
			fixed _transitionUp;
			fixed _transitionDown;
			

			fixed rand01(float seed) {
				return frac(sin(seed) * 10000.0) > .5;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv =  v.uv; //TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed trans = abs(i.uv.y  - .5) * 2.2;
				trans = .95 - trans;
				fixed bulb = i.uv.x - _xOffset;
				fixed cA = (bulb < 0);
				fixed beamA = (_xOffset + bulb) / _xOffset * cA;
				fixed beamB = (1 - _xOffset - bulb) / (1 - _xOffset) * (1 - cA);
				return fixed4(_InnerColor.xyz, saturate(trans * (beamA + beamB)));

			}
			ENDCG
		}
	}
}
