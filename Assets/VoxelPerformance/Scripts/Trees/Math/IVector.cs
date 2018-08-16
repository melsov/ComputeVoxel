/*using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mel.Math
{
    [System.Serializable]
    public struct IntVector3
    {
        public int x, y, z;

        public IntVector3(int i) : this(i, i, i) { }

        public IntVector3(uint x, uint y, uint z) : this((int)x, (int)y, (int)z) { }

        public IntVector3(int x, int y, int z) : this()
        {
            this.x = x; this.y = y; this.z = z;
        }

        public IntVector3(uint i) : this((int)i)
        {
        }

        public int this[int i] {
            get {
                return i == 0 ? x : (i == 1 ? y : z);
            }
        }

        public IEnumerable<int> Components {
            get {
                yield return x;
                yield return y;
                yield return z;
            }
        }


        public static IntVector3 zero { get { return new IntVector3(0); } }
        public static IntVector3 right { get { return new IntVector3(1, 0, 0); } }
        public static IntVector3 up { get { return new IntVector3(0, 1, 0); } }
        public static IntVector3 forward { get { return new IntVector3(0, 0, 1); } }
        public static IntVector3 left { get { return new IntVector3(-1, 0, 0); } }
        public static IntVector3 down { get { return new IntVector3(0, -1, 0); } }
        public static IntVector3 back { get { return new IntVector3(0, 0, -1); } }

        public static implicit operator Vector3(IntVector3 iv) { return new Vector3(iv.x, iv.y, iv.z); }

        public Vector3 ToVector3() { return this; }

        public static IntVector3 FromVector3(Vector3 v)
        {
            return new IntVector3((int)v.x, (int)v.y, (int)v.z);
        }
        //public static implicit operator IntVector3(Vector3 v) { return new IntVector3((int)v.x, (int)v.y, (int)v.z); }

        public static IntVector3 operator *(IntVector3 v, int b) { return new IntVector3(v.x * b, v.y * b, v.z * b); }
        public static IntVector3 operator *(IntVector3 v, float b) { return new IntVector3((int)(v.x * b), (int)(v.y * b), (int)(v.z * b)); }

        public static IntVector3 operator /(IntVector3 v, int b) { return new IntVector3(v.x / b, v.y / b, v.z / b); }
        public static IntVector3 operator /(IntVector3 v, float b) { return new IntVector3((int)(v.x / b), (int)(v.y / b), (int)(v.z / b)); }

        public static IntVector3 operator +(IntVector3 a, IntVector3 b) { return new IntVector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static IntVector3 operator -(IntVector3 a, IntVector3 b) { return new IntVector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static IntVector3 operator *(IntVector3 a, IntVector3 b) { return new IntVector3(a.x * b.x, a.y * b.y, a.z * b.z); }
        public static IntVector3 operator /(IntVector3 a, IntVector3 b) { return new IntVector3(a.x / b.x, a.y / b.y, a.z / b.z); }

        //mod
        public static IntVector3 operator %(IntVector3 a, int b) { return new IntVector3(a.x % b, a.y % b, a.z % b); }
        public static IntVector3 operator %(IntVector3 a, IntVector3 b) { return new IntVector3(a.x % b.x, a.y % b.y, a.z % b.z); }

        public static bool operator <(IntVector3 a, IntVector3 b) { return a.x < b.x && a.y < b.y && a.z < b.z; }
        public static bool operator >(IntVector3 a, IntVector3 b) { return a.x > b.x && a.y > b.y && a.z > b.z; }

        public string ToBinaryString()
        {
            return string.Format("{0}, {1}, {2}", Convert.ToString(x, 2), Convert.ToString(y, 2), Convert.ToString(z, 2));
        }


        public override string ToString()
        {
            return string.Format("IntVector3 {0}, {1}, {2}", x, y, z);
        }

        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != GetType()) { return false; }
            return Equal((IntVector3)obj);
        }

        public bool Equal(IntVector3 other) { return other.x == x && other.y == y && other.z == z; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public int MinComponent {
            get { return Mathf.Min(x, y, z); }
        }
        public int MaxComponent {
            get { return Mathf.Max(x, y, z); }
        }
      

        public uint[] ToUint3() { return new uint[] { (uint)x, (uint)y, (uint)z }; }

        public static IntVector3 FromUint3(uint[] us) { return new IntVector3((int)us[0], (int)us[1], (int)us[2]); }

        public int OctantIndex(int dimension) { return OctantIndex(new IntVector3(dimension, dimension, dimension)); }

        public int OctantIndex(IntVector3 CubeDimensions)
        {
            return (x < CubeDimensions.x / 2 ? 0 : 4) + (y < CubeDimensions.y / 2 ? 0 : 2) + (z < CubeDimensions.z / 2 ? 0 : 1);
        }

        public int SquareMagnitude {
            get { return x * x + y * y + z * z; }
        }

        public int Area { get { return x * y * z; } }

        public int XzArea { get { return x * z; } }
        public int XyArea { get { return x * y; } }
        public int YzArea { get { return y * z; } }

        public int SurfaceArea {
            get {
                return 2 * x * y + 2 * (z - 2) * y + 2 * (x - 2) * (z - 2);
            }
        }

        public IntVector3 Abs {
            get { return new IntVector3(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z)); }
        }

        public struct IndexedIntVector3
        {
            public IntVector3 v;
            public int index;

            public static implicit operator IntVector3(IndexedIntVector3 iv) { return iv.v; }
        }

        public IEnumerable<IndexedIntVector3> IteratorXYZ {
            get {
                int xStart = Mathf.Min(x, 0);
                int yStart = Mathf.Min(y, 0);
                int zStart = Mathf.Min(z, 0);
                int xLim = Mathf.Max(x, 0);
                int yLim = Mathf.Max(y, 0);
                int zLim = Mathf.Max(z, 0);
                int i = 0;
                for (int xx = xStart; xx < xLim; xx++)
                {
                    for (int yy = yStart; yy < yLim; yy++)
                    {
                        for (int zz = zStart; zz < zLim; zz++)
                        {
                            yield return new IndexedIntVector3() {
                                v = new IntVector3(xx, yy, zz),
                                index = i++
                            };
                        }
                    }
                }
            }
        }

        public IEnumerable<IntVector3> BoxCorners {
            get {
                IntVector3 v = new IntVector3(0);
                yield return v;
                v.x = x;
                yield return v;
                v.y = y;
                yield return v;
                v.x = 0;
                yield return v;
                v.z = z;
                yield return v;
                v.x = x;
                yield return v;
                v.y = 0;
                yield return v;
                v.x = 0;
                yield return v;
            }
        }

        public uint AbsValuesToUInt256()
        {
            return (((uint)Mathf.Abs(x) & 0xFF) << 16) | (((uint)Mathf.Abs(y) & 0xFF) << 8) | ((uint)Mathf.Abs(z) & 0xFF);
        }

        public int ToFlatXYZIndex(IntVector3 arrayDims)
        {
            return ((x * arrayDims.x) + y) * arrayDims.y + z;
        }

        public static int FlatXYZIndex(int x, int y, int z, IntVector3 arrayDims)
        {
            return ((x * arrayDims.x) + y) * arrayDims.y + z;
        }

        public bool IsOnAFace(IntVector3 cubeDims)
        {
            return
                x == 0 ||
                y == 0 ||
                z == 0 ||
                x == cubeDims.x - 1 ||
                y == cubeDims.y - 1 ||
                z == cubeDims.z - 1;
        }

        public static IntVector3 FromUint256(int voxel)
        {
            IntVector3 v = new IntVector3();
            v.x = (voxel >> 16) & 0xFF;
            v.y = (voxel >> 8) & 0xFF;
            v.z = voxel & 0xFF;
            return v;
        }


        public uint ToVoxel(int voxelType = 0)
        {
            return (uint)(((voxelType & 0xFF) << 24) | ((x & 0xFF) << 16) | ((y & 0xFF) << 8) | (z & 0xFF));
        }

        public static IntVector3 FromVoxelInt(int voxel)
        {
            return FromUint256(voxel);
        }

        public IntVector3 xzy { get { return new IntVector3(x, z, y); } }



    }

    public struct UIntVector3
    {
        public uint x, y, z;

        public UIntVector3(uint x, uint y, uint z)
        {
            this.x = x; this.y = y; this.z = z;
        }
        public UIntVector3(int x, int y, int z)
        {
            this.x = (uint)x; this.y = (uint)y; this.z =(uint) z;
        }

        public static implicit operator UIntVector3(IntVector3 v) { return new UIntVector3(v.x, v.y, v.z); }
        public static implicit operator IntVector3(UIntVector3 v) { return new IntVector3(v.x, v.y, v.z); }
    }

    public struct BooleanVector3
    {
        public bool x, y, z;
    }

    public enum MoveOption
    {
        Backward = -1, Nowhere, Forward
    }


   

    public struct IntBounds3
    {
        public IntVector3 start;
        public IntVector3 size;

        public IntVector3 end {
            get { return start + size; }
        }

        public int OctanctIndexOf(IntVector3 pos)
        {
            pos -= start;
            return pos.OctantIndex(size);
        }

        public IntVector3 OffsetForOctantIndex(int octantIndex)
        {
            IntVector3 result = new IntVector3()
            {
                x = (octantIndex >> 2) & 1,
                y = (octantIndex >> 1) & 1,
                z = octantIndex & 1
            };
            return result * size / 2;
        }

        public IEnumerable<IntVector3> IteratorXYZ {
            get {
                foreach(var iv in size.IteratorXYZ)
                {
                    yield return new IntVector3.IndexedIntVector3()
                    {
                        v = start + iv.v,
                        index = iv.index
                    };
                }
            }
        }
    }
}
*/