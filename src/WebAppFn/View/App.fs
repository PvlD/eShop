namespace WebAppFn.View

open Microsoft.AspNetCore.Components.Web
open Fun.Blazor


type App() =
    inherit FunComponent()

    override _.Render() =
        html.fragment [|
            doctype "html"
            html' {
                lang "EN"
                head.create [|
                    baseUrl "/"
                    meta { charset "utf-8" }
                    meta {
                        name "viewport"
                        content "width=device-width, initial-scale=1.0"
                    }
                    link {
                        rel "icon"
                        type' "image/png"
                        href "images/favicon.png"
                    }
                    link {
                        rel "stylesheet"
                        href "css/normalize.css"
                    }
                    link {
                        rel "stylesheet"
                        href "css/app.css"
                    }
                    link {
                        rel "stylesheet"
                        href "WebAppFn.styles.css"
                    }
                    HeadOutlet'.create ()
                |]
                body {
                    html.blazor<Routes> ()
                    script { src "_framework/blazor.web.js" }
                    
                }
            }
        |]
