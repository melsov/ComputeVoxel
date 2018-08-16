using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mel.Math;
using Mel.Storage;
using Mel.VoxelGen;
using System.Linq;
using System;
using VoxelPerformance;


namespace Mel.VoxelGen
{

    public class WorldGenerator : MonoBehaviour
    {
        public struct GenArea
        {
            public IntBounds3 bounds;
            public bool Dirty {
                get {
                    if(PlayerPrefs.HasKey(bounds.ToKey())) {
                        return PlayerPrefs.GetInt(bounds.ToKey()) == 1;
                    }
                    return true;
                }
                set {
                    PlayerPrefs.SetInt(bounds.ToKey(), value ? 1 : 0);
                }
            }

        }

        [SerializeField]
        IntVector3 buildArea;

        [SerializeField]
        IntVector3 initialOrigin;

        [SerializeField]
        VGenConfig vGenConfig;

        [SerializeField]
        ChunkForge forge;

        IntBounds3 genBounds {
            //TODO: vGenConfig place bounds within world 
            get { return new IntBounds3 { start = initialOrigin, size = buildArea }; }
        }

        GenArea genArea {
            get {
                return new GenArea { bounds = genBounds };
            }
        }

        void Generate()
        {
            // for the genArea

            // possibly:
            // bootstrap some seed data. 
            // such as a known full light voxel 
            // maybe this function runs once per job: e.g. per light distribution, structure distribution

            // There's a Dictionary 'neighbors' of  NeighborChunks, key = IntVector3 
            // neighbors will hold genBounds + 2 area by the end (give or take 8 for outer corners)

            
            // foreach ch
            // get the full 3D array of voxels (not packed)

            // get the same for the six surrounding chs // NOTE: for structures we'll need all 27 chunks
            // for any of the seven / 27 chunks the data may already be in neighbors. or if not forge it.

            // with this nei chunks:
            //  for each neighbor
            //     give data from a side (as in light or structures)



            // use them to form a NeighborChunks
        }




    }
}
