
namespace WebAppFn.View.Layout
open System 
open Microsoft.AspNetCore.Components.Authorization
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Http
open eShop.WebApp.Services
open Fun.Blazor
open System.Collections.Generic
open System.Net.Http
open Microsoft.FSharp.Linq.NullableOperators

[<StreamRendering()>]
 type  CartMenu() =
    inherit FunComponent()



    let mutable basketStateSubscription : IDisposable option = None
    let mutable basketItems : IReadOnlyCollection<BasketItem> option = None

    [<Inject>]
    member val Basket = Unchecked.defaultof<BasketState> with get, set

    [<Inject>]
    member val LogOutService = Unchecked.defaultof<LogOutService> with get, set

    [<Inject>]
    member val NavigationManager = Unchecked.defaultof<NavigationManager> with get, set

    [<CascadingParameter>]
    member val HttpContext = Unchecked.defaultof<HttpContext> with get, set

    member this.TotalQuantity =
        basketItems
        |> Option.map (fun items -> Seq.fold (fun s (i : BasketItem) -> s + i.Quantity) 0 items)
    override this.OnInitializedAsync () =
            basketStateSubscription <-
                Option.ofObj
                <| this.Basket.NotifyOnChange (EventCallback.Factory.Create (this, this.UpdateBasketItemsAsync))

            task {
                try
                    do! this.UpdateBasketItemsAsync ()
                with :? HttpRequestException as ex when ex.StatusCode ?= System.Net.HttpStatusCode.Unauthorized ->
                    do! this.LogOutService.LogOutAsync (this.HttpContext)


            }


    member this.UpdateBasketItemsAsync () =

            task {
                let! basketItems_ = this.Basket.GetBasketItemsAsync ()
                basketItems <- Option.ofObj basketItems_
                ()
            }


    interface IDisposable with
            member _.Dispose () =
                if basketStateSubscription.IsSome then
                    basketStateSubscription.Value.Dispose ()
            
    override this.Render() =
                let basketItemsNotEmpty =
                            if Option.isSome basketItems then
                                basketItems.Value.Count > 0
                            else
                                false
 
                html.fragment [|
                    stylesheet "View\Layout\CartMenu.fs.css"
                    a {
                        aria.label "cart"
                        href "cart"
                        img {
                            role "presentation"
                            src "icons/cart.svg"
                            if basketItemsNotEmpty then
                                span {
                                        class' "cart-badge"
                                        this.TotalQuantity
                                                    |> function
                                                        | Some q -> q.ToString ()
                                                        | None -> ""
                                     }
                            else 
                                html.none
                        }

                    }

                |]
