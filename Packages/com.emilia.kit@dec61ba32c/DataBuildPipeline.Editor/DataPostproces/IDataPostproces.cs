using System;

namespace Emilia.DataBuildPipeline.Editor
{
    public interface IDataPostproces
    {
        void Postprocess(IBuildContainer buildContainer, IBuildArgs buildArgs, Action onFinished);
    }
}