Shader "BlitScreen/CRTDiffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex ("Mask texture", 2D) = "white" {}
		_DisplacementTex("_DisplacementTex", 2D) = "white" {}
		_maskBlend ("Mask blending", Float) = 0.5
		_maskSize ("Mask Size", Float) = 1
		_Strength ("Distrortion Strength", Range(-.2, .5)) = 0
		_SelfDisplaceStrength("_SelfDisplaceStrength", Float) = .1
		_SelfDisplaceDir("Displace Red Direction", Vector) = (0, 0, 0, 0)
	}
	SubShader {

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"
		
			uniform sampler2D _MainTex;
			uniform sampler2D _MaskTex;
			uniform sampler2D _DisplacementTex;
			
			fixed _maskBlend;
			fixed _maskSize;
			half _Strength;
			float _SelfDisplaceStrength;
			fixed2 _SelfDisplaceDir;
 
			// fixed4 frag (v2f_img i) : COLOR {
			// 	half3 _out = tex2D(_MainTex, i.uv);
			// 	return fixed4(1, _out.b, _out.g, 1);

			// 	half2 n = tex2D(_DisplacementTex, i.uv);
			// 	half3 colorDisplace = tex2D(_MainTex, saturate(i.uv + _SelfDisplaceDir)); 
			// 	half2 d = n * 2 -1;
			// 	i.uv += d * _Strength;
			// 	i.uv = saturate(i.uv);
			// 	half2 mUV = i.uv * _maskSize;
			// 	mUV.y += _Time[1];
			// 	fixed4 mask = tex2D(_MaskTex, mUV);
			// 	fixed4 base = tex2D(_MainTex, i.uv);
			// 	fixed4 scanned = lerp(base, base * mask, _maskBlend	);
			//  	return lerp(scanned - fixed4(.1, 0, 0, 0) * dot(colorDisplace, colorDisplace) * _SelfDisplaceStrength, fixed4(.5, 0, 0, .5), .5 );

			// }


			 fixed4 frag (v2f_img i) : COLOR {
 fixed4 mask = tex2D(_MaskTex, i.uv * _maskSize);
 fixed4 base = tex2D(_MainTex, i.uv);
 return lerp(base, mask, _maskBlend );
 }
			ENDCG
		}
	}
}