using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Modix.Services.Images.ColorQuantization
{
    public sealed class Octree
    {
        public void Add(Color color)
        {
            var node = Root;

            for (var i = 0; i < 8; i++)
            {
                var index = GetOctreeIndex(color, i);

                if (node.Children[index] is null)
                    node.Children[index] = new OctreeNode();

                node = node.Children[index];
            }

            node.ReferenceCount++;
            node.RCount += color.R;
            node.GCount += color.G;
            node.BCount += color.B;

            _leaves.Add(node);
        }

        public PaletteItem[] GetPalette()
        {
            var palette = new PaletteItem[_leaves.Count];

            var i = 0;
            foreach (var leaf in _leaves)
            {
                palette[i] = new PaletteItem()
                {
                    Color = Color.FromArgb(
                        leaf.RCount / leaf.ReferenceCount,
                        leaf.GCount / leaf.ReferenceCount,
                        leaf.BCount / leaf.ReferenceCount),
                    Weight = leaf.ReferenceCount,
                };

                i++;
            }

            return palette;
        }

        public void Reduce()
        {
            _leaves.Clear();

            Reduce(Root);

            void Reduce(OctreeNode node)
            {
                for (var i = 0; i < node.Children.Length; i++)
                {
                    var child = node.Children[i];

                    if (child is null)
                        continue;

                    if (child.Children.All(x => x is null))
                    {
                        node.ReferenceCount += child.ReferenceCount;
                        node.RCount += child.RCount;
                        node.GCount += child.GCount;
                        node.BCount += child.BCount;

                        node.Children[i] = null;

                        _leaves.Add(node);
                    }
                    else
                    {
                        Reduce(child);
                    }
                }
            }
        }

        public OctreeNode Root { get; } = new OctreeNode();

        private static int GetOctreeIndex(Color color, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            var mask = 0b1000_0000 >> bitIndex;

            var bitIndexComplement = 7 - bitIndex;

            // Get the indicated bit and format in 0b0000_0RGB format.
            var r = (color.R & mask) >> (bitIndexComplement - 2);
            var g = (color.G & mask) >> (bitIndexComplement - 1);
            var b = (color.B & mask) >> bitIndexComplement;

            return r | g | b;
        }

        private readonly HashSet<OctreeNode> _leaves = new HashSet<OctreeNode>();
    }
}
