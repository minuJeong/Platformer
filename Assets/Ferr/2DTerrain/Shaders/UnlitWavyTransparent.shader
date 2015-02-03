Shader "Ferr/Unlit Wavy Textured Vertex Color Transparent" {
	Properties {
		_MainTex("Texture (RGBA)", 2D) = "white" {}
		_WaveSizeX("Wave Size X",  Float) = 0.25
		_WaveSizeY("Wave Size Y",  Float) = 0.25
		_WaveSpeed("Wave Speed", Float) = 4
		_PositionScale("Position Scale", Float) = 4
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent"  }
		Blend SrcAlpha OneMinusSrcAlpha

		LOD 100
		Cull      Off
		Lighting  Off
		ZWrite    Off
		Fog {Mode Off}
		
		Pass {
			CGPROGRAM
			#pragma vertex         vert
			#pragma fragment       frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4    _MainTex_ST;
			
			float _WaveSizeX;
			float _WaveSizeY;
			float _WaveSpeed;
			float _PositionScale;

			struct appdata_ferr {
			    float4 vertex   : POSITION;
			    float4 texcoord : TEXCOORD0;
			    fixed4 color    : COLOR;
			};
			struct VS_OUT {
				float4 position : SV_POSITION;
				fixed4 color    : COLOR;
				float2 uv       : TEXCOORD0;
			};

			VS_OUT vert (appdata_ferr input) {
				float4 world      = mul(_Object2World, input.vertex);
				float  waveOffset = (world.x + world.y + world.z) / _PositionScale;
				float  wave       = (_Time.z + waveOffset) * _WaveSpeed;
				
				VS_OUT result;
				result.position = mul (UNITY_MATRIX_MVP, input.vertex + float4(cos(wave) * _WaveSizeX, sin(wave) * _WaveSizeY, 0, 0));
				result.uv       = TRANSFORM_TEX (input.texcoord, _MainTex);
				result.color    = input.color;

				return result;
			}

			fixed4 frag (VS_OUT input) : COLOR {
				fixed4 color = tex2D(_MainTex, input.uv);
				return color * input.color;
			}
			ENDCG
		}
	}
}
