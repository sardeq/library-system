<%@ Page Title="Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="Reports.aspx.cs" Inherits="LibrarySystem_Main.Admin.Reports" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="filter-container">
        <div class="form-group">
            <label>User:</label>
            <asp:DropDownList ID="ddlUsers" runat="server" CssClass="form-control" 
                AppendDataBoundItems="true">
                <asp:ListItem Value="" Text="All Users"></asp:ListItem>
            </asp:DropDownList>
        </div>
        
        <div class="form-group">
            <label>Book:</label>
            <asp:DropDownList ID="ddlBooks" runat="server" CssClass="form-control" 
                AppendDataBoundItems="true">
                <asp:ListItem Value="" Text="All Books"></asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <label>Filter by Status:</label>
            <div class="checkbox">
                <label>
                    <asp:CheckBox ID="chkDueSoon" runat="server" Text=" Due Soon" />
                </label>
                <label>
                    <asp:CheckBox ID="chkOverdue" runat="server" Text=" Overdue" />
                </label>
            </div>
        </div>
        
        <div class="form-group">
            <label>Borrow From Date:</label>
            <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control datepicker" TextMode ="Date"></asp:TextBox>
        </div>
        
        <div class="form-group">
            <label>Borrow To Date:</label>
            <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control datepicker" TextMode ="Date"></asp:TextBox>
        </div>
        
        <asp:Button ID="btnView" runat="server" Text="View Report" 
            CssClass="btn btn-primary" OnClick="btnView_Click" />
    </div>

    <div class="report-results">
        <asp:GridView ID="gvReport" runat="server" CssClass="table table-striped" AutoGenerateColumns="false"
            OnRowDataBound="gvReport_RowDataBound">
            <Columns>
                <asp:TemplateField HeaderText="User">
                    <ItemTemplate>
                        <asp:Label ID="lblClientName" runat="server" Text='<%# Eval("ClientName") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Title" HeaderText="Book" />
                <asp:BoundField DataField="BorrowDate" HeaderText="Borrow Date" 
                    DataFormatString="{0:dd MMM yyyy}" />
                <asp:BoundField DataField="ReturnDate" HeaderText="Return Date" 
                    DataFormatString="{0:dd MMM yyyy}" />
                <asp:CheckBoxField DataField="PendingConfirmation" HeaderText="Pending Confirmation" />
                <asp:CheckBoxField DataField="Returned" HeaderText="Returned" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>