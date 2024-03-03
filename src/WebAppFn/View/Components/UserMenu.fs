namespace WebAppFn.Components
open System 
open Microsoft.AspNetCore.Components.Authorization
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Http

open eShop.WebApp.Services


open Fun.Blazor

 type  UserMenu() =
    inherit FunComponent()



    static member  internal  loginUrl (nav : NavigationManager) =
                    $"user/login?returnUrl={Uri.EscapeDataString (nav.ToBaseRelativePath (nav.Uri))}"

    
    [<Inject>]
    member val Nav = Unchecked.defaultof<NavigationManager> with get, set

    [<Inject>]
    member val LogOutService = Unchecked.defaultof<LogOutService> with get, set





    [<CascadingParameter>]
    member val HttpContext = Unchecked.defaultof<HttpContext> with get, set


    member private this.LogOutAsync () =
            this.LogOutService.LogOutAsync(this.HttpContext)
    override this.Render() = 
                html.fragment [|
                    stylesheet "View\Components\UserMenu.fs.css"


                    AuthorizeView'(){
                        Authorized  (fun context ->
                                    html.fragment [|
                                             fragment {

                                            if context.User.Identity = null then
                                                h3 {"" }
                                            else
                                                h3 { $"{context.User.Identity.Name}" }

                                             }
                                             div {
                                                class' "dropdown-menu"
                                                span{
                                                    class' "dropdown-button"
                                                    img {
                                                        src "icons/user.svg"
                                                        role "presentation"
                                                    }
                                                }
                                                div {
                                                    class' "dropdown-content"
                                                    a { class' "dropdown-item" ; href "user/orders"; "My orders" }
                                                    form {
                                                        class' "dropdown-item"
                                                        method "post"
                                                        action "user/logout"
                                                        onsubmit (fun _ -> this.LogOutAsync())
                                                        formName "logout"
                                                        html.blazor<AntiforgeryToken> ()
                                                        button{
                                                            type' "submit"
                                                            "Log out"
                                                        }

                                                    }
                                                }
                                             }

                                    |]
                                   )
                        NotAuthorized (fun _ -> html.fragment [|
                                        a {
                                            aria.label  "Sign in"
                                            href  (UserMenu.loginUrl this.Nav)
                                            img {
                                                src "icons/user.svg"
                                                role "presentation"
                                            }
                                        }
                                        |] )
                    }
                |]                                        



