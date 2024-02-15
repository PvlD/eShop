
namespace WebAppF.Pages

open Pages
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Sections
open Microsoft.AspNetCore.Authorization




[<RequireQualifiedAccess>]
module UserLogOut  =
    
    open System

    open Microsoft.JSInterop
    open Microsoft.AspNetCore.Components

    open Elmish
    open Bolero
    open Bolero.Html


    type Page() =
        inherit Component()
         override this.Render() = Node(fun _ _ s -> s)

