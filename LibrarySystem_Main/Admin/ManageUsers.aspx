<%@ Page Language="C#" AutoEventWireup="true" 
    CodeBehind="ManageUsers.aspx.cs" 
    Inherits="LibrarySystem_Main.ManageUsers"
    MasterPageFile="~/Site.Master"
    Async="true"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2>Manage Users</h2>
        
        <button type="button" class="login-btn" onclick="clearForm()" data-toggle="modal" data-target="#userModal">
            <i class="fas fa-plus"></i> Add New User
        </button>

        <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-bordered"
            OnRowCommand="gvUsers_RowCommand" DataKeyNames="ClientID" EmptyDataText="No users found" >
            <Columns>
                <asp:BoundField DataField="ClientName" HeaderText="Username" />
                <asp:BoundField DataField="Name" HeaderText="Full Name" />
                <asp:BoundField DataField="TypeDesc" HeaderText="User Type" />
                <asp:BoundField DataField="StatusDesc" HeaderText="Status" />
                <asp:BoundField DataField="GenderDesc" HeaderText="Gender" />
                <asp:BoundField DataField="LanguageDesc" HeaderText="Language" />
                <asp:BoundField DataField="BooksQuota" HeaderText="Book Quota" />
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="EditUser" CommandArgument='<%# Eval("ClientID") %>'
                            CssClass="btn btn-sm btn-warning" ToolTip="Edit">
                            <i class="fas fa-edit">Edit</i>
                        </asp:LinkButton>
                        <asp:LinkButton runat="server" CommandName="DeleteUser" CommandArgument='<%# Eval("ClientID") %>'
                            CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Are you sure you want to delete this user?');" ToolTip="Delete">
                            <i class="fas fa-trash">Delete</i>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="errorMsg" runat="server" CssClass="error-message-main"></asp:Label>
        <asp:Label ID="successMsg" runat="server" CssClass="suc-message"></asp:Label>

        <div class="modal fade" id="userModal" tabindex="-1" role="dialog">
            <div class="modal-dialog modal-lg" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalTitle">Manage User</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close" onclick="cleanupModals()">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hdnClientID" runat="server" />
                        <div class="form-row">
                            <div class="form-group col-md-6">
                                <label>Username</label>
                                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" Required="true" />
                            </div>
                            <div class="form-group col-md-6">
                                <label>Password</label>
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" 
                                    TextMode="Password"/>
                            </div>
                        </div>
                        <div class="form-row">
                            <div class="form-group col-md-6">
                                <label>Full Name</label>
                                <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" Required="true" />
                            </div>
                            <div class="form-group col-md-6">
                                <label>Birth Date</label>
                                <asp:TextBox ID="txtBirthDate" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                        </div>
                        <div class="form-row">
                            <div class="form-group col-md-4">
                                <label>User Type</label>
                                <asp:DropDownList ID="ddlUserType" runat="server" CssClass="form-control" 
                                    DataTextField="TypeName" DataValueField="TypeID" />
                            </div>
                            <div class="form-group col-md-4">
                                <label>Status</label>
                                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control" 
                                    DataTextField="StatusName" DataValueField="StatusID" />
                            </div>
                            <div class="form-group col-md-4">
                                <label>Gender</label>
                                <asp:DropDownList ID="ddlGender" runat="server" CssClass="form-control" 
                                    DataTextField="GenderName" DataValueField="GenderID" />
                            </div>
                        </div>
                        <div class="form-row">
                            <div class="form-group col-md-4">
                                <label>Language</label>
                                <asp:DropDownList ID="ddlLanguage" runat="server" CssClass="form-control" 
                                    DataTextField="LanguageName" DataValueField="LanguageID" />
                            </div>
                            <div class="form-group col-md-4">
                                <label>Book Quota</label>
                                <asp:TextBox ID="txtBookQuota" runat="server" CssClass="form-control" TextMode="Number" />
                            </div>
                            <div class="form-group col-md-4">
                                <label>Borrow Duration (days)</label>
                                <asp:TextBox ID="txtBorrowDuration" runat="server" CssClass="form-control" TextMode="Number" />
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        function clearForm() {
            document.getElementById('<%= hdnClientID.ClientID %>').value = '0';
            document.getElementById('<%= txtUsername.ClientID %>').value = '';
            document.getElementById('<%= txtPassword.ClientID %>').value = '';
            document.getElementById('<%= txtFullName.ClientID %>').value = '';
            document.getElementById('<%= txtBirthDate.ClientID %>').value = '';
            document.getElementById('<%= txtBookQuota.ClientID %>').value = '';
            document.getElementById('<%= txtBorrowDuration.ClientID %>').value = '';

            $('#<%= ddlUserType.ClientID %>').prop('selectedIndex', 0);
            $('#<%= ddlStatus.ClientID %>').prop('selectedIndex', 0);
            $('#<%= ddlGender.ClientID %>').prop('selectedIndex', 0);
            $('#<%= ddlLanguage.ClientID %>').prop('selectedIndex', 0);

            document.getElementById('modalTitle').innerText = 'Add New User';
        }
</script>
</asp:Content>