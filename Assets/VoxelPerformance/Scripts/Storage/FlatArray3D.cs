using Mel.Math;

namespace Mel.Storage
{
    public struct FlatArray3D<T>
    {
        IntVector3 size;
        T[] storage;

        public FlatArray3D(IntVector3 size)
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

        public T this[IntVector3 v] {
            get {
                return storage[v.ToFlatXYZIndex(size)];
            }
            set {
                storage[v.ToFlatXYZIndex(size)] = value;
            }
        }
    }


    public struct FlatArray2D<T>
    {
        IntVector2 size;
        T[] storage;

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
    }
}