namespace WebAppFn.View.Pages
open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Fun.Blazor
open eShop.WebAppComponents.Services
open eShop.WebAppComponents.Catalog
open Microsoft.AspNetCore.Components.Routing
open eShop.WebApp.Services
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Components.Forms
open WebAppFn.Components






[<Route "/item/{itemId:int}">]
type ItemPage() =
    inherit FunComponent()

    
    let mutable item : CatalogItem option = None
    let mutable numInCart = 0
    let mutable isLoggedIn = false
    let mutable notFound = false


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


    member private  this.UpdateNumInCartAsync () =
               task {
                   let! items = this.BasketState.GetBasketItemsAsync ()

                   numInCart <-
                       items
                       |> Seq.tryFind (fun i -> i.ProductId = this.ItemId)
                       |> Option.map (fun i -> i.Quantity)
                       |> Option.defaultValue 0
               }

    member private   this.AddToCartAsync () =
        task {
            if not isLoggedIn then
                this.Nav.NavigateTo (UserMenu.loginUrl (this.Nav))
            else if item |> Option.isSome then
                do! this.BasketState.AddAsync (item.Value)
                do! this.UpdateNumInCartAsync ()
        }


    override this.Render() =

        item
        |> function
            | Some item ->
                html.fragment [|
                    stylesheet "View\Pages\Item\ItemPage.fs.css"


                    PageTitle'() { $"{item.Name} | Northern Mountains" }
                    SectionContent'() {
                        SectionName "page-header-title"
                        "item.Name"
                    }
                    SectionContent'() {
                       SectionName "page-header-subtitle"
                       if item.CatalogBrand = null then
                            ""
                        else
                            item.CatalogBrand.Brand 
                    }
                    div {
                        class' "item-details"
                        img {
                            class' "item-details"
                            alt item.Name
                            src (this.ProductImages.GetProductImageUrl (item))

                        }
                        div {
                            class' "description"
                            p { item.Description }
                            p {
                                "Brand: "
                                strong { if item.CatalogBrand = null then
                                            html.none   
                                         else
                                            item.CatalogBrand.Brand
                                }
                            }
                            form {
                                class' "add-to-cart"
                                method "post"
                                dataEnhance isLoggedIn
                                onsubmit (fun _ -> this.AddToCartAsync ())
                                formName "add-to-cart"

                                childContent [|
                                                html.blazor<AntiforgeryToken> ()
                                                span {
                                                    class' "price"
                                                    item.Price.ToString("0.00")
                                                }
                                                isLoggedIn
                                                |> function
                                                    | true ->
                                                        button {
                                                            type' "submit"
                                                            title' "Add to basket"
                                                            childContentRaw """<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" xmlns="http://www.w3.org/2000/svg">
                                                                                       <path id="Vector" d="M6 2L3 6V20C3 20.5304 3.21071 21.0391 3.58579 21.4142C3.96086 21.7893 4.46957 22 5 22H19C19.5304 22 20.0391 21.7893 20.4142 21.4142C20.7893 21.0391 21 20.5304 21 20V6L18 2H6Z" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                                                                                       <path id="Vector_2" d="M3 6H21" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                                                                                       <path id="Vector_3" d="M16 10C16 11.0609 15.5786 12.0783 14.8284 12.8284C14.0783 13.5786 13.0609 14 12 14C10.9391 14 9.92172 13.5786 9.17157 12.8284C8.42143 12.0783 8 11.0609 8 10" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                                                                                   </svg>"""
                                                            "Add to shopping bag"    
                                                        }
                                                    | false ->
                                                        button {
                                                            type' "submit"
                                                            title' "Log in to purchase"
                                                            childContentRaw """<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" xmlns="http://www.w3.org/2000/svg">
                                                                                       <path d="M20 21V19C20 17.9391 19.5786 16.9217 18.8284 16.1716C18.0783 15.4214 17.0609 15 16 15H8C6.93913 15 5.92172 15.4214 5.17157 16.1716C4.42143 16.9217 4 17.9391 4 19V21" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                                                                                       <path d="M12 11C14.2091 11 16 9.20914 16 7C16 4.79086 14.2091 3 12 3C9.79086 3 8 4.79086 8 7C8 9.20914 9.79086 11 12 11Z" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                                                                                   </svg>"""
                                                            "Log in to purchase"
                                                        }
                                                        

                                                |]      

                            }
                            if numInCart > 0 then
                                p {
                                    strong { numInCart.ToString() }
                                    " in "
                                    a {
                                        href "cart"
                                        " shopping bag"
                                    }
                                }
                        }
                    }

                |]
            
            | None when notFound = true ->
                
                 html.fragment [|
                    SectionContent'() {
                                  SectionName "page-header-title"
                                  "Not found"
                              }

                    div {
                        class' "item-details"
                        p { "Sorry, we couldn't find any such product." }
                    }

                |]

            | _ -> html.none
        


