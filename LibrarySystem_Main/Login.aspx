<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" 
    Inherits="LibrarySystem_Main.Login" MasterPageFile="~/Site.Master" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Login - Library System</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="login-container">
        <div class="login-card">
            <div class="login-header">
                <h2>Welcome</h2>
                <p>Please sign in to continue</p>
            </div>
            
            <div class="login-body">
                <div class="form-group">
                    <asp:TextBox ID="txtUsername" runat="server" 
                        placeholder="Username" CssClass="login-input" />
                    <asp:Label ID="lblUsername" runat="server" CssClass="error-message"></asp:Label>
                </div>
                
                <div class="form-group">
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"
                        placeholder="Password" CssClass="login-input" />
                    <asp:Label ID="lblPassword" runat="server" CssClass="error-message"></asp:Label>
                </div>
                
                <asp:Button ID="btnLogin" runat="server" Text="Sign In" 
                    CssClass="login-btn" OnClick="btnLogin_Click" />
                
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message-main"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>