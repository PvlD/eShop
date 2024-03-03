namespace WebAppFn.View.Layout





open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Components.Endpoints
open Microsoft.AspNetCore.Components.Sections
open System.Linq
open Fun.Css
open Fun.Blazor

open WebAppFn.View.Pages
open WebAppFn.Components

type HeaderBar() as this =
    inherit FunComponent()




    [<CascadingParameter>]
    member val  HttpContext:HttpContext= null with get, set



    member private this.IsCatalog() = this.PageComponentType() 
                                      |> function
                                        | null -> false
                                        | v -> v = typeof<Catalog>
                                               

    member private this.PageComponentType() = if this.HttpContext = null then
                                                 null
                                              else
                                                if this.HttpContext.GetEndpoint() = null then
                                                    null
                                                else

                                                    this.HttpContext.GetEndpoint().Metadata.OfType<ComponentTypeMetadata>().FirstOrDefault() 
                                                    |> function
                                                        | null -> null
                                                        | x -> x.Type

    override _.Render() = 
                    html.fragment [|
                    stylesheet "View\Layout\HeaderBar.fs.css"

                    div {
                        class' ("eshop-header" +    if this.IsCatalog() then " home" else "")



                        div {
                            class' "eshop-header-hero"
                            img {
                                role "presentation"
                                src (if this.IsCatalog() then "images/header-home.webp" else "images/header.webp")
                            }
                        }
                        div {
                            class' "eshop-header-container"
                            nav {
                                class' "eshop-header-navbar"
                                a {
                                    class' "logo logo-header"
                                    href ""
                                    img {
                                        class' "logo logo-header"
                                        alt "Northern Mountains"
                                        src "images/logo-header.svg"

                                    }
                                }
                                html.blazor<UserMenu> ()
                                html.blazor<CartMenu>()
                            }
                            div {
                                class' "eshop-header-intro"
                                h1 { 
                                    SectionOutlet'() {
                                      SectionName "page-header-title"
                                }
                                }
                                p {
                                    SectionOutlet'() {
                                      SectionName "page-header-subtitle"
                                    }
                                    
                                }
                            }
                        }

                    }
                    |]