namespace Emilia.Kit
{
    public static class HierarchyAssetExtension
    {
        public static IHierarchyAsset GetRootAsset(this IHierarchyAsset graphAsset)
        {
            IHierarchyAsset rootGraphAsset = graphAsset;
            while (rootGraphAsset.parent != null) rootGraphAsset = rootGraphAsset.parent;
            return rootGraphAsset;
        }
    }
}