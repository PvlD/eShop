module WebAppF.Index

open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Bolero.Server.Html


let page = doctypeHtml {
    head {
        meta { attr.charset "UTF-8" }
        meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
        title { "WebAppF" }
        ``base`` { attr.href "/" }
        link { attr.rel "stylesheet"; attr.href "css/normalize.css" }
        link { attr.rel "stylesheet"; attr.href "css/app.css" }
        link { attr.rel "stylesheet"; attr.href "WebAppF.styles.css" }
        link { attr.rel "shortcut icon"; attr.href "images/favicon.png" }
        comp<HeadOutlet>
    }
    body {
        div {
            attr.id "main"
            comp<WebAppF.Main.App> 
        }
        boleroScript
    }
}

[<Route "/{*path}">]
type Page() =
    inherit Bolero.Component()
    override _.Render() = page
