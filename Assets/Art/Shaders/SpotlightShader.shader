Shader "Zomz/SpotlightShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_CharacterPosition("Char Pos", vector) = (0,0,0,0)
		_CircleRadius("Spotlight Size",Range(0,20)) = 3
		_RingSize("Ring Size",Range(0,5)) = 1
		_Brightness ("Spotlight Brightness",Range(0,0.3)) = 0.15
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float4 _CharacterPosition;
		float _CircleRadius;
		float _RingSize;
		float _Brightness;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)


		void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			float dist = distance(IN.worldPos, _CharacterPosition.xyz);

			//Spotlight
			if(dist<_CircleRadius)
			{
				c = c + _Brightness;
			}
			//Blend Ring
			else if(dist>_CircleRadius && dist<_CircleRadius+_RingSize)
			{
				float blendStrength = dist - _CircleRadius;
				c = c + lerp(_Brightness,0,blendStrength/_RingSize);
			}
			//Outer
			else{
				c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			}

			// Albedo comes from a texture tinted by color

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
