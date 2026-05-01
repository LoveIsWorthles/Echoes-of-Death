Shader "Custom/URP_StencilFog" {
    Properties {
        _BaseColor ("Fog Color", Color) = (0, 0, 0, 0.98)
    }
    SubShader {
        // Queue=Transparent ensures this draws AFTER the mask
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        // Only draw if the secret number IS NOT 1
        Stencil {
            Ref 1
            Comp NotEqual 
        }

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            float4 _BaseColor;

            Varyings vert(Attributes v) {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float4 frag(Varyings i) : SV_Target {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}