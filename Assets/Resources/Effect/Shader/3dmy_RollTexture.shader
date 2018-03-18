// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "3dmyshader/Roll Texture(1)" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Main Texture", 2D) = "white" {}
	_NoiseTex ("Noise Texture (RG)", 2D) = "white" {}
	_RollTimeX ("Roll Time X", Float) = 0.2
	_RollTimeY ("Roll Time Y", Float) = 0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	// ---- Fragment program cards
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_particles
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float _RollTimeX;
			float _RollTimeY;
			fixed4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texMain : TEXCOORD0;
				float2 texNoise : TEXCOORD1;
			};
			float4 _MainTex_ST;
			float4 _NoiseTex_ST;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texMain = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texNoise = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				return o;
			}
			fixed4 frag (v2f i) : COLOR
			{
				half2 uvoft = i.texMain;
				uvoft.x += _Time.yx * _RollTimeX;
				uvoft.y += _Time.yx * _RollTimeY;
				fixed4 offsetColor = tex2D(_NoiseTex, i.texNoise);
				fixed4 mainColor = tex2D(_MainTex, uvoft);
				return 2.0 * i.color * _TintColor * mainColor * offsetColor.a;
			}
			ENDCG
		}
	} 	
}
}