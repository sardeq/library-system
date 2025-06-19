<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Chatbot.aspx.cs" 
    Inherits="LibrarySystem_Main.General.Chatbot" MasterPageFile="~/Site.Master" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-3">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5>Chat Sessions</h5>
                        <asp:Button ID="btnNewChat" runat="server" Text="New Chat" 
                            CssClass="btn btn-sm btn-primary" OnClick="btnNewChat_Click" />
                    </div>
                    <div class="card-body p-0">
                        <asp:Repeater ID="rptChatSessions" runat="server" 
                            OnItemCommand="rptChatSessions_ItemCommand">
                            <ItemTemplate>
                                <div class="chat-session-item p-2 border-bottom">
                                    <asp:LinkButton runat="server" CommandName="SelectChat"
                                        CommandArgument='<%# Eval("ChatId") %>'
                                        CssClass='<%# GetChatItemCss(Eval("ChatId")) %> text-decoration-none text-dark d-block"'>
                                        <div class="chat-title font-weight-bold"><%# Eval("Title") %></div>
                                        <div class="chat-date small text-muted"><%# Eval("CreatedDate", "{0:g}") %></div>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
            
            <!-- Chat Container -->
            <div class="col-md-9">
                <div class="card">
                    <div class="card-header">
                        <h5>Chat</h5>
                    </div>
                    <div class="card-body chat-container" style="height: 400px; overflow-y: scroll;" 
                        id="chatScrollContainer">
                        <asp:Literal ID="litChatHistory" runat="server"></asp:Literal>
                    </div>
                </div>
                
                <div class="input-group mt-3">
                    <asp:TextBox ID="txtMessage" runat="server"
                        CssClass="form-control" placeholder="Type your question..." 
                        onkeypress="if(event.keyCode==13) {document.getElementById('<%= btnSend.ClientID %>').click(); return false;}" />
                    <div class="input-group-append">
                        <asp:Button ID="btnSend" runat="server" Text="Send"
                            CssClass="btn btn-primary" OnClick="btnSend_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <style>
        .chat-session-item {
            transition: background-color 0.2s;
        }
        .chat-session-item:hover {
            background-color: #f8f9fa;
        }
        .active-chat {
            background-color: #e3f2fd;
            font-weight: bold;
        }
        .chat-title {
            font-size: 0.9rem;
        }
        .chat-date {
            font-size: 0.7rem;
        }
        .chat-message { 
            padding: 8px 12px; 
            margin: 5px; 
            border-radius: 10px; 
            max-width: 80%; 
        }
        .user-message { 
            background-color: #d1ecf1; 
            margin-left: auto; 
            text-align: right; 
        }
        .bot-message { 
            background-color: #f8f9fa; 
            margin-right: auto; 
        }
        .error-message { 
            background-color: #f8d7da; 
            margin-right: auto; 
        }
    </style>
</asp:Content>