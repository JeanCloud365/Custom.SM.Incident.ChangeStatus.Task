using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess;
using Microsoft.EnterpriseManagement.ConsoleFramework;
using System.Windows.Forms;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.ConnectorFramework;

namespace Custom.SM.Incident.ChangeStatus.UI
{
    class ConsoleTask : ConsoleCommand
    {
        // this function is executed when the user clicks the custom task defined in the accompaning management pack
        public override void ExecuteCommand(IList<Microsoft.EnterpriseManagement.ConsoleFramework.NavigationModelNodeBase> nodes, Microsoft.EnterpriseManagement.ConsoleFramework.NavigationModelNodeTask task, ICollection<string> parameters)
        {
            // get a management group instance using the sdk helper (FrameworkServices). This allows us to fetch and manipulate SCSM items
            IManagementGroupSession emg = FrameworkServices.GetService<IManagementGroupSession>();
            // Create a reference to the incident class and use it to fetch the selected / open incident on which the task was executed. Right now, multi-select is not supported because only the first item of the nodes (selected items) array is retrieved.
            ManagementPackClass IncidentClass = emg.ManagementGroup.EntityTypes.GetClass(new Guid("a604b942-4c7b-2fb2-28dc-61dc6f465c68"));
            EnterpriseManagementObject IncidentObject = emg.ManagementGroup.EntityObjects.GetObject<EnterpriseManagementObject>(new Guid(nodes[0]["$Id$"].ToString()), ObjectQueryOptions.Default);
            // Create an instance of the update-status form and populate its properties with the information we retrieved
            AssignmentForm form = new AssignmentForm();
            form.emg = emg.ManagementGroup;
            // we need to convert the tier-class representing the incident status to a guid-format used by the GUI
            ManagementPackEnumeration SelectedStatus = (ManagementPackEnumeration)IncidentObject[IncidentClass, "Status"].Value;
            form.StatusTier = SelectedStatus.Id;
            // show the GUI as a dialog (popup) and record the outcome of it (OK or cancel)
            DialogResult r = form.ShowDialog();
            // if the outcome is OK, we process the data collected in the GUI
            if (r == DialogResult.OK)
            {
                // we use the nes status GUID recorded in the GUI to assign the chosen status to the incident object
                IncidentObject[IncidentClass, "Status"].Value = emg.ManagementGroup.EntityTypes.GetEnumeration(form.StatusTier);
                // depending on whether the status was set to 'resolved' or not, we either log the entered comment in the GUI to an analyst comment or a resolution description
                if (form.StatusTier == new Guid("2b8830b6-59f0-f574-9c2a-f4b4682f1681"))
                {
                    // in case of a resolved-status, log the info in the resolution-fields of the incident
                    IncidentObject[IncidentClass, "ResolutionCategory"].Value = emg.ManagementGroup.EntityTypes.GetEnumeration(form.ResolutionTier);
                    IncidentObject[IncidentClass, "ResolutionDescription"].Value = form.Comment;


                }
                else
                {
                    // if a comment was filled in and the status is not set to resolved, create a new analyst comment
                    if (form.Comment.Length > 0)
                    {
                        ManagementPackClass CommentClass = emg.ManagementGroup.EntityTypes.GetClass(new Guid("f14b70f4-878c-c0e1-b5c1-06ca22d05d40"));
                        EnterpriseManagementObjectProjection CommentObject = new EnterpriseManagementObjectProjection(emg.ManagementGroup, CommentClass);
                        Guid Id = Guid.NewGuid();
                        CommentObject.Object[CommentClass, "Id"].Value = Id.ToString();
                        CommentObject.Object[CommentClass, "DisplayName"].Value = Id.ToString();
                        CommentObject.Object[CommentClass, "EnteredBy"].Value = Environment.UserDomainName + "\\" + Environment.UserName;
                        CommentObject.Object[CommentClass, "EnteredDate"].Value = DateTime.Now.ToUniversalTime();
                        CommentObject.Object[CommentClass, "Comment"].Value = form.Comment;
                        // we need to create the comment as a related object of the incident using a projection
                        EnterpriseManagementObjectProjection IncidentProjection = new EnterpriseManagementObjectProjection(IncidentObject);
                        ManagementPackRelationship CommentRelationship = emg.ManagementGroup.EntityTypes.GetRelationshipClass(new Guid("835a64cd-7d41-10eb-e5e4-365ea2efc2ea"));
                        IncidentProjection.Add(CommentObject, CommentRelationship.Target);
                        IncidentProjection.Commit();
                       
                    }



                }
                // after the processing has been completed, commit the data to the management group and refresh the form / grid in the SCSM console
                IncidentObject.Commit();
                this.RequestViewRefresh();




            }
            else { return; }
        }
    }

            





                
        
    
}
