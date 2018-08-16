using System;
using System.Collections.Generic;
using g3;
using Mel.Math;
using UnityEngine;


namespace Mel.Trees
{
    public struct FlatVONode<T>
    {
        public int firstChildIndex;
        public T data;
        public byte depth;
        public byte octantIndex;
    }

    public struct FlatVOTree<T>
    {
        public FlatVONode<T>[][] tree;
    }

    public struct OctPositionVector
    {
        public bool bx, by, bz;

        public int x { get { return bx ? 1 : 0; } }
        public int y { get { return by ? 1 : 0; } }
        public int z { get { return bz ? 1 : 0; } }


        public int flatIndex {
            get { return (x << 2) | (y << 1) | z; }
        }

        public Vector3f ToVector3f() { return new Vector3f(x, y, z); }

        public Vector3f localPosition() { return ToVector3f() - Vector3f.One / 2f; }

        public string ToShortString() { return string.Format("{0}{1}{2}", x, y, z); }

        public override string ToString()
        {
            return string.Format("OctPositionVector: {0}" , ToShortString());
        }

        public Bounds octant(Bounds b)
        {
            return new Bounds(
                (Vector3f)b.center + b.extents * localPosition(), 
                b.extents);
        }

        public bool Shift(Vector3f.CardinalDirection dir, out OctPositionVector shifted)
        {
            shifted = this;
            switch (dir)
            {
                case Vector3f.CardinalDirection.Backward:
                    if (!shifted.bz) return false;
                    shifted.bz = false;
                    return true;
                case Vector3f.CardinalDirection.Forward:
                    if (shifted.bz) return false;
                    shifted.bz = true;
                    return true;
                case Vector3f.CardinalDirection.Down:
                    if (!shifted.by) return false;
                    shifted.by = false;
                    return true;
                case Vector3f.CardinalDirection.Up:
                    if (shifted.by) return false;
                    shifted.by = true;
                    return true;
                case Vector3f.CardinalDirection.Left:
                    if (!shifted.bx) return false;
                    shifted.bx = false;
                    return true;
                case Vector3f.CardinalDirection.Right:
                    if (shifted.bx) return false;
                    shifted.bx = true;
                    return true;
                case Vector3f.CardinalDirection.Nowhere:
                default:
                    return false;

            }

        }

        public void Shift(bool x, bool y, bool z)
        {
            if (x) bx = !bx; if (y) by = !by; if (z) bz = !bz;
        }

        public static OctPositionVector FromPosition(Vector3f pos, Vector3f reference)
        {
            return new OctPositionVector()
            {
                bx = pos.x > reference.x,
                by = pos.y > reference.y,
                bz = pos.z > reference.z
            };
        }

        public static OctPositionVector FromIndex(int localIndex)
        {
            bool x = false, y = false, z = false;
            switch(localIndex)
            {
                case 0: default: break;
                case 1: z = true; break;
                case 2: y = true; break;
                case 3: z = y = true; break;
                case 4: x = true; break;
                case 5: x = z = true; break;
                case 6: z = y = true; break;
                case 7: x = y = z = true; break;
            }
            return new OctPositionVector() { bx = x, by = y, bz = z };
        }
    }

    //CONSIDER:
    // could store 8 voxels (one byte per voxel) in two BitVector32s (or simply 2 ints) or in a BitArray

    //
    // Fixed depth octree for Minecraft like situations
    // This may stretch the definition of an octree?
    // Supports, setting, getting, ray cast get first hit
    // Not supported at the moment: removing
    //
    public class VoxelOctree<T>
    {
        public int _maxDepth { get; private set; }
        public Bounds bounds { get; private set; }
        VONode root;
        
        private float magNudge = .5f;

        public VoxelOctree(int _maxDepth, Bounds bounds)
        {
            this._maxDepth = _maxDepth;
            this.bounds = bounds;
            root = new VOInternalNode();
        }

        #region set-get

        public FlatVOTree<T> GetFlatTree()
        {
            
            var result = new Queue<FlatVONode<T>>[_maxDepth];
            for (int i = 0; i < _maxDepth; ++i) { result[i] = new Queue<FlatVONode<T>>(); }

            var nodes = new Stack<VOBoundsDepthNode>();
            nodes.Push(new VOBoundsDepthNode(root, bounds, 0, -1));
            while (nodes.Count > 0)
            {
                var node = nodes.Pop();
                int childCount = 0;
                for(int i = 0; i < 8; ++i)
                {
                    if(node.node[i] != null)
                    {
                        childCount++;
                        nodes.Push(new VOBoundsDepthNode(node.node[i], OctPositionVector.FromIndex(i).octant(node.bounds), node.depth + 1, i));
                    }
                }
                

            }

            //compiler relax
            return default(FlatVOTree<T>);
            
        }

        public bool Set(T leaf, Vector3f pos)
        {
            if(!bounds.Contains(pos)) { return false; }
            var subBounds = bounds;
            VONode node = root;
            VONode subNode;
            for(int i = 1; i <= _maxDepth; ++i)
            {
                subNode = node.GetAt(pos, subBounds.center);
                if(subNode == null)
                {
                    if (i == _maxDepth) subNode = new VOLeafNode();
                    else subNode = new VOInternalNode();
                    node.SetAt(subNode, pos, subBounds.center);
                }
                subBounds = GetOctant(subBounds, pos);
                node = subNode;
            }
            node.data = leaf;
            return true;
        }

        public bool Get(Vector3f pos, out T leaf)
        {
            leaf = default(T);
            if(!bounds.Contains(pos)) { return false; }
            var subBounds = bounds;
            VONode subNode = root;
            for(int i = 1; i <= _maxDepth; ++i)
            {
                subNode = subNode.GetAt(pos, subBounds.center);
                if(subNode == null) { return false; }
                subBounds = GetOctant(subBounds, pos);
            }
            leaf = subNode.data;
            return true;
        }

        public struct DBUGColorBounds
        {
            public Color color;
            public Bounds bounds;
            public IntVector3 coord;
            public bool isLeaf;
            public Ray3f ray;
            public string text;
            public Vertex vertex;
            public bool validVertex;

            public static implicit operator DBUGColorBounds (string s) { return new DBUGColorBounds() { text = s }; }
        }


        //
        // Get's the first leaf hit by a ray, if any.
        // TODO: revisit the logic. Exiting when we exit tree bounds feels like it shouldn't be necessary
        //
        public bool GetFirstRayhit(Ray3f ray, out T leaf, out List<DBUGColorBounds> debugTraversal, out List<Ray3f> rayStepsDebug)
        {

            debugTraversal = new List<DBUGColorBounds>();
            rayStepsDebug = new List<Ray3f>();

            leaf = default(T);
            var node = new VOBoundsDepthNode(root, bounds, 0);
            var ancestors = new Stack<VOBoundsDepthNode>();
            Vector3f pos;
            if(! EscapeVector.EnterPosition(bounds, ray, out pos))
            {
                return false;
            }

            ray.origin = pos;

            int iterSafety = 0;
            while (true && iterSafety++ < (int)(Mathf.Pow(8, _maxDepth) / 4))
            {
                if(!bounds.Contains(ray.origin))
                {
                    return false;
                }

                if (node.node.isLeaf)
                {
                    leaf = node.node.data;
                    return true;
                }

                OctPositionVector nextChildPosVector;

                var nextChild = node.node.GetAt(ray.origin, node.bounds.center, out nextChildPosVector);

                if(nextChild != null)
                {
                    ancestors.Push(node);
                    node = new VOBoundsDepthNode(nextChild, nextChildPosVector.octant(node.bounds), node.depth + 1);
                    continue;
                }

                // else find a sibling or pop to parent
                int doIterSafety = 0;
                VOBoundsDepthNode sibling;
                do
                { 
                    // while the ray origin is still within the parent bounds, see if there's a sibling along the ray
                    Vector3f escapeMagnitudes;
                    var subjectPosVector = OctPositionVector.FromPosition(ray.origin, node.bounds.center);

                    var validRayMove = SiblingFromRay(subjectPosVector, node, ray, out sibling, out escapeMagnitudes);
                    ray.origin += ray.direction * escapeMagnitudes.MinAbs;
                    if (validRayMove)
                    {

                        if (sibling.node != null)
                        {
                            ancestors.Push(node);
                            node = sibling;
                        } 

                    } else
                    {
                        if(ancestors.Count > 0)
                        {
                            node = ancestors.Pop();
                            break;

                        } else
                        {
                            return false;
                        }
                    }
                    if(doIterSafety++ > 20)
                    {
                        Debug.LogWarning("problems: hit inner do-while iter safety"); break;
                    }
                } while (sibling.node == null);

            }
            Debug.LogWarning("We didn't want to get here. ray traversing octree ");
            return false;
        }

        bool SiblingFromRay(OctPositionVector subjectPos, VOBoundsDepthNode parent, Ray3f ray, out VOBoundsDepthNode sibling, out Vector3f escapeMagnitudes)
        { 
            sibling = new VOBoundsDepthNode(null, default(Bounds));

            Bounds subjectBounds = subjectPos.octant(parent.bounds);
            escapeMagnitudes = EscapeVector.GetCornerMagnitudes(subjectBounds, ray) + new Vector3f(magNudge, magNudge, magNudge);

            OctPositionVector siblingPosVector;
            var cardinalDirection = ray.direction.GetDirection(escapeMagnitudes.IndexOfAbsMin);
            if(subjectPos.Shift(cardinalDirection, out siblingPosVector))
            {
                sibling = new VOBoundsDepthNode(parent.node[siblingPosVector.flatIndex], siblingPosVector.octant(parent.bounds), parent.depth + 1);
                return true;
            }
            return false;
        }



        public IEnumerable<VONode> GetAllNonNullNodes()
        {
            Stack<VODepthNode> nodes = new Stack<VODepthNode>();
            nodes.Push(new VODepthNode(root, 0));
            while(nodes.Count > 0)
            {
                var node = nodes.Pop();
                yield return node.node;
                foreach(var n in node.node.nodes)
                {
                    if(n == null) { continue; }
                    nodes.Push(new VODepthNode(n, node.depth + 1));
                }
            }
        }

        public IEnumerable<VOBoundsDepthNode> GetAllNonNullBoundsNodes()
        {
            var nodes = new Stack<VOBoundsDepthNode>();
            nodes.Push(new VOBoundsDepthNode(root, bounds));
            while(nodes.Count > 0)
            {
                var node = nodes.Pop();
                yield return node;
                if(node.node.isLeaf) { continue; }
                for(int i = 0; i < 8; ++i)
                {
                    var n = node.node[i];
                    if(n == null) { continue; }
                    nodes.Push(
                        new VOBoundsDepthNode(n, OctPositionVector.FromIndex(i).octant(node.bounds))
                        );
                }
            }
        }

        #endregion

        #region other-public-methods

        public Vector3f LeafSize()
        {
            return (Vector3f)bounds.size / Mathf.Pow(2, _maxDepth);
        }

        public Vector3f NodeSizeAtDepth(int depth)
        {
            return (Vector3f)bounds.size / Mathf.Pow(2, depth);
        }

        #endregion

        Bounds GetOctant(Bounds container, Vector3f pos)
        {
            return OctPositionVector.FromPosition(pos, container.center).octant(container);
        }

        #region VONode

        public abstract class VONode
        {
            public abstract T data { get; set; }
            protected abstract NodeChildren<VONode> children { get; set; }
            public abstract bool isLeaf { get; }

            public IEnumerable<VONode> nodes {
                get {
                    if(children != null) foreach (var n in children.nodes) { yield return n; }
                }
            }
            public void SetAt(VONode node, Vector3f pos, Vector3f reference)
            {
                if(children == null) { return; }
                children.SetAt(node, pos, reference);
            }

            public VONode GetAt(Vector3f pos, Vector3f reference)
            {
                if(children == null) { return null; }
                return children.GetAt(pos, reference);
            }

            public VONode GetAt(Vector3f pos, Vector3f reference, out OctPositionVector childPositionVector)
            { 
                if(children == null) { childPositionVector = default(OctPositionVector); return null; }
                return children.GetAt(pos, reference, out childPositionVector);
            }

            public VONode this[int i] {
                get {
                    if(children == null)
                    {
                        return null;
                    }
                    return children[i];
                }
            }

        }

        public class VOInternalNode : VONode
        {

            public override T data {
                get {
                    return default(T);
                }
                set {

                }
            }

            public NodeChildren<VONode> _children = new NodeChildren<VONode>();

            protected override NodeChildren<VONode> children {
                get {
                    return _children;
                }
                set {
                    _children = value;
                }
            }

            public override bool isLeaf { get { return false; } }
        }

        public class VOLeafNode : VONode
        {
            T _data;
            public override T data {
                get {
                    return _data;
                }
                set {
                    _data = value;
                }
            }

            protected override NodeChildren<VONode> children {
                get { return null; }
                set { }
            }

            public override bool isLeaf { get { return true; } }
        }

        struct VODepthNode
        {
            public VONode node;
            public int depth;

            public VODepthNode(VONode node, int depth) { this.node = node; this.depth = depth; }
        }

        public struct VOBoundsDepthNode
        {
            public VONode node;
            public Bounds bounds;
            public int depth;
            public int octantIndex;

            public VOBoundsDepthNode(VONode node, Bounds bounds) : this(node, bounds, -1)  { }
            public VOBoundsDepthNode(VONode node, Bounds bounds, int depth) : this(node, bounds, depth, -1) { }
            public VOBoundsDepthNode(VONode node, Bounds bounds, int depth, int octantIndex) {
                this.node = node; this.bounds = bounds; this.depth = depth; this.octantIndex = octantIndex;
            }



        }

        #endregion

        //
        // Maintain a morton code order array of nodes
        //
        public class NodeChildren<N> where N : VONode
        {
            N[] storage = new N[8];

            public IEnumerable<N> nodes {
                get {
                    foreach(var n in storage) { yield return n; }
                }
            }

            public int Length { get { return 8; } }

            public N this[int i] {
                get {
                    return storage[i % storage.Length];
                }
            }

            public void SetAt(N node, Vector3f pos, Vector3f reference)
            {
                storage[GetIndex(pos, reference)] = node;
            }

            public N GetAt(Vector3f pos, Vector3f reference)
            {
                return storage[GetIndex(pos, reference)];
            }

            public N GetAt(Vector3f pos, Vector3f reference, out OctPositionVector childPositionVector)
            {
                return storage[GetIndex(pos, reference, out childPositionVector)];
            }


            int GetIndex(Vector3f pos, Vector3f reference) {
                return OctPositionVector.FromPosition(pos, reference).flatIndex;
            }

            int GetIndex(Vector3f pos, Vector3f reference, out OctPositionVector childPositionVector)
            {
                childPositionVector = OctPositionVector.FromPosition(pos, reference);
                return childPositionVector.flatIndex;
            }


        }

        #region drawing

        public IEnumerable<Bounds> GetAllNonNullNodeBounds()
        {
            foreach(VOBoundsDepthNode bn in GetAllNonNullBoundsNodes())
            {
                yield return bn.bounds;
            }
        }

        #endregion

    }
}
