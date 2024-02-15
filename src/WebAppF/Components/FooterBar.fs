namespace WebAppF.Components

open Bolero
open Bolero.Html

module FooterBar =

    type FooterBar() =
        inherit Component()


        override _.CssScope = CssScopes.FooterBar

        override this.Render () =
            footer {
                attr.``class`` "eshop-footer"

                div {
                    attr.``class`` "eshop-footer-content"

                    div {
                        attr.``class`` "eshop-footer-row"

                        img {
                            attr.``class`` "logo logo-footer"
                            "role" => "presentation"
                            attr.``src`` "images/logo-footer.svg"
                        }

                        p { "© Northern Mountains" }
                    }
                }
            }
