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
                                <div class='chat-session-item p-2 border-bottom <%# GetChatItemCss(Eval("ChatId")) %>'>
                                    <div class="d-flex justify-content-between align-items-center w-100">
                                        <asp:LinkButton runat="server" CommandName="SelectChat"
                                            CommandArgument='<%# Eval("ChatId") %>'
                                            CssClass="text-decoration-none text-dark flex-grow-1 me-2">
                                            <div class="chat-title font-weight-bold text-truncate">
                                                <%# Eval("Title") %>
                                            </div>
                                            <div class="chat-date small text-muted">
                                                <%# Eval("CreatedDate", "{0:g}") %>
                                            </div>
                                        </asp:LinkButton>

                                        <asp:LinkButton runat="server" CommandName="DeleteChat"
                                            CommandArgument='<%# Eval("ChatId") %>'
                                            CssClass="btn btn-sm btn-icon text-danger"
                                            ToolTip="Delete Chat"
                                            OnClientClick="return confirm('Are you sure you want to delete this chat?');">
                                            <img src="delete.png" alt="Delete Icon" style="height:16px; width:16px;" />
                                        </asp:LinkButton>

                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
            
            <div class="col-md-9 d-flex flex-column" style="min-height: 80vh;">
                <div class="card flex-grow-1 d-flex flex-column shadow-sm">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0">
                            <i class="fas fa-comments text-primary me-2"></i>
                            <asp:Literal ID="litActiveChatTitle" runat="server" Text="Chat"></asp:Literal>
                        </h5>
                    </div>
                    <div class="card-body chat-container flex-grow-1" id="chatScrollContainer">
                        <asp:PlaceHolder ID="phEmptyChat" runat="server" Visible="false">
                            <div class="empty-chat-placeholder">
                                <i class="fas fa-robot fa-3x mb-3 text-muted"></i>
                                <h4>Welcome to the Library Bot!</h4>
                                <p>Select a chat on the left or start a new one.</p>
                            </div>
                        </asp:PlaceHolder>
                        <asp:Literal ID="litChatHistory" runat="server"></asp:Literal>
                    </div>
                    <div class="card-footer bg-white">
                        <div id="imagePreview" class="mt-2" style="display:none;">
                            <img id="previewImg" src="#" alt="Preview" />
                            <button type="button" class="btn btn-sm btn-icon btn-danger" onclick="clearImage()">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <div class="input-group">
                           <label class="btn btn-outline-secondary" title="Upload Image" 
                                onclick="document.getElementById('<%= fileUploadImage.ClientID %>').click();">
                                <img src="file.png" alt="Attach Icon" style="height:16px; width:16px;" />
                            </label>
                            <asp:FileUpload ID="fileUploadImage" runat="server" Style="display:none;" />
                            <asp:TextBox ID="txtMessage" runat="server"
                                CssClass="form-control" placeholder="Type your message..."
                                autocomplete="off"
                                onkeypress="if(event.keyCode==13 && !event.shiftKey) {document.getElementById('<%= btnSend.ClientID %>').click(); return false;}" />
                            <asp:Button ID="btnSend" runat="server" Text="Send"
                                CssClass="btn btn-primary" OnClick="btnSend_Click" />
                        </div>
                    </div>
                </div>
        </div>
    </div>
        </div>

    <style>
    :root {
        --primary-color: #007bff;
        --light-primary: #e3f2fd;
        --secondary-color: #6c757d;
        --text-color: #343a40;
        --border-color: #dee2e6;
        --bg-light: #f8f9fa;
        --user-msg-bg: #007bff;
        --bot-msg-bg: #e9ecef;
    }

    .card {
        border: none;
        border-radius: 0.75rem;
    }

    .shadow-sm {
        box-shadow: 0 .125rem .25rem rgba(0,0,0,.075) !important;
    }

    .chat-session-item {
        transition: background-color 0.2s ease-in-out;
        cursor: pointer;
    }

    .chat-session-item:hover {
        background-color: var(--light-primary);
    }

    .active-chat {
        background-color: var(--primary-color) !important;
        color: white;
    }

    .active-chat .chat-title, .active-chat .chat-date {
        color: white;
    }

    .btn-icon {
        background: transparent;
        border: none;
    }

    .chat-container {
        padding: 1.5rem;
        overflow-y: auto;
        display: flex;
        flex-direction: column;
        background-color: var(--bg-light);
    }

    .empty-chat-placeholder {
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        text-align: center;
        height: 100%;
        color: #6c757d;
    }

    .chat-message-container {
        display: flex;
        margin-bottom: 1rem;
        animation: fadeIn 0.4s ease;
    }

    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(15px); }
        to { opacity: 1; transform: translateY(0); }
    }

    .chat-message {
        padding: 0.75rem 1rem;
        border-radius: 1.1rem;
        max-width: 80%;
        box-shadow: 0 2px 5px rgba(0,0,0,0.08);
    }

    .user-message-container {
        justify-content: flex-end;
    }

    .user-message {
        background-color: var(--user-msg-bg);
        color: white;
        border-bottom-right-radius: 0.25rem;
    }

    .bot-message-container {
        justify-content: flex-start;
    }

    .bot-message {
        background-color: var(--bot-msg-bg);
        color: var(--text-color);
        border-bottom-left-radius: 0.25rem;
    }

    .message-time {
        font-size: 0.7rem;
        opacity: 0.8;
        margin-top: 0.25rem;
        text-align: right;
    }

    #imagePreview {
        position: relative;
        display: none;
        margin-bottom: 0.5rem;
    }

    #imagePreview img {
        max-height: 80px;
        border: 1px solid var(--border-color);
        padding: 5px;
        border-radius: 0.5rem;
    }

    #imagePreview .btn-danger {
        position: absolute;
        top: -10px;
        right: -10px;
        background: white;
        border-radius: 50%;
        width: 24px;
        height: 24px;
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .card-footer .input-group .form-control {
        border-right: none;
    }
    
    .card-footer .input-group .btn {
        z-index: 2;
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
            document.getElementById('previewImg').src = '';
            document.getElementById('imagePreview').style.display = 'none';
        }

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
    </script>
</asp:Content>