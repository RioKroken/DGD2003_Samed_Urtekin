// Videodaki teknikle aynı mantık (Shader Graph "Easy Outline" / inverted hull):
// Vertex: Object Position + Object Normal * genişlik
// Cull Front → sadece genişletilmiş mesh'in arka yüzleri = silüet çizgisi
// Kaynak fikir: https://www.youtube.com/watch?v=ODab7a-tOlY
Shader "Gorkem/EasyOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 1, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "InvertedHullOutline"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Front
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 n = normalize(input.normalOS);
                float3 posOS = input.positionOS.xyz + n * _OutlineWidth;
                output.positionHCS = TransformObjectToHClip(posOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return half4(_OutlineColor.rgb, 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
