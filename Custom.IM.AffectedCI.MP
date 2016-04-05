<?xml version="1.0" encoding="utf-8"?>
<ManagementPack xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" ContentReadable="true" SchemaVersion="1.1">
<Manifest>
    <Identity>
        <ID>Custom.IM.AffectedCI</ID>
        <Version>1.0.0.0</Version>
    </Identity>
    <Name>Custom.IM.AffectedCI</Name>
    <References>
        <Reference Alias="System">
            <ID>System.Library</ID> 
            <Version>7.5.8501.0</Version> 
            <PublicKeyToken>31bf3856ad364e35</PublicKeyToken> 
        </Reference>
        <Reference Alias="Console">
            <ID>Microsoft.EnterpriseManagement.ServiceManager.UI.Console</ID> 
            <Version>7.5.3079.0</Version> 
            <PublicKeyToken>31bf3856ad364e35</PublicKeyToken> 
        </Reference>
    </References>
</Manifest>
<Categories>
    <Category ID="Custom.IM.AffectedCI.Category" Value="Console!Microsoft.EnterpriseManagement.ServiceManager.ManagementPack">
        <ManagementPackName>Custom.IM.AffectedCI</ManagementPackName>
        <ManagementPackVersion>1.0.0.0</ManagementPackVersion>
    </Category>
</Categories>
<LanguagePacks>
    <LanguagePack ID="ENU" IsDefault="true">
        <DisplayStrings>
            <DisplayString ElementID="Custom.IM.AffectedCI">
                <Name>Custom.IM.AffectedCI</Name>
            </DisplayString>
        </DisplayStrings>
    </LanguagePack>
</LanguagePacks>
</ManagementPack>