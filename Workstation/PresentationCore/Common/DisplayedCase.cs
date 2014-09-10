using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using System.Windows.Data;

namespace L3.Cargo.Workstation.PresentationCore.Common
{
    public class DisplayedCase : IDisposable
    {
        #region Private Members

        private string m_CaseID;

        private bool m_IsCaseEditable;

        private bool m_IsCTICase;

        private bool m_IsFTICase;

        private bool m_IsTIPResultReturned;

        private bool m_IsPrimaryCase;

        private ContentInstances m_ContentInstances;

        private MainPanelInstances m_MainPanelInstances;

        private PanelLayout m_PanelLayout;

        private TabControl m_SecTabControl;

        private List<StatusBarItem> m_StatusBarItems;

        private TabControl m_Parent;

        private TabItem m_TabItem;

        private PrinterObjects m_PrinterObjects;

        #endregion Private Members


        #region Public Members

        public string CaseID
        {
            get
            {
                return m_CaseID;
            }
            set
            {
                m_CaseID = value;
            }
        }

        public bool IsCaseEditable
        {
            get
            {
                return m_IsCaseEditable;
            }

            set
            {
                m_IsCaseEditable = value;
            }
        }

        public bool IsCTICase
        {
            get
            {
                return m_IsCTICase;
            }

            set
            {
                m_IsCTICase = value;
            }
        }

        public bool IsFTICase
        {
            get
            {
                return m_IsFTICase;
            }

            set
            {
                m_IsFTICase = value;
            }
        }


        public bool IsTIPResultReturned
        {
            get
            {
                return m_IsTIPResultReturned;
            }

            set
            {
                m_IsTIPResultReturned = value;
            }
        }

        public bool IsPrimaryCase
        {
            get
            {
                return m_IsPrimaryCase;
            }
        }

        public ContentInstances ContentInstances
        {
            get
            {
                return m_ContentInstances;
            }

            set
            {
                m_ContentInstances = value;
                PopulatePanels();
            }
        }

        public MainPanelInstances mainPanelInstances
        {
            get
            {
                return m_MainPanelInstances;
            }

            set
            {
                m_MainPanelInstances = value;
                PopulateFrameworkMainPanel();
            }
        }

        public PanelLayout PanelLayout
        {
            get
            {
                return m_PanelLayout;
            }

            set
            {
                m_PanelLayout = value;
            }
        }

        public TabControl SecTabControl
        {
            get
            {
                return m_SecTabControl;
            }

            set
            {
                m_SecTabControl = value;
            }
        }

        public List<StatusBarItem> StatusBarItems
        {
            get
            {
                return m_StatusBarItems;
            }
        }

        public TabControl Parent
        {
            get
            {
                return m_Parent;
            }

            set
            {
                m_Parent = value;
            }
        }

        public TabItem CaseTabItem
        {
            get
            {
                return m_TabItem;
            }

            set
            {
                m_TabItem = value;
            }
        }

        public PrinterObjects PrinterObjects
        {
            get
            {
                return m_PrinterObjects;
            }
        }

        #endregion Public Members


        #region Constructors

        public DisplayedCase(string caseId, bool IsPrimaryCase)
        {
            m_CaseID = caseId;
            m_IsPrimaryCase = IsPrimaryCase;
            m_IsCaseEditable = IsPrimaryCase;
            m_IsCTICase = false;
            m_IsFTICase = false;
            m_IsTIPResultReturned = false;
            m_PanelLayout = new PanelLayout();
            m_SecTabControl = new TabControl();
            m_StatusBarItems = new List<StatusBarItem>();
            m_PrinterObjects = new PrinterObjects();
        }

        public DisplayedCase(string caseId, bool IsPrimaryCase, ContentInstances contentInstances, MainPanelInstances mainPanelInstances)
        {
            if (String.IsNullOrWhiteSpace(caseId))
            {
                throw new Exception(ErrorMessages.CASE_ID_INVALID);
            }

            m_CaseID = caseId;
            m_IsPrimaryCase = IsPrimaryCase;
            m_IsCaseEditable = IsPrimaryCase;
            m_IsCTICase = false;
            m_IsFTICase = false;
            m_IsTIPResultReturned = false;
            m_ContentInstances = contentInstances;
            m_MainPanelInstances = mainPanelInstances;
            m_PanelLayout = new PanelLayout();
            m_SecTabControl = new TabControl();
            m_StatusBarItems = new List<StatusBarItem>();
            m_PrinterObjects = new PrinterObjects();
        }

        public DisplayedCase(string caseId, bool IsPrimaryCase, ContentInstances contentInstances, 
            PanelLayout panelLayout, MainPanelInstances mainPanelInstances)
        {
            if (String.IsNullOrWhiteSpace(caseId))
            {
                throw new Exception(ErrorMessages.CASE_ID_INVALID);
            }

            m_CaseID = caseId;
            m_IsPrimaryCase = IsPrimaryCase;
            m_IsCaseEditable = IsPrimaryCase;
            m_IsCTICase = false;
            m_IsFTICase = false;
            m_IsTIPResultReturned = false;
            m_ContentInstances = contentInstances;
            m_MainPanelInstances = mainPanelInstances;
            m_PanelLayout = panelLayout;
            m_SecTabControl = new TabControl();
            m_StatusBarItems = new List<StatusBarItem>();
            m_PrinterObjects = new PrinterObjects();
        }

        #endregion Constructors


        #region Private Methods

        private void PopulateFrameworkMainPanel()
        {
            
        }

        private void PopulatePanels()
        {
            foreach (ContentInstance content in ContentInstances)
            {
                try
                {
                    foreach (LayoutInfo layoutInfo in content.Instance.UserControlDisplays)
                    {
                        TabControl tabControl;

                        switch (layoutInfo.Panel)
                        {
                            case PanelAssignment.MainPanel:
                                tabControl = m_PanelLayout.MainTabControl;
                                break;
                            case PanelAssignment.SubPanel:
                                tabControl = m_PanelLayout.SubTabControl;
                                break;
                            case PanelAssignment.InfoPanel:
                                tabControl = m_PanelLayout.InfoTabControl;
                                break;
                            case PanelAssignment.SecondaryPanel:
                                tabControl = m_SecTabControl;
                                break;
                            default:
                                tabControl = m_PanelLayout.MainTabControl;
                                break;
                        }

                        TabItem displayTabItem = new TabItem();
                        DockPanel tabPanel = new DockPanel();

                        // Bind the header text to the resource
                        var binding = new Binding(layoutInfo.Name);
                        binding.Source = CultureResources.getDataProvider();
                        BindingOperations.SetBinding(displayTabItem, TabItem.HeaderProperty, binding);

                        displayTabItem.Content = tabPanel;

                        tabPanel.Children.Insert(0, layoutInfo.Display);

                        if (!tabControl.Items.Contains(displayTabItem))
                        {
                            if (layoutInfo.BringToFront)
                            {
                                tabControl.Items.Insert(0, displayTabItem);
                            }
                            else
                            {
                                tabControl.Items.Add(displayTabItem);
                            }
                        }

                        if (layoutInfo.StatusItems != null)
                        {
                            foreach (StatusBarItem statusItem in layoutInfo.StatusItems)
                            {
                                if (!StatusBarItems.Contains(statusItem))
                                    StatusBarItems.Add(statusItem);
                            }
                        }
                    }
                    if (content.Instance.PrinterObject != null)
                    {
                        m_PrinterObjects.Add(content.Instance.PrinterObject);
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Log Message here.
                    //MessageBox.Show(ex.ToString());
                }
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void RemoveDecisionControl()
        {
            if (m_MainPanelInstances != null)
            {
                MainPanelInstance decisionInstance = m_MainPanelInstances.Find("Decision");
                if (decisionInstance != null)
                {
                    m_MainPanelInstances.Remove(decisionInstance);
                }
            }
        }

        public void Dispose()
        {
            if (this.Parent != null && this.CaseTabItem != null)
            {
                this.Parent.Items.Remove(this.CaseTabItem);
            }
            m_Parent = null;
            m_TabItem = null;

            m_PrinterObjects.Clear();

            if (m_SecTabControl != null)
            {
                m_SecTabControl.Items.Clear();
                m_SecTabControl = null;
            }

            m_PanelLayout.Dispose();

            m_ContentInstances.Clear();

            if(m_MainPanelInstances != null)
                m_MainPanelInstances.Clear();
        }

        #endregion Public Methods
    }
}
