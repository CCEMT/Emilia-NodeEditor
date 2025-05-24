using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;

namespace Emilia.DataBuildPipeline.Editor
{
    public class DataPostprocesManage : BuildSingleton<DataPostprocesManage>
    {
        private List<IDataPostproces> _postprocessList = new List<IDataPostproces>();

        public DataPostprocesManage()
        {
            Type[] types = TypeCache.GetTypesDerivedFrom<IDataPostproces>().Where((type) => type.IsAbstract == false && type.IsInterface == false).ToArray();
            var amount = types.Length;
            for (var i = 0; i < amount; i++)
            {
                Type type = types[i];
                var postprocess = Activator.CreateInstance(type);
                IDataPostproces iDataPostproces = postprocess as IDataPostproces;
                if (iDataPostproces != null) this._postprocessList.Add(iDataPostproces);
            }
        }

        public List<IDataPostproces> GetDataPostproces(IBuildArgs buildArgs)
        {
            Type argsType = buildArgs.GetType();

            Dictionary<int, IDataPostproces> dataPostprocesMap = new Dictionary<int, IDataPostproces>();
            List<IDataPostproces> dataPostprocesList = new List<IDataPostproces>();

            while (argsType != typeof(object))
            {
                int amount = this._postprocessList.Count;
                for (var i = 0; i < amount; i++)
                {
                    IDataPostproces postproces = this._postprocessList[i];
                    Type type = postproces.GetType();
                    
                    BuildPipelineAttribute attribute = type.GetCustomAttribute<BuildPipelineAttribute>();
                    if (attribute == null) continue;
                    
                    if (attribute.argsType != argsType) continue;
                    
                    int priority = 0;
                    BuildSequenceAttribute sequenceAttribute = type.GetCustomAttribute<BuildSequenceAttribute>();
                    if (sequenceAttribute != null) priority = sequenceAttribute.priority;
                    
                    if (dataPostprocesMap.TryAdd(priority, postproces) == false) continue;
                    
                    dataPostprocesList.Add(postproces);
                }
                
                argsType = argsType.BaseType;
            }

            dataPostprocesList.Sort((a, b) => {
                BuildSequenceAttribute attributeA = a.GetType().GetCustomAttribute<BuildSequenceAttribute>();
                BuildSequenceAttribute attributeB = b.GetType().GetCustomAttribute<BuildSequenceAttribute>();
                return attributeA.priority.CompareTo(attributeB.priority);
            });

            return dataPostprocesList;
        }
    }
}