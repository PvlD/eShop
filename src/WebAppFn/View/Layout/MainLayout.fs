namespace WebAppFn.View.Layout

open Microsoft.AspNetCore.Components
open Fun.Blazor
open WebAppFn.Components.Chatbot

type MainLayout() as this =
    inherit LayoutComponentBase()

    let content = 
        html.fragment [|
            html.blazor<HeaderBar> ()
            div { this.Body }
            html.blazor<ShowChatbotButton> ()
            html.blazor<FooterBar> ()
            //ComponentBuilder<FooterBar>.create()

        |]

    override _.BuildRenderTree(builder) = content.Invoke(this, builder, 0) |> ignore
