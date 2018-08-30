using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mel.VoxelGen
{
    public class TestAsyncTaskCollection : MonoBehaviour
    {
        ConcurrentDictionary<int, Task<int>> tasks = new ConcurrentDictionary<int, Task<int>>();
        ConcurrentDictionary<int, int> results = new ConcurrentDictionary<int, int>();
        private void Start()
        {
            Generate();
        }

        async void Generate()
        {
            int count = 6;
            for(int i = 0; i < count; ++i)
            {
                int input = i % (count / 2);
                int output;
                if (results.ContainsKey(input))
                {
                    output = results[input];
                    Debug.Log("skipped recalculation of : " + input);
                }
                else
                {
                    output = await tasks.GetOrAdd(input, getIntAsync(input));
                }

            }
        }

        async Task<int> getIntAsync(int input)
        {
            await Task.Delay(1000);
            return input * 2;
        }


    }
}
