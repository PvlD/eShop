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
open Microsoft.AspNetCore.Components.Authorization




    [<FunInteractiveServerAttribute()>]
    type OrdersRefreshOnStatusChange() =
        inherit FunComponent()
        let mutable  orderStatusChangedSubscription:IDisposable option = None

        [<Inject>]
        member val  AuthenticationStateProvider = Unchecked.defaultof<AuthenticationStateProvider> with get, set
        [<Inject>]
        member val  OrderStatusNotificationService = Unchecked.defaultof<OrderStatusNotificationService> with get, set
        [<Inject>]
        member val  Nav = Unchecked.defaultof<NavigationManager > with get, set

        override this.Render() = html.none

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

