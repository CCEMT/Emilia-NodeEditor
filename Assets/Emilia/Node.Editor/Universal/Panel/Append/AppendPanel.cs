using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class AppendPanel : GraphPanel
    {
        struct AppendPanelInfo : IEquatable<AppendPanelInfo>
        {
            public IGraphPanel graphPanel;
            public string displayName;

            public bool Equals(AppendPanelInfo other) => Equals(this.graphPanel, other.graphPanel) && this.displayName == other.displayName;
            public override bool Equals(object obj) => obj is AppendPanelInfo other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(this.graphPanel, this.displayName);
        }

        public float leftMargin = 5;
        public float rightMargin = 5f;

        private IGraphPanel selectedPanel;
        private List<AppendPanelInfo> graphPanels = new List<AppendPanelInfo>();

        private IMGUIContainer toggleContainer;

        public AppendPanel()
        {
            name = nameof(AppendPanel);

            toggleContainer = new IMGUIContainer(OnToggleGUI);
            toggleContainer.name = $"{nameof(AppendPanel)}-Toolbar";

            Add(this.toggleContainer);

            RegisterCallback<GeometryChangedEvent>((_) => { this.toggleContainer.style.width = layout.width; });
        }

        public void SetMargins(float size)
        {
            this.leftMargin = size;
            this.rightMargin = size;
        }

        public void AddGraphPanel<T>(string displayName)
        {
            IGraphPanel graphPanel = ReflectUtility.CreateInstance<T>() as IGraphPanel;
            if (graphPanel == null) return;

            AppendPanelInfo panelInfo = new AppendPanelInfo {
                graphPanel = graphPanel,
                displayName = displayName
            };

            graphPanels.Add(panelInfo);

            if (graphPanels.Count > 0) SwitchPanel(graphPanels.FirstOrDefault().graphPanel);

        }

        public void RemoveGraphPanel<T>()
        {
            IGraphPanel graphPanel = ReflectUtility.CreateInstance<T>() as IGraphPanel;
            if (graphPanel == null) return;

            AppendPanelInfo panelInfo = graphPanels.FirstOrDefault(p => p.graphPanel == graphPanel);
            if (panelInfo.graphPanel == null) return;

            graphPanels.Remove(panelInfo);

            if (selectedPanel == panelInfo.graphPanel)
            {
                selectedPanel.Dispose();
                selectedPanel.rootView.RemoveFromHierarchy();
                selectedPanel = null;
            }

            if (graphPanels.Count > 0) SwitchPanel(graphPanels.FirstOrDefault().graphPanel);

        }

        private void SwitchPanel(IGraphPanel panel)
        {
            if (this.selectedPanel != null)
            {
                selectedPanel.Dispose();
                panel.rootView.RemoveFromHierarchy();
            }

            panel.Initialize(graphView);
            Add(panel.rootView);

            selectedPanel = panel;
        }

        private void OnToggleGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Space(this.leftMargin);

            int appendPanelCount = graphPanels.Count;
            for (int i = 0; i < appendPanelCount; i++)
            {
                AppendPanelInfo panelInfo = graphPanels[i];
                bool isSelected = selectedPanel == panelInfo.graphPanel;

                if (GUILayout.Toggle(isSelected, panelInfo.displayName, EditorStyles.toolbarButton))
                {
                    if (isSelected) continue;
                    SwitchPanel(panelInfo.graphPanel);
                }
            }

            GUILayout.Space(this.rightMargin);

            GUILayout.EndHorizontal();
        }
    }
}