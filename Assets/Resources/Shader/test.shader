Shader "Custom/test" {
	Properties
	{
		//TOONY COLORS
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
		_HColor ("Highlight Color", Color) = (0.6,0.6,0.6,1.0)
		_SColor ("Shadow Color", Color) = (0.4,0.4,0.4,1.0)
		
		//DIFFUSE
		_MainTex ("Main Texture (RGB) Spec/Refl Mask (A) ", 2D) = "white" {}
		
		//TOONY COLORS RAMP
		_Ramp ("#RAMPT# Toon Ramp (RGB)", 2D) = "gray" {}
		_RampThreshold ("#RAMPF# Ramp Threshold", Range(0,1)) = 0.5
		_RampSmooth ("#RAMPF# Ramp Smoothing", Range(0.01,1)) = 0.1
		
		//BUMP
		_BumpMap ("#NORM# Normal map (RGB)", 2D) = "bump" {}
		
		//SPECULAR
		_SpecColor ("#SPEC# Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("#SPEC# Shininess", Range(0.01,2)) = 0.1
		_SpecSmooth ("#SPECT# Smoothness", Range(0,1)) = 0.05
		
	}
	
	SubShader
	{
		pass {

			ZTest off Cull Off ZWrite on
			
			CGPROGRAM
			
			#include "UnityCG.cginc"
			
			#pragma vertex vert  
			#pragma fragment fragSobel
			
			sampler2D _MainTex;  
			uniform half4 _MainTex_TexelSize;
			
			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};
			  
			v2f vert(appdata_base v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				float3 normal = mul(UNITY_MATRIX_MVP, v.normal);
				o.pos = o.pos + float4(normal * 0.1f, 0.0f);
				o.uv = v.texcoord;
						 
				return o;
			}
			
			fixed4 fragSobel(v2f i) : SV_Target {
				return float4(1.0f, 0, 0, 1);
 			}
			
			ENDCG
		}


		pass {

			Tags { "RenderType"="Opaque" }
			LOD 200
			
			CGPROGRAM
			
			#include "../../Scripts/CitrusFramework/SDKs/Toony_Colors_Pro2/Shaders 2.0/Include/TCP2_Include.cginc"
			
			#pragma surface surf ToonyColorsSpec 
			#pragma target 3.0
			#pragma glsl
			
			#pragma shader_feature TCP2_RAMPTEXT
			#pragma shader_feature TCP2_BUMP
			#pragma shader_feature TCP2_SPEC TCP2_SPEC_TOON
			#pragma shader_feature TCP2_LIGHTMAP
			
			//================================================================
			// VARIABLES
			
			fixed4 _Color;
			sampler2D _MainTex;
			
		#if TCP2_BUMP
			sampler2D _BumpMap;
		#endif
			fixed _Shininess;
			
			struct Input
			{
				half2 uv_MainTex : TEXCOORD0;
		#if TCP2_BUMP
				half2 uv_BumpMap : TEXCOORD1;
		#endif
			};
			
			//================================================================
			// SURFACE FUNCTION
			
			void surf (Input IN, inout SurfaceOutput o)
			{
				half4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = c.rgb * _Color.rgb;
				o.Alpha = c.a * _Color.a;
				
				//Specular
				o.Gloss = c.a;
				o.Specular = _Shininess;
		#if TCP2_BUMP
				//Normal map
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		#endif
			}
			
			ENDCG
		}
	}
	
	Fallback "Diffuse"
	CustomEditor "TCP2_MaterialInspector"
}
