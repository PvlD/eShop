namespace WebAppF.Pages

open System
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
open System.Collections.Generic
open System.Linq
open Microsoft.AspNetCore.Authorization

[<RequireQualifiedAccess>]
module CartPage =

    [<Route("/cart")>]
    [<Authorize>]
    type Page() =
        inherit Component()


        let mutable basketItems : IReadOnlyCollection<BasketItem> option = None

        let TotalPriceAsStr () =
            basketItems
            |> Option.map (fun bi -> bi.Sum (fun i -> Decimal.Multiply (i.Quantity, i.UnitPrice)))
            |> Option.map (fun totalPrice -> totalPrice.ToString ("0.00"))
            |> Option.defaultValue ""



        let TotalQuantityAsStr () =
            basketItems
            |> Option.map (fun bi -> bi.Sum (fun i -> i.Quantity))
            |> Option.map (fun totalQuantity -> totalQuantity.ToString ())
            |> Option.defaultValue ""




        override _.CssScope = CssScopes.CartPage



        [<SupplyParameterFromForm>]
        member val UpdateQuantityId = Nullable () : Nullable<int> with get, set

        [<SupplyParameterFromForm>]
        member val UpdateQuantityValue = Nullable () : Nullable<int> with get, set


        [<Inject>]
        member val ProductImages = Unchecked.defaultof<IProductImageUrlProvider> with get, set

        [<Inject>]
        member val Nav = Unchecked.defaultof<NavigationManager> with get, set

        [<Inject>]
        member val Basket = Unchecked.defaultof<BasketState> with get, set





        override this.Render () =
            concat {
                addPageTitle "Shopping Bag | Northern Mountains"
                addPageHeaderTitle "Shopping Bag"

                div {
                    attr.``class`` "cart"

                    basketItems
                    |> function
                        | Some items ->
                            if items.Count = 0 then
                                p {
                                    text "Your shopping bag is empty"

                                    a {
                                        attr.href ""
                                        text "Continue Shopping"
                                    }

                                }
                            else

                            concat {
                                div {
                                    attr.``class`` "cart-items"

                                    div {
                                        attr.``class`` "cart-item-header"

                                        div {
                                            attr.``class`` "catalog-item-info"
                                            text "Product"
                                        }

                                        div {
                                            attr.``class`` "catalog-item-quantity"
                                            text "Quantity"
                                        }

                                        div {
                                            attr.``class`` "catalog-item-total"
                                            text "Total"
                                        }
                                    }

                                    forEach (items)
                                    <| fun item ->
                                        let quantity = this.CurrentOrPendingQuantity (item.ProductId, item.Quantity)

                                        div {
                                            attr.``class`` "cart-item"
                                            attr.key item.ProductId

                                            div {
                                                attr.``class`` "catalog-item-info"

                                                img {
                                                    attr.alt item.ProductName

                                                    attr.``src`` (
                                                        this.ProductImages.GetProductImageUrl (item.ProductId)
                                                    )
                                                }

                                                div {
                                                    attr.``class`` "catalog-item-content"

                                                    p {
                                                        attr.``class`` "name"
                                                        text item.ProductName
                                                    }

                                                    p {
                                                        attr.``class`` "price"
                                                        text ($"""${item.UnitPrice.ToString ("0.00")}""")
                                                    }
                                                }

                                            }

                                            div {
                                                attr.``class`` "catalog-item-quantity"

                                                form {
                                                    "data-enhance" => true
                                                    attr.method "post"

                                                    input {
                                                        attr.``type`` "hidden"
                                                        attr.``name`` "_handler"
                                                        attr.``value`` "update-cart"
                                                    }

                                                    comp<AntiforgeryToken>

                                                    input {
                                                        "aria-label" => "product quantity"
                                                        attr.``type`` "number"
                                                        attr.``name`` "UpdateQuantityValue"
                                                        attr.``value`` (quantity.ToString ())
                                                        "min" => "0"
                                                    }

                                                    button {
                                                        attr.``type`` "submit"
                                                        attr.``class`` "button button-secondary"
                                                        attr.name "UpdateQuantityId"
                                                        attr.value (item.ProductId.ToString ())
                                                        text "Update"
                                                    }

                                                }

                                            }

                                            div {
                                                attr.``class`` "catalog-item-total"

                                                text (
                                                    $"""${Decimal.Multiply(quantity, item.UnitPrice).ToString ("0.00")}"""
                                                )
                                            }
                                        }

                                }

                                div {
                                    attr.``class`` "cart-summary"

                                    div {
                                        attr.``class`` "cart-summary-container"

                                        div {
                                            attr.``class`` "cart-summary-header"

                                            img {
                                                "role" => "presentation"
                                                attr.src "icons/cart.svg"
                                                "Your shopping bag"

                                                span {
                                                    attr.``class`` "filter-badge"
                                                    text (TotalQuantityAsStr ())

                                                }
                                            }
                                        }

                                        div {
                                            attr.``class`` "cart-summary-total"
                                            div { "Total" }
                                            div { text ($"""${TotalPriceAsStr ()}""") }
                                        }

                                        a {
                                            attr.href "checkout"
                                            attr.``class`` "button button-primary"
                                            text "Checkout"
                                        }

                                        a {
                                            attr.href ""
                                            attr.``class`` "cart-summary-link"

                                            img {
                                                "role" => "presentation"
                                                attr.src "icons/arrow-left.svg"
                                            }

                                            p { "Continue Shopping" }

                                        }


                                    }
                                }

                                form {

                                    on.task.submit <| fun _ -> this.UpdateQuantityAsync ()
                                    formWithOnsubmit "update-cart"
                                }
                            }

                        | None -> p { text "Loading..." }


                }

            }


        member internal this.UpdateQuantityAsync () =
            task {
                let id = this.UpdateQuantityId.Value
                let quantity = this.UpdateQuantityValue.Value
                do! this.Basket.SetQuantityAsync (id, quantity)
                let! basketItems_ = this.Basket.GetBasketItemsAsync ()
                basketItems <- Option.ofObj basketItems_
            }




        override this.OnInitializedAsync () =
            task {
                let! basketItems_ = this.Basket.GetBasketItemsAsync ()
                basketItems <- Option.ofObj basketItems_

            }

        member internal this.CurrentOrPendingQuantity (productId, cartQuantity) : int =

            if this.UpdateQuantityId.GetValueOrDefault (-1) = productId then
                Option.ofNullable this.UpdateQuantityValue |> Option.defaultValue -1
            else
                cartQuantity
