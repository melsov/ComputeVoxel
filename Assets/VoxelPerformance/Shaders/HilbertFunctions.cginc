#ifndef HILBERT_FUNCTIONS
#define HILBERT_FUNCTIONS

#include "HilbertTables.cginc"


///////////////////////
// Hilbert functions //
///////////////////////

bool isDifferentHilbertCube(uint prevVoxel, uint nextVoxel, uint cubeSize) {
    uint3 p, n;
    p.x = (prevVoxel / 65536) & 255;
    p.y = (prevVoxel / 256) & 255;
    p.z = prevVoxel & 256;

    n.x = (nextVoxel / 65536) & 255;
    n.y = (nextVoxel / 256) & 255;
    n.z = nextVoxel & 255;

    p /= cubeSize;
    n /= cubeSize;

    return p.x != n.x || p.y != n.y || p.z != n.z;
}


uint3 TransposeIndex(uint index, int bits)
{
	int dimensions = 3;
    uint3 result;

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

int FlatHilbertIndex(uint3 hindex, int bits)
{
    int result = 0;
    int dimensions = 3;

    for(int i = 0; i < dimensions; ++i)
    {
        for(int j = 0; j < bits; ++j)
        {
            result |= (int)(((hindex[dimensions - i - 1] >> j) & 1) << (j * dimensions + i)); 
        }
    }
    return result;
}


/// <summary>
/// Convert the Hilbert index into an N-dimensional point expressed as a vector of uints.
///
/// Note: In Skilling's paper, this function is named TransposetoAxes.
/// </summary>
/// <param name="transposedIndex">The Hilbert index stored in transposed form.</param>
/// <param name="bits">Number of bits per coordinate.</param>
/// <returns>Coordinate vector.</returns>
uint3 HilbertAxes(uint3 transposedIndex, int bits)
{
    uint3 X = transposedIndex; // (uint[])transposedIndex.Clone();
    int n = 3; // X.Length; // n: Number of dimensions
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
uint3 HilbertIndexTransposed(uint3 hilbertAxes, int bits)
{
    uint3 X = hilbertAxes;
    int n = 3;//hilbertAxes.Length; // n: Number of dimensions
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



int CoordsToFlatHilbertIndex(uint3 coords, int bits)
{
    return FlatHilbertIndex(HilbertIndexTransposed(coords.xzy, bits), bits); // coords.SwizzleYZ().HilbertIndexTransposed(bits).FlatHilbertIndex(bits);
}
 

uint3 HilbertIndexToCoords(uint index, int bits)
{
    return HilbertAxes(TransposeIndex(index, bits), bits).xzy; //  TransposeIndex(index, bits, dimensions).HilbertAxes(bits).SwizzleYZ();
}



#endif
