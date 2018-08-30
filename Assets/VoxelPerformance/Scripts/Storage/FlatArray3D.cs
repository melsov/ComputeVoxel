using Mel.Math;
using System.Collections.Generic;

namespace Mel.Storage
{
    public struct FlatArray3D<T> 
    {
        IntVector3 _size;
        public IntVector3 size { get { return _size; } }
        T[] storage;

        public T[] GetStorage() { return storage; }

        void SetData(T[] data)
        {
            if(data.Length != storage.Length) { throw new System.Exception("wut? why?"); }
            storage = data;
        }

        public static FlatArray3D<Y> FromArray<Y>(Y[] data, IntVector3 size)
        {
            FlatArray3D<Y> result;
            result._size = size;
            result.storage = data;
            return result;
        }

        public FlatArray3D(IntVector3 size)
        {
            this._size = size;
            storage = new T[size.Area];
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
                return storage[v.ToFlatXYZIndex(_size)];
            }
            set {
                storage[v.ToFlatXYZIndex(_size)] = value;
            }
        }

        public T this[IntVector2 v, NeighborDirection nd] {
            get {
                return this[v.ToCubeFace(nd, _size)];
            } 
            set {
                this[v.ToCubeFace(nd, _size)] = value;
            }
        }

        public IEnumerable<T> Iterator {
            get {
                foreach(var t in storage) { yield return t; }
            }
        }


    }


    public struct FlatArray2D<T>
    {
        IntVector2 size;
        public T[] storage { get; private set; }

        public FlatArray2D(IntVector2 size)
        {
            this.size = size;
            storage = new T[size.Area];
        }

        public T this[int i] {
            get {
                return storage[i];
            }
            set {
                storage[i] = value;
            }
        }

        public T this[IntVector2 v] {
            get {
                return storage[v.ToFlatXYIndex(size)];
            }
            set {
                storage[v.ToFlatXYIndex(size)] = value;
            }
        }

        public void SetData(T[] storage)
        {
            this.storage = storage;
        }
    }
}