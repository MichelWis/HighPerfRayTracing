using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    private TreeNode rootNode;
    public readonly Vector3Int Offset;
    // this needs to be set by the world to be true as soon as a change in the underlying data is performed, so we can safely leave the chunk as is when the data hasnt changed but a draw request has been issued.
    private bool hasChanged { get { return false; } }

    private List<List<uint>> nodeBuffer= new List<List<uint>>()
        {
            new List<uint>(),
            new List<uint>(),
            new List<uint>(),
            new List<uint>(),
            new List<uint>(),
        };

    public Chunk(Vector3Int offset)
    {
        Offset = offset;
        rootNode = new TreeNode(PixelingConstants.WorldChunkTreeLevel, offset);
        rootNode.ConstructNodes();
    }

    public Mesh CreateMesh()
    {
        if(hasChanged)
            rootNode.ConstructNodes();
        
        Mesh mesh = new Mesh();
        rootNode.Draw();

        mesh.vertices = rootNode.vertices.ToArray();
        mesh.triangles = rootNode.triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }

    public uint[] CreateOctreeRepresentation()
    {
        // only rebuild chunk if its data has changed
        if (hasChanged)
            rootNode.ConstructNodes();

        // clear all the nodes of the previous draw request
        for (int i = 0; i < PixelingConstants.WorldChunkTreeLevel; i++)
            nodeBuffer[i].Clear();
        rootNode.Create1DRepresentation(nodeBuffer);

        var chunkData = PrepareChunkDataForUpload();

        // add the position information to the chunk:
        chunkData.InsertRange(0, new uint[] { (uint)Offset.x, (uint)Offset.y, (uint)Offset.z });

        return chunkData.ToArray();
    }

    private List<uint> PrepareChunkDataForUpload()
    {
        List<uint> voxBuffer = new List<uint>();

        int overallIndex = 0;
        int currNodeTC = 0;

        for (int i = 0; i < 4; i++) // LOD
        {
            overallIndex += nodeBuffer[i].Count;
            for (int j = 0; j < nodeBuffer[i].Count - 1; j += 2) // Node
            {
                if (0!=(nodeBuffer[i][j] >> 16))    // this is an internal node.
                {
                    nodeBuffer[i][j] = (nodeBuffer[i][j] & 0xFFFF); // reset childStart bits
                    // just add the node our pointer is pointing to
                    // that way, the first nodes of the first node of the firs... are correctly being set to <overallIndex>
                    nodeBuffer[i][j] |= (uint)(overallIndex + currNodeTC) << 16;
                    //nodeBuffer[i][j + 1] = (uint)(overallIndex + currNodeTC);

                    // loop from the last one node we connected until we find the next:
                    // to find the next child with the flag bit set to 1
                    for (int k = currNodeTC; k < nodeBuffer[i + 1].Count; k += 2)
                    {
                        var data1 = TreeNode.DecodeVoxelData(nodeBuffer[i + 1][k]);
                        if (data1[4] != 0) // if this is the last in a group, increment the "to connect"-index to the index of the next node
                        {
                            currNodeTC = k + 2; // start with the next node in the next search.
                            break; // only take the "first last" node, skip the others
                        }
                    }
                }

                voxBuffer.Add(nodeBuffer[i][j]);
                voxBuffer.Add(nodeBuffer[i][j + 1]);
            }

            currNodeTC = 0;
        }

        voxBuffer.AddRange(nodeBuffer[nodeBuffer.Count - 1]);

        return voxBuffer;
    }
}
