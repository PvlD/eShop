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
open Microsoft.AspNetCore.Authorization
open System.Collections.Generic
open System.Linq





[<Route "/cart">]
[<Authorize()>]
[<StreamRendering>]
type CartPage() =
    inherit FunComponent()


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


    override this.Render() =

        html.fragment [|
                   
                   stylesheet "View\Pages\Cart\Cart.fs.css"

                   PageTitle'() { "Shopping Bag | Northern Mountains" }
                   SectionContent'() {
                       SectionName "page-header-title"
                       "Shopping bag"
                   }
                   div {
                     class' "cart"
                     basketItems
                    |> function
                        | Some items when items.Count = 0 ->
                                     p { 
                                        "Your shopping bag is empty."
                                        a {
                                            href ""
                                            "Continue Shopping"
                                        }
                                      }
                        | Some items ->
                            html.fragment [|
                            div {
                                class' "cart-items"
                                div {
                                    class' "cart-item-header"
                                    html.fragment [|
                                        div {
                                            class' "catalog-item-info"
                                            "Product"
                                        }
                                        div {   
                                            class' "catalog-item-quantity"
                                            "Quantity"
                                        }
                                        div {
                                            class' "catalog-item-total"
                                            "Total"
                                        }
                                    |]            
                                }
                                for item in items do
                                    let quantity = this.CurrentOrPendingQuantity (item.ProductId, item.Quantity)
                                    div {
                                        class' "cart-item"
                                        key' item.Id
                                        div {
                                            class' "catalog-item-info"
                                            img {
                                                alt item.ProductName
                                                src ( this.ProductImages.GetProductImageUrl (item.ProductId))

                                            }
                                            div{
                                                class' "catalog-item-content"
                                                p {
                                                    class' "name"
                                                    item.ProductName
                                                }
                                                p {
                                                    class' "price"
                                                    $"""${item.UnitPrice.ToString ("0.00")}"""
                                                }
                                            }
                                        }
                                    
                                        div {
                                            class' "catalog-item-quantity"
                                            form {
                                                method "post"
                                                dataEnhance
                                                input {
                                                    type' "hidden"
                                                    name "_handler"
                                                    value "update-cart"
                                                }
                                                html.blazor<AntiforgeryToken> ()
                                                input {
                                                    type' "number"
                                                    name "UpdateQuantityValue"
                                                    value (quantity.ToString ())
                                                    "min", "0"
                                                    aria.label "product quantity"
                                                }
                                                button {
                                                    type' "submit"
                                                    name "UpdateQuantityId"
                                                    class' "button button-secondary"
                                                    value (string item.ProductId)
                                                    "Update"
                                                }

                                            }
                                        }
                                        div {
                                            class' "catalog-item-total"
                                            $"""${Decimal.Multiply(quantity, item.UnitPrice).ToString ("0.00")}"""
                                        }
                            }
                            }              
                            div {
                                class' "cart-summary"
                                div {
                                    class' "cart-summary-container"
                                    div {
                                        class' "cart-summary-header"
                                        img {
                                            role "presentation"
                                            src "icons/cart.svg"
                                            "Your shopping bag"
                                            span {
                                                class' "filter-badge"
                                                TotalQuantityAsStr ()
                                            }
                                        }
                                    }
                                    div {
                                        class' "cart-summary-total"
                                        div { "Total" }
                                        div {  TotalPriceAsStr () }
                                    }
                                    a {
                                        href "checkout"
                                        class' "button button-primary"
                                        "Checkout"
                                    }
                                    a {
                                        href ""
                                        class' "cart-summary-link"
                                        img {
                                            role "presentation"
                                            src "icons/arrow-left.svg"
                                            p { "Continue Shopping" }
                                        }
                                    }


                                }
                            }
                            form {
                                method "post"
                                onsubmit (fun _ -> this.UpdateQuantityAsync ())
                                formName "update-cart"
                            }
                            |]
                        | None -> p { "Loading..." }
                   }
                   |]