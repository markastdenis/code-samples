using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

using System.Data.SqlClient;
using System.IO;
using DynamicParameterLibrary.Actions;
using DynamicParameterLibrary.Extras;
using DynamicParameterLibrary.Data;
using DynamicParameterLibrary.Security;
using DynamicParameterLibrary.Execution;

namespace DynamicParameterLibrary.Management
{
    public class ActionItemsManager
    {

        #region PRIVATE MEMBERS


        private ListBox actionItemsListBox;
        private List<ActionItem> actionItems = new List<ActionItem>();
        private ActionItem selectedActionItem;
        private int actionTypeID;
        private Form baseForm;
        private Panel templatePanel;
        private GroupBox templateGroupBox;
        private bool metaDataLoaded = false;
        private Button goButton;
        private string currentUserName;
        private AIDynamicParameterEventHandlers dpControlEventHanders;
        private TabPage tabPage;
        private ConnectionManager conman;

        // testing
        private StringBuilder textIndexIndexItemSelectedItem = new StringBuilder();

        #endregion PRIVATE MEMBERS

        #region CONSTRUCTORS

        public ActionItemsManager(int ActionTypeID, Form frm, Panel pnl, GroupBox gb, ListBox lb, Button goButton, string currentUserName, ConnectionManager ConnManager)
        {
            actionTypeID = ActionTypeID;
            baseForm = frm;
            templatePanel = pnl;
            templateGroupBox = gb;
            this.currentUserName = currentUserName;
            this.actionItemsListBox = lb;
            this.conman = ConnManager;
            this.goButton = goButton;
            this.goButton.Visible = false;
            this.goButton.Click += new System.EventHandler(this.GoButtonClick);
        }

        public ActionItemsManager(int ActionTypeID, Form frm, TabPage tab, Panel pnl, GroupBox gb, ListBox lb, Button goButton, string currentUserName, ConnectionManager ConnManager)
        {
            actionTypeID = ActionTypeID;
            baseForm = frm;
            templatePanel = pnl;
            templateGroupBox = gb;
            this.currentUserName = currentUserName;
            this.actionItemsListBox = lb;
            this.conman = ConnManager;
            this.goButton = goButton;
            this.goButton.Visible = false;
            this.tabPage = tab;
            this.goButton.Click += new System.EventHandler(this.GoButtonClick);
        }

        #endregion CONSTRUCTORS

        #region PUBLIC PROPERTIES

        public ActionItem SelectedActionItem
        {
            get { return selectedActionItem; }
            set { if (selectedActionItem != value) selectedActionItem = value; }
        }

        public int ActionItemCount
        {
            get { return actionItems.Count; }
        }


        #endregion PUBLIC PROPERTIES

        #region PUBLIC METHODS

        public static List<ListObject> GetActionTypes(ConnectionManager ConnManager)
        {
            List<ListObject> list = new List<ListObject>();
            DataSet ds = new DataSet();
            DataLayer dl = new DataLayer((int)Enums.ActionTypes.AIDesign, ConnManager);
            ds = dl.GetActionTypes();
            for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];
                ListObject mylo = new ListObject();
                mylo.IDValue = (int)dr.ItemArray[0];
                mylo.DisplayValue = dr.ItemArray[1].ToString();
                list.Add(mylo);
            }
            return list;
        }

        public void LoadActionItemsFromDatabase()
        {
            try
            {
                DataLayer dl = new DataLayer((int)Enums.ActionTypes.AIDesign, conman);
                DataSet result = dl.LoadActionItemsFromDatabase(actionTypeID, currentUserName, conman.ApplicationDatabaseID);
                if (tabPage == null)
                {
                    actionItems = AIManagerMetadataLoader.LoadDBMetaDataResults(result.Tables[0], templatePanel, templateGroupBox, baseForm, actionTypeID, currentUserName, conman);
                }
                else
                {
                    actionItems = AIManagerMetadataLoader.LoadDBMetaDataResults(result.Tables[0], templatePanel, templateGroupBox, tabPage, actionTypeID, currentUserName,conman);
                }
                dpControlEventHanders = new AIDynamicParameterEventHandlers(goButton, actionItems);
                this.metaDataLoaded = true;
                this.actionItemsListBox.DataSource = actionItems;
                this.actionItemsListBox.Refresh();
                this.actionItemsListBox.DisplayMember = "ActionItemName";
                this.actionItemsListBox.Update();
                this.actionItemsListBox.SelectedIndex = -1;
                this.actionItemsListBox.SelectedIndexChanged -= this.lstActionItems_SelectedIndexChanged;
                this.actionItemsListBox.SelectedIndexChanged += this.lstActionItems_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                this.WriteErrorRecord(ex);
                MessageBox.Show("Error: " + ex.Message, "LoadActionItemsFromDatabase()");
            }
        }

        public void HideActionItemPanels()
        {
            foreach (ActionItem ai in actionItems) ai.ActionItemPanel.Visible = false;
        }


        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private void SetGoButtonStatus()
        {
            bool isVisible = true;
            foreach (DynamicParameter dp in this.selectedActionItem.DynamicParameterList)
            {
                if (!dp.IsFulfilled) isVisible = false;
            }
            goButton.Visible = isVisible;
        }

        private void WriteErrorRecord(Exception ex)
        {
            if (selectedActionItem != null)
            {
                ErrorManager.WriteErrorRecord(ex, selectedActionItem, conman);
            }
            else
            {
                ErrorManager.WriteErrorRecord(ex, currentUserName, conman);
            }
        }

        private void WriteErrorRecord(string message, string stacktrace)
        {
            if (selectedActionItem != null)
            {
                ErrorManager.WriteErrorRecord(message, stacktrace, selectedActionItem, conman);
            }
            else
            {
                ErrorManager.WriteErrorRecord(message, stacktrace, currentUserName, conman);
            }
        }

        #endregion PRIVATE METHODS

        #region BASE FORM EVENT HANDLING

        private void lstActionItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            
            try
            {
                if (lb.SelectedIndex == -1) return;
                if (selectedActionItem != null)
                {
                    selectedActionItem.IsInitializing = true;
                    selectedActionItem.InitializeAllParameters();
                    selectedActionItem.IsInitializing = false;
                }
                if (this.metaDataLoaded && lb.SelectedIndex > -1)
                {
                    selectedActionItem = (ActionItem)lb.SelectedItem;
                    Cursor.Current = Cursors.WaitCursor;
                    if (!selectedActionItem.IsParameterQueryDataLoaded)
                    {
                        selectedActionItem.LoadParameterQueryData();
                    }
                    if (!selectedActionItem.IsFullyLoadedAndDisplayed)
                    {
                        AIParameterProcessing.LoadActionItemParametersForDisplay(selectedActionItem, this.dpControlEventHanders);
                    }
                    else
                    {
                        SelectedActionItem.RefreshParameterQueryData();
                        selectedActionItem.InitializeAllParameters();
                    }
                    HideActionItemPanels();

                    selectedActionItem.ActionItemPanel.Visible = true;

                    LinkLabel ll;
                    if (selectedActionItem.ActionItemGroupBox.Controls["llAIInfo"] == null)
                    {
                        ll = new LinkLabel();
                        ll.Name = "llAIInfo";
                        ll.Text = "info";
                        ll.Visible = false;
                        ll.AutoSize = true;
                        selectedActionItem.ActionItemGroupBox.Controls.Add(ll);
                        ll.Dock = DockStyle.Right;
                        ll.Top = 0;
                        ll.Margin = new Padding(0);
                        ll.Click += new System.EventHandler(this.InfoLinkLabelClick);
                    }
                    else
                    {
                        ll = ll = (LinkLabel)selectedActionItem.ActionItemGroupBox.Controls["llAIInfo"];
                    }
                    if (selectedActionItem.ToolTipText.Length > 0)
                    {
                        ll.Tag = selectedActionItem.ToolTipText;
                        ll.Visible = true;
                    }
                    else
                    {
                        ll.Tag = null;
                        ll.Visible = false;
                    }
                }
                if (selectedActionItem.IsFullyLoadedAndDisplayed) SetGoButtonStatus();
            }
            catch (Exception ex)
            {
                this.WriteErrorRecord(ex);
                MessageBox.Show("Error: " + ex.Message, "lstActionItems_SelectedIndexChanged()");
                selectedActionItem = null;
                HideActionItemPanels();
                lb.SelectedItem = null;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                ExecutionManager.Execute(selectedActionItem, baseForm, conman);
                SetGoButtonStatus();
            }
            catch (Exception ex)
            {
                this.WriteErrorRecord(ex);
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                SetGoButtonStatus();
                Cursor.Current = Cursors.Default;
            }
        }

        private void InfoLinkLabelClick(object sender, EventArgs e)
        {
            // this method displays html info text in a small form containing a web browser
            Form frm = new Form();
            frm.Size = new Size(600, 400);
            frm.Padding = new Padding(5, 5, 5, 10);
            WebBrowser wb = new WebBrowser();
            wb.Margin = new Padding(10);
            frm.Controls.Add(wb);
            frm.BackColor = Color.White;
            frm.Text = selectedActionItem.ActionItemName + " - Information";
            wb.DocumentText = selectedActionItem.ToolTipText;
            wb.Dock = DockStyle.Fill;
            wb.Visible = true;
            frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            frm.ShowDialog();
        }


        #endregion BASE FORM EVENT HANDLING

    }
}
