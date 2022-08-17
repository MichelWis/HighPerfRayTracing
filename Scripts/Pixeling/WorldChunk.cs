using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class WorldChunk
{
    MeshFilter meshFilter;
    GameObject chunkObject;

    TreeNode chunkTreeRootNode;
    ComputeBuffer buff;

    public void Generate(GameObject chunkObj,Vector3Int chunkOffset)
    {/*
        chunkObject = chunkObj;
        
        meshFilter = chunkObj.GetComponent<MeshFilter>();

        Material material = chunkObj.GetComponent<MeshRenderer>().material;

        chunkTreeRootNode = new TreeNode(PixelingConstants.WorldChunkTreeLevel);

        chunkTreeRootNode.ConstructNodes();

        chunkTreeRootNode.Simplify();

        List<int>[] buffer = new List<int>();
        chunkTreeRootNode.Create1DRepresentation(buffer);
        buffer[0] = TreeNode.PackVoxelData(4, 0, 4095);
        buffer[0] = int.MaxValue;

        buff = new ComputeBuffer(buffer.Count, 4, ComputeBufferType.Structured, ComputeBufferMode.Immutable);

        material.SetBuffer("_VPD", buff);
        buff.SetData(buffer.ToArray());

        chunkObj.GetComponent<MeshRenderer>().material = material;

        /*
        //chunkTreeRootNode.Draw();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        stopWatch.Stop();
        UnityEngine.Debug.Log($"time elapsed for building and redering this chunk: {stopWatch.ElapsedMilliseconds}ms.");
        Mesh mesh = new Mesh();

        mesh.vertices = chunkTreeRootNode.vertices.ToArray();
        mesh.triangles = chunkTreeRootNode.triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        meshFilter.mesh = mesh;*/
    }


}
