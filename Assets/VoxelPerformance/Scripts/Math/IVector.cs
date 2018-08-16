using System;
using System.Collections.Generic;
using System.Linq;
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
            set {
                switch (i)
                {
                    case 0: default: x = i; break;
                    case 1: y = i; break;
                    case 2: z = i; break;
                }
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

        public static bool operator <=(IntVector3 a, IntVector3 b) { return a.x <= b.x && a.y <= b.y && a.z <= b.z; }
        public static bool operator >=(IntVector3 a, IntVector3 b) { return a.x >= b.x && a.y >= b.y && a.z >= b.z; }



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

        public int ComponentSum() { return x + y + z; }

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

        public IEnumerable<IndexedIntVector3> IteratorYXZ {
            get {
                int xStart = Mathf.Min(x, 0);
                int yStart = Mathf.Min(y, 0);
                int zStart = Mathf.Min(z, 0);
                int xLim = Mathf.Max(x, 0);
                int yLim = Mathf.Max(y, 0);
                int zLim = Mathf.Max(z, 0);
                int i = 0;
                for (int yy = yStart; yy < yLim; yy++)
                {
                    for (int xx = xStart; xx < xLim; xx++)
                    {
                        for (int zz = zStart; zz < zLim; zz++)
                        {
                            yield return new IndexedIntVector3()
                            {
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

        public void ClampComponent(int componentIndex, int min, int max)
        {
            this[componentIndex] = (int)Mathf.Clamp(this[componentIndex], min, max);
        }

        public void ClampY(int min, int max) { ClampComponent(1, min, max); }

        public IntVector3 ClampedY(int min, int max)
        {
            var copy = this;
            copy.ClampY(min, max);
            return copy;
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

        public bool IsUnitDirection() {
            return Abs.ComponentSum() == 1;
        }

        public static IntVector3 FromUint256(uint voxel)
        {
            return FromUint256((int)voxel);
        }

        public static IntVector3 FromUint256(int voxel)
        {
            IntVector3 v = new IntVector3();
            v.x = voxel >> 16 & 0xFF;
            v.y = voxel >> 8 & 0xFF;
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

    public enum NeighborDirections
    {
        Right, Left, Up, Down, Forward, Back
    }

    public struct CubeNeighbors6
    {

        public IntVector3 center;

        public IntVector3 Right { get { return Get(NeighborDirections.Right); } }
        public IntVector3 Left { get { return Get(NeighborDirections.Left); } }
        public IntVector3 Up { get { return Get(NeighborDirections.Up); } }
        public IntVector3 Down { get { return Get(NeighborDirections.Down); } }
        public IntVector3 Forward { get { return Get(NeighborDirections.Forward); } }
        public IntVector3 Back { get { return Get(NeighborDirections.Back); } }

        public static IntVector3 relative(int i) {
            switch (i)
            {
                case 0: default: return IntVector3.right;
                case 1: return IntVector3.left;
                case 2: return IntVector3.up;
                case 3: return IntVector3.down;
                case 4: return IntVector3.forward;
                case 5: return IntVector3.back;
            }
        }

        public bool IsNeighbor(IntVector3 candidate) { return IsNeighbor(center, candidate); }

        public static bool IsNeighbor(IntVector3 subject, IntVector3 candidate)
        {
            return (candidate - subject).IsUnitDirection();
        }

        public IntVector3 Get(NeighborDirections nd) { return this[(int)nd]; }

        public IntVector3 this[int i] {
            get {
                return center + relative(i);
            }
        }

        public IEnumerable<IntVector3> GetNeighbors {
            get {
                for(int i=0; i < 6; ++i) { yield return this[i]; }
            }
        }

    }

    [Serializable]
    public struct IntVector2
    {
        public int x, y;

        public int Area {
            get {
                return x * y;
            }
        }

        public IntVector2(int x, int y)
        {
            this.x = x; this.y = y;
        }

        public struct IndexedIntVector2
        {
            public int index;
            public IntVector2 v;

            public static implicit operator IntVector2(IndexedIntVector2 iv) { return iv.v; }
        }

        public IEnumerable<IntVector3.IndexedIntVector3> IteratorXZAtY(int yHeight)
        {
            int xStart = Mathf.Min(x, 0);
            //int yStart = Mathf.Min(yHeight, 0);
            int zStart = Mathf.Min(y, 0);
            int xLim = Mathf.Max(x, 0);
            //int yLim = Mathf.Max(yHeight, 0);
            int zLim = Mathf.Max(y, 0);
            int i = 0;
            for (int xx = xStart; xx < xLim; xx++)
            {
                for (int zz = zStart; zz < zLim; zz++)
                {
                    yield return new IntVector3.IndexedIntVector3()
                    {
                        v = new IntVector3(xx, yHeight, zz),
                        index = i++
                    };
                }
            }

        }

        public IEnumerable<IndexedIntVector2> IteratorXY(int yHeight)
        {
            int xStart = Mathf.Min(x, 0);
            int yStart = Mathf.Min(y, 0);
            int xLim = Mathf.Max(x, 0);
            int yLim = Mathf.Max(y, 0);
            int i = 0;
            for (int xx = xStart; xx < xLim; xx++)
            {
                for (int yy = yStart; yy < yLim; yy++)
                {
                    yield return new IndexedIntVector2 {
                        index = i++,
                        v = new IntVector2(xx, yy)
                    };
                }
            }

        }

        public int ToFlatXYIndex(IntVector2 size)
        {
            return x * size.y + y;
        }
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

        public static IntBounds3 FromCenterHalfSize(IntVector3 center, IntVector3 halfSize)
        {
            return new IntBounds3
            {
                start = center - halfSize,
                size = halfSize * 2
            };
        }

        public IntBounds3 ExpandBordersAdditive(IntVector3 BorderWidth)
        {
            return new IntBounds3
            {
                start = start - BorderWidth,
                size = size + BorderWidth * 2
            };
        }

        public bool Contains(IntVector3 p)
        {
            return p >= start && p < end;
        }

        public bool ContainsInclusive(IntVector3 p)
        {
            return p >= start && p <= end;
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

        public IEnumerable<IntVector3> IteratorYXZ {
            get {
                foreach (var iv in size.IteratorYXZ)
                {
                    yield return new IntVector3.IndexedIntVector3()
                    {
                        v = start + iv.v,
                        index = iv.index
                    };
                }
            }
        }

        public string ToKey()
        {
            return string.Format("IBounds{0}{1}{2}-{3}{4}{5}", start.x, start.y, start.z, end.x, end.y, end.z);
        }
    }

    public struct ProximityIterator3
    {
        int index;
        ProximityOrderCoords positions;

        public ProximityIterator3(int size)
        {
            index = 0;
            positions = new ProximityOrderCoords(size);
        }

        public bool Next(out IntVector3 next)
        {
            if(index < positions.Length)
            {
                next = positions[index];
                index++;
                return true;
            }

            next = default(IntVector3);
            return false;
        }

        public void Reset()
        {
            index = 0;
        }
    }
}
