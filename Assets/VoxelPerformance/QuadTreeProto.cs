using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mel.Math;
using Mel.Util;

namespace Mel.VoxelGen
{

    public struct Node
    {
        public int data;
        public byte childMask;
        public Vector2 dbugPos;
    }

    public class QuadTreeProto : MonoBehaviour
    {
        [SerializeField] int MaxDepth = 3;
        [SerializeField] Vector2 displayDimensions;

        [SerializeField] Transform displayNodePrefab;
        Node[] nodes;

        //TODO: add test nodes
        // tests...
        private void Awake()
        {
            nodes = new Node[RequiredStorage(MaxDepth)];
            if (displayDimensions.SqrMagnitude() < Mathf.Epsilon)
            {
                displayDimensions = new Vector2(15, 15);
            }
        }

        private int LeafCount { get { return (int)Mathf.Pow(4, MaxDepth); } }

        private int SideLength { get { return (int)Mathf.Pow(2, MaxDepth); } }

        static int RequiredStorage(int MaxDepth)
        {
            int result = 0;
            for(int i=0; i <= MaxDepth; ++i)
            {
                result += (int)Mathf.Pow(4, i);
            }
            return result;
        }

        int IndexForPos01(Vector2 pos01)
        {
            return (pos01.x > .5f ? 1 : 0) + (pos01.y > .5f ? 2 : 0);
        }


        Vector2 RelPosAtChildIndex(int childIndex)
        {
            return new Vector2((childIndex % 2) * .5f, (childIndex / 2) * .5f);
        }

        Vector2 CornerAtDepth(Vector2 pos01, int depth)
        {
            pos01 *= Mathf.Pow(2, depth - 1);
            return new Vector2(Mathf.Round(pos01.x), Mathf.Round(pos01.y));
        }

        void SetChildBit(Node n, int bit)
        {
            n.childMask |= (byte)(1 << bit);
        }

        static int InterLeaveBits(int x, int y, int bits)
        {
            int result = 0;
            Debug.Log(string.Format("x {0} , y {1}", x, y));
            for(int i=0; i < bits; ++i)
            {
                result |= (x & 1) << (i * 2 + 1);
                result |= (y & 1) << (i * 2);
                x = x >> 1;
                y = y >> 1;
            }
            Debug.Log("interleaved index: " + result);
            return result;
        }

        Vector2 PosFromIndex(int index)
        {
            int x = 0, y = 0;
            for(int i=0; i<MaxDepth; ++i)
            {
                x |= ((index >> (i * 2 + 1)) & 1) << i; // (1 << (i * 2 + 1)));
                y |= ((index >> (i * 2)) & 1) << i; // & (1 << (i * 2)));
            }
            print("x: " + x + " y: " + y);
            return new Vector2(x, y);
        }

        int IndexViaNormalize(Vector2 pos01)
        {
            int x = (int)(pos01.x * SideLength);
            int y = (int)(pos01.y * SideLength);
            return Mathf.Clamp(InterLeaveBits(x, y, MaxDepth), 0, LeafCount);
        }

        void SetNode(Vector2 pos01, int value)
        {
            int index = IndexViaNormalize(pos01);
            var node = nodes[index];
            node.data = value;
            nodes[index] = node;
        }

        void SetNodeOLD(Vector2 pos01, int value)
        {
            var nodeCorner = new Vector2(0, 0);
            var relPos = pos01;
            var curr = nodes[0];
            int index = 0;
            for (int i = 0; i < MaxDepth; ++i)
            {
                int childIndex = IndexForPos01(relPos);
                SetChildBit(curr, childIndex);
                nodeCorner += CornerAtDepth(relPos, i) / Mathf.Pow(2, i);
                curr.data = value;
                curr.dbugPos = nodeCorner;
                nodes[index] = curr;

                relPos -= nodeCorner;
                index = IndexAtDepth(i, childIndex);
                try
                {
                    curr = nodes[index];
                } catch (IndexOutOfRangeException ioe)
                {
                    Debug.LogWarning("index: " + index + " node length: " + nodes.Length + "   " + ioe.ToString());
                }
            }
        }

        int IndexAtDepth(int depth, int childIndex)
        {
            int nextStartIndex = GetNextStartIndex(depth + 1);
            int nextOffsetIndex = childIndex * (int)Mathf.Pow(4, depth);
            return nextStartIndex + nextOffsetIndex;
        }

        IEnumerable<int> RelativeChildIndices(Node n)
        {
            for(int i = 0; i < 4; ++i)
            {
                if (((n.childMask >> i) & 1) == 1)
                {
                    yield return i;
                }
            }
        }

        private void Start()
        {
            addTestPoints();
        }

        private void addTestPoints()
        {
            Vector2[] ps = new Vector2[]
            {
                new Vector2(.4f, .4f),
                new Vector2(.6f, .6f),
                new Vector2(.4f, .9f),
                new Vector2(.9f, .1f),
            };

            foreach(var p in ps)
            {
                SetNode(p, 1);
            }

            Display();
        }

        private void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                Vector2 screenPos = Input.mousePosition;
                SetNode(screenPos.normalized, 1);
                RemoveChildren();
                Display();
            }
        }

        void RemoveChildren()
        {
            transform.DestroyAllChildrenImmediate();
        }

        void Display()
        {
            RemoveChildren();
            for(int i=0; i<nodes.Length; ++i)
            {
                var node = nodes[i];
                if(node.data == 0)
                {
                    continue;
                }
                int depth = MaxDepth; // DepthForIndex(i);
                var corner = PosFromIndex(i); // DeriveCornerPos(i);

                var nodeDisplay = Instantiate(displayNodePrefab);
                nodeDisplay.SetParent(transform);
                Vector3 pos = corner;
                pos.z = -3f * depth;
                nodeDisplay.localPosition = pos;
                Vector3 scale = displayDimensions / Mathf.Pow(2f, depth);
                scale.z = 1f;
                nodeDisplay.localScale = scale;

                Debug.Log(string.Format("{0} : {1} depth: {2}", i, corner, depth));
            }
        }

        Vector2 DeriveCornerPos(int index)
        {
            int depth = DepthForIndex(index);
            int cIndex = index;
            Vector2 result = Vector2.zero;
            for(int i = depth; i >=1; --i)
            {
                int start = StartIndices[i];
                int offSet = cIndex - start;
                int quadMod = offSet % 4;
                result += RelPosAtChildIndex(quadMod) / Mathf.Pow(2, depth - 1);
                cIndex = StartIndices[i - 1] + offSet / 4;
            }
            return result;
        }

        static int[] StartIndices =
        {
            0, 1, 5, 21, 85, 341
        };

        private int DepthForIndex(int index)
        {
            for(int i=1; i<StartIndices.Length; ++i)
            {
                if (index < StartIndices[i]) return i - 1;
            }
            throw new Exception("why did you do this?");
        }

        private int GetNextStartIndex(int depth)
        {
            return StartIndices[depth];
        }
    }
}
