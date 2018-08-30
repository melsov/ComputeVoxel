

//bkup

      [maxvertexcount(12)]
      void geom( point inputGS p[1], inout TriangleStream<input> triStream )
      {
             
        float4 pos = p[0].pos * float4( _Size, _Size, _Size, 1 );

        float4 shift;
        float4 voxelPosition = pos + _chunkPosition;

        float halfS = _Size * 0.5 * _mipStretch;  // x, y, z is the center of the voxel, paint sides offset by half of Size


        input pIn1, pIn2, pIn3, pIn4;

        pIn1.uv = float2( 0.0f, 0.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;
        pIn2.uv = float2( 0.0f, 1.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;
        pIn3.uv = float2( 1.0f, 0.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;
        pIn4.uv = float2( 1.0f, 1.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;

        //
        // NeighborBits12
        //
        uint neiBits12 = asuint(p[0]._color.w);
        //
        // global light calcluation
        //
        fixed3 voxLessThanCam = step( voxelPosition.xyz, _cameraPosition.xyz);
        fixed3 corner = voxLessThanCam - (1 - voxLessThanCam);
        fixed3 xNorm = fixed3(corner.x, 0, 0);
        fixed3 yNorm = fixed3(0, corner.y, 0);
        fixed3 zNorm = fixed3(0, 0, corner.z);

        // float3 fakeLightGlobalPos = float3(1000, 1000, 300);
        fixed3 lightDir = normalize(voxelPosition.xyz - _globalLight); // fakeLightGlobalPos);

        fixed3 xyzLight = fixed3(dot(lightDir, xNorm), dot(lightDir, yNorm), dot(lightDir, zNorm)) * -1;
        xyzLight = (1 - _minAmbianLight) * (xyzLight / 2 + .5) + _minAmbianLight;


        shift = (_cameraPosition.x < voxelPosition.x)?float4( 1, 1, 1, 1 ):float4( -1, 1, -1, 1 );

        pIn1._color = pIn2._color = pIn3._color = pIn4._color = p[0]._color * xyzLight.x; //1

        pIn1.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, -halfS, halfS, 0 )  ));
        triStream.Append( pIn1 );

        pIn2.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, halfS, halfS, 0 ) ));
        triStream.Append( pIn2 );

        pIn3.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, -halfS, -halfS, 0 )  ));
        triStream.Append( pIn3 );

        pIn4.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, halfS, -halfS, 0 )  ));
        triStream.Append( pIn4 );

        triStream.RestartStrip();


        // shadows
        pIn1._color = pIn2._color = pIn3._color = pIn4._color = p[0]._color * xyzLight.y; // .7;

        shift = (_cameraPosition.y < voxelPosition.y)?float4( 1, 1, 1, 1 ):float4( 1, -1, -1, 1 );

        pIn1.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, -halfS, halfS, 0 ) ));
        triStream.Append( pIn1 );

        pIn2.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, -halfS, -halfS, 0 ) ));
        triStream.Append( pIn2 );

        pIn3.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( halfS, -halfS, halfS, 0 )  ));
        triStream.Append( pIn3 );

        pIn4.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( halfS, -halfS, -halfS, 0 )  ));
        triStream.Append( pIn4 );

        triStream.RestartStrip();


        //side shadows
        pIn1._color = pIn2._color = pIn3._color = pIn4._color = p[0]._color * xyzLight.z; // .9;

        shift = (_cameraPosition.z < voxelPosition.z)?float4( 1, 1, 1, 1 ):float4( -1, 1, -1, 1 );

        pIn1.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, -halfS, -halfS, 0 )  ));
        triStream.Append( pIn1 );

        pIn2.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( -halfS, halfS, -halfS, 0 )  ));
        triStream.Append( pIn2 );

        pIn3.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( halfS, -halfS, -halfS, 0 )  ));
        triStream.Append( pIn3 );

        pIn4.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + shift*float4( halfS, halfS, -halfS, 0 )  ));
        triStream.Append( pIn4 );

        triStream.RestartStrip();
      }