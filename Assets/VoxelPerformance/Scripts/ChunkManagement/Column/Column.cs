using Mel.Extensions;
using Mel.Math;
using Mel.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mel.VoxelGen
{
    public class Column<T>
    {
        VGenConfig _vg;
        VGenConfig vGenConfig {
            get {
                if (!_vg) { _vg = GameObject.FindObjectOfType<VGenConfig>(); }
                return _vg;
            }
        }

        Dictionary<int, T> chunks = new Dictionary<int, T>();
        public IntVector2 position { get; private set; }

        public Column(IntVector2 pos)
        {
            position = pos;
        }

        public IEnumerable<int> Keys {
            get {
                foreach(var k in chunks.Keys) { yield return k; }
            }
        }

        public IEnumerable<T> Values {
            get {
                foreach(var v in chunks.Values) { yield return v; }
            }
        }

        public T this[int height] {
            get {
                return chunks[height];
            }
            set {
                chunks[height] = value;
            }
        }

        public bool Get(int height, out T item)
        {
            if(chunks.ContainsKey(height))
            {
                item = chunks[height];
                return true;
            }
            item = default(T);
            return false;
        }

        public void SetOrUpdate(int height, T item)
        {
            chunks.AddOrSet(height, item);
        }

        public void SetRangeToDefault(int min, int max)
        {
            for(int i=min; i<max; ++i) { SetOrUpdate(i, default(T)); }
        }

        public bool ContainsRange(int start, int size)
        {
            for(int i=start;i<start + size; ++i)
            {
                if(!chunks.ContainsKey(i)) { return false; }
            }
            return true;
        }



        public async Task<Column<T>> GatherDataForKeysAsync(Func<T, Task<T>> GetItem)
        {
            var keys = chunks.Keys.ToArray();
            // order descending? TODO?
            for(int i=0; i<keys.Length; ++i) 
            {
                chunks[keys[i]] = await GetItem(chunks[keys[i]]);
            }
            return this;
        }
    }

    public struct ColumnAndHeightMap<T>
    {
        public Column<T> column;
        public HeightMap heightMap;
    }

    public class ColumnMap3<T>
    {
        Dictionary<IntVector2, Column<T>> columns = new Dictionary<IntVector2, Column<T>>();

        public Column<T> Get(IntVector2 v)
        {
            if(columns.ContainsKey(v))
            {
                return columns[v];
            }
            return null;
        }

        Column<T> GetOrAdd(IntVector2 v)
        {
            var col = Get(v);
            if(col == null)
            {
                col = new Column<T>(v);
                AddOrSet(v, col);
            }
            return col;
        }

        public void AddOrSet(IntVector2 v, Column<T> col)
        {
            if (columns.ContainsKey(v))
            {
                columns[v] = col;
            }
            else
            {
                columns.Add(v, col);
            }
        }


        public bool Contains(IntVector2 v) { return columns.ContainsKey(v); }

        public void SetItem(IntVector3 v, T item)
        {
            var col =  GetOrAdd(v.xz);
            col[v.y] = item;
        }

        public bool GetItem(IntVector3 v, out T item)
        {
            var col = Get(v.xz);
            if(col == null)
            {
                item = default(T);
                return false;
            }
            return col.Get(v.y, out item);
        }

        public bool ContainsRange(IntVector2 v, int start, int size)
        {
            var col = Get(v);
            if(col == null) { return false; }
            return col.ContainsRange(start, size);
        }

        public void Clear()
        {
            columns.Clear();
        }

        public IEnumerable<Column<T>> GetColumns()
        {
            foreach(var col in columns.Values)
            {
                yield return col;
            }
        }
    }

    public class HeightMap
    {
        FlatArray2D<int> storage;

        public HeightMap(IntVector2 size)
        {
            storage = new FlatArray2D<int>(size);
        }

        public int this[IntVector2 v] {
            get {
                return storage[v];
            }
            set {
                storage[v] = value;
            }
        }

        public void setData(int[] data)
        {
            storage.SetData(data);
        }
    }

    public class HeightLookup
    {
        Dictionary<IntVector2, HeightMap> maps = new Dictionary<IntVector2, HeightMap>();

        public HeightMap this[IntVector2 v] {
            get {
                if(maps.ContainsKey(v))
                {
                    return maps[v];
                }
                return null;
            }
            set {
                if(maps.ContainsKey(v)) { maps[v] = value; }
                else { maps.Add(v, value); }
            }
        }

        
    }
}
