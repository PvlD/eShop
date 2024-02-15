namespace WebAppF.Components

open System

open Bolero
open Bolero.Html

open Microsoft.AspNetCore.Components.Authorization
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Http
open Hlp.Html
open eShop.WebApp.Services


module UserMenu =

    let internal loginUrl (nav : NavigationManager) =
        $"user/login?returnUrl={Uri.EscapeDataString (nav.ToBaseRelativePath (nav.Uri))}"



    type UserMenu() =
        inherit Component()

        override _.CssScope = CssScopes.UserMenu


        [<Inject>]
        member val Nav = Unchecked.defaultof<NavigationManager> with get, set

        [<Inject>]
        member val LogOutService = Unchecked.defaultof<LogOutService> with get, set





        [<CascadingParameter>]
        member val HttpContext = Unchecked.defaultof<HttpContext> with get, set

        override this.Render () =


            comp<AuthorizeView> {

                attr.fragmentWith "Authorized"
                <| fun (context : AuthenticationState) ->



                    concat {
                        if context.User.Identity = null then
                            h3 { attr.empty () }
                        else
                            h3 { $"{context.User.Identity.Name}" }

                        div {
                            attr.``class`` "dropdown-menu"

                            span {
                                attr.``class`` "dropdown-button"
                                attr.href "#"

                                img {
                                    attr.src "icons/user.svg"
                                    "role" => "presentation"
                                }
                            }

                            div {
                                attr.``class`` "dropdown-content"

                                a {
                                    attr.``class`` "dropdown-item"
                                    attr.href "user/orders"
                                    "My orders"
                                }

                                form {
                                    attr.``class`` "dropdown-item"
                                    attr.method "post"
                                    attr.action "user/logout"
                                    on.task.submit (fun _ -> this.LogOutService.LogOutAsync (this.HttpContext))

                                    formWithOnsubmit "logout"

                                    concat {
                                        comp<AntiforgeryToken>

                                        button {
                                            attr.``type`` "submit"
                                            "Log out"
                                        }
                                    }
                                }
                            }
                        }

                    }

                attr.fragmentWith "NotAuthorized"
                <| fun (_ : AuthenticationState) ->

                    a {

                        attr.href (loginUrl this.Nav)
                        "aria-label" => "Sign in"

                        img {
                            attr.src "icons/user.svg"
                            "role" => "presentation"
                        }
                    }
            }
