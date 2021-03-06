#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Ginger.Environments;
using Ginger.Run;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.SourceControl;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class ExecutionResultsFolderTreeItem : TreeViewItemBase, ITreeViewItem
    {
        RunSetsExecutionsHistoryPage mRusSetsExecutionsPage;

        public string Folder { get; set; }
        public string Path { get; set; }

        public Solution Solution { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        override public string NodePath()
        {
            return Path + @"\";
        }
        override public Type NodeObjectType()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFile;
            if (IsGingerDefualtFolder)
            {
                ImageFile = "@ExecutionRes_16x16.png";
            }
            else
            {
                ImageFile = "@Folder2_16x16.png";
            }

            return TreeViewUtils.CreateItemHeader(Folder, ImageFile, Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            return Childrens;           
        }
        
        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mRusSetsExecutionsPage == null)
            {
                mRusSetsExecutionsPage = new RunSetsExecutionsHistoryPage(RunSetsExecutionsHistoryPage.eExecutionHistoryLevel.Solution);
            }
            return mRusSetsExecutionsPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }


        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Execution Logger Configurations", ExecutionResultsConfigWindow, null, "@Config3_16x16.png");
            TV.AddToolbarTool("@Config3_16x16.png", "Execution Logger Configurations", ExecutionResultsConfigWindow);

            TreeViewUtils.AddMenuItem(mContextMenu, "Open Execution Results Default Folder", OpenExecutionResultsFolder, null, "@Folder_16x16.png");
            TV.AddToolbarTool("@Folder_16x16.png", "Open Execution Results Default Folder", OpenExecutionResultsFolder);                 
        }

        private void ExecutionResultsConfigWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            Ginger.Reports.ExecutionLoggerConfiguration.ExecutionResultsConfigurationPage();
        }

        private void OpenExecutionResultsFolder(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewFolderFiles(GetExecutionResultsFolder());
        } 

        private string GetExecutionResultsFolder()
        {
            if (App.UserProfile.Solution != null && App.UserProfile.Solution.ExecutionLoggerConfigurationSetList != null && App.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Count > 0)
                return Ginger.Run.ExecutionLogger.GetLoggerDirectory(App.UserProfile.Solution.ExecutionLoggerConfigurationSetList[0].ExecutionLoggerConfigurationExecResultsFolder);
            else
                return string.Empty;
        }
    }
}
