using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PixelingConstants
{
    public const int WorldChunkTreeLevel = 5;

    public const int WorldSize = 9;
    public static int ChunkSize = 16;

    public static uint MATERIAL_CODE_EMPTY = 0x000000;

    public static readonly uint[] Materials = new uint[4]
    {
        0xFF0000,
        0x00FF00,
        0x0000FF,
        0x00FFFF,
    };

    public static readonly Vector3Int[] childOffsets = new Vector3Int[8]
    {
        new Vector3Int(0,0,0), // 0
        new Vector3Int(0,0,1),// 1
        new Vector3Int(0,1,0),// 2
        new Vector3Int(0,1,1),// 3

        new Vector3Int(1,0,0),// 4
        new Vector3Int(1,0,1),// 5
        new Vector3Int(1,1,0),// 6
        new Vector3Int(1,1,1),// 7
    };

    public static readonly Vector3Int[] faceNeighborOffsets = new Vector3Int[6]
    {
        new Vector3Int(0,-1,0),
        new Vector3Int(0,0,-1),
        new Vector3Int(0,1,0),

        new Vector3Int(0,0,1),
        new Vector3Int(-1,0,0),
        new Vector3Int(1,0,0),

    };

    public static readonly int[,] triangleLookup = new int[6, 6]
    {
        { 4, 1, 0, 5, 1, 4 },
        { 0, 2, 4, 2, 6, 4 }, //
        { 2, 3, 6, 3, 7, 6 }, //

        { 1, 5, 3, 5, 7, 3 },
        { 0, 1, 2, 1, 3, 2 },
        { 4, 6, 5, 6, 7, 5 }, //
    };

}
