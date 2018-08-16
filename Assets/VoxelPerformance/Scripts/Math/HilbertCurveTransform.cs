﻿using Mel.Math;
using UnityEngine;

namespace HilbertExtensions
{
    // credit: Paul Chernoch https://stackoverflow.com/questions/499166/mapping-n-dimensional-value-to-a-point-on-hilbert-curve/10384110#10384110

    /// <summary>
    /// Convert between Hilbert index and N-dimensional points.
    /// 
    /// The Hilbert index is expressed as an array of transposed bits. 
    /// 
    /// Example: 5 bits for each of n=3 coordinates.
    /// 15-bit Hilbert integer = A B C D E F G H I J K L M N O is stored
    /// as its Transpose                        ^
    /// X[0] = A D G J M                    X[2]|  7
    /// X[1] = B E H K N        <------->       | /X[1]
    /// X[2] = C F I L O                   axes |/
    ///        high low                         0------> X[0]
    ///        
    /// NOTE: This algorithm is derived from work done by John Skilling and published in "Programming the Hilbert curve".
    /// (c) 2004 American Institute of Physics.
    /// 
    /// </summary>
    public static class HilbertCurveTransform
    {

        #region convenience-methods

        static uint[] TransposeIndex(uint index, int bits, int dimensions)
        {
            uint[] result = new uint[dimensions];

            for(int i = bits - 1; i >= 0; --i)
            {
                for(int j = dimensions - 1; j >= 0; --j)
                {
                    int shift = j + i * dimensions;
                    uint b = (index >> shift) & 1;
                    result[dimensions - j - 1] |= b << i;
                }
            }
            return result;
        }

        static int FlatHilbertIndex(this uint[] hindex, int bits)
        {
            int result = 0;
            int dimensions = hindex.Length;

            for(int i = 0; i < dimensions; ++i)
            {
                for(int j = 0; j < bits; ++j)
                {
                    result |= (int)(((hindex[dimensions - i - 1] >> j) & 1) << (j * dimensions + i)); 
                }
            }
            return result;
        }

        public static Vector3 ToVector3(this uint[] u3) { return new Vector3(u3[0], u3[1], u3[2]); }


        public static int CoordsToFlatHilbertIndex(this uint[] coords, int bits)
        {
            return coords.SwizzleYZ().HilbertIndexTransposed(bits).FlatHilbertIndex(bits);
        }

        public static uint[] HilbertIndexToCoords(uint index, int bits, int dimensions)
        {
            return TransposeIndex(index, bits, dimensions).HilbertAxes(bits).SwizzleYZ();
        }

        static uint[] SwizzleYZ(this uint[] coords) { return new uint[] { coords[0], coords[2], coords[1] }; }

        #endregion

        /// <summary>
        /// Convert the Hilbert index into an N-dimensional point expressed as a vector of uints.
        ///
        /// Note: In Skilling's paper, this function is named TransposetoAxes.
        /// </summary>
        /// <param name="transposedIndex">The Hilbert index stored in transposed form.</param>
        /// <param name="bits">Number of bits per coordinate.</param>
        /// <returns>Coordinate vector.</returns>
        public static uint[] HilbertAxes(this uint[] transposedIndex, int bits)
        {
            var X = (uint[])transposedIndex.Clone();
            int n = X.Length; // n: Number of dimensions
            uint N = 2U << (bits - 1), P, Q, t;
            int i;
            // Gray decode by H ^ (H/2)
            t = X[n - 1] >> 1;
            // Corrected error in Skilling's paper on the following line. The appendix had i >= 0 leading to negative array index.
            for (i = n - 1; i > 0; i--)
                X[i] ^= X[i - 1];
            X[0] ^= t;
            // Undo excess work
            for (Q = 2; Q != N; Q <<= 1)
            {
                P = Q - 1;
                for (i = n - 1; i >= 0; i--)
                    if ((X[i] & Q) != 0U)
                        X[0] ^= P; // invert
                    else
                    {
                        t = (X[0] ^ X[i]) & P;
                        X[0] ^= t;
                        X[i] ^= t;
                    }
            } // exchange
            return X;
        }

        /// <summary>
        /// Given the axes (coordinates) of a point in N-Dimensional space, find the distance to that point along the Hilbert curve.
        /// That distance will be transposed; broken into pieces and distributed into an array.
        /// 
        /// The number of dimensions is the length of the hilbertAxes array.
        ///
        /// Note: In Skilling's paper, this function is called AxestoTranspose.
        /// </summary>
        /// <param name="hilbertAxes">Point in N-space.</param>
        /// <param name="bits">Depth of the Hilbert curve. If bits is one, this is the top-level Hilbert curve.</param>
        /// <returns>The Hilbert distance (or index) as a transposed Hilbert index.</returns>
        public static uint[] HilbertIndexTransposed(this uint[] hilbertAxes, int bits)
        {
            var X = (uint[])hilbertAxes.Clone();
            var n = hilbertAxes.Length; // n: Number of dimensions
            uint M = 1U << (bits - 1), P, Q, t;
            int i;
            // Inverse undo
            for (Q = M; Q > 1; Q >>= 1)
            {
                P = Q - 1;
                for (i = 0; i < n; i++)
                    if ((X[i] & Q) != 0)
                        X[0] ^= P; // invert
                    else
                    {
                        t = (X[0] ^ X[i]) & P;
                        X[0] ^= t;
                        X[i] ^= t;
                    }
            } // exchange
            // Gray encode
            for (i = 1; i < n; i++)
                X[i] ^= X[i - 1];
            t = 0;
            for (Q = M; Q > 1; Q >>= 1)
                if ((X[n - 1] & Q) != 0)
                    t ^= Q - 1;
            for (i = 0; i < n; i++)
                X[i] ^= t;

            return X;
        }

    }
}

