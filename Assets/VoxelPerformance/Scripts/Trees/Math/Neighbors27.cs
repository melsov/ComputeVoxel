using Mel.Extensions;
using Mel.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace Mel.Math
{
    public static class Cube9
    {
        public static IntVector3[] relative;

        static Cube9()
        {
            relative = new IntVector3[9];
            foreach(var v in new IntVector2(3, 3).IteratorXZAtY(0))
            {
                relative[v.index] = v.v - new IntVector3(1, 0, 1);
            }
        }

        public static IEnumerable<IntVector3> IteratorXZ {
            get {
                foreach(var v in relative) { yield return v; }
            }
        }

    }

    public static class Lookup27
    {
        public static IntVector3 ChunkIndex(IntVector3 centerRelativeIndex, IntVector3 chunkSize, out IntVector3 neighborChunkKey)
        {
            var result = IntVector3.AbsMod(centerRelativeIndex, chunkSize);
            neighborChunkKey = (centerRelativeIndex / chunkSize).ToNegOneZeroOne;
            return result;
        }
    }

    public static class Cube27
    {
        public static IntVector3[] relative;

        static Cube27()
        {
            relative = new IntVector3[27];
            foreach(var v in new IntVector3(3).IteratorXYZ)
            {
                relative[v.index] = v.v - IntVector3.one;
            }
        }

        public static IEnumerable<IntVector3> IteratorXYZ {
            get {
                foreach(var v in relative) { yield return v; }
            }
        }

        internal static IEnumerable<IntVector3> NeighborsOfIncludingCenter(IntVector3 center)
        {
            foreach(var v in relative)
            {
                yield return v + center;
            }
        }

        public static bool IsNeighborOf(IntVector3 center, IntVector3 candidate)
        {
            return (center - candidate).Abs.AllOnesOrZeros;
        }

        public static IntVector3 ToFlatArray3Index(IntVector3 center, IntVector3 candidate)
        {
            return candidate - center + new IntVector3(1);
        }
    }

    public struct Neighbors27<T>
    {
        //public Dictionary<IntVector3, T> storage;
        public FlatArray3D<T> storage;

        public IntVector3 center;

        public Neighbors27(IntVector3 center)
        {
            this.center = center;
            storage = new FlatArray3D<T>(IntVector3.one * 3); 
        }

        //public bool Contains(IntVector3 v) { return storage.ContainsKey(v); }

        public int Count { get { return storage.Length; } }

        public T this[IntVector3 v] {
            get {
                return storage[Cube27.ToFlatArray3Index(center,v)]; //.SafeGet(v);
            }
            set {
                if (Cube27.IsNeighborOf(center, v))
                {
                    storage[Cube27.ToFlatArray3Index(center, v)] = value;
                }
            }
        }

        public T Get(IntVector3 v)
        {
            return this[v];
        }

        public bool Get(IntVector3 v, out T item)
        {
            if (!Cube27.IsNeighborOf(center, v))
            {
                item = default(T);
                return false;
            }
            item = this[v];
            return true;
        }

        public bool Set(IntVector3 v, T item)
        {
            if(Cube27.IsNeighborOf(center, v))
            {
                this[v] = item;
                return true;
            }
            return false;
        }

        public T this[NeighborDirection nd] {
            get {
                return Get(nd);
            }
            set {
                Set(nd, value);
            }
        }

        public T Get(NeighborDirection nd)
        {
            return this[center + CubeNeighbors6.Relative(nd)];
        }

        public void Set(NeighborDirection nd, T item)
        {
            this[center + CubeNeighbors6.Relative(nd)] = item;
        }

        public IEnumerable<T> Iterator {
            get {
                foreach(var t in storage.Iterator) { yield return t; }
            }
        }

        public IntBounds3 bounds {
            get {
                return new IntBounds3
                {
                    start = center - IntVector3.one,
                    size = new IntVector3(3)
                };
            }
        }

        

    }

    public struct NativeNeighbors27<T> where T : struct
    {
        public NativeFlatArray3D<T> storage;

        public IntVector3 center;

        public NativeNeighbors27(IntVector3 center, Allocator alloc = Allocator.TempJob)
        {
            this.center = center;
            storage = new NativeFlatArray3D<T>(new IntVector3(3), alloc);
        }

        //public NativeNeighbors27(IntVector3 center, T[] data, Allocator alloc = Allocator.TempJob) 
        //{
        //    this.center = center;
        //    storage = NativeFlatArray3D<T>.FromArray(data, new IntVector3(3), alloc);
        //}

        //public static NativeNeighbors27<Y> FromNeighbor27<Y>(Neighbors27<Y> neibs, Allocator alloc = Allocator.TempJob) where Y : struct
        //{
        //    return new NativeNeighbors27<Y>(neibs.center, neibs.storage.GetStorage(), alloc);
        //}

        //public bool Contains(IntVector3 v) { return storage.ContainsKey(v); }

        public int Count { get { return storage.Length; } }

        public T this[IntVector3 v] {
            get {
                return storage[Cube27.ToFlatArray3Index(center, v)]; //.SafeGet(v);
            }
            set {
                if (Cube27.IsNeighborOf(center, v))
                {
                    storage[Cube27.ToFlatArray3Index(center, v)] = value;
                }
            }
        }

        public T Get(IntVector3 v)
        {
            return this[v];
        }

        public bool Get(IntVector3 v, out T item)
        {
            if (!Cube27.IsNeighborOf(center, v))
            {
                item = default(T);
                return false;
            }
            item = this[v];
            return true;
        }

        public bool Set(IntVector3 v, T item)
        {
            if (Cube27.IsNeighborOf(center, v))
            {
                this[v] = item;
                return true;
            }
            return false;
        }

        public T this[NeighborDirection nd] {
            get {
                return Get(nd);
            }
            set {
                Set(nd, value);
            }
        }

        public T Get(NeighborDirection nd)
        {
            return this[center + CubeNeighbors6.Relative(nd)];
        }

        public void Set(NeighborDirection nd, T item)
        {
            this[center + CubeNeighbors6.Relative(nd)] = item;
        }

        public IEnumerable<T> Iterator {
            get {
                foreach (var t in storage.Iterator) { yield return t; }
            }
        }

        public void Dispose()
        {
            storage.Dispose();
        }

    }
}
