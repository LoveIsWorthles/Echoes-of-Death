Shader "Custom/URP_StencilMask" {
    SubShader {
        // Queue=Geometry-100 ensures the mask is drawn BEFORE the fog
        Tags { "RenderType"="Opaque" "Queue"="Geometry-100" "RenderPipeline"="UniversalPipeline" }
        ColorMask 0
        ZWrite Off

        // Write the secret number "1"
        Stencil {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings vert(Attributes v) {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float4 frag(Varyings i) : SV_Target {
                return float4(0,0,0,0);
            }
            ENDHLSL
        }
    }
}