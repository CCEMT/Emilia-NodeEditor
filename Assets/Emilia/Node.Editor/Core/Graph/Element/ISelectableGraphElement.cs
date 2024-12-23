using System.Collections.Generic;
using UnityEngine;

namespace Emilia.Node.Editor
{
    public interface ISelectableGraphElement
    {
        IEnumerable<Object> CollectSelectedObjects();

        void UpdateSelected();
    }
}