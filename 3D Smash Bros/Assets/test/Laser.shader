Shader "Laser"
{
    Properties
    {
        Vector2_48da6910d2a14d629822e55c17af442c("Speed", Vector) = (-0.5, 0, 0, 0)
        [HDR]Color_9b86f8e1b98947f692ebd161fb82f952("Color", Color) = (0, 1.043137, 2, 1)
        Vector1_b2ae68e338784337b99bdc9665cfbec0("Edge", Float) = 2.4
        Vector2_4b58e89ca621426cae62b9f03e81b483("Scale", Vector) = (0.5, 1, 0, 0)
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        Pass
        {
            Name "Sprite Unlit"
            Tags
            {
                "LightMode" = "Universal2D"
            }

            // Render State
            Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITEUNLIT
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float2 Vector2_48da6910d2a14d629822e55c17af442c;
        float4 Color_9b86f8e1b98947f692ebd161fb82f952;
        float Vector1_b2ae68e338784337b99bdc9665cfbec0;
        float2 Vector2_4b58e89ca621426cae62b9f03e81b483;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_21522b53e93649b5be78f38166b46236_Out_0 = IN.uv0;
            float _Split_74383f7cbc184c61a2eb3da422a74853_R_1 = _UV_21522b53e93649b5be78f38166b46236_Out_0[0];
            float _Split_74383f7cbc184c61a2eb3da422a74853_G_2 = _UV_21522b53e93649b5be78f38166b46236_Out_0[1];
            float _Split_74383f7cbc184c61a2eb3da422a74853_B_3 = _UV_21522b53e93649b5be78f38166b46236_Out_0[2];
            float _Split_74383f7cbc184c61a2eb3da422a74853_A_4 = _UV_21522b53e93649b5be78f38166b46236_Out_0[3];
            float Constant_b3d9f499f73b42a4821b3f8e8fabc49a = 3.141593;
            float _Multiply_fcb388f6849b48b693a2ee5c0636b345_Out_2;
            Unity_Multiply_float(_Split_74383f7cbc184c61a2eb3da422a74853_G_2, Constant_b3d9f499f73b42a4821b3f8e8fabc49a, _Multiply_fcb388f6849b48b693a2ee5c0636b345_Out_2);
            float _Sine_ba554da07510433fb8b46ed599a63177_Out_1;
            Unity_Sine_float(_Multiply_fcb388f6849b48b693a2ee5c0636b345_Out_2, _Sine_ba554da07510433fb8b46ed599a63177_Out_1);
            float _Property_564bbea26f904b838d3db2ba1b076037_Out_0 = Vector1_b2ae68e338784337b99bdc9665cfbec0;
            float _Power_a3b670665cb9401e8f51374989c62879_Out_2;
            Unity_Power_float(_Sine_ba554da07510433fb8b46ed599a63177_Out_1, _Property_564bbea26f904b838d3db2ba1b076037_Out_0, _Power_a3b670665cb9401e8f51374989c62879_Out_2);
            float2 _Property_dfb15abecb5445a59d04f3bc714ff354_Out_0 = Vector2_4b58e89ca621426cae62b9f03e81b483;
            float2 _Property_0cfef5df612646c5b0b6ef42cbc9a9da_Out_0 = Vector2_48da6910d2a14d629822e55c17af442c;
            float2 _Multiply_5c883ebf008b40b683ea26c22ca2381b_Out_2;
            Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_0cfef5df612646c5b0b6ef42cbc9a9da_Out_0, _Multiply_5c883ebf008b40b683ea26c22ca2381b_Out_2);
            float2 _TilingAndOffset_2251a520ed3f43ba82638aebd24e3c8f_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_dfb15abecb5445a59d04f3bc714ff354_Out_0, _Multiply_5c883ebf008b40b683ea26c22ca2381b_Out_2, _TilingAndOffset_2251a520ed3f43ba82638aebd24e3c8f_Out_3);
            float _Voronoi_558164e164a646b7bbf807dd58a44916_Out_3;
            float _Voronoi_558164e164a646b7bbf807dd58a44916_Cells_4;
            Unity_Voronoi_float(_TilingAndOffset_2251a520ed3f43ba82638aebd24e3c8f_Out_3, 2, 4.2, _Voronoi_558164e164a646b7bbf807dd58a44916_Out_3, _Voronoi_558164e164a646b7bbf807dd58a44916_Cells_4);
            float _Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2;
            Unity_Multiply_float(_Power_a3b670665cb9401e8f51374989c62879_Out_2, _Voronoi_558164e164a646b7bbf807dd58a44916_Out_3, _Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2);
            float4 _Property_1213d4f42bad49ffbe933b85e8877389_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_9b86f8e1b98947f692ebd161fb82f952) : Color_9b86f8e1b98947f692ebd161fb82f952;
            float4 _Multiply_34a3c2bf4cc841b0a60478bc733cbbfd_Out_2;
            Unity_Multiply_float((_Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2.xxxx), _Property_1213d4f42bad49ffbe933b85e8877389_Out_0, _Multiply_34a3c2bf4cc841b0a60478bc733cbbfd_Out_2);
            surface.BaseColor = (_Multiply_34a3c2bf4cc841b0a60478bc733cbbfd_Out_2.xyz);
            surface.Alpha = _Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Sprite Unlit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            // Render State
            Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITEFORWARD
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 texCoord0;
            float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
            float4 uv0;
            float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
            float3 ObjectSpaceNormal;
            float3 ObjectSpaceTangent;
            float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
            float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            float4 interp1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            output.interp1.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            output.color = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float2 Vector2_48da6910d2a14d629822e55c17af442c;
        float4 Color_9b86f8e1b98947f692ebd161fb82f952;
        float Vector1_b2ae68e338784337b99bdc9665cfbec0;
        float2 Vector2_4b58e89ca621426cae62b9f03e81b483;
        CBUFFER_END

        // Object and Global properties

            // Graph Functions
            
        void Unity_Multiply_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }

        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }

        void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }


        inline float2 Unity_Voronoi_RandomVector_float (float2 UV, float offset)
        {
            float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
            UV = frac(sin(mul(UV, m)));
            return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
        }

        void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
        {
            float2 g = floor(UV * CellDensity);
            float2 f = frac(UV * CellDensity);
            float t = 8.0;
            float3 res = float3(8.0, 0.0, 0.0);

            for(int y=-1; y<=1; y++)
            {
                for(int x=-1; x<=1; x++)
                {
                    float2 lattice = float2(x,y);
                    float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                    float d = distance(lattice + offset, f);

                    if(d < res.x)
                    {
                        res = float3(d, offset.x, offset.y);
                        Out = res.x;
                        Cells = res.y;
                    }
                }
            }
        }

        void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_21522b53e93649b5be78f38166b46236_Out_0 = IN.uv0;
            float _Split_74383f7cbc184c61a2eb3da422a74853_R_1 = _UV_21522b53e93649b5be78f38166b46236_Out_0[0];
            float _Split_74383f7cbc184c61a2eb3da422a74853_G_2 = _UV_21522b53e93649b5be78f38166b46236_Out_0[1];
            float _Split_74383f7cbc184c61a2eb3da422a74853_B_3 = _UV_21522b53e93649b5be78f38166b46236_Out_0[2];
            float _Split_74383f7cbc184c61a2eb3da422a74853_A_4 = _UV_21522b53e93649b5be78f38166b46236_Out_0[3];
            float Constant_b3d9f499f73b42a4821b3f8e8fabc49a = 3.141593;
            float _Multiply_fcb388f6849b48b693a2ee5c0636b345_Out_2;
            Unity_Multiply_float(_Split_74383f7cbc184c61a2eb3da422a74853_G_2, Constant_b3d9f499f73b42a4821b3f8e8fabc49a, _Multiply_fcb388f6849b48b693a2ee5c0636b345_Out_2);
            float _Sine_ba554da07510433fb8b46ed599a63177_Out_1;
            Unity_Sine_float(_Multiply_fcb388f6849b48b693a2ee5c0636b345_Out_2, _Sine_ba554da07510433fb8b46ed599a63177_Out_1);
            float _Property_564bbea26f904b838d3db2ba1b076037_Out_0 = Vector1_b2ae68e338784337b99bdc9665cfbec0;
            float _Power_a3b670665cb9401e8f51374989c62879_Out_2;
            Unity_Power_float(_Sine_ba554da07510433fb8b46ed599a63177_Out_1, _Property_564bbea26f904b838d3db2ba1b076037_Out_0, _Power_a3b670665cb9401e8f51374989c62879_Out_2);
            float2 _Property_dfb15abecb5445a59d04f3bc714ff354_Out_0 = Vector2_4b58e89ca621426cae62b9f03e81b483;
            float2 _Property_0cfef5df612646c5b0b6ef42cbc9a9da_Out_0 = Vector2_48da6910d2a14d629822e55c17af442c;
            float2 _Multiply_5c883ebf008b40b683ea26c22ca2381b_Out_2;
            Unity_Multiply_float((IN.TimeParameters.x.xx), _Property_0cfef5df612646c5b0b6ef42cbc9a9da_Out_0, _Multiply_5c883ebf008b40b683ea26c22ca2381b_Out_2);
            float2 _TilingAndOffset_2251a520ed3f43ba82638aebd24e3c8f_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_dfb15abecb5445a59d04f3bc714ff354_Out_0, _Multiply_5c883ebf008b40b683ea26c22ca2381b_Out_2, _TilingAndOffset_2251a520ed3f43ba82638aebd24e3c8f_Out_3);
            float _Voronoi_558164e164a646b7bbf807dd58a44916_Out_3;
            float _Voronoi_558164e164a646b7bbf807dd58a44916_Cells_4;
            Unity_Voronoi_float(_TilingAndOffset_2251a520ed3f43ba82638aebd24e3c8f_Out_3, 2, 4.2, _Voronoi_558164e164a646b7bbf807dd58a44916_Out_3, _Voronoi_558164e164a646b7bbf807dd58a44916_Cells_4);
            float _Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2;
            Unity_Multiply_float(_Power_a3b670665cb9401e8f51374989c62879_Out_2, _Voronoi_558164e164a646b7bbf807dd58a44916_Out_3, _Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2);
            float4 _Property_1213d4f42bad49ffbe933b85e8877389_Out_0 = IsGammaSpace() ? LinearToSRGB(Color_9b86f8e1b98947f692ebd161fb82f952) : Color_9b86f8e1b98947f692ebd161fb82f952;
            float4 _Multiply_34a3c2bf4cc841b0a60478bc733cbbfd_Out_2;
            Unity_Multiply_float((_Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2.xxxx), _Property_1213d4f42bad49ffbe933b85e8877389_Out_0, _Multiply_34a3c2bf4cc841b0a60478bc733cbbfd_Out_2);
            surface.BaseColor = (_Multiply_34a3c2bf4cc841b0a60478bc733cbbfd_Out_2.xyz);
            surface.Alpha = _Multiply_f3584d68b6264b8ab54a37498d5007e6_Out_2;
            return surface;
        }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal =           input.normalOS;
            output.ObjectSpaceTangent =          input.tangentOS.xyz;
            output.ObjectSpacePosition =         input.positionOS;

            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





            output.uv0 =                         input.texCoord0;
            output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

            return output;
        }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}