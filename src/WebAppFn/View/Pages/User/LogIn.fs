namespace WebAppFn.View.Pages.User
open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Fun.Blazor
open eShop.WebAppComponents.Services
open eShop.WebAppComponents.Catalog
open Microsoft.AspNetCore.Components.Routing
open Microsoft.AspNetCore.Authorization






[<Route "/user/login">]
[<Authorize()>]
type LogIn() =
    inherit FunComponent()

    [<Inject>]
    member val  Nav = Unchecked.defaultof<NavigationManager > with get, set


 
    [<SupplyParameterFromQuery>]
    member val   ReturnUrl = null: string with  get, set 


    override   this.OnInitialized ()=
                            let  returnUrl = ( Option.defaultValue "/"  (Option.ofObj this.ReturnUrl) )
                            let url = new Uri(returnUrl, UriKind.RelativeOrAbsolute)
                            this.Nav.NavigateTo(if url.IsAbsoluteUri then  "/" else  returnUrl)


    override this.Render() = html.none