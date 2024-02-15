namespace WebAppF.Pages

open Bolero
open Microsoft.AspNetCore.Components
open eShop.WebAppComponents.Services
open eShop.WebApp.Services
open eShop.WebAppComponents.Catalog
open WebAppF.Pages.Pages
open Bolero.Html
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Http
open WebAppF.Components
open Hlp.Html

[<RequireQualifiedAccess>]
module ItemPage =

    type Page() =
        inherit Component()

        let mutable item : CatalogItem option = None
        let mutable numInCart = 0
        let mutable isLoggedIn = false
        let mutable notFound = false


        override _.CssScope = CssScopes.ItemPage


        [<Parameter>]
        member val ItemId = 0 : int with get, set


        [<Inject>]
        member val CatalogService = Unchecked.defaultof<CatalogService> with get, set

        [<Inject>]
        member val BasketState = Unchecked.defaultof<BasketState> with get, set

        [<Inject>]
        member val ProductImages = Unchecked.defaultof<IProductImageUrlProvider> with get, set

        [<Inject>]
        member val Nav = Unchecked.defaultof<NavigationManager> with get, set

        [<CascadingParameter>]
        member val HttpContext = Unchecked.defaultof<HttpContext> with get, set



        override this.Render () =

            item
            |> function
                | Some item ->
                    concat {
                        { pageTitle = $"{item.Name} | Northern Mountains"
                          page_header_title = item.Name
                          page_header_subtitle =
                            if item.CatalogBrand = null then
                                ""
                            else
                                item.CatalogBrand.Brand }
                        |> addPageDescriptionComp

                        div {
                            attr.``class`` "item-details"

                            img {
                                attr.``alt`` item.Name
                                attr.``src`` (this.ProductImages.GetProductImageUrl (item))
                            }

                            div {
                                attr.``class`` "description"
                                p { item.Description }

                                p {
                                    "Brand:"

                                    strong {
                                        if item.CatalogBrand = null then
                                            empty ()
                                        else
                                            item.CatalogBrand.Brand
                                    }
                                }

                                form {
                                    attr.``class`` "add-to-cart"
                                    attr.method "post"

                                    on.task.submit (fun _ -> this.AddToCartAsync ())

                                    "data-enhance" => isLoggedIn
                                    formWithOnsubmit "add-to-cart"


                                    comp<AntiforgeryToken         >

                                    span {
                                        attr.``class`` "price"
                                        item.Price.ToString ("0.00")
                                    }

                                    isLoggedIn
                                    |> function
                                        | true ->
                                            button {
                                                attr.``type`` "submit"
                                                attr.title "Add to cart"

                                                svg {
                                                    attr.width "24"
                                                    attr.height "24"
                                                    "viewBox" => "0 0 24 24"
                                                    "fill" => "none"
                                                    "stroke" => "currentColor"
                                                    "xmlns" => "http://www.w3.org/2000/svg"

                                                    elt "path" {
                                                        attr.id "Vector"

                                                        "d"
                                                        => "M6 2L3 6V20C3 20.5304 3.21071 21.0391 3.58579 21.4142C3.96086 21.7893 4.46957 22 5 22H19C19.5304 22 20.0391 21.7893 20.4142 21.4142C20.7893 21.0391 21 20.5304 21 20V6L18 2H6Z"

                                                        "stroke-width" => "1.5"
                                                        "stroke-linecap" => "round"
                                                        "stroke-linejoin" => "round"
                                                    }

                                                    elt "path" {
                                                        attr.id "Vector_2"
                                                        "d" => "M3 6H21"
                                                        "stroke-width" => "1.5"
                                                        "stroke-linecap" => "round"
                                                        "stroke-linejoin" => "round"
                                                    }

                                                    elt "path" {
                                                        attr.id "Vector_3"

                                                        "d"
                                                        => "M16 10C16 11.0609 15.5786 12.0783 14.8284 12.8284C14.0783 13.5786 13.0609 14 12 14C10.9391 14 9.92172 13.5786 9.17157 12.8284C8.42143 12.0783 8 11.0609 8 10"

                                                        "stroke-width" => "1.5"
                                                        "stroke-linecap" => "round"
                                                        "stroke-linejoin" => "round"

                                                    }

                                                }

                                                "Add to shopping bag"
                                            }

                                        | false ->
                                            button {
                                                attr.``type`` "submit"
                                                attr.title "Log in to purchase"

                                                svg {
                                                    attr.width "24"
                                                    attr.height "24"
                                                    "viewBox" => "0 0 24 24"
                                                    "fill" => "none"
                                                    "stroke" => "currentColor"

                                                    elt "path" {
                                                        "d"
                                                        => "M20 21V19C20 17.9391 19.5786 16.9217 18.8284 16.1716C18.0783 15.4214 17.0609 15 16 15H8C6.93913 15 5.92172 15.4214 5.17157 16.1716C4.42143 16.9217 4 17.9391 4 19V21"

                                                        "stroke-width" => "1.5"
                                                        "stroke-linecap" => "round"
                                                        "stroke-linejoin" => "round"
                                                    }

                                                    elt "path" {
                                                        "d"
                                                        => "M12 11C14.2091 11 16 9.20914 16 7C16 4.79086 14.2091 3 12 3C9.79086 3 8 4.79086 8 7C8 9.20914 9.79086 11 12 11Z"

                                                        "stroke-width" => "1.5"
                                                        "stroke-linecap" => "round"
                                                        "stroke-linejoin" => "round"
                                                    }

                                                }

                                                "Log in to purchase"
                                            }
                                }

                                if numInCart > 0 then
                                    p {
                                        strong { $"{numInCart}" }
                                        " in "

                                        a {
                                            attr.href $"/cart"
                                            "shopping bag"
                                        }
                                    }
                            }
                        }
                    }

                | None when notFound = true ->
                    concat {
                        addPageHeaderTitle "Not found"

                        div {
                            attr.``class`` "item-details"
                            p { "Sorry, we couldn't find any such product." }
                        }

                    }

                | _ -> empty ()


        override this.OnInitializedAsync () =

            isLoggedIn <-
                this.HttpContext
                |> Option.ofObj
                |> function
                    | Some c ->
                        c.User.Identity
                        |> Option.ofObj
                        |> function
                            | Some identity -> identity.IsAuthenticated = true
                            | None -> false
                    | None -> false

            task {
                let! item_ = this.CatalogService.GetCatalogItem (this.ItemId)
                item <- Option.ofObj item_
                do! this.UpdateNumInCartAsync ()
            }

        member this.UpdateNumInCartAsync () =
            task {
                let! items = this.BasketState.GetBasketItemsAsync ()

                numInCart <-
                    items
                    |> Seq.tryFind (fun i -> i.ProductId = this.ItemId)
                    |> Option.map (fun i -> i.Quantity)
                    |> Option.defaultValue 0
            }

        member this.AddToCartAsync () =
            task {
                if not isLoggedIn then
                    this.Nav.NavigateTo (UserMenu.loginUrl (this.Nav))
                else if item |> Option.isSome then
                    do! this.BasketState.AddAsync (item.Value)
                    do! this.UpdateNumInCartAsync ()
            }
