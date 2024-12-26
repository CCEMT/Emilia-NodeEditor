using System.Collections.Generic;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface ISelectableGraphElement
    {
        /// <summary>
        /// 收集选中的对象
        /// </summary>
        IEnumerable<Object> CollectSelectedObjects();

        /// <summary>
        /// 更新选中状态
        /// </summary>
        void UpdateSelected();
    }
}