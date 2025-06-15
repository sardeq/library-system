<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Chatbot.aspx.cs" 
    Inherits="LibrarySystem_Main.General.Chatbot" MasterPageFile="~/Site.Master" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2 class="mb-4">Library Assistant</h2>
        
        <div class="card">
            <div class="card-body" style="height: 400px; overflow-y: scroll;" id="chatScrollContainer">
                <asp:Literal ID="litChatHistory" runat="server"></asp:Literal>
            </div>
        </div>
        
        <div class="input-group mt-3">
            <asp:TextBox
                ID="txtMessage"
                runat="server"
                CssClass="form-control"
                placeholder="Type your question..."
                ClientIDMode="Static"></asp:TextBox>
            <div class="input-group-append">
                <%-- Changed OnClientClick to a server-side OnClick event --%>
                <asp:Button
                    ID="btnSend"
                    runat="server"
                    Text="Send"
                    CssClass="btn btn-primary"
                    OnClick="btnSend_Click" />
            </div>
        </div>
    </div>

    <script>
        (function () {
            var chatContainer = document.getElementById("chatScrollContainer");
            chatContainer.scrollTop = chatContainer.scrollHeight;
        })();
    </script>

    <style>
        .chat-message { padding: 8px 12px; margin: 5px; border-radius: 10px; max-width: 80%; }
        .user-message { background-color: #d1ecf1; margin-left: auto; text-align: right; }
        .bot-message { background-color: #f8f9fa; margin-right: auto; }
        .error-message { background-color: #f8d7da; margin-right: auto; }
    </style>
</asp:Content>