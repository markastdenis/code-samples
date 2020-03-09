using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using DynamicParameterLibrary.Actions;
using DynamicParameterLibrary.Data;
using DynamicParameterLibrary.Extras;

namespace DynamicParameterLibrary.Security
{
    public class RunAuditActionItem
    {
        // * the function of this class is to audit user-initiated executions and save the data to the database. 
        // * data stored includes username, what was executed, when, and what values were executed on. 
        // * ConnectionManager contains code that establishes and manages connections to the relevant database and reporting servers.
        // * this class is instantiated prior to actionitem execution, writes to the database just prior to execution, and updates a success flag 
        //      upon successful execution. 

        #region PRIVATE MEMBERS

        private int actionItemRunID;
        private int actionItemID;
        private string actionItemName;
        private string actionItemQuery;
        private int actionTypeID;
        private string userName;
        private bool successful = false;
        private List<RunAuditActionItemParameter> parameterList = new List<RunAuditActionItemParameter>();
        private ConnectionManager conman;

        #endregion PRIVATE MEMBERS

        #region CONTRUCTORS

        public RunAuditActionItem(ActionItem ai, ConnectionManager ConnManager)
        {
            this.actionItemID = int.Parse(ai.ActionItemID); // ai.ActionItemID is a string, so must be converted. 
            this.actionItemName = ai.ActionItemName;
            this.actionItemQuery = ai.ProcedureOriginal;
            this.actionTypeID = ai.ActionTypeID;
            this.userName = ai.CurrentUserName;
            this.conman = ConnManager;
            foreach (DynamicParameter dp in ai.DynamicParameterList)
            {
                RunAuditActionItemParameter parm = new RunAuditActionItemParameter();
                parm.DataTypeID = dp.DataTypeID;
                parm.DynamicParameterID = dp.DynamicParameterID;
                parm.ParameterName = dp.ParameterName;
                parm.ParameterValue = dp.ParameterValue;
                this.parameterList.Add(parm);
            }
        }

        #endregion CONTRUCTORS

        #region PUBLIC PROPERTIES

        public int ActionItemRunID
        {
            get { return actionItemRunID; }
            set { if (actionItemRunID != value) actionItemRunID = value; }
        }
        public int ActionItemID
        {
            get { return actionItemID; }
            set { if (actionItemID != value) actionItemID = value; }
        }
        public string ActionItemName
        {
            get { return actionItemName; }
            set { if (actionItemName != value) actionItemName = value; }
        }
        public string ActionItemQuery
        {
            get { return actionItemQuery; }
            set { if (actionItemQuery != value) actionItemQuery = value; }
        }
        public int ActionTypeID
        {
            get { return actionTypeID; }
            set { if (actionTypeID != value) actionTypeID = value; }
        }

        public string UserName
        {
            get { return userName; }
            set { if (userName != value) userName = value; }
        }
        public bool Successful
        {
            get { return successful; }
            set { if (successful != value) successful = value; }
        }

        public List<RunAuditActionItemParameter> ParameterList
        {
            get { return parameterList; }
            set { if (parameterList != value) parameterList = value; }
        }

        #endregion PUBLIC PROPERTIES

        #region PUBLIC METHODS

        public void CreateRunAuditRecord()
        {
            try
            {
                // check to see if audit logging is turned on ... if not, exit
                bool auditON = Properties.Settings.Default.AuditOn;
                if (!auditON) return;
                if (!Properties.Settings.Default.AIDesignAuditOn && this.actionTypeID == (int)Enums.ActionTypes.AIDesign) return;
                DataLayer dl = new DataLayer((int)Enums.ActionTypes.AIDesign, conman);
                List<SqlParameter> parmList = new List<SqlParameter>();
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "ActionItemID";
                parm.SqlDbType = SqlDbType.Int;
                parm.Value = this.actionItemID;
                parmList.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "ActionItemName";
                parm.SqlDbType = SqlDbType.NVarChar;
                parm.Size = 50;
                parm.Value = this.actionItemName;
                parmList.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "ActionItemQuery";
                parm.SqlDbType = SqlDbType.NVarChar;
                parm.Size = -1;
                parm.Value = this.actionItemQuery;
                parmList.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "ActionTypeID";
                parm.SqlDbType = SqlDbType.Int;
                parm.Value = this.actionTypeID;
                parmList.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "UserName";
                parm.SqlDbType = SqlDbType.NVarChar;
                parm.Size = 50;
                parm.Value = this.userName;
                parmList.Add(parm);

                this.actionItemRunID = dl.ExecuteStoredProcedure_int("isp_DYN_CreateActionItemRun", parmList);

                foreach (RunAuditActionItemParameter aiparm in this.parameterList)
                {
                    parmList = new List<SqlParameter>();

                    parm = new SqlParameter();
                    parm.ParameterName = "ActionItemRunID";
                    parm.SqlDbType = SqlDbType.Int;
                    parm.Value = actionItemRunID;
                    parmList.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "DynamicParameterID";
                    parm.SqlDbType = SqlDbType.Int;
                    parm.Value = aiparm.DynamicParameterID;
                    parmList.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "ParameterName";
                    parm.SqlDbType = SqlDbType.NVarChar;
                    parm.Size = 50;
                    parm.Value = aiparm.ParameterName;
                    parmList.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "DataTypeID";
                    parm.SqlDbType = SqlDbType.Int;
                    parm.Value = aiparm.DataTypeID;
                    parmList.Add(parm);

                    parm = new SqlParameter();
                    parm.ParameterName = "ParameterValue";
                    parm.SqlDbType = SqlDbType.NVarChar;
                    parm.Size = -1;
                    parm.Value = aiparm.ParameterValue;
                    parmList.Add(parm);

                    dl.ExecuteStoredProcedure("isp_DYN_CreateActionItemRunParameter", parmList);
                }
            }
            catch
            {
                throw;
            }
        }

        public void UpdateRunAuditSuccess(bool success)
        {
            this.successful = success;
            DataLayer dl = new DataLayer((int)Enums.ActionTypes.AIDesign, conman);
            dl.UpdateActionItemRunAuditSuccess(this.actionItemRunID, success);
        }

        #endregion PUBLIC METHODS

    }
}
