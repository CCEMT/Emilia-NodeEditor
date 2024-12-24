using System.Collections.Generic;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface IGraphAsset
    {
        void SetChildren(List<Object> childAssets);
        List<Object> GetChildren();
        
        void CollectAsset(List<Object> allAssets);
    }
}