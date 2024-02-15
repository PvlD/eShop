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


[<RequireQualifiedAccess>]
module UserOrders=

    [<Route("/user/orders")>]
    [<Authorize()>]
    type Page() =
        inherit Component()

        let  mutable orders:OrderRecord[] option = None

        override _.CssScope = CssScopes.Orders        

        [<Inject>]
        member val  OrderingService = Unchecked.defaultof<OrderingService > with get, set


        override   this.OnInitializedAsync ()=
                                task {
                                    let! orders_ = this.OrderingService.GetOrders()
                                    orders <- Option.ofObj orders_

                                }
                            


                            
         override this.Render() = 
                        concat {
                            addPageTitle "Orders | Northern Mountains"
                            addPageHeaderTitle "Orders"
                            comp<OrdersRefreshOnStatusChange.Comp> { attr.empty ()}
                            div {
                                attr.``class`` "orders"
                                orders |> function
                                    | None -> 
                                            p{"Loading..."}
                                    | Some orders when  orders.Length =0  -> 
                                            p{"You haven't yet placed any orders."}
                                    | Some orders ->
                                      concat {
                                        ul {
                                            attr.``class`` "orders-list"
                                            li {
                                                attr.``class`` "orders-header orders-item"
                                                div{
                                                    text("Number")
                                                }
                                                div{
                                                    text("Date")
                                                }
                                                div{
                                                    attr.``class`` "total-header"
                                                    text("Total")
                                                }
                                                div{
                                                    text("Status")
                                                }
                                            }        
                                        }

                                        forEach orders <| fun order -> 
                                                        li {
                                                            attr.``class`` "orders-item"
                                                            div{
                                                                attr.``class`` "orders-number"
                                                                text(order.OrderNumber.ToString())
                                                            }
                                                            div{
                                                                attr.``class`` "orders-date"
                                                                text(order.Date.ToString())
                                                            }
                                                            div{
                                                                attr.``class`` "orders-total"
                                                                text( $"""${order.Total.ToString("0.00")}""")
                                                            }
                                                            div{
                                                                attr.``class`` "orders-status"
                                                                span{
                                                                    attr.``class`` $"status {order.Status}"
                                                                    text(order.Status.ToString())
                                                                }
                                                                
                                                            }

                                                        }
                                        }        
                                                
                            
                            }

                        }

