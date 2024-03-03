namespace WebAppFn.Components.Chatbot
open System 
open System.Linq
open  Microsoft.AspNetCore.Components.Authorization
open  Microsoft.SemanticKernel
open  Microsoft.Extensions.DependencyInjection
open  Microsoft.SemanticKernel.ChatCompletion
open  eShop.WebApp.Chatbot

open Microsoft.AspNetCore.Components

open eShop.WebApp.Services
open Fun.Blazor
open Microsoft.JSInterop
open eShop.WebAppComponents.Services
open Microsoft.Extensions.Logging

 
 type  Chatbot() =
    inherit FunComponent()

    let mutable missingConfiguration =false
    let mutable  chatState:ChatState = null
    let mutable   textbox:ElementReference = Unchecked.defaultof<ElementReference>
    let mutable    chat:ElementReference = Unchecked.defaultof<ElementReference>;
    let mutable      messageToSend:string = null;
    let mutable       thinking:bool = false;
    let mutable        jsModule:IJSObjectReference = null;



    [<Inject>]
    member val JS = Unchecked.defaultof<IJSRuntime> with get, set
    
    [<Inject>]
    member val Nav = Unchecked.defaultof<NavigationManager> with get, set
    [<Inject>]
    member val CatalogService = Unchecked.defaultof<CatalogService> with get, set
    [<Inject>]
    member val BasketState = Unchecked.defaultof<BasketState> with get, set
    [<Inject>]
    member val AuthenticationStateProvider = Unchecked.defaultof<AuthenticationStateProvider> with get, set
    [<Inject>]
    member val LoggerFactory = Unchecked.defaultof<ILoggerFactory> with get, set
    [<Inject>]
    member val ServiceProvider = Unchecked.defaultof<IServiceProvider> with get, set


    override this.OnInitializedAsync()=
                    task {
                        let  kernel = this.ServiceProvider.GetService<Kernel>();
                        if kernel <> null then
                            let!  auth = this.AuthenticationStateProvider.GetAuthenticationStateAsync();
                            chatState <- new ChatState(this.CatalogService, this.BasketState, auth.User, this.Nav, kernel, this.LoggerFactory);
                        
                        else
                        
                            missingConfiguration <- true
                        
                    }
    member private this.SendMessageAsync() =
                    task {
                        let  messageCopy = messageToSend |> Option.ofObj
                                           |> Option.map (fun s -> s.Trim())
                                           |> Option.defaultValue null
                        messageToSend <- null

                        if chatState <> null && not <|String.IsNullOrEmpty(messageCopy) then
                            thinking <- true;
                            do! chatState.AddUserMessageAsync(messageCopy, onMessageAdded= fun () -> this.StateHasChanged());
                            thinking <- false;
                        
                    }
    override this.OnAfterRenderAsync( firstRender:bool) =
                            task {
                                if jsModule = null then
                                   let! jsModule_ = this.JS.InvokeAsync<IJSObjectReference>("import", "./View/Components/Chatbot/Chatbot.fs.js");
                                   jsModule <- jsModule_
                                do! jsModule.InvokeVoidAsync("scrollToEnd", chat);

                                if firstRender then
                                    do! textbox.FocusAsync();
                                    do! jsModule.InvokeVoidAsync("submitOnEnter", textbox);
                                
                            }                    
    override this.Render() = 
                html.fragment [|
                    stylesheet "View\Components\Chatbot\Chatbot.fs.css"

                    div {
                        class' "floating-pane"
                        a {
                            href $"""{this.Nav.GetUriWithQueryParameter("chat", null)}"""
                            class' "hide-chatbot"
                            title' "Close .Net Concierge"
                            span{ "✖"}  
                        }
                        div {
                            class' "chatbot-chat"
                            ref (fun el -> chat <- el)
                            if chatState <> null then
                                for message in chatState.Messages.Where (fun m -> m.Role = AuthorRole.Assistant || m.Role = AuthorRole.User) do
                                    if not <| String.IsNullOrWhiteSpace(message.Content) then
                                        p{
                                            key' message
                                            class' $"message mesage-{message.Role}" 
                                            childContentRaw (MessageProcessor.AllowImages(message.Content).ToString())
                                        }

                            else if missingConfiguration then
                                p{
                                    class' "message message-assistant"
                                    strong { "The chatbot is missing required configuration." }
                                    " Please edit appsettings.json in the WebApp project. You'll need an API key to enable AI features."
                                }  
                            if thinking then
                                p{
                                    class' "thinking"
                                    "Thinking..."
                                }               
                        }
                        form {
                            class' "chatbot-input"
                            onsubmit (fun _ -> this.SendMessageAsync())
                            textarea {
                                placeholder "Start chatting..."
                                value messageToSend
                                onchange (fun a -> messageToSend <- a.Value |> string)
                                ref (fun el -> textbox <- el)
                            }
                            button {
                                type' "submit"
                                title' "Send"
                                disabled (chatState = null)
                                "Send"
                            }
                        }
                    }
                    
                |]    