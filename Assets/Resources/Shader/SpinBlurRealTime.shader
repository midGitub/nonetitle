// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpinBlurRealTime"
{
	Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius("Radius", Range(0.0, 1.0)) = 0.5
        _Step("Step", Range(0.001, 0.1)) = 0.01 //don't be too small, otherwise performance will be bad
        _Opacity("Opacity", Range(0.0, 1.0)) = 0.7
    }

	SubShader
	{
		//SpinBlur.shader: used for Graphics.Blit, so don't set tags, blend or stencil.
    	//SpinBlurRealTime.shader: used for real-time, so set them.
    	Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		Blend SrcAlpha OneMinusSrcAlpha

		Stencil
		{
			Ref 1
			Comp Equal
		}

	 	Pass {
	        CGPROGRAM

	        #pragma vertex vert
	        #pragma fragment frag
	        #include "UnityCG.cginc"

	        sampler2D _MainTex;
	        fixed4 _Color;
	        float _Radius;
	        float _Step;
	        float _Opacity;

	        struct appdata_t     
	        {     
	            float4 vertex   : POSITION;     
	            float4 color    : COLOR;     
	            float2 texcoord : TEXCOORD0;     
	        };

	        struct v2f {
	            float4 pos : SV_POSITION;
	            fixed4 color    : COLOR;
	            float2 uv : TEXCOORD0;
	        };

	        v2f vert( appdata_t v ) 
	        {
	            v2f o;
	            o.pos = UnityObjectToClipPos(v.vertex);
	            o.uv = v.texcoord.xy;
	            o.uv.y = o.uv.y * 2 - 0.5;
	            o.color = v.color * _Color;
	            return o;
	        }

	        fixed4 grabPixel(v2f i, float dx, float dy)
	        {
	        	fixed4 result;
	        	float x = i.uv.x + dx;
	        	float y = i.uv.y + dy;
	        	if(y >= 1.0 || y <= 0.0)
	        		result = fixed4(0, 0, 0, 0);
	        	else
	        		result = tex2D(_MainTex, float2(x, y));
	        	return result;
	        }

	        fixed4 frag(v2f i) : SV_Target 
	        {
			    fixed4 sum = grabPixel(i, 0.0, 0.0);
	            int measurments = 1;

				for (float r = 0; r <= _Radius; r += _Step)
	            {
					sum += grabPixel(i, 0.0, r);
	                sum += grabPixel(i, 0.0, -r);
	                measurments += 2;
	            }

	            sum = sum / measurments * i.color * _Opacity;
	            return sum;
	        }
	        ENDCG
	    }
	}
}
