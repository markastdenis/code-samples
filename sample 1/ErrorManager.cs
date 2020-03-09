using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using DynamicParameterLibrary.Actions;
using DynamicParameterLibrary.Data;

namespace DynamicParameterLibrary.Security
{
    public static class ErrorManager
    {

        // * this class is used exclusively throughout the project code whenever errors occur, and its function is to create an erroractionitem 
        //      and store the error info to the database. 
        // * ConnectionManager contains code that establishes and manages connections to the relevant database and reporting servers.

        public static void WriteErrorRecord(Exception ex, ActionItem ai, ConnectionManager ConnManager)
        {
            ErrorActionItem err = new ErrorActionItem(ex, ai, ConnManager);
            err.CreateActionItemRunErrorRecord();
        }

        public static void WriteErrorRecord(Exception ex, string username, ConnectionManager ConnManager)
        {
            ErrorActionItem err = new ErrorActionItem(ex, username, ConnManager);
            err.CreateActionItemRunErrorRecord();
        }

        public static void WriteErrorRecord(string errorMessage, string stackTrace, ActionItem ai, ConnectionManager ConnManager)
        {
            ErrorActionItem err = new ErrorActionItem(errorMessage, stackTrace, ai, ConnManager);
            err.CreateActionItemRunErrorRecord();
        }

        public static void WriteErrorRecord(string errorMessage, string stackTrace, string username, ConnectionManager ConnManager)
        {
            ErrorActionItem err = new ErrorActionItem(errorMessage, stackTrace, username, ConnManager);
            err.CreateActionItemRunErrorRecord();
        }

    }
}
