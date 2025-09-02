using System;
using System.Collections;
using System.Collections.Generic;

namespace Emilia.DataBuildPipeline.Editor
{
    public class DataPostproces
    {
        public IEnumerator StartProcess(IBuildContainer container, IBuildArgs args)
        {
            List<IDataPostproces> postprocessList = DataPostprocesManage.instance.GetDataPostproces(args);
            int amount = postprocessList.Count;
            for (int i = 0; i < amount; i++)
            {
                IDataPostproces postproces = postprocessList[i];

                bool isFinish = false;

                try
                {
                    postproces.Postprocess(container, args, () => isFinish = true);
                }
                catch (Exception e)
                {
                    isFinish = true;
                    container.buildReport.result = BuildResult.Failed;
                    container.buildReport.AddErrorMessage($"DataPostproces:{postproces} 出现错误： {e.Message}\n{e.StackTrace}");
                }

                while (isFinish == false) yield return 0;
            }
        }
    }
}