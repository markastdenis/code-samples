using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using DynamicParameterLibrary.Actions;
using DynamicParameterLibrary.Data;
using DynamicParameterLibrary.Extras;

namespace DynamicParameterLibrary.Security
{
    public class ErrorActionItem
    {

        // * the function of this class is to write info from any errors encountered during execution of project code to the database. 
        // * ConnectionManager contains code that establishes and manages connections to the relevant database and reporting servers.
        // * fauxMessage and fauxStackTrace refer to error messages and stacktraces that are not system generated, but rather created by code. 

        #region PRIVATE MEMBERS

        private int actionItemRunID;
        private int actionItemID;
        private string userName;
        private string errorMessage;
        private string stackTrace;
        private DateTime errorDateTime;
        private ConnectionManager conman;

        #endregion PRIVATE MEMBERS

        #region CONSTRUCTORS 

        public ErrorActionItem(Exception ex, string username, ConnectionManager ConnManager)
        {
            this.userName = username;
            this.errorDateTime = DateTime.Now;
            this.errorMessage = ex.Message;
            this.stackTrace = ex.StackTrace;
            this.actionItemID = -1;
            this.actionItemRunID = -1;
            this.conman = ConnManager;
        }
        public ErrorActionItem(Exception ex, ActionItem ai, ConnectionManager ConnManager)
        {
            this.userName = ai.CurrentUserName;
            this.errorDateTime = DateTime.Now;
            this.errorMessage = ex.Message;
            this.stackTrace = ex.StackTrace;
            this.actionItemID = int.Parse(ai.ActionItemID);
            this.actionItemRunID = -1;
            this.conman = ConnManager;
            if (ai.ActionItemAudit != null)
            {
                this.actionItemRunID = ai.ActionItemAudit.ActionItemRunID;
            }
        }
        public ErrorActionItem(string fauxMessage, string fauxStackTrace, string username, ConnectionManager ConnManager)
        {
            this.userName = username;
            this.errorDateTime = DateTime.Now;
            this.errorMessage = fauxMessage;
            this.ErrorStackTrace = fauxStackTrace;
            this.actionItemID = -1;
            this.actionItemRunID = -1;
            this.conman = ConnManager;
        }
        public ErrorActionItem(string fauxMessage, string fauxStackTrace, ActionItem ai, ConnectionManager ConnManager)
        {
            this.userName = ai.CurrentUserName;
            this.errorDateTime = DateTime.Now;
            this.errorMessage = fauxMessage;
            this.ErrorStackTrace = fauxStackTrace;
            this.actionItemID = int.Parse(ai.ActionItemID);
            this.actionItemRunID = -1;
            this.conman = ConnManager;
            if (ai.ActionItemAudit != null)
            {
                this.actionItemRunID = ai.ActionItemAudit.ActionItemRunID;
            }
        }


        #endregion CONSTRUCTORS

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
        public string UserName
        {
            get { return userName; }
            set { if (userName != value) userName = value; }
        }
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { if (errorMessage != value) errorMessage = value; }
        }
        public string ErrorStackTrace
        {
            get { return stackTrace; }
            set { if (stackTrace != value) stackTrace = value; }
        }
        public DateTime ErrorDateTime
        {
            get { return errorDateTime; }
            set { if (errorDateTime != value) errorDateTime = value; }
        }

        #endregion PUBLIC PROPERTIES

        #region PUBLIC METHODS

        public void CreateActionItemRunErrorRecord()
        {
            try
            {
                // check to see if error logging is turned on ... if not, exit
                bool errorsON = Properties.Settings.Default.CollectErrors;
                if (!errorsON) return;
                DataLayer dl = new DataLayer((int)Enums.ActionTypes.AIDesign, conman);

                List<SqlParameter> parmList = new List<SqlParameter>();
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = "UserName";
                parm.SqlDbType = SqlDbType.NVarChar;
                parm.Size = 50;
                parm.Value = this.userName;
                parmList.Add(parm);

                if (actionItemRunID > 0)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "ActionItemRunID";
                    parm.SqlDbType = SqlDbType.Int;
                    parm.Value = this.actionItemRunID;
                    parmList.Add(parm);
                }
                if (actionItemID > 0)
                {
                    parm = new SqlParameter();
                    parm.ParameterName = "ActionItemID";
                    parm.SqlDbType = SqlDbType.Int;
                    parm.Value = this.actionItemID;
                    parmList.Add(parm);
                }
                parm = new SqlParameter();
                parm.ParameterName = "ErrorMessage";
                parm.SqlDbType = SqlDbType.NVarChar;
                parm.Size = -1;
                parm.Value = this.errorMessage;
                parmList.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "ErrorStackTrace";
                parm.SqlDbType = SqlDbType.NVarChar;
                parm.Size = -1;
                parm.Value = this.stackTrace;
                parmList.Add(parm);

                parm = new SqlParameter();
                parm.ParameterName = "ErrorDateTime";
                parm.SqlDbType = SqlDbType.DateTime;
                parm.Value = this.errorDateTime;
                parmList.Add(parm);

                dl.ExecuteStoredProcedure("isp_DYN_CreateActionItemRunError", parmList);
            }
            catch
            {
                throw;
            }
        }

        #endregion PUBLIC METHODS
    }
}
