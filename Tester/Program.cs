using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Custom.SM.Incident.ChangeStatus.UI;
using Microsoft.EnterpriseManagement;
using System.Windows.Forms;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            EnterpriseManagementConnectionSettings emgcs = new EnterpriseManagementConnectionSettings("ngrpscsm01.bosal.local");
            emgcs.Domain = "bosal";
            emgcs.UserName = "zferranti";
            System.Security.SecureString Password = new System.Security.SecureString();
            foreach(char c in "F5rr1nt9"){Password.AppendChar(c);}
            emgcs.Password = Password;
            EnterpriseManagementGroup emg = new EnterpriseManagementGroup(emgcs);
            AssignmentForm Form = new AssignmentForm();
            Form.emg = emg;
            Form.ShowDialog();
            
        }
    }
}
