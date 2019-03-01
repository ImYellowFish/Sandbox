Shader "NaiveBlock/SimpleTerrainLateral" {
	Properties {
		_TerrainTex ("Terrain Tex", 2D) = "white" {}
		_UndergroundTex("Underground Tex", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _TerrainTex;
		sampler2D _UndergroundTex;

		struct Input {
			float4 color : COLOR;
			float2 uv_TerrainTex;
			float2 uv2_UndergroundTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c1 = tex2D (_UndergroundTex, IN.uv2_UndergroundTex) * _Color;
			fixed4 c2 = tex2D(_TerrainTex, IN.uv_TerrainTex) * _Color;
			fixed4 c = lerp(c1, c2, IN.color.r);
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
