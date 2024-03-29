﻿Shader "UI/AngleMask"
{
    Properties
    {
		_Split ("Split", Vector) = (0,1,0)
		_Origin ("Split Origin (screen coord)", Vector) = (.5, .5, 0)
		_SplitDist ("Split Border Width", Float) = 1
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Cam1 ("Cam1", 2D) = "magenta" {}
		_Cam2 ("Cam2", 2D) = "cyan" {}
		_Angle ("Split Angle", Float) = 0
		_Reflect ("Split Reflection", Range(0, 1)) = 0
		_Color ("Tint", Color) = (1,1,1,1)

		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _Cam1;
            sampler2D _Cam2;
			float _SplitDist;
			float _Angle;
			float _Reflect;
			float4 _Split;
			float4 _Origin;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
				half2 origin = IN.texcoord.xy - _Origin.xy;
				half angle = _Angle * lerp(1, sign(-origin.x * _Split.x - origin.y * _Split.y), _Reflect);
				half cosAngle = cos(angle);
				half sinAngle = sin(angle);
				half2 split = half2(cosAngle*_Split.x - sinAngle * _Split.y, sinAngle*_Split.x + cosAngle*_Split.y);
				float sval = (origin.x * split.y - origin.y * split.x);
				half4 cam1 = tex2D(_Cam1, IN.texcoord);
				half4 cam2 = tex2D(_Cam2, IN.texcoord);
				half b = .01 * _SplitDist * (_SplitDist - 2);
				half4 tex = cam1 * step(sval, b) + cam2 * step(-sval, b) + half4(0,0,0,1) * step(-abs(sval), -b);
                half4 color = (tex + _TextureSampleAdd) * IN.color;

				/*
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
				*/

                return color;
            }
        ENDCG
        }
    }
}
