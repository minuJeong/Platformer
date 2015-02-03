sampler2D _MainTex;
half4     _MainTex_ST;

#ifdef FERR2DT_LIGHTMAP
sampler2D_half unity_Lightmap;
half4          unity_LightmapST;
#endif

struct appdata_ferr_lit {
    float4 vertex     : POSITION;
    half2  texcoord   : TEXCOORD0;
    #ifdef FERR2DT_LIGHTMAP
    half2  lightcoord : TEXCOORD1;
    #endif
    fixed4 color      : COLOR;
};
struct VS_OUT_LIT {
	float4 pos     : SV_POSITION;
	half2  uv      : TEXCOORD0;
	#ifdef FERR2DT_LIGHTMAP
	half2  lightuv : TEXCOORD1;
	#endif
	#if MAX_LIGHTS > 0
	half3  viewpos : TEXCOORD2;
	#endif
	fixed4 color   : COLOR;
};

float3 GetLight(int i, float3 aViewPos) {
	half3  toLight = unity_LightPosition[i].xyz - aViewPos * unity_LightPosition[i].w;
	half   distSq  = dot(toLight, toLight);
	half   atten   = 2.0 / ((distSq * unity_LightAtten[i].z) + 1.0);

	return ((unity_LightColor[i].rgb * pow(atten,1.75)));
}

VS_OUT_LIT vert (appdata_ferr_lit input) {
    VS_OUT_LIT result;
    result.pos     = mul (UNITY_MATRIX_MVP, input.vertex);
    #if MAX_LIGHTS > 0
    result.viewpos = mul (UNITY_MATRIX_MV,  input.vertex).xyz;
    #endif
    result.uv      = TRANSFORM_TEX (input.texcoord, _MainTex);
    result.color   = input.color;
    #ifdef FERR2DT_LIGHTMAP
    result.lightuv = input.lightcoord * unity_LightmapST.xy + unity_LightmapST.zw; 
    #endif
    
	return result;
}

fixed4 frag (VS_OUT_LIT inp) : COLOR {
	fixed4 color = tex2D(_MainTex, inp.uv);
	#ifdef FERR2DT_LIGHTMAP
	fixed3 light = DecodeLightmap(tex2D(unity_Lightmap, inp.lightuv));
	#else
	fixed3 light = UNITY_LIGHTMODEL_AMBIENT;
	#endif
	
	#if MAX_LIGHTS > 0
	for (int i = 0; i < MAX_LIGHTS; i++) {
		light += GetLight(i, inp.viewpos);
	}
	#endif
	
	color = color * inp.color;
	color.rgb *= light;
	return color;
}