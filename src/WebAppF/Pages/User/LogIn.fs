
namespace WebAppF.Pages

open Pages
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Sections
open Microsoft.AspNetCore.Authorization




[<RequireQualifiedAccess>]
module UserLogIn  =
    
    open System

    open Microsoft.JSInterop
    open Microsoft.AspNetCore.Components

    open Elmish
    open Bolero
    open Bolero.Html


    
        

    [<Route("/user/login")>]
    [<Authorize()>]
    type Page() =
        inherit Component()


        [<Inject>]
        member val  Nav = Unchecked.defaultof<NavigationManager > with get, set


         
        [<SupplyParameterFromQuery>]
        member val   ReturnUrl = null: string with  get, set 


        override   this.OnInitialized ()=
                                let  returnUrl = ( Option.defaultValue "/"  (Option.ofObj this.ReturnUrl) )
                                let url = new Uri(returnUrl, UriKind.RelativeOrAbsolute)
                                this.Nav.NavigateTo(if url.IsAbsoluteUri then  "/" else  returnUrl)
                                


                                
         override this.Render() = Node(fun _ _ s -> s)

