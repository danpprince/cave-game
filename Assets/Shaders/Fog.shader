﻿// From https://github.com/KaimaChen/Unity-Shader-Demo/blob/master/UnityShaderProject/Assets/Depth/Shaders/Fog.shader
Shader "Kaima/Depth/Fog"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FogColor("Fog Color", Color) = (1,1,1,1)
		_FogDensity("Fog Density", Float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;
			float4 _FogColor;
			float _FogDensity;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.uv, v.uv);
				#if UNITY_UV_STARTS_AT_TOP
					if(_MainTex_TexelSize.y < 0)
						o.uv.w = 1 - o.uv.w;
				#endif
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 imageColor = tex2D(_MainTex, i.uv.xy);
				// Avoid overwriting torch particles, minotaur eyes, coin reflections, etc
				if (imageColor.r > 0.8) {
					return imageColor;
				}

				float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv.zw));
				float linearDepth = Linear01Depth(depth);
				float fogDensity = saturate(linearDepth * _FogDensity);
				float4 finalColor = lerp(imageColor, _FogColor, fogDensity);
				return finalColor;
			}
			ENDCG
		}
	}
}
