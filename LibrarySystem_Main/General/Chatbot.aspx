<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Chatbot.aspx.cs" 
    Inherits="LibrarySystem_Main.General.Chatbot" MasterPageFile="~/Site.Master" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid h-100">
        <div class="row h-100">
            <!-- Chat Sessions Panel -->
            <div class="col-md-3 d-flex flex-column" style="min-height: 80vh;">
                <div class="card flex-grow-1 d-flex flex-column">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5>Chat Sessions</h5>
                        <asp:Button ID="btnNewChat" runat="server" Text="New Chat" 
                            CssClass="btn btn-sm btn-primary" OnClick="btnNewChat_Click" />
                    </div>
                    <div class="card-body p-0 flex-grow-1" style="overflow-y: auto;">
                        <asp:Repeater ID="rptChatSessions" runat="server" OnItemCommand="rptChatSessions_ItemCommand">
                            <ItemTemplate>
                                <div class='chat-session-item p-2 border-bottom position-relative <%# GetChatItemCss(Eval("ChatId")) %>'>
            
                                    <asp:LinkButton runat="server" CommandName="SelectChat"
                                        CommandArgument='<%# Eval("ChatId") %>'
                                        CssClass="text-decoration-none text-dark d-block">
                
                                        <div class="d-flex justify-content-between align-items-center">
                                            <div class="chat-title font-weight-bold text-truncate me-2"><%# Eval("Title") %></div>
                                            <asp:LinkButton runat="server" CommandName="DeleteChat"
                                                CommandArgument='<%# Eval("ChatId") %>'
                                                CssClass="btn btn-sm btn-link text-danger p-0"
                                                OnClientClick="return confirm('Delete this chat?');">
                                                <i class="fas fa-trash-alt"></i>
                                            </asp:LinkButton>
                                        </div>
                                        <div class="chat-date small text-muted"><%# Eval("CreatedDate", "{0:g}") %></div>
                                    </asp:LinkButton>
            
                                    <%-- 3. Remove the inline style and add the new class to the indicator div --%>
                                    <div class="position-absolute top-0 end-0 h-100 active-indicator" 
                                         style="width: 4px; background-color: #0d6efd;">
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
            
            <div class="col-md-9 d-flex flex-column" style="min-height: 80vh;">
                <div class="card flex-grow-1 d-flex flex-column">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0">Chat</h5>
                        <div class="badge bg-primary rounded-pill">
                            <asp:Literal ID="litActiveChatTitle" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="card-body chat-container flex-grow-1" 
                        style="overflow-y: auto; display: flex; flex-direction: column;" 
                        id="chatScrollContainer">
                        <asp:Literal ID="litChatHistory" runat="server"></asp:Literal>
                    </div>
                </div>
                
                <div class="input-group mt-3">
                    <label class="btn btn-outline-secondary" for="fileUploadImage">
                        <i class="fas fa-image"></i>
                        <asp:FileUpload ID="fileUploadImage" runat="server" style="display:none" />
                    </label>

                    <asp:TextBox ID="txtMessage" runat="server"
                        CssClass="form-control" placeholder="Type your question..." 
                        autocomplete="off"
                        onkeypress="if(event.keyCode==13) {document.getElementById('<%= btnSend.ClientID %>').click(); return false;}" />
                    
                    <div class="input-group-append">
                        <asp:Button ID="btnSend" runat="server" Text="Send"
                            CssClass="btn btn-primary" OnClick="btnSend_Click" />
                    </div>
                </div>

                <div id="imagePreview" class="mt-2" style="display:none;">
                    <img id="previewImg" src="#" alt="Preview" style="max-height: 100px; border: 1px solid #ddd; padding: 5px;"/>
                    <button type="button" class="btn btn-sm btn-danger ml-2" onclick="clearImage()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

            </div>
        </div>
    </div>

    <style>

        chat-session-item .active-indicator {
    display: none;
}

.chat-session-item.active-chat .active-indicator {
    display: block;
}


        .chat-session-item {
            transition: background-color 0.2s;
            cursor: pointer;
        }
        .chat-session-item:hover {
            background-color: #f8f9fa;
        }
        .active-chat {
            background-color: #e3f2fd;
        }
        .chat-title {
            font-size: 0.9rem;
        }

        .chat-date {
            font-size: 0.7rem;
        }

        .chat-message-container {
            display: flex;
            margin-bottom: 0.75rem;
        }

        .chat-message { 
            padding: 12px 16px; 
            border-radius: 18px; 
            max-width: 85%;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
            position: relative;
            animation: fadeIn 0.3s ease;
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(10px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .user-message-container {

            justify-content: flex-end;
        }

        .user-message { 
            background-color: #0d6efd; 
            color: white;
            border-bottom-right-radius: 4px;
        }

        .bot-message-container {
            justify-content: flex-start;
        }

        .bot-message { 
            background-color: #f0f2f5; 
            border-bottom-left-radius: 4px;
        }

        .error-message { 
            background-color: #f8d7da; 
            margin: 0 auto;
            text-align: center;
            max-width: 90%;
        }
        .message-time {
            font-size: 0.65rem;
            opacity: 0.7;
            margin-top: 4px;
            text-align: right;
        }
        .typing-indicator {
            display: inline-block;
            padding: 12px 16px;
            background-color: #f0f2f5;
            border-radius: 18px;
            border-bottom-left-radius: 4px;
        }
        .typing-indicator span {
            height: 8px;
            width: 8px;
            float: left;
            margin: 0 1px;
            background-color: #9E9EA1;
            display: block;
            border-radius: 50%;
            opacity: 0.4;
        }
        .typing-indicator span:nth-of-type(1) {
            animation: typing 1s infinite;
        }
        .typing-indicator span:nth-of-type(2) {
            animation: typing 1s infinite 0.2s;
        }
        .typing-indicator span:nth-of-type(3) {
            animation: typing 1s infinite 0.4s;
        }

        .chat-image-preview {
            border: 1px solid #eee;
            padding: 5px;
            border-radius: 5px;
            background: #f8f9fa;
            margin-top: 5px;
        }
        .user-message-container .chat-image-preview {
            float: right;
        }

        @keyframes typing {
            0%, 100% {
                transform: translateY(0);
            }

            50% {
                transform: translateY(-5px);
            }
        }
    </style>

    <script>
        function scrollToBottom() {
            var container = document.getElementById('chatScrollContainer');
            container.scrollTop = container.scrollHeight;
        }
        
        function showTypingIndicator() {
            var typingHtml = `<div class="chat-message-container bot-message-container">
                <div class="typing-indicator">
                    <span></span><span></span><span></span>
                </div>
            </div>`;
            
            document.getElementById('chatScrollContainer').insertAdjacentHTML('beforeend', typingHtml);
            scrollToBottom();
        }
        
        function focusInput() {
            document.getElementById('<%= txtMessage.ClientID %>').focus();
        }
        
        window.onload = function() {
            scrollToBottom();
            focusInput();
        };

        document.getElementById('<%= fileUploadImage.ClientID %>').addEventListener('change', function (e) {
            if (this.files && this.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    document.getElementById('previewImg').src = e.target.result;
                    document.getElementById('imagePreview').style.display = 'flex';
                }
                reader.readAsDataURL(this.files[0]);
            }
        });

        function clearImage() {
            document.getElementById('<%= fileUploadImage.ClientID %>').value = '';
        document.getElementById('imagePreview').style.display = 'none';
    }
    </script>
</asp:Content>