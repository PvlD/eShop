module WebAppF.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Templating.Client

open WebAppF.Components.HeaderBar
open WebAppF.Components.FooterBar
open WebAppF.Pages
open Microsoft.JSInterop



/// Routing endpoints definition.
type Page =
    | [<EndPoint "/?brand={brandId}&page={page}&type={itemTypeId}">] Home of      brandId : int option *      page : int option *      itemTypeId : int option
    | [<EndPoint "/item/{itemId}">] Item of itemId : int
    | [<EndPoint "/cart">] Cart
    | [<EndPoint "/user/orders">] UserOrders
    | [<EndPoint "/checkout">] Checkout
    | [<EndPoint "/user/login">] UserLogin
    | [<EndPoint "/user/logout">] UserLogout



/// The Elmish application's model.
type Model = { page : Page ; error : string option }


let initModel =
    {
        page = Home (None, None, None)
        error = None
    }




/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Error of exn
    | ClearError

let update (js : IJSRuntime) message model =
    match message with
    | SetPage page -> { model with page = page }, Cmd.none

    | Error exn -> { model with error = Some exn.Message }, Cmd.none
    | ClearError -> { model with error = None }, Cmd.none



/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">



let view model dispatch =
    let isCatalog =
        model.page
        |> function
            | Home _ -> true
            | _ -> false

    Main()
        .HeaderBar(comp<HeaderBar> { "IsCatalog" => fun () -> isCatalog })
        .Body(
            cond model.page
            <| function
                | Home (brandId, page, itemTypeId) ->
                    comp<Catalog.Page> {
                        "brandId" => brandId
                        "page" => page
                        "itemTypeId" => itemTypeId

                    }


                | Item itemId -> comp<ItemPage.Page> { "ItemId" => itemId }
                | Cart -> comp<CartPage.Page> { attr.empty () }
                | UserOrders -> comp<UserOrders.Page> { attr.empty () }
                | Checkout -> comp<CheckoutPage.Page> { attr.empty () }
                | UserLogin -> comp<UserLogIn.Page> { attr.empty () }
                | UserLogout -> comp<UserLogOut.Page> { attr.empty () }


        )
        .FooterBar(comp<FooterBar> { attr.empty () })
        .Elt ()

type App() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.App

    override this.Program =
        let update = update this.JSRuntime

        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg (SetPage (Home (None, None, None)))) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
