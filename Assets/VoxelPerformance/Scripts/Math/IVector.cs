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

        #region static-unit-vectors

        public static IntVector3 zero => new IntVector3(0); 
        public static IntVector3 one => new IntVector3(1); 
        public static IntVector3 right { get { return new IntVector3(1, 0, 0); } }
        public static IntVector3 up { get { return new IntVector3(0, 1, 0); } }
        public static IntVector3 forward { get { return new IntVector3(0, 0, 1); } }

        public static IntVector3 left { get { return new IntVector3(-1, 0, 0); } }
        public static IntVector3 down { get { return new IntVector3(0, -1, 0); } }
        public static IntVector3 back { get { return new IntVector3(0, 0, -1); } }

        public static IntVector3 forwardRight => right + forward;
        public static IntVector3 forwardLeft => left + forward;
        public static IntVector3 backRight => right + back;
        public static IntVector3 backLeft => left + back;

        public static IntVector3 forwardUp => up + forward;
        public static IntVector3 forwardDown => down + forward;
        public static IntVector3 backUp => up + back;
        public static IntVector3 backDown => down + back;

        public static IntVector3 rightUp => right + up;
        public static IntVector3 rightDown => right + down;
        public static IntVector3 leftUp => left + up;
        public static IntVector3 leftDown => left + down;

        public static IntVector3 maskRight { get { return new IntVector3(0, 1, 1); } }
        public static IntVector3 maskUp { get { return new IntVector3(1, 0, 1); } }
        public static IntVector3 maskForward { get { return new IntVector3(1, 1, 0); } }

        public static IntVector3 maskLeft { get { return new IntVector3(0, 1, 1); } }
        public static IntVector3 maskDown { get { return new IntVector3(1, 0, 1); } }
        public static IntVector3 maskBack { get { return new IntVector3(1, 1, 0); } }

        #endregion

        public IntVector3 ZeroToOneNonZeroToZero { get {
                return new IntVector3(x == 0 ? 1 : 0, y == 0 ? 1 : 0, z == 0 ? 1 : 0);
            } }


        public IntVector3 ToNegOneZeroOne {
            get {
                return new IntVector3( x.ToNegOneZeroOne(), y.ToNegOneZeroOne(), z.ToNegOneZeroOne());
            }
        }

        public static implicit operator Vector3(IntVector3 iv) { return new Vector3(iv.x, iv.y, iv.z); }

        public Vector3 ToVector3() { return this; }

        public static IntVector3 FromVector3(Vector3 v)
        {
            return new IntVector3((int)v.x, (int)v.y, (int)v.z);
        }
        //public static implicit operator IntVector3(Vector3 v) { return new IntVector3((int)v.x, (int)v.y, (int)v.z); }

        #region operators

        public static IntVector3 operator *(IntVector3 a, IntVector3 b) { return new IntVector3(a.x * b.x, a.y * b.y, a.z * b.z); }
        public static IntVector3 operator *(IntVector3 v, int b) { return new IntVector3(v.x * b, v.y * b, v.z * b); }
        public static IntVector3 operator *(IntVector3 v, float b) { return new IntVector3((int)(v.x * b), (int)(v.y * b), (int)(v.z * b)); }

        public static IntVector3 operator /(IntVector3 a, IntVector3 b) { return new IntVector3(a.x / b.x, a.y / b.y, a.z / b.z); }
        public static IntVector3 operator /(IntVector3 v, int b) { return new IntVector3(v.x / b, v.y / b, v.z / b); }
        public static IntVector3 operator /(IntVector3 v, float b) { return new IntVector3((int)(v.x / b), (int)(v.y / b), (int)(v.z / b)); }

        public static IntVector3 operator +(IntVector3 a, IntVector3 b) { return new IntVector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static IntVector3 operator +(IntVector3 a, int b) { return new IntVector3(a.x + b, a.y + b, a.z + b); }

        public static IntVector3 operator -(IntVector3 a, IntVector3 b) { return new IntVector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static IntVector3 operator -(IntVector3 a, int b) { return new IntVector3(a.x - b, a.y - b, a.z - b); }

        //mod
        public static IntVector3 operator %(IntVector3 a, int b) { return new IntVector3(a.x % b, a.y % b, a.z % b); }
        public static IntVector3 operator %(IntVector3 a, IntVector3 b) { return new IntVector3(a.x % b.x, a.y % b.y, a.z % b.z); }

        public static bool operator <(IntVector3 a, IntVector3 b) { return a.x < b.x && a.y < b.y && a.z < b.z; }
        public static bool operator >(IntVector3 a, IntVector3 b) { return a.x > b.x && a.y > b.y && a.z > b.z; }

        public static bool operator <=(IntVector3 a, IntVector3 b) { return a.x <= b.x && a.y <= b.y && a.z <= b.z; }
        public static bool operator >=(IntVector3 a, IntVector3 b) { return a.x >= b.x && a.y >= b.y && a.z >= b.z; }

        #endregion

        public static IntVector3 AbsMod(IntVector3 v, IntVector3 size)
        {
            while (v.x < 0) v.x += size.x;
            while (v.y < 0) v.y += size.y;
            while (v.z < 0) v.z += size.z;

            return v % size;
        }

        #region standard-overrides

        public string ToBinaryString()
        {
            return string.Format("{0}, {1}, {2}", Convert.ToString(x, 2), Convert.ToString(y, 2), Convert.ToString(z, 2));
        }

        public string ToShortString()
        {
            return string.Format("{0}, {1}, {2}", x, y, z);
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

        #endregion


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

        public bool AllOnesOrZeros {
            get {
                return x > -1 && x < 2 && y > -1 && y < 2 && z > -1 && z < 2;
            }
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

        public IEnumerable<IndexedIntVector3> IteratorXZYTopDown {
            get {
                int xStart = Mathf.Min(x, 0);
                int yStart = Mathf.Max(y, 0);
                int zStart = Mathf.Min(z, 0);
                int xLim = Mathf.Max(x, 0);
                int yLim = Mathf.Min(y, 0);
                int zLim = Mathf.Max(z, 0);
                int i = 0;
                for (int zz = zStart; zz < zLim; zz++)
                {
                    for (int xx = xStart; xx < xLim; xx++)
                    {
                        for (int yy = yStart - 1; yy >= yLim; yy--)
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

        public IEnumerable<IndexedIntVector3> IteratorYXZTopDown {
            get {
                int xStart = Mathf.Min(x, 0);
                int yStart = Mathf.Max(y, 0);
                int zStart = Mathf.Min(z, 0);
                int xLim = Mathf.Max(x, 0);
                int yLim = Mathf.Min(y, 0);
                int zLim = Mathf.Max(z, 0);
                int i = 0;
                for (int yy = yStart - 1; yy >= yLim; yy--)
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

        public int ToFlatZXYIndex(IntVector3 arrayDims)
        {
            return ((z * arrayDims.x) + x) * arrayDims.y + y;
        }

        public int ToFlatXZYIndex(IntVector3 arrayDims)
        {
            return ((x * arrayDims.z) + z) * arrayDims.y + y;
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

        public IntVector2 xz { get { return new IntVector2(x, z); } }

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

    public enum NeighborDirection
    {
        Right, Left, Up, Down, Forward, Back
    }



    public struct CubeNeighbors6
    {

        public IntVector3 center;

        public IntVector3 Right { get { return Get(NeighborDirection.Right); } }
        public IntVector3 Left { get { return Get(NeighborDirection.Left); } }
        public IntVector3 Up { get { return Get(NeighborDirection.Up); } }
        public IntVector3 Down { get { return Get(NeighborDirection.Down); } }
        public IntVector3 Forward { get { return Get(NeighborDirection.Forward); } }
        public IntVector3 Back { get { return Get(NeighborDirection.Back); } }

        public static NeighborDirection Opposite(NeighborDirection nd)
        {
            return (NeighborDirection)(((int)nd +( (int)nd % 2 == 0 ? 1 : 5)) % 6);
        }

        public static NeighborDirection[] GetDirections {
            get {
                return new NeighborDirection[]
                {
                    NeighborDirection.Right, NeighborDirection.Left, NeighborDirection.Up, NeighborDirection.Down, NeighborDirection.Forward, NeighborDirection.Back
                };
            }
        }

        public static IntVector3 Relative(NeighborDirection nd)
        {
            switch (nd)
            {
                case NeighborDirection.Right: default: return IntVector3.right;
                case NeighborDirection.Left: return IntVector3.left;
                case NeighborDirection.Up: return IntVector3.up;
                case NeighborDirection.Down: return IntVector3.down;
                case NeighborDirection.Forward: return IntVector3.forward;
                case NeighborDirection.Back: return IntVector3.back;
            }
        }

        public static IntVector3 RelativeMask(NeighborDirection nd)
        {
            switch (nd)
            {
                case NeighborDirection.Right: default: return IntVector3.maskRight;
                case NeighborDirection.Left: return IntVector3.maskLeft;
                case NeighborDirection.Up: return IntVector3.maskUp;
                case NeighborDirection.Down: return IntVector3.maskDown;
                case NeighborDirection.Forward: return IntVector3.maskForward;
                case NeighborDirection.Back: return IntVector3.maskBack;
            }
        }

        public static int Positive(NeighborDirection nd)
        {
            switch (nd)
            {
                case NeighborDirection.Right: default: return 1;
                case NeighborDirection.Left: return 0;
                case NeighborDirection.Up: return 1;
                case NeighborDirection.Down: return 0;
                case NeighborDirection.Forward: return 1;
                case NeighborDirection.Back: return 0;
            }
        }

        public static IEnumerable<NeighborDirection> TouchFaces(IntVector3 p, IntVector3 size)
        {
            if (p.x == 0) yield return NeighborDirection.Left;
            if (p.x == size.x - 1) yield return NeighborDirection.Right;
            if (p.y == 0) yield return NeighborDirection.Down;
            if (p.y == size.y - 1) yield return NeighborDirection.Up;
            if (p.z == 0) yield return NeighborDirection.Back;
            if (p.z == size.z - 1) yield return NeighborDirection.Forward;
        }

        public static IEnumerable<NeighborDirection> TouchFacesXZ(IntVector3 p, IntVector3 size)
        {
            if (p.x == 0) yield return NeighborDirection.Left;
            if (p.x == size.x - 1) yield return NeighborDirection.Right;
            if (p.z == 0) yield return NeighborDirection.Back;
            if (p.z == size.z - 1) yield return NeighborDirection.Forward;
        }

        public static IEnumerable<IntVector3> EscapeFaces(IntVector3 p, IntVector3 size)
        {
            foreach (var nd in TouchFaces(p, size))
            {
                yield return Relative(nd);
            }
        }

        public static IEnumerable<IntVector3> EscapeFacesXZ(IntVector3 p, IntVector3 size)
        {
            foreach(var nd in TouchFacesXZ(p, size))
            {
                yield return Relative(nd);
            }
        }

        public static bool IsOnFace(IntVector3 p, IntVector3 size)
        {
            if (p.x == 0) return true;
            else if (p.x == size.x - 1) return true;
            if (p.y == 0) return true;
            else if (p.y == size.y - 1) return true;
            if (p.z == 0) return true;
            else if (p.z == size.z - 1) return true;
            return false;
        }

        public static IntVector3 SnapToFace(IntVector3 p, IntVector3 size, NeighborDirection nd)
        {
            var zeroOut = RelativeMask(nd) * p;
            return zeroOut + (size - 1) * Relative(nd) * Positive(nd);
        }


        public static NeighborDirection[] Directions = new NeighborDirection[] 
        {
            NeighborDirection.Right, NeighborDirection.Left,
            NeighborDirection.Up, NeighborDirection.Down,
            NeighborDirection.Forward, NeighborDirection.Back
        };

        public static NeighborDirection[] DirectionsXZ = new NeighborDirection[]
        {
            NeighborDirection.Right, NeighborDirection.Left,
            NeighborDirection.Forward, NeighborDirection.Back
        };
        


        public static IntVector3 Relative(int i) {
            return Relative((NeighborDirection)i);
        }

        public bool IsNeighbor(IntVector3 candidate) { return IsNeighbor(center, candidate); }

        public static bool IsNeighbor(IntVector3 subject, IntVector3 candidate)
        {
            return (candidate - subject).IsUnitDirection();
        }

        public IntVector3 Get(NeighborDirection nd) { return this[(int)nd]; }

        public IntVector3 this[int i] {
            get {
                return center + Relative(i);
            }
        }

        public static IEnumerable<IntVector3> NeighborsOf(IntVector3 p)
        {
            foreach(var nd in Directions)
            {
                yield return p + Relative(nd);
            }
        }

        public static IEnumerable<IntVector3> NeighborsOfXZ(IntVector3 p)
        {
            foreach(var nd in DirectionsXZ)
            {
                yield return Relative(nd) + p;
            }
        }

        public static IEnumerable<IntVector3> NeighborsOfIncludingCenter(IntVector3 p)
        {
            yield return p;
            foreach(var v in NeighborsOf(p)) { yield return v; }
        }

        public IEnumerable<IntVector3> GetNeighbors {
            get {
                for(int i=0; i < 6; ++i) { yield return this[i]; }
            }
        }

    }

    public static class CubeNeighbors4
    {
        public static IntVector3[] directions =
        {
            IntVector3.right, IntVector3.left, IntVector3.forward, IntVector3.back
        };
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

        public static IntVector2 operator +(IntVector2 a, IntVector2 b) { return new IntVector2(a.x + b.x, a.y + b.y); }
        public static IntVector2 operator -(IntVector2 a, IntVector2 b) { return new IntVector2(a.x - b.x, a.y - b.y); }

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

        public IEnumerable<IndexedIntVector2> IteratorXY()
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

        public IntVector3 ToIntVector3XZWithY(int yHeight) { return new IntVector3(x, yHeight, y); }

        public int ToFlatXYIndex(IntVector2 size)
        {
            return x * size.y + y;
        }

        public IntVector3 ToCubeFace(NeighborDirection dir, IntVector3 cubeDimension)
        {
            var face = IntVector3.zero;
            var altDim = IntVector3.zero;
            switch (dir)
            {
                case NeighborDirection.Right:
                case NeighborDirection.Left:
                    face = new IntVector3(x, y, 0); break;
                case NeighborDirection.Up:
                case NeighborDirection.Down:
                    face = new IntVector3(x, 0, y); break;
                case NeighborDirection.Forward:
                case NeighborDirection.Back:
                    face = new IntVector3(0, y, x); break;
                default:
                    break;
            }
            switch (dir)
            {
                case NeighborDirection.Right:
                    altDim = IntVector3.forward; break;
                case NeighborDirection.Up:
                    altDim = IntVector3.up; break;
                case NeighborDirection.Forward:
                    altDim = IntVector3.right; break;
                default:
                    break;
            }
            return face + altDim * cubeDimension;

        }

        public override string ToString()
        {
            return string.Format("IntVector2: {0}, {1}", x, y);
        }

    }

    public struct IntBounds3
    {
        public IntVector3 start;
        public IntVector3 size;

        public IntVector3 end {
            get { return start + size; }
        }

        public int Area { get { return size.Area; } }

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

        public IntBounds3 ExpandedBordersAdditive(IntVector3 BorderWidth)
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

        public IntVector3 RelativeOrigin(IntVector3 p)
        {
            return p - start;
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

        public IEnumerable<IntVector3> IteratorXZYTopDown {
            get {
                foreach (var iv in size.IteratorXZYTopDown)
                {
                    yield return new IntVector3.IndexedIntVector3()
                    {
                        v = start + iv.v,
                        index = iv.index
                    };
                }
            }
        }

        public IEnumerable<IntVector3> IteratorYXZTopDown {
            get {
                foreach (var iv in size.IteratorYXZTopDown)
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

        public IEnumerable<IntVector2> IteratorXZ {
            get {
                foreach(var iv in size.xz.IteratorXY())
                {
                    yield return new IntVector2(iv.v.x, iv.v.y) + start.xz;
                }
            }
        }

        public string ToKey(string tag="")
        {
            return string.Format("IBounds{0}{1}{2}-{3}{4}{5}-{6}", start.x, start.y, start.z, end.x, end.y, end.z, tag);
        }

        public override string ToString()
        {
            return string.Format("IntBounds3: start: {0} . size: {1} ", start.ToShortString(), size.ToShortString());
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

        public ProximityIterator3(IntVector3 cube)
        {
            index = 0;
            positions = new ProximityOrderCoords(cube);
        }

        public ProximityIterator3(IntVector3 cube, IntVector3 boundsSize)
        {
            index = 0;
            positions = new ProximityOrderCoords(cube, boundsSize);
        }

        public bool HasNext { get { return index < positions.Length; } }

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

    public static class IntegerExtensions
    {
        public static int ToNegOneZeroOne(this int i)
        {
            return i < 0 ? -1 : (i > 0 ? 1 : 0);
        }
    }
}
