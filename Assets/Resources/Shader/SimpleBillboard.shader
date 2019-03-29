Shader "NaiveBlock/SimpleBillboard"
{
	Properties
	{
		_MainColor ("MainColor", Color) = (0.5, 0.5, 0.5, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _MainColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				float3 viewPos = float4(UnityObjectToViewPos(v.vertex.xyz), 1.0);
				float3 normal = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(v.normal, 0.0))).xyz;
				normal.y = 0;
				float cosNoZ = abs(dot(normalize(normal), float3(0,0,1)));
				float sinNoZ = sqrt(1.0 - cosNoZ * cosNoZ) * -sign(normal.x * normal.z);
				float3x3 billboardMatrix = float3x3(
						cosNoZ,  0,  -sinNoZ,
						0,       1,  0,
						sinNoZ, 0,  cosNoZ
					);
				//o.vertex = mul(UNITY_MATRIX_P, float4(mul(billboardMatrix, viewPos), 1.0));
				o.vertex = UnityObjectToClipPos(float4(mul(billboardMatrix, v.vertex.xyz), 1.0));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = _MainColor;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
