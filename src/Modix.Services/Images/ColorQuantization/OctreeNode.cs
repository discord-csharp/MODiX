namespace Modix.Services.Images.ColorQuantization
{
    public sealed class OctreeNode
    {
        public int ReferenceCount { get; set; }

        public int RCount { get; set; }

        public int GCount { get; set; }

        public int BCount { get; set; }

        public OctreeNode[] Children { get; } = new OctreeNode[8];
    }
}
