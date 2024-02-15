namespace WebAppF.Components

open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components

open Microsoft.AspNetCore.Components.Sections

open WebAppF.Components.UserMenu
open WebAppF.Components.CartMenu

module HeaderBar =


    type HeaderBar() =
        inherit Component()


        [<Parameter>]
        member val IsCatalog = (fun () -> false) with get, set


        override _.CssScope = CssScopes.HeaderBar

        override this.Render () =

            let headerImage, cclass =
                if this.IsCatalog () then
                    "images/header-home.webp", "home"
                else
                    "images/header.webp", ""


            div {
                attr.``class`` $"eshop-header {cclass}"

                div {
                    attr.``class`` "eshop-header-hero"

                    img {
                        "role" => "presentation"
                        attr.``src`` headerImage
                    }
                }

                div {
                    attr.``class`` "eshop-header-container"

                    nav {
                        attr.``class`` "eshop-header-navbar"

                        a {
                            attr.``class`` "logo logo-header"
                            attr.href ""

                            img {
                                attr.alt "Northern Mountains"
                                attr.src "images/logo-header.svg"
                                attr.``class`` "logo logo-header"
                            }
                        }

                        comp<UserMenu> { attr.empty () }


                        comp<CartMenu> { attr.empty () }

                    }

                    div {
                        attr.``class`` "eshop-header-intro"
                        h1 { comp<SectionOutlet> { "SectionName" => "page-header-title" } }

                        p {
                            comp<SectionOutlet> { "SectionName" => "page-header-subtitle" }


                        }
                    }

                }

            }
