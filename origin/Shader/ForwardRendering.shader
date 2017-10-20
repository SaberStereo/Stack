// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "OwnShader/ForwardRendering" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Specular ("Specular", Color) = (1,1,1,1)
		_Gloss ("Gloss", Range(8,255)) = 20 
	}
	SubShader{
		Tags { "RenderType"="Opaque" "Queue" = "Transparent"}

		Pass{
			Tags{"LightMode" = "ForwardBase"}

			CGPROGRAM

			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag


			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Color;
			fixed4 _Specular;
			float _Gloss;

			struct a2v{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float3 worldnormal : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
				SHADOW_COORDS(2)
			};

			v2f vert(a2v v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldnormal = mul(unity_ObjectToWorld,v.normal);
				o.worldpos = mul(unity_ObjectToWorld,v.vertex).xyz;

				TRANSFER_SHADOW(o);

				return o;
			}

			float4 frag (v2f i) : SV_Target{
				float3 worldnormal = normalize(i.worldnormal);
				float3 worldlightdir = normalize(_WorldSpaceLightPos0.xyz);

				float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				float halflambert = dot(worldnormal,worldlightdir)*0.5 + 0.5;

				float3 diffuse = _LightColor0.rgb * _Color.rgb * saturate(dot(worldnormal, worldlightdir));//halflambert;

				float3 viewdir = normalize(_WorldSpaceCameraPos.xyz - i.worldpos);
				float3 halfdir = normalize(worldlightdir + viewdir);
				float3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(worldnormal,halfdir)), _Gloss);

//				float shadow = SHADOW_ATTENUATION(i);
//				float atten = 1.0;
				UNITY_LIGHT_ATTENUATION(atten,i,i.worldpos);

				return float4(ambient + (diffuse + specular) * atten , 1.0);
			}

			ENDCG
		}
		Pass{
			Tags{"LightMode" = "ForwardAdd"}

			Blend One One

			CGPROGRAM

			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex vert
			#pragma fragment frag


			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Color;
			fixed4 _Specular;
			float _Gloss;

			struct a2v{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float3 worldnormal : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
			};

			v2f vert(a2v v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldnormal = mul(unity_ObjectToWorld,v.normal);
				o.worldpos = mul(unity_ObjectToWorld,v.vertex).xyz;

				return o;
			}

			float4 frag (v2f i) : SV_Target{
				float3 worldnormal = normalize(i.worldnormal);
				#ifdef USING_DIRECTIONAL_LIGHT
					float3 worldlightdir = normalize(_WorldSpaceLightPos0.xyz);
				#else
					float3 worldlightdir = normalize(_WorldSpaceLightPos0.xyz - i.worldpos);
				#endif


				float3 diffuse = _LightColor0.rgb * _Color.rgb *(dot(worldnormal,worldlightdir)*0.5 + 0.5);

				float3 viewdir = normalize(_WorldSpaceCameraPos.xyz - i.worldpos);
				float3 halfdir = normalize(worldlightdir + viewdir);
				float3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(worldnormal,halfdir)), _Gloss);

				#ifdef USING_DIRECTIONAL_LIGHT
					fixed atten = 1.0;   
				#else
					float3 LightCoord = mul(unity_WorldToLight, float4(i.worldpos,1)).xyz;
					float atten = tex2D(_LightTexture0, dot(LightCoord,LightCoord).rr).UNITY_ATTEN_CHANNEL;
					//float distence = length(_WorldSpaceLightPos0.xyz - i.worldpos);
					//float atten = 1/distence;
				#endif

				return float4((diffuse + specular) * atten , 1.0);
			}


			ENDCG
		}
	}
	FallBack "VertexLit"
}
