namespace WebAppFn.View.Pages.User
open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Fun.Blazor
open eShop.WebAppComponents.Services
open eShop.WebAppComponents.Catalog
open Microsoft.AspNetCore.Components.Routing
open Microsoft.AspNetCore.Authorization


[<Route "/user/logout">]
type LogOut() =
    inherit FunComponent()
    override this.Render() = html.none