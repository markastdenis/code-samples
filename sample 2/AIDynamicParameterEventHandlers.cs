using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using DynamicParameterLibrary.Actions;
using DynamicParameterLibrary.Extras;

namespace DynamicParameterLibrary.Management
{
    public class AIDynamicParameterEventHandlers
    {

        #region PRIVATE MEMBERS

        private Button goButton;
        private List<ActionItem> aiList;
        private ActionItem selectedActionItem;

        #endregion PRIVATE MEMBERS

        #region CONSTRUCTORS

        public AIDynamicParameterEventHandlers(Button GoButton, List<ActionItem> AIList)
        {
            this.goButton = GoButton;
            this.aiList = AIList;
        }


        #endregion CONSTRUCTORS

        #region PUBLIC PROPERTIES

        public string CurrentUserName
        {
            get
            {
                return aiList.First<ActionItem>().CurrentUserName;
            }
        }

        #endregion PUBLIC PROPERTIES

        #region PARAMETER EVENT HANDLERS

        public void dpComboBoxTextChanged(object sender, EventArgs e)
        {
            int actionItemID;
            DynamicParameter dp;
            ComboBox cbo = (ComboBox)sender;
            dp = (DynamicParameter)cbo.Tag;
            if (!dp.IsInitializing)
            {
                ListObject mylo;
                string indexItemName;
                string compareText = cbo.Text;
                if (cbo.SelectedIndex > -1)
                {
                    mylo = (ListObject)cbo.SelectedItem;
                    indexItemName = mylo.DisplayValue;
                }
                else
                {
                    indexItemName = "Sel Index is -1";
                }
                int compareIndex = cbo.FindString(compareText);
                if (compareText == string.Empty) compareIndex = -1;
                if (compareIndex == -1)
                {
                    dp.ParameterValue = string.Empty;
                    dp.SelectedQueryDataListObjects = new List<ListObject>();
                    cbo.ResetText();

                    //code change mar. 05 2018 below
                    this.dpListSelectionChanged(sender, null);
                }
                //////////////////////else // this is new code to fix matts issue.....hes such a combobox niggly
                //////////////////////{
                //////////////////////    ListObject lo = new ListObject();
                //////////////////////    string compareFullText = cbo.Text;
                //////////////////////    int compareFullIndex = cbo.FindStringExact(compareFullText);
                //////////////////////    if (compareFullText.Length > 0)
                //////////////////////    {
                //////////////////////        for (int i = 0; i <= cbo.Items.Count - 1; i++)
                //////////////////////        {
                //////////////////////            lo = (ListObject)cbo.Items[i];
                //////////////////////            if (lo.DisplayValue == compareFullText)
                //////////////////////            {
                //////////////////////                cbo.SelectedItem = lo;
                //////////////////////                cbo.SelectedText = lo.DisplayValue;
                //////////////////////            }
                //////////////////////        }
                //////////////////////        //cbo.SelectedIndex = compareFullIndex;
                //////////////////////    }
                //////////////////////}


                //this.textIndexIndexItemSelectedItem.AppendLine("ComboBoxText: " + compareText + ",  IndexofMatch: " + compareIndex.ToString() + ",  IndexItemName:  " + indexItemName);
                SetGoButtonStatus(GetSelectedActionItem(dp.ActionItemID));
            }
        }
        public void dpListSelectionChanged(object sender, EventArgs e)
        {
            switch (sender.GetType().ToString())
            {
                case "System.Windows.Forms.ComboBox":
                    ComboBox cbo = (ComboBox)sender;
                    DynamicParameter dp_cbo = (DynamicParameter)cbo.Tag;
                    if (!dp_cbo.IsInitializing)
                    {
                        if (cbo.SelectedIndex == -1)
                        {
                            // we need to flush parameter values (which sets to unfulfilled)
                            dp_cbo.ParameterValue = string.Empty;
                            dp_cbo.ParameterValueDisplayValue = string.Empty;
                            dp_cbo.SelectedQueryDataListObjects = new List<ListObject>();
                            cbo.Text = string.Empty;
                        }
                        else
                        {
                            ListObject mylo = (ListObject)cbo.SelectedItem;
                            dp_cbo.SelectedQueryDataListObjects = new List<ListObject>();
                            dp_cbo.SelectedQueryDataListObjects.Add(mylo);
                            if (dp_cbo.DataTypeID == (int)Enums.DataTypes.NVarchar)
                            {
                                dp_cbo.ParameterValue = mylo.DisplayValue;
                            }
                            else
                            {
                                dp_cbo.ParameterValue = mylo.IDValue.ToString();
                            }

                            dp_cbo.ParameterValueDisplayValue = mylo.DisplayValue.ToString();
                        }
                    }
                    if (dp_cbo.DependencyChildren.Count > 0)
                    {
                        dp_cbo.LoadParameterQueryDataForDependencyChildren();
                    }
                    SetGoButtonStatus(GetSelectedActionItem(dp_cbo.ActionItemID));
                    break;
                case "System.Windows.Forms.ListBox":
                    // need to deal with multiselect on this one
                    ListBox lb = (ListBox)sender;
                    DynamicParameter dp_lb = (DynamicParameter)lb.Tag;
                    if (!dp_lb.IsInitializing)
                    {
                        if (lb.SelectedIndex == -1)
                        {
                            dp_lb.ParameterValue = "";
                        }
                        else
                        {
                            if (dp_lb.IsMultiSelect)
                            {
                                dp_lb.SelectedQueryDataListObjects = new List<ListObject>();
                                foreach (object item in lb.SelectedItems)
                                {
                                    ListObject mylo = (ListObject)item;
                                    dp_lb.SelectedQueryDataListObjects.Add(mylo);
                                }
                                //string paramList = string.Empty;
                                //string displayList = string.Empty;
                                LoadMultiSelectionParameterAndDisplayValues(dp_lb);
                                //dp_lb.ParameterValue = paramList;
                                ////if (dp_lb.UsesCurrentApplicationUserName) dp_lb.ParameterValue = displayList;
                                //dp_lb.ParameterValueDisplayValue = displayList;

                                //dp_lb.ParameterValue = this.GetIDListString(dp_lb.SelectedQueryDataListObjects);
                                //dp_lb.ParameterValueDisplayValue = this.GetSelectedItemsDisplayString(dp_lb.SelectedQueryDataListObjects);
                                //LoadMultiSelectionParameterValue(dp_lb.SelectedQueryDataListObjects, dp_lb.ParameterValue, dp_lb.ParameterValueDisplayValue);
                                //string myname = "";
                                //dp_lb.SelectedQueryDataListObjects.Add((ListObject)lb.SelectedItem);
                            }
                            else
                            {
                                ListObject mylo = (ListObject)lb.SelectedItem;
                                if (dp_lb.DataTypeID == (int)Enums.DataTypes.Int)
                                {
                                    dp_lb.ParameterValue = mylo.IDValue.ToString();
                                }
                                else
                                {
                                    dp_lb.ParameterValue = mylo.DisplayValue.ToString();
                                }
                                dp_lb.ParameterValueDisplayValue = mylo.DisplayValue.ToString();
                            }
                        }
                    }
                    SetGoButtonStatus(GetSelectedActionItem(dp_lb.ActionItemID));
                    break;
                default:
                    break;

            }
            //////////////////////////////////SetGoButtonStatus();
        }
        public void dpComboBoxOpened(object sender, EventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            //cbo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            cbo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None; ;
        }
        public void dbComboBoxClosed(object sender, EventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            //cbo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            cbo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        }
        public void dbComboBoxLostFocus(object sender, EventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            string compareFullText = cbo.Text;
            int compareFullIndex = cbo.FindStringExact(compareFullText);
            if (compareFullIndex > -1 && cbo.SelectedIndex == -1) cbo.SelectedIndex = compareFullIndex;
        }
        private void LoadMultiSelectionParameterAndDisplayValues(DynamicParameter dp)
        {

            StringBuilder sbParameterlist = new StringBuilder();
            StringBuilder sbDisplayList = new StringBuilder();
            List<ListObject> loList = dp.SelectedQueryDataListObjects;
            if (dp.MultiSelectionDisplayListBox != null)  dp.MultiSelectionDisplayListBox.Items.Clear();

            for (int i = 0; i <= loList.Count - 1; i++)
            {
                ListObject lo = loList.ElementAt(i);
                // this might be where i actually want to add the code to load display control..seems like more appropriate
                if (dp.MultiSelectionDisplayListBox != null) dp.MultiSelectionDisplayListBox.Items.Add(lo);

                if (i < loList.Count - 1)
                {
                    switch (dp.DataTypeID)
                    {
                        case ((int)Enums.DataTypes.Int):
                            sbParameterlist.Append(lo.IDValue.ToString() + ",");
                            break;
                        //case ((int)Enums.DataTypes.NVarchar):
                        //    sbParameterlist.Append("'" + lo.DisplayValue + "',");
                        //    break;
                        //case ((int)Enums.DataTypes.DateTime):
                        //    sbParameterlist.Append("'" + lo.DisplayValue + "',");
                        //    break;

                        default:
                            sbParameterlist.Append(lo.DisplayValue + ",");
                            break;
                    }
                    sbDisplayList.Append(lo.DisplayValue + ", ");
                    //if (dp.DataTypeID == (int)Enums.DataTypes.Int)
                    //{
                    //    sbParameterlist.Append(lo.IDValue.ToString() + ", ");
                    //}
                    //else
                    //sbIDlist.Append(lo.IDValue.ToString() + ",");
                }
                else
                {
                    switch (dp.DataTypeID)
                    {
                        case ((int)Enums.DataTypes.Int):
                            sbParameterlist.Append(lo.IDValue.ToString());
                            break;
                        //case ((int)Enums.DataTypes.NVarchar):
                        //    sbParameterlist.Append("'" + lo.DisplayValue + "'");
                        //    break;
                        //case ((int)Enums.DataTypes.DateTime):
                        //    sbParameterlist.Append("'" + lo.DisplayValue + "'");
                        //    break;

                        default:
                            sbParameterlist.Append(lo.DisplayValue);
                            break;
                    }
                    sbDisplayList.Append(lo.DisplayValue);
                }
            }
            dp.ParameterValue = sbParameterlist.ToString();
            dp.ParameterValueDisplayValue = sbDisplayList.ToString();
        }
        private string GetIDListString(List<ListObject> loList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= loList.Count - 1; i++)
            {
                ListObject lo = loList.ElementAt(i);
                if (i < loList.Count - 1)
                {
                    sb.Append(lo.IDValue.ToString() + ",");
                }
                else
                {
                    sb.Append(lo.IDValue.ToString());
                }
            }
            return sb.ToString();
        }
        private string GetSelectedItemsDisplayString(List<ListObject> loList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= loList.Count - 1; i++)
            {
                ListObject lo = loList.ElementAt(i);
                if (i < loList.Count - 1)
                {
                    sb.Append(lo.DisplayValue + ", ");
                }
                else
                {
                    sb.Append(lo.DisplayValue);
                }
            }
            return sb.ToString();
        }
        public void dpCheckedChanged(object sender, EventArgs e)
        {
            switch (sender.GetType().ToString())
            {
                case "System.Windows.Forms.CheckBox":
                    CheckBox cb = (CheckBox)sender;
                    DynamicParameter dp_cb = (DynamicParameter)cb.Tag;
                    if (!dp_cb.IsInitializing) dp_cb.ParameterValue = cb.Checked.ToString();
                    SetGoButtonStatus(GetSelectedActionItem(dp_cb.ActionItemID));
                    break;
                case "System.Windows.Forms.RadioButton":
                    RadioButton rb = (RadioButton)sender;
                    DynamicParameter dp_rb = (DynamicParameter)rb.Tag;
                    if (!dp_rb.IsInitializing) dp_rb.ParameterValue = rb.Checked.ToString();
                    SetGoButtonStatus(GetSelectedActionItem(dp_rb.ActionItemID));
                    break;
                default:
                    break;
            }
        }
        public void dpDTPickerValueChanged(object sender, EventArgs e)
        {
            DateTimePicker dtp = (DateTimePicker)sender;
            DynamicParameter dp_dtp = (DynamicParameter)dtp.Tag;
            if (!dp_dtp.IsInitializing) dp_dtp.ParameterValue = dtp.Value.ToString();
            SetGoButtonStatus(GetSelectedActionItem(dp_dtp.ActionItemID));
        }
        public void dpNumericUpDownValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            DynamicParameter dp_nud = (DynamicParameter)nud.Tag;
            if (!dp_nud.IsInitializing) dp_nud.ParameterValue = nud.Value.ToString();
            SetGoButtonStatus(GetSelectedActionItem(dp_nud.ActionItemID));
        }
        public void dpTextBoxTextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            DynamicParameter dp_tb = (DynamicParameter)tb.Tag;
            if (!dp_tb.IsInitializing)
            {
                dp_tb.ParameterValue = tb.Text;
            }
            SetGoButtonStatus(GetSelectedActionItem(dp_tb.ActionItemID));
        }
        public void dpControlGotFocus(object sender, EventArgs e)
        {
            switch (sender.GetType().ToString())
            {
                case "System.Windows.Forms.NumericUpDown":
                    NumericUpDown nud = (NumericUpDown)sender;
                    nud.Select(0, 15);
                    break;
                case "System.Windows.Forms.TextBox":
                    TextBox tb = (TextBox)sender;
                    //DynamicParameter dp_tb = (DynamicParameter)tb.Tag;
                    tb.SelectAll();
                    //if (!dp_rb.IsInitializing) dp_rb.ParameterValue = rb.Checked.ToString();
                    break;
                default:
                    break;
            }
        }
        private void SetGoButtonStatus(ActionItem ai)
        {
            bool isVisible = true;
            foreach (DynamicParameter dp in ai.DynamicParameterList)
            {
                if (!dp.IsFulfilled) isVisible = false;
            }
            goButton.Visible = isVisible;
        }
        private ActionItem GetSelectedActionItem(string actionitemID)
        {
            ActionItem AI = aiList.Find(item => item.ActionItemID == actionitemID);
            return AI;
        }

        #endregion  PARAMETER EVENT HANDLERS

    }
}
