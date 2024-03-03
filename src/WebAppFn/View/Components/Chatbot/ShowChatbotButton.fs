
namespace WebAppFn.Components.Chatbot
open System 
open Microsoft.AspNetCore.Components.Authorization
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Http

open eShop.WebApp.Services


open Fun.Blazor

 type  ShowChatbotButton() =
    inherit FunComponent()


    
    [<Inject>]
    member val Nav = Unchecked.defaultof<NavigationManager> with get, set

    [<SupplyParameterFromQuery(Name = "chat")>]
     member val ShowChat =false with get, set
    

    override this.Render() = 
                html.fragment [|
                    stylesheet "View\Components\Chatbot\ShowChatbotButton.fs.css"

                    a {
                        class' "show-chatbot"
                        href $"""{this.Nav.GetUriWithQueryParameter("chat", true)}"""
                        title' "Show chatbot"

                    }
                    if  this.ShowChat then
                        html.blazor<Chatbot>()

                |]                                        




