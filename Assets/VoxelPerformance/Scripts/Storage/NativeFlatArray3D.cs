using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace Mel.Storage
{

    public struct NativeFlatArray3D<T> where T : struct
    {
        IntVector3 size;
        NativeArray<T> storage;

        void SetData(T[] data, Allocator alloc = Allocator.TempJob)
        {
            if (data.Length != storage.Length) { throw new System.Exception("wut? why?"); }
            storage = new NativeArray<T>(data, alloc);
        }

        public static NativeFlatArray3D<Y> FromArray<Y>(Y[] data, IntVector3 size, Allocator alloc = Allocator.TempJob) where Y : struct
        {
            NativeFlatArray3D<Y> result;
            result.size = size;
            result.storage = new NativeArray<Y>(data, alloc);
            return result;
        }

        public NativeFlatArray3D(IntVector3 size, Allocator alloc = Allocator.TempJob)
        {
            this.size = size;
            storage = new NativeArray<T>(new T[size.Area], alloc);
        }

        public static NativeFlatArray3D<Y> FromFlatArray3D<Y>(FlatArray3D<Y> f3, Allocator alloc = Allocator.TempJob) where Y : struct
        {
            return FromArray(f3.GetStorage(), f3.size, alloc);
        }

        public int Length {
            get {
                return storage.Length;
            }
        }

        public T this[int i] {
            get {
                return storage[i];
            }
            set {
                storage[i] = value;
            }
        }

        public T this[IntVector3 v] {
            get {
                return storage[v.ToFlatXYZIndex(size)];
            }
            set {
                storage[v.ToFlatXYZIndex(size)] = value;
            }
        }

        public T this[IntVector2 v, NeighborDirection nd] {
            get {
                return this[v.ToCubeFace(nd, size)];
            }
            set {
                this[v.ToCubeFace(nd, size)] = value;
            }
        }

        public IEnumerable<T> Iterator {
            get {
                foreach (var t in storage) { yield return t; }
            }
        }

        internal void Dispose()
        {
            if (storage.IsCreated)
            {
                storage.Dispose();
            }
        }
    }
   
}
