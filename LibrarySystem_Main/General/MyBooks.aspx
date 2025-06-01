<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="MyBooks.aspx.cs" 
    Inherits="LibrarySystem_Main.General.MyBooks" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2>My Borrowed Books</h2>
        
        <asp:GridView ID="gvMyBooks" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-bordered" DataKeyNames="BookID">
            <Columns>
                <asp:BoundField DataField="Title" HeaderText="Title" />
                <asp:BoundField DataField="BorrowDate" HeaderText="Borrow Date" DataFormatString="{0:d}" />
                <asp:BoundField DataField="ReturnDate" HeaderText="Due Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button runat="server" Text="Return" CssClass="btn btn-primary" 
                            CommandArgument='<%# Eval("BookID") %>' OnClientClick='<%# "showReviewModal(\"" + Eval("BookID") + "\"); return false;" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="errorMsg" runat="server" CssClass="error-message-main"></asp:Label>
        <asp:Label ID="successMsg" runat="server" CssClass="suc-message"></asp:Label>

        <div class="modal fade" id="reviewModal" tabindex="-1" role="dialog" aria-labelledby="reviewModalLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="reviewModalLabel">Leave a Review</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <asp:TextBox ID="txtReviewText" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="How was your experience?"></asp:TextBox>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnSubmitReview" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmitReview_Click" />
                    </div>
                    <asp:HiddenField ID="hdnSelectedBookId" runat="server" />
                </div>
            </div>
        </div>
    </div>

    <script>
        var currentBookId;

        function showReviewModal(bookId) {
            $('#<%= hdnSelectedBookId.ClientID %>').val(bookId);
            $('#reviewModal').modal('show');
        }
    </script>
</asp:Content>

