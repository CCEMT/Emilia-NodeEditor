﻿using System;
using Emilia.Node.Editor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    [Serializable]
    public class UniversalGraphLocalSetting : IGraphLocalSetting
    {
        public Vector3 position;
        public Vector3 scale = Vector3.one;
    }
}