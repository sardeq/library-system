<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="Return.aspx.cs" Inherits="LibrarySystem_Main.Admin.Return" Async="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2>Pending Returns</h2>
        <asp:GridView ID="gvPendingReturns" runat="server" AutoGenerateColumns="false"
            DataKeyNames="ClientID,BookID"  CssClass="table table-striped table-bordered">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:CheckBox ID="chkSelect" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ClientName" HeaderText="Borrower" />
                <asp:BoundField DataField="Title" HeaderText="Book Title" />
                <asp:BoundField DataField="BorrowDate" HeaderText="Borrow Date" DataFormatString="{0:d}" />
            </Columns>
        </asp:GridView>
        <asp:Button ID="btnConfirmReturns" runat="server" Text="Confirm Returns" 
            CssClass="btn btn-warning" OnClick="btnConfirmReturns_Click" />
                <asp:Label ID="errorMsg" runat="server" CssClass="error-message-main"></asp:Label>
        <asp:Label ID="successMsg" runat="server" CssClass="suc-message"></asp:Label>
    </div>
</asp:Content>