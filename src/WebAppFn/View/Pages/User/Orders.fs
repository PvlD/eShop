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
open Microsoft.AspNetCore.Authorization







    [<Route("/user/orders")>]
    [<Authorize()>]
    [<StreamRendering()>]
    type Orders() =
        inherit FunComponent()

        let  mutable orders:OrderRecord[] option = None

        

        [<Inject>]
        member val  OrderingService = Unchecked.defaultof<OrderingService > with get, set


        override   this.OnInitializedAsync ()=
                                task {
                                    let! orders_ = this.OrderingService.GetOrders()
                                    orders <- Option.ofObj orders_

                                }
                            


                            
         override this.Render() = 
                    html.fragment [|

                            
                            stylesheet "View/Pages/User/Orders.fs.css"

                            PageTitle'() { "Orders | Northern Mountains" }
                            SectionContent'() {
                                SectionName "page-header-title"
                                "Orders"
                            }
                            html.blazor<OrdersRefreshOnStatusChange>()
                            div { class' "orders";
                            orders |> function
                                | None ->
                                        p { "Loading..." }
                                | Some orders  when  orders.Length =0   ->
                                        p { "You haven't yet placed any orders." }
                                | Some orders ->
                                    html.fragment [|
                                        ul {
                                            class' "orders-list"
                                            li {
                                                class' "orders-header orders-item"
                                                div {"Number"}
                                                div {"Date"}
                                                div { class' "total-header"; "Total"}
                                                div {"Status"}
                                            }
                                            for order in orders do
                                                li {
                                                    class' "orders-item"
                                                    div {class' "order-number"; order.OrderNumber.ToString()}
                                                    div {class' "order-date"; order.Date.ToString()}
                                                    div { class' "order-total"; $"""${order.Total.ToString("0.00")}""" }
                                                    div { class' "order-status";
                                                        span {class' $"status {order.Status.ToLower()}"; order.Status.ToString()}
                                                    }                                                    
                                                }
                                        }
                                    |]

                            }
                        |]
                        
