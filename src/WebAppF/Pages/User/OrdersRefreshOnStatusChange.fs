
namespace WebAppF.Pages

open Pages
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Sections
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Routing
open Bolero
open Microsoft.AspNetCore.Components
open eShop.WebApp.Services
open Bolero.Html
open Microsoft.AspNetCore.Components.Authorization
open System



[<RequireQualifiedAccess>]
module OrdersRefreshOnStatusChange=

    [<BoleroRenderMode(BoleroRenderMode.Server)>]
    type Comp() =
        inherit Component()

        let mutable  orderStatusChangedSubscription:IDisposable option = None

        [<Inject>]
        member val  AuthenticationStateProvider = Unchecked.defaultof<AuthenticationStateProvider> with get, set
        [<Inject>]
        member val  OrderStatusNotificationService = Unchecked.defaultof<OrderStatusNotificationService> with get, set
        [<Inject>]
        member val  Nav = Unchecked.defaultof<NavigationManager > with get, set

        override this.Render() = empty()

        interface IDisposable with
            member this.Dispose() =
                    if orderStatusChangedSubscription.IsSome then
                            orderStatusChangedSubscription.Value.Dispose()


        member internal    this.HandleOrderStatusChanged() =
    
            try 
    
                this.Nav.Refresh()
    
            with (ex:Exception) ->
    
                // If there's an exception, we want to handle it on this circuit,
                // and not throw it to the upstream caller
                let _ = base.DispatchExceptionAsync(ex)
                ()

                    
        member  private this.callback() =
                base.InvokeAsync(this.HandleOrderStatusChanged)

        override this.OnAfterRenderAsync( firstRender:bool) =
            task {
                if firstRender then
                    let! buyerId = this.AuthenticationStateProvider.GetBuyerIdAsync()
                    if not <| String.IsNullOrEmpty(buyerId) then
                        

                        orderStatusChangedSubscription <- Option.ofObj <| this.OrderStatusNotificationService.SubscribeToOrderStatusNotifications(buyerId, fun _ -> this.callback())

            }

