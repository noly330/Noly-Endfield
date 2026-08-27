Shader "Endfield/PerfectDodgeGhost"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.05, 0.55, 1, 0.28)
        _RimColor ("Rim Color", Color) = (0.15, 0.85, 1, 1)
        _RimPower ("Rim Power", Float) = 2.2
        _EmissionIntensity ("Emission Intensity", Float) = 2.6
        _Alpha ("Alpha", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _RimColor;
                half _RimPower;
                half _EmissionIntensity;
                half _Alpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInputs.normalWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);
                // Fresnel 边缘光：轮廓越靠边越亮（蓝白）
                half fresnel = pow(saturate(1.0 - dot(normalWS, viewDirWS)), _RimPower);
                half3 color = _BaseColor.rgb + _RimColor.rgb * fresnel * _EmissionIntensity;
                half alpha = saturate(_Alpha + fresnel * 0.6);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Unlit"
}
