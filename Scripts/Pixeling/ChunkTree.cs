using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class TreeNode
{
    public enum NodeType
    {
        INTERNAL = 0, LEAF, NULL, ROOT
    }

    public uint vMat;

    public NodeType Type;
    public TreeNode[] Children;
    public TreeNode Parent;
    public int Size;
    public int Lod;
    public Vector3Int Origin;
    public Vector3Int SamplingOffset;

    public byte Solidity = 0b0000000; // none are solid, is dynamically changed by children

    [HideInInspector]
    public List<int> triangles;
    [HideInInspector]
    public List<Vector3> vertices;

    public TreeNode(TreeNode parent, int idx, List<Vector3> verts, List<int> tris, Vector3Int samplingOffset)
    {
        SamplingOffset = samplingOffset;

        triangles = tris;
        vertices = verts;

        Parent = parent;
        Type = NodeType.NULL;
        Children = new TreeNode[8];
        Size = parent.Size / 2;
        Lod = parent.Lod + 1;
        Origin = parent.Origin + Size * PixelingConstants.childOffsets[idx];
    }

    public TreeNode(int level, Vector3Int samplingOffset)
    {
        SamplingOffset = samplingOffset;
        triangles = new List<int>();
        vertices = new List<Vector3>();

        Parent = null;
        Origin = new Vector3Int(0,0,0);
        Size = (int)Mathf.Pow(2, level - 1);
        Lod = 0;
        Children = new TreeNode[8];
        Type = NodeType.INTERNAL;
    }

    /// <summary>
    /// builds and automatically simplifies the tree such that it removes all children if they are the same material.
    /// </summary>
    public void ConstructNodes()
    {
        Type = NodeType.INTERNAL;
        if (Size == 1)
        {
            if (ConstructLeafNode())
            {
                Solidity = 255;
            }
            Children = null;
            return;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                Children[i] = new TreeNode(this, i, vertices, triangles, SamplingOffset);
                Children[i].ConstructNodes();

                Solidity |= Children[i].Solidity == 255 ? (byte)(1 << i) : (byte)0;
            }

            vMat = Children[0].vMat;
            List<uint> mats = new List<uint>();
            for (int i = 0; i < 8; i++)
            {
                mats.Add(Children[i].vMat);
            }
            for (int i = 0; i < 8; i++)
            {
                if (Children[i].vMat != Children[0].vMat || Children[i].Type == NodeType.INTERNAL)
                {
                    vMat = mats.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();

                    // THIS NEEDS TO BE REMOVED AND THE RESULTING CRASH NEEDS TO BE FIXED:
                    if(vMat == 0)
                    {
                        vMat = PixelingConstants.Materials[3];
                    }
                    return;
                }
            }

            // children dont add any additional information.
            Children = null;
            Type = NodeType.LEAF;

        }
    }

    /// <summary>
    /// returns whether or not the voxel is solid
    /// </summary>
    /// <returns></returns>
    bool ConstructLeafNode()
    {
        Type = NodeType.LEAF;
        // determine if there is an intersection 
        
        //if(Origin.X * Origin.Y * Origin.Z < 300)
        // 0.4..0.6
        if (World.NoiseGen.Density3DPerlinBase(Origin + SamplingOffset) > 0.51f)
        {
            Vector3 coord = Origin + SamplingOffset;
            coord = coord *2f;
            int choice = 0.25f > World.NoiseGen.Density3DPerlinBase(coord.x, coord.y, coord.z) ? 0: 0.5f > World.NoiseGen.Density3DPerlinBase(coord.x, coord.y, coord.z) ? 1 : 0.75f > World.NoiseGen.Density3DPerlinBase(coord.x, coord.y, coord.z) ? 2 : 3;
            vMat = PixelingConstants.Materials[choice];
            return true;
        }

        vMat = PixelingConstants.MATERIAL_CODE_EMPTY;
        return false;
    }


    #region RayMarchingData


    #region morton
    private int toMorton()
    {
        int res = 0;

        res |= (Origin.x & 0b1) << 0; // 01
        res |= (Origin.x & 0b10) << 2; // 01 000
        res |= (Origin.x & 0b100) << 4; // 01 000 000
        res |= (Origin.x & 0b1000) << 6; // 01 000 000 000

        res |= (Origin.y & 0b1) << 1; // 010
        res |= (Origin.y & 0b10) << 3; // 010 000
        res |= (Origin.y & 0b100) << 5; // 010 000 000
        res |= (Origin.y & 0b1000) << 7; // 010 000 000 000

        res |= (Origin.z & 0b1) << 2; // 100 
        res |= (Origin.z & 0b10) << 4; // 100 000
        res |= (Origin.z & 0b100) << 6; // 100 000 000
        res |= (Origin.z & 0b1000) << 8; // 100 000 000

        return res;
    }
    // this morton code only supports up to 16 values per coordinate (4 bits)
    private ushort toMorton(Vector3Int Origin)
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

        return (ushort)res;
    }

    Vector3Int fromMorton(uint morton)
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

        return new Vector3Int((int)x, (int)y, (int)z);
    }
    #endregion morton

    private uint EncodePos(Vector3Int pos)
    {
        return (uint)((pos.x << 10) | (pos.y << 5) | pos.z);
    }

    /// <summary>
    /// encodes the data into the format: [childStartIndex;X;Y;Z;isLastChild (default:0, can be set manually)], (depending on the input format:)[R;G;B]
    /// </summary>
    /// <param name="childStartIdx"></param>
    /// <param name="posEncoded"></param>
    /// <param name="isLastChild"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    private uint[] PackVoxelData(ushort childStartIdx, uint posEncoded, uint color, int isLastChild= 0)
    {
        uint[] res = new uint[2]
        {
            0, color
        };

        res[0] = (uint)isLastChild & 1;
        res[0] |= (uint)(posEncoded << 1);
        res[0] |= (uint)(childStartIdx << 16);

        return res;
    }

    public static uint[] DecodeVoxelData(uint vD)
    {
        uint childStart = vD >> 16;

        uint pX = (vD >> 11) & 31;
        uint pY = (vD >> 6) & 31;
        uint pZ = (vD >> 1) & 31;
        uint isLastChild = (vD) & 1;

        return new uint[] { childStart, pX, pY, pZ, isLastChild };
    }

    public void Create1DRepresentation(List<List<uint>> buffer)
    {
        // set islastchild flag

        if (Children != null)
        {
            buffer[0].AddRange(PackVoxelData(9, EncodePos(Origin), vMat));
            buffer[0][0] = buffer[0][0] | 1;
            for (int i = 0; i < 8; i++)
            {
                if (Children[i] != null)
                {
                    if (Children[i].vMat != PixelingConstants.MATERIAL_CODE_EMPTY)
                    {
                        Children[i].create1DRepresentation(buffer);
                    }
                }
            }
            if (buffer[Lod + 1].Count > 0) // if we did in fact add children:
            {
                buffer[Lod + 1][buffer[Lod + 1].Count - 2] = buffer[Lod + 1][buffer[Lod + 1].Count - 2] | 1;
            }
        }
    }

    private void create1DRepresentation(List<List<uint>> buffer)
    {
        if (Children == null) // add node if node is leaf
        {
            buffer[Lod].AddRange(PackVoxelData(0, EncodePos(Origin), vMat));
            return;
        }

        for(int i = 0; i < 8; i++)
        {
            if(Children[i] != null)
            {
                if (Children[i].vMat != PixelingConstants.MATERIAL_CODE_EMPTY)
                {
                    Children[i].create1DRepresentation(buffer);
                }
            }
        }
        if (buffer[Lod+1].Count > 0)
        {
            buffer[Lod + 1][buffer[Lod + 1].Count - 2] = buffer[Lod + 1][buffer[Lod + 1].Count - 2] | 1;
            buffer[Lod].AddRange(PackVoxelData(9, EncodePos(Origin), vMat));
        }
    }

    #endregion RayMarchingData



    #region Meshing

    public void Draw()
    {

        if (Children != null)
        {
            for (int i = 0; i < 8; i++)
            {
                Children[i].Draw();
            }
        }
        else
        {
            if (vMat != 0)
            {
                AddToMesh();
            }
        }
    }

    private void AddToMesh()
    {
        for(int i = 0; i < 6; i++)
        {
            var pos = Origin + Size * PixelingConstants.faceNeighborOffsets[i];
            TreeNode neighbor = FindNodeInTree(pos);

            if (neighbor != null)
            {
                while(neighbor.Parent != null && neighbor.Size < Size)
                {
                    if (neighbor.Origin == pos)
                    {
                        neighbor = neighbor.Parent;
                    }
                    else break;
                }
                // draw
                if (neighbor.Solidity != 255)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        triangles.Add(vertices.Count);
                        int vertexIdx = PixelingConstants.triangleLookup[i, j];
                        var vertexPos = PixelingConstants.childOffsets[vertexIdx] * Size + Origin;
                        vertices.Add(new Vector3Int(vertexPos.x, vertexPos.y, vertexPos.z));
                    }
                }
                // else dont.
            }

        }
    }

    #endregion Meshing
    /// <summary>
    /// finds the smallest node possible at the specified integer position. 
    /// if point lies at border of nodes, returns the node that contains this Vector3Int as its minimizing position 
    /// (if at corner, returns the one that has vector3Int as origin)
    /// </summary>
    /// <param name="pos">The position you are looking for.</param>
    /// <returns></returns>
    public TreeNode FindNodeInTree(Vector3Int pos)
    {
        TreeNode res = this;
        while (!res.IsInNode(pos))
        {
            if (res.Parent != null)
                res = res.Parent;
            else break;
        }
        if (res != null)
        {
           
            while (true)
            {
                if (res.Children == null)
                    return res;
                bool found = false;
                for (int i = 0; i < 8; i++)
                {
                    if (res.Children[i].IsInNode(pos))
                    {
                        res = res.Children[i];
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return res;
            }
        }

        return res;
    }

    /// <summary>
    /// checks whether pos is located inside this node.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool IsInNode(Vector3Int pos)
    {
        if (pos.x >= Origin.x && pos.x < Origin.x + Size)
        {
            if (pos.y >= Origin.y && pos.y < Origin.y + Size)
            {
                if (pos.z >= Origin.z && pos.z < Origin.z + Size)
                {
                    return true;
                }
            }
        }
        return false;
    }
}