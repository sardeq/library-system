<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Borrow.aspx.cs" MasterPageFile="~/Site.Master" Inherits="LibrarySystem_Main.General.Borrow" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2>Available Books</h2>
        
        <asp:GridView ID="gvBooks" runat="server" AutoGenerateColumns="false" 
                DataKeyNames="BookID" CssClass="table table-striped table-bordered">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="chkSelect" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Title" HeaderText="Title" />
                <asp:BoundField DataField="Author" HeaderText="Author" />
                <asp:BoundField DataField="BooksAvailable" HeaderText="Available" />
            </Columns>
        </asp:GridView>
        
        <asp:Button ID="btnBorrow" runat="server" Text="Borrow Selected" 
            CssClass="btn btn-primary" OnClick="btnBorrow_Click" />

        <asp:Label ID="errorMsg" runat="server" CssClass="error-message-main"></asp:Label>
        <asp:Label ID="successMsg" runat="server" CssClass="suc-message"></asp:Label>
    </div>
</asp:Content>
