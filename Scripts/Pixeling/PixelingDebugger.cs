using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class PixelingDebugger : MonoBehaviour
{
    public static long GetSizeOfObjectSerialized(object obj)
    {
        long size = 0;
        object o = new object();
        using (Stream s = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, o);
            size = s.Length;
        }
        return size;
    }


    private static void DEBUG_RunGPUTraversalCPU(List<uint> voxBuffer)
    {

        uint curr = 0;
        int lod = 0;
        uint[] nodeStack = { 0, 0, 0, 0, 0, 0 };
        int iter = 0;
        while (true)
        {
            while (true)
            {
                iter++;
                if (iter > 5000)
                {
                    return;
                }
                curr = nodeStack[lod];
                uint childStart = voxBuffer[(int)curr] >> 16;

                uint pX = (voxBuffer[(int)curr] >> 11) & 31;
                uint pY = (voxBuffer[(int)curr] >> 6) & 31;
                uint pZ = (voxBuffer[(int)curr] >> 1) & 31;


                uint isLastChild = (voxBuffer[(int)curr]) & 1;

                if (isLastChild == 0)
                {
                    nodeStack[lod] = curr + 2;
                }
                else if (nodeStack[lod + 1] == 0 && curr != 0)
                {
                    break;
                }

                if (childStart == 0)
                { // is leaf
                }
                else
                {                              // is internal
                    lod += 1;
                    nodeStack[lod] = childStart;
                    continue;
                }

                if (isLastChild != 0)
                {
                    if (curr != 0)
                    {
                        break;
                    }
                }
            }
            if (lod > 1)
            {
                nodeStack[lod] = 0;
                lod--;
            }
            else
            {
                break;
            }
        }
    }

}
