using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class World : MonoBehaviour
{
    public static Noise NoiseGen = new Noise(0.1f, 1);

    public TextMeshProUGUI debugMenu;
    public GameObject chunkObj;
    public GameObject chunkMeshObject;

    ComputeBuffer buff;
    ComputeBuffer indexBuff;

    public Dictionary<Vector3Int, Chunk> Chunks = new Dictionary<Vector3Int, Chunk>();

    // Start is called before the first frame update
    void Start()
    {
        Material material = chunkObj.GetComponent<MeshRenderer>().material;

        Chunk c;

        chunkObj.GetComponent<MeshRenderer>().material = material;

        for (int x = 0; x < PixelingConstants.WorldSize; x++)
        {
            for (int y = 0; y < PixelingConstants.WorldSize; y++)
            {
                for (int z = 0; z < PixelingConstants.WorldSize; z++)
                {
                    c = new Chunk(new Vector3Int(x * PixelingConstants.ChunkSize, y * PixelingConstants.ChunkSize, z * PixelingConstants.ChunkSize));
                    Chunks[c.Offset] = c;
                }
            }
        }

        List<uint> voxBuffer = new List<uint>();
        List<int> chunkIndices = new List<int>();

        for (int x = 0; x < PixelingConstants.WorldSize; x++)
        {
            for (int y = 0; y < PixelingConstants.WorldSize; y++)
            {
                for (int z = 0; z < PixelingConstants.WorldSize; z++)
                {
                    Vector3Int chunkPos = new Vector3Int(x * PixelingConstants.ChunkSize, y * PixelingConstants.ChunkSize, z * PixelingConstants.ChunkSize);
                    c = Chunks[chunkPos];
                    var chunkData = c.CreateOctreeRepresentation();
                    if (chunkData.Length > 0)
                    {
                        voxBuffer.AddRange(chunkData);
                        chunkIndices.Add(voxBuffer.Count);
                        chunkIndices.AddRange(new int[] { x * PixelingConstants.ChunkSize, y * PixelingConstants.ChunkSize, z * PixelingConstants.ChunkSize });
                    }
                }
            }
        }
        // marker of the end of the array
        chunkIndices.AddRange(new int[]{ 0,0,0,0});

        buff = new ComputeBuffer(voxBuffer.Count, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        indexBuff = new ComputeBuffer(chunkIndices.Count, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);


        material.SetBuffer("_VPD", buff);
        buff.SetData(voxBuffer.ToArray());
        material.SetBuffer("_ChunkIndices", indexBuff);
        indexBuff.SetData(chunkIndices.ToArray());
    }

    private void OnDestroy()
    {
        buff.Release();
    }

    // Update is called once per frame
    void Update()
    {
        debugMenu.SetText($"FPS: {1.0f / Time.deltaTime}");
    }
}