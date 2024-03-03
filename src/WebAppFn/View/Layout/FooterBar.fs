namespace WebAppFn.View.Layout

open Fun.Blazor

type FooterBar() =
    inherit FunComponent()

    override _.Render() = 
                html.fragment [|
                    stylesheet "View\Layout\FooterBar.fs.css"

                    footer {
                        class' "eshop-footer"
                        div {
                            class' "eshop-footer-content"
                            div {
                                class' "eshop-footer-row"
                                img {
                                    class' "logo logo-footer"
                                    src "images/logo-footer.svg"
                                    role "presentation"
                                }
                                p {
                                    "© Northern Mountains"
                                }
                            }

                        }
                    }
                |]                                        

