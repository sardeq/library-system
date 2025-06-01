<%@ Page Title="Account Settings" Language="C#" AutoEventWireup="true" 
    CodeBehind="Account.aspx.cs" 
    Inherits="LibrarySystem_Main.General.Account"
    MasterPageFile="~/Site.Master"  Async="true"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="main-content">
        <h2>My Account</h2>
        
        <div class="card">
            <div class="card-body">
                <div class="form-row">
                    <div class="form-group col-md-6">
                        <asp:Literal runat="server" Text="<%$ Resources:Resources, Username%>" />
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                    <div class="form-group col-md-6">
                        <asp:Literal runat="server" Text="<%$ Resources:Resources, FullName%>" />
                        <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" />
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-group col-md-6">
                        <asp:Literal runat="server" Text="<%$ Resources:Resources, BirthDate%>" />
                        <asp:TextBox ID="txtBirthDate" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                    <div class="form-group col-md-6">
                        <asp:Literal runat="server" Text="<%$ Resources:Resources, UserType%>" />
                        <asp:TextBox ID="txtUserType" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-group col-md-6">
                        <asp:Literal runat="server" Text="<%$ Resources:Resources, BookQuota%>" />
                        <asp:TextBox ID="txtBookQuota" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                    <div class="form-group col-md-6">
                        <asp:Literal runat="server" Text="<%$ Resources:Resources, BorrowDuration%>" />
                        <asp:TextBox ID="txtBorrowDuration" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                </div>
                
                <div class="form-group">
                    <asp:Button ID="btnSave" runat="server" Text="<%$ Resources:Resources, SaveChanges %>" 
                        CssClass="p-btn" OnClick="btnSave_Click" UseSubmitBehavior="false"/>
                    <button type="button" class="p-btn" data-toggle="modal" 
                        data-target="#passwordModal">
                        Change Password
                    </button>
                </div>
            </div>
        </div>

        <div class="form-group">
            <label><asp:Literal Text="<%$ Resources:Resources, Language %>" runat="server" /></label>
            <asp:DropDownList ID="ddlLanguage" runat="server" CssClass="form-control" AutoPostBack="true"
                OnSelectedIndexChanged="ddlLanguage_SelectedIndexChanged">
                <asp:ListItem Value="1" Text="<%$ Resources:Resources, English %>" />
                <asp:ListItem Value="2" Text="<%$ Resources:Resources, Arabic %>" />
            </asp:DropDownList>
        </div>

                <asp:Label ID="errorMsg" runat="server" CssClass="error-message-main"></asp:Label>
        <asp:Label ID="successMsg" runat="server" CssClass="suc-message"></asp:Label>

        <asp:Button CssClass="logBtn" ID="btnLogout" runat="server" 
                            Text="Logout" OnClick="Logout_Click" UseSubmitBehavior="false"/>
        
        <div class="modal fade" id="passwordModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Change Password</h5>
                        <button type="button" class="close" data-dismiss="modal">
                            <span>&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label>Current Password</label>
                            <asp:TextBox ID="txtCurrentPassword" runat="server" 
                                TextMode="Password" CssClass="form-control" Required="true" MaxLength="15" />
                        </div>
                        <div class="form-group">
                            <label>New Password</label>
                            <asp:TextBox ID="txtNewPassword" runat="server" 
                                TextMode="Password" CssClass="form-control" Required="true" MaxLength="15" />
                        </div>
                        <div class="form-group">
                            <label>Confirm New Password</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" 
                                TextMode="Password" CssClass="form-control" Required="true" MaxLength="15" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnChangePassword" runat="server" Text="<%$ Resources:Resources, ChangePassword %>"
                            CssClass="p-btn" OnClick="btnChangePassword_Click" />
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>