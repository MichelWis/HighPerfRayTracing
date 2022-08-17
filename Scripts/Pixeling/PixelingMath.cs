using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntCoord
{
    public short X, Y, Z;

    public IntCoord(short x, short y, short z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static IntCoord operator +(IntCoord a, IntCoord b)
    {
        return new IntCoord((short)(a.X + b.X), (short)(a.Y + b.Y), (short)(a.Z + b.Z));
    }

    public static IntCoord operator -(IntCoord a, IntCoord b)
    {
        return new IntCoord((short)(a.X - b.X), (short)(a.Y - b.Y), (short)(a.Z - b.Z));
    }

    public static IntCoord operator *(IntCoord a, IntCoord b)
    {
        return new IntCoord((short)(a.X * b.X), (short)(a.Y * b.Y), (short)(a.Z * b.Z));
    }

    public static IntCoord operator *(short a, IntCoord b)
    {
        return new IntCoord((short)(a * b.X), (short)(a * b.Y), (short)(a * b.Z));
    }
    public static IntCoord operator *(int a, IntCoord b)
    {
        return new IntCoord((short)(a * b.X), (short)(a * b.Y), (short)(a * b.Z));
    }
    public static IntCoord operator *(IntCoord b, short a)
    {
        return new IntCoord((short)(a * b.X), (short)(a * b.Y), (short)(a * b.Z));
    }
    public static IntCoord operator *(IntCoord b, int a)
    {
        return new IntCoord((short)(a * b.X), (short)(a * b.Y), (short)(a * b.Z));
    }

    public static IntCoord operator /(IntCoord a, IntCoord b)
    {
        return new IntCoord((short)(a.X / b.X), (short)(a.Y / b.Y), (short)(a.Z / b.Z));
    }

    public static IntCoord operator /(short a, IntCoord b)
    {
        return new IntCoord((short)(a / b.X), (short)(a / b.Y), (short)(a / b.Z));
    }



    public static bool operator ==(IntCoord a, IntCoord b)
    {
        return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }
    public static bool operator !=(IntCoord a, IntCoord b)
    {
        return !(a == b);
    }
}