namespace WebAppF.Components

open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open eShop.WebApp.Services
open System
open Microsoft.AspNetCore.Http
open System.Collections.Generic
open System.Net.Http
open Microsoft.FSharp.Linq

module CartMenu =

    type CartMenu() =
        inherit Component()


        let mutable basketStateSubscription : IDisposable option = None
        let mutable basketItems : IReadOnlyCollection<BasketItem> option = None




        override _.CssScope = CssScopes.CartMenu


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


        override this.Render () =
            let basketItemsNotEmpty =
                if Option.isSome basketItems then
                    basketItems.Value.Count > 0
                else
                    false

            a {
                "aria-label" => "cart"
                attr.href "cart"

                img {
                    "role" => "presentation"
                    attr.src "icons/cart.svg"
                }

                if basketItemsNotEmpty then
                    span {
                        attr.``class`` "cart-badge"

                        this.TotalQuantity
                        |> function
                            | Some q -> q.ToString ()
                            | None -> ""

                    }
                else
                    empty ()



            }


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
