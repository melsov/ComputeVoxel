using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mel.Extensions;
using Mel.Math;
using UnityEngine;

namespace Mel.VoxelGen
{
    /*
     *  Rubik's cube view top to bottom.
        each x is a 'cross neighbor.'
        c is the center voxel

      0 x 0
      x 0 x
      0 x 0

      x 0 x
      0 c 0
      x 0 x

      0 x 0
      x 0 x
      0 x 0
     */


    public struct CrossBits12
    {
        public uint storage { get; private set; }

        static int Shift(IntVector3 v)
        {
            v += 1;
            int result = v.y << 2;
            if (v.y == 1) // middle
            {
                result |= v.z + (v.x >> 1);
            } else
            {
                result |= (v.z << 1) + v.x - ((v.x * v.z) >> 1) - 1;
            }
            return result;
        }

        public void SetBit(IntVector3 v, bool isSolid)
        {
            storage = SetBit(v, isSolid, storage);
        }

        public static uint SetBit(IntVector3 v, bool isSolid, uint stor)
        {
            if(!CrossNeighbors12.IsCrossNeighborPosition(v)) {
                return 999999999;
            }
            int shift = Shift(v);
            Debug.Log("shift " + shift + " for " + v);
            stor = (uint) (isSolid ? stor | (uint) (1 << shift ) : stor & ~(1 << shift));
            return stor;
        }

        public bool GetBit(IntVector3 v)
        {
            return ((storage >> Shift(v)) & 1) == 1;
        }

        public static void TestAndMakeTable()
        {

            uint stor = 0;
            //foreach(var pos in CubeNeighbors4.directions)
            //{
            //    var subject = pos + IntVector3.up;
            //    stor = SetBit(subject, true, stor);
            //    Debug.Log(Convert.ToString(stor, 2));
            //}
            IntVector3[] table = new IntVector3[12];
            stor = 0;
            foreach(var pos in CrossNeighbors12.members)
            {
                int shift = Shift(pos);
                table[shift] = pos;
                stor = SetBit(pos, true, stor);
                string st = Convert.ToString(stor, 2);
                Debug.Log(st);
            }
            StringBuilder s = new StringBuilder();
            for(int i=0; i<table.Length; ++i)
            {
                s.Append(string.Format("float3({0}),",table[i].ToShortString()));
            }
            Debug.Log(s.ToString());

        }

  

    }

    public static class CrossNeighbors12
    {
        public static IntVector3[] members;
        public static HashSet<IntVector3> memberSet;

        static CrossNeighbors12()
        {
            members = new IntVector3[12];
            int index = 0;

            var center = IntVector3.up;
            foreach(var dir in CubeNeighbors4.directions)
            {
                members[index++] = center + dir;
            }

            center.y--;
            foreach(var dir in DiagonalXZ.diagonals)
            {
                members[index++] = dir;
            }

            center.y--;
            foreach(var dir in CubeNeighbors4.directions)
            {
                members[index++] = center + dir;
            }

            memberSet = new HashSet<IntVector3>();
            foreach(var mem in members)
            {
                memberSet.Add(mem);
            }
        }


        public static bool IsCrossNeighborPosition(IntVector3 v)
        {
            return memberSet.Contains(v);
        }
    }

    public static class NeighborXZBBitOrder
    {

        public static Dictionary<IntVector3, NeighborDirection> lookup;

        static NeighborXZBBitOrder()
        {
            lookup = new Dictionary<IntVector3, NeighborDirection>(4);
            for(int i=0; i < 4; ++i)
            {
                lookup.Add(CubeNeighbors4.directions[i], (NeighborDirection)i);
            }
        }

        public static bool BitAt(IntVector3 v, out int bit)
        {
            if (lookup.ContainsKey(v))
            {
                bit = (int)lookup[v];
                return true;
            }

            bit = -99999;
            return false;
        }

        public static int BitAt(NeighborDirection nd)
        {
            switch (nd)
            {
                case NeighborDirection.Right: return 0;
                case NeighborDirection.Left: return 1;
                case NeighborDirection.Forward: return 2;
                case NeighborDirection.Back: return 3;
                default: throw new Exception("Please don't ask me about the other directions");
            }
        }
    }


    public static class DiagonalXZ
    {
        public static Dictionary<IntVector3, DiagonalDirectionsXZ> Directions;

        public static IntVector3[] diagonals =
        {
            IntVector3.forwardRight, IntVector3.backRight, IntVector3.forwardLeft, IntVector3.backLeft
        };

        static DiagonalXZ()
        {
            Directions = new Dictionary<IntVector3, DiagonalDirectionsXZ>();
            for(int i = 0; i < diagonals.Length; ++i)
            {
                Directions.Add(diagonals[i], (DiagonalDirectionsXZ)i);
            }
        }

        public static int BitAt(DiagonalDirectionsXZ dd)
        {
            return (int)dd;
        }

        public static bool BitAt(IntVector3 v, out int bit)
        {
            if (Directions.ContainsKey(v))
            {
                bit = (int)Directions[v];
                return true;
            }

            bit = -9999;
            return false;
        }
    }

    public enum DiagonalDirectionsXZ
    {
        ForwardRight, BackRight, ForwardLeft, BackLeft
    }

}
