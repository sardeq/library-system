<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" 
    CodeBehind="ManageBooks.aspx.cs" Inherits="LibrarySystem_Main.Admin.ManageBooks" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2>Manage Books</h2>
        
        <div class="mb-3">
            <asp:Button ID="btnAddNew" runat="server" Text="Add New Book" 
                CssClass="btn btn-success" OnClick="btnAddNew_Click" />
        </div>

        <asp:GridView ID="gvBooks" runat="server" AutoGenerateColumns="false" 
            CssClass="table table-striped table-bordered" DataKeyNames="BookID"
            OnRowCommand="gvBooks_RowCommand" EmptyDataText="No books found">
            <Columns>
                
                <asp:BoundField DataField="BookID" HeaderText="Book ID" ReadOnly="true" />
                <asp:BoundField DataField="Title" HeaderText="Title" />
                <asp:BoundField DataField="Author" HeaderText="Author" />
                <asp:BoundField DataField="ReleaseDate" HeaderText="Release Date" 
                    DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="BorrowDuration" HeaderText="Borrow Duration (days)" />
                <asp:BoundField DataField="BooksAvailable" HeaderText="Available Copies" />
                <asp:TemplateField HeaderText="Borrow Type">
                    <ItemTemplate>
                        <%# ((int)Eval("BorrowType") == 0 ? "All" : "Teachers Only") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="EditBook" Text="Edit" 
                            CssClass="btn btn-sm btn-warning" />
                        <asp:LinkButton runat="server" CommandName="DeleteBook" Text="Delete" 
                            CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Are you sure?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="errorMsg" runat="server" CssClass="error-message-main"></asp:Label>
        <asp:Label ID="successMsg" runat="server" CssClass="suc-message"></asp:Label>

        <!-- Add/Edit Modal -->
        <div class="modal fade" id="bookModal" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalTitle"></h5>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label>Book ID</label>
                            <asp:TextBox ID="txtBookID" runat="server" CssClass="form-control" 
                                ReadOnly="true"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Title</label>
                            <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Author</label>
                            <asp:TextBox ID="txtAuthor" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Release Date</label>
                            <asp:TextBox ID="txtReleaseDate" runat="server" 
                                TextMode="Date" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Borrow Duration (days)</label>
                            <asp:TextBox ID="txtBorrowDuration" runat="server" 
                                TextMode="Number" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Total Copies</label>
                            <asp:TextBox ID="txtTotalCopies" runat="server" 
                                TextMode="Number" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Borrow Type</label>
                            <asp:DropDownList ID="ddlBorrowType" runat="server" CssClass="form-control">
                                <asp:ListItem Value="0">All Users</asp:ListItem>
                                <asp:ListItem Value="1">Teachers Only</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnSave" runat="server" Text="Save" 
                            CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <button type="button" class="btn btn-secondary" 
                            data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>