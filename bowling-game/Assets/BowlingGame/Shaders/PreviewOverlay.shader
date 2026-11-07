Shader "BowlingGame/PreviewOverlay"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _OccludedSaturation ("Occluded Saturation", Range(0, 1)) = 0.15
        _OccludedBrightness ("Occluded Brightness", Range(0, 1)) = 0.65
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+100" }
        Pass
        {
            Name "PreviewOverlay"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _OccludedSaturation;
                half _OccludedBrightness;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _BaseColor;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float lineDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                half4 color = input.color;
                if (lineDepth > sceneDepth + 0.002)
                {
                    half gray = dot(color.rgb, half3(0.2126, 0.7152, 0.0722));
                    color.rgb = lerp(gray.xxx, color.rgb, _OccludedSaturation) * _OccludedBrightness;
                }
                return color;
            }
            ENDHLSL
        }
    }
}

