// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/DefaultPixeling"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeShit("Time Shit", Range(0,1)) = 0.5
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers d3d11_9x

            #include "UnityCG.cginc"
            #define MAX_STEPS 100
            #define VIEW_DIST 256
            Buffer<uint> _VPD;
            Buffer<int> _ChunkIndices;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                float3 rayOrg : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.rayOrg = _WorldSpaceCameraPos;
                o.hitPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
             

            int toMorton(int3 Origin)
            {
                int res = 0;

                res |= (Origin.x & 1) << 0; // 01
                res |= (Origin.x & 2) << 2; // 01 000
                res |= (Origin.x & 4) << 4; // 01 000 000
                res |= (Origin.x & 8) << 6; // 01 000 000 000

                res |= (Origin.y & 1) << 1; // 010
                res |= (Origin.y & 2) << 3; // 010 000
                res |= (Origin.y & 4) << 5; // 010 000 000
                res |= (Origin.y & 8) << 7; // 010 000 000 000

                res |= (Origin.z & 1) << 2; // 100 
                res |= (Origin.z & 2) << 4; // 100 000
                res |= (Origin.z & 4) << 6; // 100 000 000
                res |= (Origin.z & 8) << 8; // 100 000 000

                return res;
            }

            uint3 fromMorton(uint morton)
            {
                uint x = 0, y = 0, z = 0;

                x += ((morton >> 0) & 1);
                x += ((morton >> 3) & 1) << 1;
                x += ((morton >> 6) & 1) << 2;
                x += ((morton >> 9) & 1) << 3;
                x += ((morton >> 12) & 1) << 4;

                y += (morton >> 1) & 1;
                y += ((morton >> 4) & 1) << 1;
                y += ((morton >> 7) & 1) << 2;
                y += ((morton >> 10) & 1) << 3;
                y += ((morton >> 13) & 1) << 4;

                z += (morton >> 2) & 1;
                z += ((morton >> 5) & 1) << 1;
                z += ((morton >> 8) & 1) << 2;
                z += ((morton >> 11) & 1) << 3;
                z += ((morton >> 14) & 1) << 4;

                return uint3(x, y, z);
            }

            /*
            float intersection(float3 rayOrigin, float3 iRayDir, float3 boxMin, float3 boxMax) {
                float3 tMin = (boxMin - rayOrigin) * iRayDir;
                float3 tMax = (boxMax - rayOrigin) * iRayDir;
                float3 t1 = min(tMin, tMax);
                float3 t2 = max(tMin, tMax);
                float tNear = max(max(t1.x, t1.y), t1.z);
                float tFar = min(min(t2.x, t2.y), t2.z);
                
                if (tFar > max(tNear, 0.0)) {
                    return tNear;
                }
                else {
                    return -1;
                }
            };

            float intersection(float3 ro, float3 invRd, float3 bmin, float3 bmax) {
                float t1 = (bmin[0] - ro[0]) * invRd[0];
                float t2 = (bmax[0] - ro[0]) * invRd[0];

                float tmin = min(t1, t2);
                float tmax = max(t1, t2);

                for (int i = 1; i < 3; ++i) {
                    t1 = (bmin[i] - ro[i]) * invRd[i];
                    t2 = (bmax[i] - ro[i]) * invRd[i];

                    tmin = max(tmin, min(t1, t2));
                    tmax = min(tmax, max(t1, t2));
                }

                if (tmax > tmin) {
                    if (tmin < 0.0) {
                        return tmax;
                    }
                    return tmin;
                }
                return -1;
            }
            */
            
            float intersectionChunk(float3 ro, float3 invRd, float3 bmin, float3 bmax) {
                float tx1 = (bmin.x - ro.x) * invRd.x;
                float tx2 = (bmax.x - ro.x) * invRd.x;

                float tmin = min(tx1, tx2);
                float tmax = max(tx1, tx2);

                float ty1 = (bmin.y - ro.y) * invRd.y;
                float ty2 = (bmax.y - ro.y) * invRd.y;

                tmin = max(tmin, min(ty1, ty2));
                tmax = min(tmax, max(ty1, ty2));

                float tz1 = (bmin.z - ro.z) * invRd.z;
                float tz2 = (bmax.z - ro.z) * invRd.z;

                tmin = max(tmin, min(tz1, tz2));
                tmax = min(tmax, max(tz1, tz2));

                if (tmin < tmax) {
                    return max(tmax, tmin);
                }
                else {
                    return -1;
                }
            }

            float intersection(float3 ro, float3 invRd, float3 bmin, float3 bmax) {
                float tx1 = (bmin.x - ro.x) * invRd.x;
                float tx2 = (bmax.x - ro.x) * invRd.x;

                float tmin = min(tx1, tx2);
                float tmax = max(tx1, tx2);

                float ty1 = (bmin.y - ro.y) * invRd.y;
                float ty2 = (bmax.y - ro.y) * invRd.y;

                tmin = max(tmin, min(ty1, ty2));
                tmax = min(tmax, max(ty1, ty2));

                float tz1 = (bmin.z - ro.z) * invRd.z;
                float tz2 = (bmax.z - ro.z) * invRd.z;

                tmin = max(tmin, min(tz1, tz2));
                tmax = min(tmax, max(tz1, tz2));

                if (tmax > tmin) {

                    if (tmin <= 0.0 && tmax >= 0.0) {
                        return 0.001;
                    }

                    return tmax;
                }
                return -1;
            }

            // returns -1,-1,-1 if not hit; returns x,y,z if hit
            int4 RayTraceChunk(float3 rD, float3 invRd, float3 ro, int startIndex) {
                const float lodSize[] = {
                    16, 8, 4, 2, 1
                };
                const int lodOffsetLookup[] = {
                    0, 8, 72, 584, 4680, 37448
                };
                const float3 nodeBounds[] = {
                    float3(1,1,1) * lodSize[0],
                    float3(1,1,1) * lodSize[1],
                    float3(1,1,1) * lodSize[2],
                    float3(1,1,1) * lodSize[3],
                    float3(1,1,1) * lodSize[4],
                };



                // 3 bytes in the chunk header tell us the chunkOffsets in format: [x],[y],[z]
                int3 chunkOffset = int3(_VPD[startIndex], _VPD[startIndex + 1], _VPD[startIndex + 2]);
                ro = ro - chunkOffset;

                uint nodeStack[6] = { 0,0,0,0,0, 0 };
                uint nodeHistory[6] = { 0,0,0,0,0, 0 };
                int lod = 0;
                uint curr = 0;
                uint globalVIndex;

                const int4 notHit = int4(-1,-1,-1,-1);
                uint res = 0;
                int3 resPos = int3(-1,-1,-1);
                float bestDist = 99999;
                
                uint childStart;
                uint pX;
                uint pY;
                uint pZ;
                uint isLastChild;
                uint3 position;
                uint vData = 0;

                // do a little bit of a safeguard so we dont enter the loop if our chunk is not visible
                float dist = intersection(ro, invRd, float3(0,0,0), nodeBounds[0]);
                if (dist < 0) {
                    return notHit;
                }
                int iter = 0;
                while (true) {
                    while (true) {
                        iter++;
                        curr = nodeStack[lod];
                        globalVIndex = curr+ startIndex + 3;

                        // decodes the data from the format: [childStartIndex;X;Y;Z;isLastChild (default:0, can be set manually)], (depending on the input format:)[R;G;B]

                        vData = _VPD[globalVIndex];
                        isLastChild = (vData) & 1;

                        if (isLastChild == 0) {
                            nodeStack[lod] = curr + 2;
                        }
                        else if (nodeHistory[lod] == curr && curr != 0)
                        {
                            break;
                        }
                        nodeHistory[lod] = curr;

                        pX = (vData >> 11) & 31;
                        pY = (vData >> 6 ) & 31;
                        pZ = (vData >> 1 ) & 31;
                        
                        position = uint3(pX, pY, pZ);
                        dist = intersection(ro, invRd, position, position + nodeBounds[lod]);
                        if (dist > 0 && dist < bestDist) {
                            childStart = vData >> 16;
                            if (childStart == 0 || lod > clamp(11 - length(ro) * 0.1, 0, 5)) {          // is leaf   
                                bestDist = dist;
                                // this sets the color to the color of the best known voxel
                                res = globalVIndex + 1;
                                resPos = position;
                            }
                            else {                          // is internal
                                lod += 1;
                                nodeStack[lod] = childStart;
                                continue;
                            }
                        }


                        if (isLastChild != 0) {
                            if (curr != 0) {
                                break;
                            }
                        }
                    }

                    // if we're here, we reached the end of a bunch of children of a parent node. this COULD mean that we're at the end of the octree
                    if (lod > 1) {
                        nodeStack[lod] = 0;
                        lod--;
                    }
                    else {
                        return int4(resPos, _VPD[res]);
                    }

                }
                return notHit;
            }

            float4 getColorFromInt(int c) {
                return float4(float(c & 255) * 0.003922, float((c >> 8) & 255) * 0.003922, float((c >> 16) & 255) * 0.003922, 1);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 ro = i.rayOrg;
                const int4 notHit = int4(-1,-1,-1, -1);
                float3 rd = i.hitPos - i.rayOrg;
                //float3 sgn = sign(rd);

                float3 invRd = normalize(1.0 / rd);

                float bestDist = 99999;
                float4 res = float4(0,0,0,1);
                
                uint totalIters = 0;
                int curr = 0;
                int numChunks = 0;
                while(_ChunkIndices[curr] != 0) {
                    int3 pos = int3(_ChunkIndices[curr+1], _ChunkIndices[curr+2], _ChunkIndices[curr+3]);
                    // distance to the chunk
                    float dist = intersectionChunk(ro, invRd, pos + float3(0,0,0), pos + float3(32,32,32));
                    // is the chunk being hit? and is the hit distance smaller than the best dist?
                    if (dist > 0 && dist < bestDist) {
                        int4 tmp = RayTraceChunk(rd, invRd, ro, _ChunkIndices[curr - 4]);

                        if(!any(tmp == notHit)){
                            res = getColorFromInt(tmp.a);
                            bestDist = dist;
                        }
                    }
                    curr += 4;
                } 

                return res;
                
            }
            ENDHLSL
        }
    }
}
