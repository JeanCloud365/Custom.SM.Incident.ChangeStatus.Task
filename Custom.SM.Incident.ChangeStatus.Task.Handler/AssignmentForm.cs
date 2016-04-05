using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Common;

namespace Custom.SM.Incident.ChangeStatus.UI
{
    public partial class AssignmentForm : Form
    {
        // these variables are used to exchange information between the SCSM task handler and the GUI form
        public EnterpriseManagementGroup emg = null;
        public Guid StatusTier = Guid.Empty;
        public Guid ResolutionTier = Guid.Empty;
        public string Comment;
        // CanClose is a work-around feature to prevent the GUI from closing too quickly
        private bool CanClose = false;
       
       
       // We use a tier-struct as a form of mini-class to store guid / displayname information for SCSM enumerations
        private struct TierStruct  {
            public string Name;
            public Guid Id;
            public override string ToString()
            {
                return Name;
            }
        }
        private IList<TierStruct> StatusTierStructs;
        private IList<TierStruct> ResolutionTierStructs;


        public AssignmentForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
            
        // we use the static id's of the incident status and resolution enumerations to recursively (using the PopulateChildren helper function) populate the entire structure of the enum in our structs  
            StatusTierStructs = new List<TierStruct>();
            ResolutionTierStructs = new List<TierStruct>();
        
            string IncidentStatusTier = "89b34802-671e-e422-5e38-7dae9a413ef8";
            string IncidentResolutionTier = "72674491-02cb-1d90-a48f-1b269eb83602";
            ManagementPackEnumerationCriteria EnumCrit = new ManagementPackEnumerationCriteria("Parent = '" + IncidentStatusTier + "'");
            IList<ManagementPackEnumeration> StatusEnums = emg.EntityTypes.GetEnumerations(EnumCrit);
            EnumCrit = new ManagementPackEnumerationCriteria("Parent = '" + IncidentResolutionTier + "'");
            IList<ManagementPackEnumeration> ResolutionEnums = emg.EntityTypes.GetEnumerations(EnumCrit);

            StatusEnums = StatusEnums.OrderBy(o => o.Ordinal).ToList();
            foreach (ManagementPackEnumeration Enum in StatusEnums)
            {
                TierStruct Item = new TierStruct();
                Item.Name = Enum.DisplayName;
                Item.Id = Enum.Id;
                StatusTierStructs.Add(Item);
                PopulateChildren(Item.Id.ToString(), Item.Name + " - ",ref StatusTierStructs);


            }

            ResolutionEnums = ResolutionEnums.OrderBy(o => o.Ordinal).ToList();
            foreach (ManagementPackEnumeration Enum in ResolutionEnums)
            {
                TierStruct Item = new TierStruct();
                Item.Name = Enum.DisplayName;
                Item.Id = Enum.Id;
                ResolutionTierStructs.Add(Item);
                PopulateChildren(Item.Id.ToString(), Item.Name + " - ",ref ResolutionTierStructs);


            }





           
           
          // compare the current status and resolution tier state provided by the task handler to set the selected items correctly in the combo-boxes
            foreach (TierStruct Item in StatusTierStructs)
            {
                cmbStatus.Items.Add(Item);
                if (StatusTier != Guid.Empty && Item.Id == StatusTier)
                {
                    cmbStatus.SelectedItem = Item;
                }
            }

            foreach (TierStruct Item in ResolutionTierStructs)
            {
                cmbSolution.Items.Add(Item);
                if (ResolutionTier != Guid.Empty && Item.Id == ResolutionTier)
                {
                    cmbSolution.SelectedItem = Item;
                }
            }










        }
        // small helper function that enables the recursive population of an entire enumeration
         private bool PopulateChildren(string TierId,string ParentLabel,ref IList<TierStruct> TierList){
            
            ManagementPackEnumerationCriteria EnumCrit = new ManagementPackEnumerationCriteria("Parent = '" + TierId + "'");
            
            IList<ManagementPackEnumeration> Enums = emg.EntityTypes.GetEnumerations(EnumCrit);
            if (Enums.Count > 0)
            {
                Enums = Enums.OrderBy(o => o.Ordinal).ToList();
                foreach (ManagementPackEnumeration Enum in Enums)
                {
                    TierStruct Item = new TierStruct();
                    Item.Name = ParentLabel + Enum.DisplayName;
                    Item.Id = Enum.Id;
                    TierList.Add(Item);
                    PopulateChildren(Item.Id.ToString(), Item.Name + " - ",ref TierList);


                }
                return true;
            }
            else { return false; };
}
        // this function fires when the user clicks the OK-button: it validates whether a resolution comment and status have been set when the status is set to resolved. If not, an error-popup shows prompting the analyst to correct the issue.
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (cmbStatus.SelectedItem != null && cmbStatus.SelectedItem.ToString().Length != 0)
            {
                if (cmbStatus.SelectedItem.ToString().EndsWith("Resolved"))
                {
                    if (cmbSolution.SelectedItem == null || txtComment.Text.Length == 0)
                    {
                        MessageBox.Show(this,"Please choose a resolution category and fill in a solution comment", "Cannot resolve ticket", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CanClose = false;
                        
                    } else
                    {
                        if (cmbSolution.SelectedItem != null)
                        {
                            ResolutionTier = ((TierStruct)cmbSolution.SelectedItem).Id;
                        }
                        
                        CanClose = true;
                    }
                }
                else {

                   
                    if (cmbSolution.SelectedItem != null)
                    {
                        ResolutionTier = ((TierStruct)cmbSolution.SelectedItem).Id;
                    }
                    
                    CanClose = true;
                    
                }
            } else
            {
                MessageBox.Show(this,"Please choose a ticket status", "Cannot update ticket", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CanClose = false;
                return;
                
            }
            // all selected data is stored in the form's variabled. These variables are used by the task-handler to correctly update the selected incident
            StatusTier = ((TierStruct)cmbStatus.SelectedItem).Id;
            Comment = txtComment.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // if the analyst cancels the dialog, we do nothing
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            CanClose = true;
            this.Close();
        }

        // if the user chooses 'resolved' in the status-combobox in the GUI, we show an additional combo-box where a resolution category must be chosen
        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStatus.SelectedItem.ToString().EndsWith("Resolved"))
            {
                cmbSolution.Visible = true;
                label3.Visible = true;
            } else
            {
                cmbSolution.Visible = false;
                label3.Visible = false;
            }

        }

        // this is a workaround for a mysterious 'form validates and closes unexpectedly' issue. We check wether the code has signalled that the form can actually be closed or not.
        private void AssignmentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CanClose && this.DialogResult != DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }

   

}

