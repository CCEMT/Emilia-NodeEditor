using System;

namespace Emilia.DataBuildPipeline.Editor
{
    public interface IDataBuild
    {
        void Build(IBuildContainer buildContainer, Action onFinished);
    }
}