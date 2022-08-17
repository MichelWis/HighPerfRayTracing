using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    float Scale = .05f;
    public float Amplitude = 10f;

    public Noise(float scale, float amplitude) 
    {
        Scale = scale;
        Amplitude = amplitude;
    }

    public float Density3DPerlinBase(float x, float y, float z)
    {
        var XY = Mathf.PerlinNoise(Scale * x, Scale * y);
        var XZ = Mathf.PerlinNoise(Scale * x, Scale * z);
        var YX = Mathf.PerlinNoise(Scale * y, Scale * z);
        var YZ = Mathf.PerlinNoise(Scale * y, Scale * z);
        var ZX = Mathf.PerlinNoise(Scale * z, Scale * x);
        var ZY = Mathf.PerlinNoise(Scale * z, Scale * y);

        return Amplitude * (XY + XZ + YX + YZ + ZX + ZY) / 6f;
    }

    public float Density3DPerlinBase(int x, int y, int z)
    {
        var XY = Mathf.PerlinNoise(Scale * x, Scale * y);
        var XZ = Mathf.PerlinNoise(Scale * x, Scale * z);
        var YX = Mathf.PerlinNoise(Scale * y, Scale * z);
        var YZ = Mathf.PerlinNoise(Scale * y, Scale * z);
        var ZX = Mathf.PerlinNoise(Scale * z, Scale * x);
        var ZY = Mathf.PerlinNoise(Scale * z, Scale * y);

        return Amplitude * ((XY + XZ + YX + YZ + ZX + ZY)) / 6f;
    }

    public float Density3DPerlinBase(Vector3Int coord)
    {
        var XY = Mathf.PerlinNoise(Scale * coord.x, Scale * coord.y);
        var XZ = Mathf.PerlinNoise(Scale * coord.x, Scale * coord.z);
        var YX = Mathf.PerlinNoise(Scale * coord.y, Scale * coord.z);
        var YZ = Mathf.PerlinNoise(Scale * coord.y, Scale * coord.z);
        var ZX = Mathf.PerlinNoise(Scale * coord.z, Scale * coord.x);
        var ZY = Mathf.PerlinNoise(Scale * coord.z, Scale * coord.y);

        return Amplitude * ((XY + XZ + YX + YZ + ZX + ZY)) / 6f;
    }

    public Vector3 NoiseNormal(Vector3 pos)
    {
        return NoiseNormal(pos.x, pos.y, pos.z);
    }

    public Vector3 NoiseNormal(float x, float y, float z)
    {
        float baseNoise = Density3DPerlinBase(x + .001f, y, z);
        float gradX = baseNoise - Density3DPerlinBase(x + .001f, y, z);
        float gradY = baseNoise - Density3DPerlinBase(x, y + .001f, z);
        float gradZ = baseNoise - Density3DPerlinBase(x, y, z + .001f);

        return new Vector3(gradX, gradY, gradZ);
    }
}