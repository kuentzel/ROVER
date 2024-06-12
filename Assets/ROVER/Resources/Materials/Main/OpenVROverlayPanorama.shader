Shader "VR/OpenVROverlayPanorama"
{
    Properties
    {
        //_EyeLeft ("Texture", 2D) = "white" {}
        //_EyeRight ("Texture", 2D) = "white" {}
      }
    SubShader
    {
        Tags {"RenderType"="Transparent"}//{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        //ZWrite Off
        //Blend SrcAlpha OneMinusSrcAlpha
        //Cull front 
        LOD 100

        Pass
        {
            CGPROGRAM
             // use "vert" function as the vertex shader
            #pragma vertex vert
            // use "frag" function as the pixel (fragment) shader
            #pragma fragment frag

            
            // vertex shader inputs
            struct appdata
            {
                uint vid : SV_VertexID; // vertex position
                float2 uv : TEXCOORD0; // texture coordinate
            };

            // vertex shader outputs ("vertex to fragment")
            struct PerVertex
            {
                float2 uv : TEXCOORD0; // texture coordinate
                float4 Position : SV_POSITION; // clip space position
            };



            // vertex shader
            PerVertex vert (uint vid : SV_VertexID)
            {
                PerVertex o;
                // Takes NDC (0,0 center, y down, x right, z forward) space and converts into UV space
                o.uv = float2((vid << 1) & 2, vid & 2);
                o.Position = float4(o.uv * 2.0f + -1.0f, 0.0f, 1.0f);
                return o;
            }


            // textures we will sample
            sampler2D _EyeLeft;
            sampler2D _EyeRight;

            float4x4 _LookRotation;
            float _HalfFOVInRadians;

            static const float PI = 3.141592;
            static const float HALF_PI = 0.5 * PI;
            static const float QUARTER_PI = 0.25 * PI;

            // pixel shader; returns low precision ("fixed4" type)
            // color ("SV_Target" semantic)
            float4 frag (PerVertex p) : SV_Target
            {
                float4 outColor;
                // convert UV (0,0 upper right) (1, 1 lower left) to XY (0,0 lower left) (1, 1 upper right)
	            float2 xy = float2(p.uv.x, 1 - p.uv.y);

	            // convert to -1, -1 lower left, 1, 1 upper right
	            xy = float2(2.0 * xy - 1.0);

	            // (-pi, -pi_half) lower left, (pi, pi_half) upper right
	            xy *= float2(PI, HALF_PI);

	            // Convert from one equirect to 2 stacked equirects (left on top of right eye)
	            // scaling by two ([-.5, .5 pi] to [-pi. pi]) 
	            xy.y *= 2;

	            // then subtracting half pi  for top, adding pi back for bot
	            bool isTop = xy.y >= 0;
	            if (isTop) {
		        //[-PI_HALF, PI_HALF] 
		        xy.y -= HALF_PI;
	            } else {
		        // [-PI_HALF, PI_HALF]
		        xy.y += HALF_PI;
	            }

	            // current coordinate space is left eye on top, right eye on bottom
	            // both eyes go from -pi -> pi left to right and -pi/2 -> pi/2 bottom to top

	            // Get scalar for modifying projection from cubemap (90 fov) to eye target fov
	            float fovScalar = tan(_HalfFOVInRadians) / tan(QUARTER_PI);

	            // create vector looking out at equirect CubeMap
	            float3 cubeMapLookupDirection = float3(sin(xy.x), 1.0, cos(xy.x)) * float3(cos(xy.y), sin(xy.y), cos(xy.y));

	            // rotate look direction by inverse of horizontal stageSpace look vector.
	            // this is a trick to prevent a full cube map render of the scene, the only valid
	            // equirectangular projections will be near whatever is treated as forward and backward traditionally
	            cubeMapLookupDirection = mul(_LookRotation,float4(cubeMapLookupDirection, 0)).xyz;

	            // project the vector onto the 2d texture
	            // this will be wrong everywhere that is not near the rotated forward plane of the cubeMap
	            // U = ((X/|Z|) + 1) / 2
	            // V = ((Y/|Z|) + 1) / 2
	            // always project the +Z axis of a cube map
	            // X/|Z|, -Y/|Z| places uv coords in -1, 1. + 1 / 2 shifts to 0 -> 1
	            // fovScalar scales U/V from 90 degrees into eye fov that was rendered with.
	            float projectLookOntoUAxis = ((cubeMapLookupDirection.x / abs(cubeMapLookupDirection.z) / fovScalar) + 1) / 2;
	            float projectLookOntoVAxis = 1 - (((cubeMapLookupDirection.y / abs(cubeMapLookupDirection.z) / fovScalar) + 1) / 2);

	            float2 eyeUV = float2(projectLookOntoUAxis, projectLookOntoVAxis);

	            // copy color from the right eye texture
	            if (isTop) {
		            outColor = tex2D(_EyeLeft, eyeUV);
	            } else {
		            outColor = tex2D(_EyeRight, eyeUV);
                }

                return outColor;
            }
            ENDCG
        }
    }
}
