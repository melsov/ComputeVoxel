using UnityEngine;
using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;

namespace Mel.VoxelGen
{

    public interface IIndexedVertex3 : IVertex
    {
        int index { get; set; }
        Vector3 Vec3 { get; }
    }

    public interface IIndexedVoxel3 : IIndexedVertex3
    {
        int voxel { get; }
    }

    public class IntVoxel3 : IIndexedVoxel3
    {
        public int voxel { get; private set; }
        public int index { get; set; }

        readonly double[] _Position;
        public double[] Position { get { return _Position; } }

        public Vector3 Vec3 { get { return new Vector3((float)Position[0], (float)Position[1], (float)Position[2]); } }

        public double x { get { return Position[0]; } }
        public double y { get { return Position[1]; } }
        public double z { get { return Position[2]; } }

        public int voxelType { get { return (voxel >> 24) & 0xFF; } }

        public IntVector3 intVector3 { get { return new IntVector3() { x = (int)x, y = (int)y, z = (int)z }; } }

        public IntVoxel3(int _voxel, int index)
        {
            this.voxel = _voxel;
            this.index = index;
            _Position = new double[] { (_voxel >> 16) & 0xFF, (_voxel >> 8) & 0xFF, _voxel & 0xFF };
        }

        public static IntVoxel3 FromPackedVoxel(int i, int bytePosition, uint fourVoxels, VGenConfig vGenConfig)
        {
            // PGen encodes voxel position by index: z, x, y (y least significant).
            // Voxel values encoded in 8 bits, packed 4 per uint.
            int vi = i * 4 + bytePosition;
            int z = vi / (vGenConfig.ChunkSizeY * vGenConfig.ChunkSizeX);
            int x = (vi % (vGenConfig.ChunkSizeZ * vGenConfig.ChunkSizeY)) / vGenConfig.ChunkSizeY;
            int y = vi % vGenConfig.ChunkSizeY; // ?? (vGenConfig.ChunkSizeZ * vGenConfig.ChunkSizeX);

            int voxel = ( (((int)fourVoxels >> (8 * bytePosition)) & 0xFF) << 24 ) | ((x & 0xFF) << 16) | ((y & 0xFF) << 8) | (z & 0xFF);
            return new IntVoxel3(voxel, vi);
        }
    }

    struct Vertex3 : IIndexedVertex3
    {
        Vector3 v;
        readonly double[] _Position;

        public int index {
            get; set;
        }

        public Vector3 Vec3 { get { return v; } }

        public Vertex3(Vector3 v, int index)
        {
            this.v = v;
            this.index = index;
            _Position = new double[] { v.x, v.y, v.z };
        }

        public double[] Position {
            get { return _Position; }
        }
    }

    public class VConvexFace<TVertex> : ConvexFace<TVertex, VConvexFace<TVertex>>
    where TVertex : IIndexedVoxel3
    {
        public int index(int i)
        {
            return Vertices[i].index;
        }

        public Vector3 normal {
            get {
                return Vector3.Cross(Vertices[1].Vec3 - Vertices[0].Vec3, Vertices[2].Vec3 - Vertices[1].Vec3);
            }
        }
    }

    public class MConvexFace<TVertex> : ConvexFace<TVertex, MConvexFace<TVertex>>
    where TVertex : IIndexedVertex3
    {
        public int index(int i)
        {
            return Vertices[i].index;
        }

        public Vector3 normal {
            get {
                return Vector3.Cross(Vertices[1].Vec3 - Vertices[0].Vec3, Vertices[2].Vec3 - Vertices[1].Vec3);
            }
        }
    }
 
}
