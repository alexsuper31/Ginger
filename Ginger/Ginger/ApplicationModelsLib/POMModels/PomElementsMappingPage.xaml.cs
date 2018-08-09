﻿using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for MappedUIElementsPage.xaml
    /// </summary>
    public partial class PomElementsMappingPage : Page
    {

        ApplicationPOMModel mPOM;
        public ObservableList<ElementLocator> mLocators = new ObservableList<ElementLocator>();
        public ObservableList<ControlProperty> mProperties = new ObservableList<ControlProperty>();
        GenericWindow _GenWin;
        IWindowExplorer mWinExplorer;

        public PomAllElementsPage.eElementsContext mContext;

        public PomElementsMappingPage(ApplicationPOMModel POM, PomAllElementsPage.eElementsContext context, IWindowExplorer winExplorer)
        {
            InitializeComponent();
            mPOM = POM;
            mWinExplorer = winExplorer;
            mContext = context;


            SetControlsGridView();

            if(mContext == PomAllElementsPage.eElementsContext.Mapped) 
                xMainElementsGrid.DataSourceList = mPOM.MappedUIElements;
            else
                xMainElementsGrid.DataSourceList = mPOM.UnMappedUIElements;

            InitControlPropertiesGridView();
            InitLocatorsGrid();

        }




        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToAddList = xMainElementsGrid.grdMain.SelectedItems.Cast<ElementInfo>().ToList();

            foreach (ElementInfo EI in ItemsToAddList)
            {
                mPOM.MappedUIElements.Add(EI);
                mPOM.UnMappedUIElements.Remove(EI);
            }
        }

        private void RemoveButtonClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToRemoveList = xMainElementsGrid.grdMain.SelectedItems.Cast<ElementInfo>().ToList();

            foreach (ElementInfo EI in ItemsToRemoveList)
            {
                mPOM.MappedUIElements.Remove(EI);
                mPOM.UnMappedUIElements.Add(EI);
            }
        }



        private void SetControlsGridView()
        {

            xMainElementsGrid.SetTitleLightStyle = true;
            

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTitle), Header = "Element Title", WidthWeight = 100, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), Header = "Description", WidthWeight = 100, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementType), Header = "Element Type", WidthWeight = 60, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "View Details", WidthWeight = 8, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["HighlightButton"] });

            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
            {
                xMainElementsGrid.AddToolbarTool("@Import_16x16.png", "Remove Items from mapped list", new RoutedEventHandler(RemoveButtonClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddMappedElementRow));
            }
            else
            {
                xMainElementsGrid.AddToolbarTool("@Import_16x16.png", "Add Items to mapped list", new RoutedEventHandler(AddButtonClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddUnMappedElementRow));
            }

            //xMainElementsGrid.AddToolbarTool("@Spy_24x24.png", "Live Spy- Hover with the mouse over the Element you want to spy and Click/Hold Down 'Ctrl' Key", new RoutedEventHandler(LiveSpyHandler));


            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
        }

        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;

        private void LiveSpyHandler(object sender, RoutedEventArgs e)
        {
            if (LiveSpyButton.IsChecked == true)
            {

                if (dispatcherTimer == null)
                {
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(timenow);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }

                dispatcherTimer.IsEnabled = true;
            }
            else
            {
                dispatcherTimer.IsEnabled = false;
            }
        }

        ElementInfo mSpyElement;

        private void timenow(object sender, EventArgs e)
        {
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                xStatusLable.Content = "Spying Element, Please Wait...";
                GingerCore.General.DoEvents();
                mSpyElement = mWinExplorer.GetControlFromMousePosition();
                if (mSpyElement != null)
                {
                    xStatusLable.Content = mSpyElement.XPath;
                    FocusSpyItemOnElementsGrid();
                    mWinExplorer.HighLightElement(mSpyElement);
                }
                else
                {
                    xStatusLable.Content = "Failed to spy element.";
                    GingerCore.General.DoEvents();
                }
            }
        }

        private void FocusSpyItemOnElementsGrid()
        {
            if (mSpyElement == null) return;
            foreach (ElementInfo EI in mPOM.MappedUIElements)
            {
                mWinExplorer.UpdateElementInfoFields(EI);//Not sure if needed

                if (EI.XPath == mSpyElement.XPath && EI.Path == mSpyElement.Path)
                {
                    mPOM.MappedUIElements.CurrentItem = EI;
                    xMainElementsGrid.ScrollToViewCurrentItem();
                    break;
                }
            }
        }


        private void AddMappedElementRow(object sender, RoutedEventArgs e)
        {
            mPOM.MappedUIElements.Add(new ElementInfo());
        }
        private void AddUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            mPOM.UnMappedUIElements.Add(new ElementInfo());
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string Title = "Mappaed Elements Page";

            GingerCore.General.LoadGenericWindow(ref _GenWin, null, windowStyle, Title, this);
        }

        private void InitLocatorsGrid()
        {
            //TODO: need to add Help text or convert to icon...
            //xLocatorsGrid.AddButton("Test", TestSelectedLocator);
            //xLocatorsGrid.AddButton("Test All", TestAllLocators);

            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            //view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });

            List<GingerCore.General.ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList });

            //defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), WidthWeight = 30 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 20 ,ReadOnly = true});
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 8, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["TestElementButton"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.TestStatus), WidthWeight = 20 });


            xLocatorsGrid.AddToolbarTool("@Play_16x16.png", "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
            xLocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocatorHandler));
            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();
            xLocatorsGrid.DataSourceList = mLocators;
            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void AddLocatorHandler(object sender, RoutedEventArgs e)
        {
            mLocators.Add(new ElementLocator());
        }


        private void InitControlPropertiesGridView()
        {
            // Grid View
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 20, ReadOnly = true });

            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
            xPropertiesGrid.SetTitleLightStyle = true;
            xPropertiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPropertyHandler));
            xPropertiesGrid.DataSourceList = mProperties;
        }

        private void AddPropertyHandler(object sender, RoutedEventArgs e)
        {
            mProperties.Add(new ControlProperty());
        }




        //private void TestSelectedLocator(object sender, RoutedEventArgs e)
        //{
        //    //ElementLocator EL = (ElementLocator)mLocators.CurrentItem;
        //    //ObservableList<ElementInfo> list = mWindowExplorerDriver.GetElements(EL);
        //    //EL.Count = list.Count;
        //}


        //private void TestAllLocators(object sender, RoutedEventArgs e)
        //{
        //    //foreach (ElementLocator EL in mLocators)
        //    //{
        //    //    ObservableList<ElementInfo> list = mWindowExplorerDriver.GetElements(EL);
        //    //    EL.Count = list.Count;
        //    //}
        //}
        bool IsFirstSelection = true;

        private void MappedElementsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            if (IsFirstSelection)
            {
                xDetailsExpander.IsExpanded = true;
                IsFirstSelection = false;
            }

            
            mLocators.Clear();
            mProperties.Clear();
            if (((DataGrid)sender).SelectedItem != null)
            {
                ElementInfo SelectedElement = (ElementInfo)((DataGrid)sender).SelectedItem;
                xDetailsExpanderLabel.Content = SelectedElement.ElementTitle + " Details";
                foreach (ElementLocator EL in SelectedElement.Locators)
                    mLocators.Add(EL);


                //ObservableList<ControlProperty> ElementsProperties = SelectedElement.GetElementProperties();
                ObservableList<ControlProperty> ElementsProperties = mWinExplorer.GetElementProperties(SelectedElement);
                foreach (ControlProperty CP in ElementsProperties)
                    mProperties.Add(CP);

            }




        }

        private ElementInfo mCurrentItem { get { return (ElementInfo)xMainElementsGrid.CurrentItem; } }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            mWinExplorer.HighLightElement(mCurrentItem);
        }

        private void DetailsGrid_Expanded(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(100, GridUnitType.Star);
        }

        private void DetailsGrid_Collapsed(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(35);
        }

        private void TestElementButtonClicked(object sender, RoutedEventArgs e)
        {
            
        }

        private void TestAllElementsLocators(object sender, RoutedEventArgs e)
        {
            
        }
    }
}