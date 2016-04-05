get-module|Remove-Module -force
Set-PSDebug -Strict
[threading.thread]::CurrentThread.CurrentCulture = 'en-US'
$Global:Log = ""
function Log($Message){
    if($Global:Log -eq ""){
        New-EventLog -LogName 'Operations Manager' -Source 'Custom' -ErrorAction SilentlyContinue
        $Global:Log = "$Message`r`n"
    } else {
        $Global:Log += "$Message`r`n"
    } 
    Write-Output $Message
}
try{
    Import-Module smlets -Force -Global
    Log 'Office To Assignment Group Workflow (SR) Started'
    
    $GUID = '734f814c-464a-5936-9184-51316f40fdeb'
    Log "Service Request GUID: $GUID"
    $Ticket = Get-SCSMObject -Id $GUID
    Log "Service Request ID: $($Ticket.Name)"
    $UserClass = Get-SCSMClass -Name 'System.Domain.User'

    $Relationship = Get-SCSMRelationshipClass -Name 'System.WorkItemAffectedUser'

    $AffectedUser = Get-SCSMRelatedObject -SMObject $Ticket -Relationship $Relationship
    if($AffectedUser -ne $null){
        Log "Affected User: $($AffectedUser.DisplayName) with Office '$($AffectedUser.Office)'"
        $ConfigList = Get-SCSMTopLevelEnumeration -Name 'Custom.SM.OfficeToAssignmentGroup.MP.ENUM_Settings'
        $Items = Get-SCSMChildEnumeration -Depth 0 -Enumeration $ConfigList
        $Item = $null
        $Item = $Items|?{$_.DisplayName -eq $AffectedUser.Office}
        if($Item -ne $null){
            $SubItems = Get-SCSMChildEnumeration -Depth OneLevel -Enumeration $Item
            $TierList = Get-SCSMTopLevelEnumeration -Name 'ServiceRequestSupportGroupEnum'
            $TierItems = Get-SCSMChildEnumeration -Depth Recursive -Enumeration $TierList
            $Target = $null
            foreach($SubItem in $SubItems){
     
                if($Target -eq $null){
    
                    $Target = $TierItems|?{$_.DisplayName -eq $SubItem.DisplayName}
                }
            }
    
            if($Target -ne $null){
                Log "Mapping found: $($Item.DisplayName) --> $($Target.DisplayName)"

                Set-SCSMObject -SMObject $Ticket -PropertyHashtable @{'SupportGroup'=$Target.DisplayName}
            } else {
                Log "No mapping found"
            }
        } else { Log "No mapping found" }
    } else { Log "No affected user found"}
} catch {
    Log "Error: $_"
} finally {
    Log "Workflow ended"
    Write-EventLog -LogName 'Operations Manager' -Source 'Custom' -EntryType Information -Message $Global:Log -EventId 0

    Get-Module|Remove-Module -Force
    
}