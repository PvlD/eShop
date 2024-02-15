namespace WebAppF.Pages

open eShop.WebAppComponents.Catalog
open eShop.WebAppComponents.Services
open System.Linq
open Microsoft.AspNetCore.Components.Routing
open Pages
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Sections

[<RequireQualifiedAccess>]
module Catalog =

    open System

    open Microsoft.JSInterop
    open Microsoft.AspNetCore.Components

    open Elmish
    open Bolero
    open Bolero.Html


    let private pageDescription : PageDescription =
        { pageTitle = "Northern Mountains"
          page_header_title = "Ready for a new adventure?"
          page_header_subtitle = "Start the season with the latest in clothing and equipment." }


    let pageSize : int = 9

    let private GetVisiblePageIndexes (result : CatalogResult, pageSize : int) =

        let maxP =
            (result.Count / pageSize, result.Count % pageSize)
            |> function
                | (0, 0) -> 0
                | (0, _) -> 1
                | (d, 0) -> d
                | (d, _) -> d + 1


        seq { 1..maxP }




    let private PageIndexToUri (nav : NavigationManager, pageIndex : int) =
        nav.GetUriWithQueryParameter (
            "page",
            pageIndex
            |> function
                | 1 -> Nullable ()
                | _ -> Nullable pageIndex
        )




    type Page() =
        inherit Component()

        let mutable catalogResult : CatalogResult option = None


        override _.CssScope = CssScopes.Catalog




        [<Parameter>]
        member val Page = None : int option with get, set

        [<Parameter>]
        member val BrandId = None : int option with get, set

        [<Parameter>]
        member val ItemTypeId = None : int option with get, set




        [<Inject>]
        member val CatalogService = Unchecked.defaultof<CatalogService> with get, set

        [<Inject>]
        member val Nav = Unchecked.defaultof<NavigationManager> with get, set


        override this.OnAfterRenderAsync (firstRender : bool) = base.OnAfterRenderAsync (firstRender)




        override this.OnInitializedAsync () =
            task {
                let! catalogResult_ =
                    this.CatalogService.GetCatalogItems (
                        ((1, this.Page) ||> Option.defaultValue) - 1,
                        pageSize,
                        this.BrandId |> Option.toNullable,
                        this.ItemTypeId |> Option.toNullable
                    )

                catalogResult <- Option.ofObj catalogResult_
                return ()
            }

        override this.Render () =

            concat {

                comp<PageTitle> { attr.fragment "ChildContent" (text pageDescription.pageTitle) }

                comp<SectionContent> {
                    attr.fragment "ChildContent" (text pageDescription.page_header_title)
                    "SectionName" => "page-header-title"
                }

                comp<SectionContent> {
                    attr.fragment "ChildContent" (text pageDescription.page_header_subtitle)

                    "SectionName" => "page-header-subtitle"
                }



                div {
                    attr.``class`` "catalog"


                    comp<CatalogSearch> {
                        "BrandId" => Option.toNullable this.BrandId
                        "ItemTypeId" => Option.toNullable this.ItemTypeId
                    }


                    cond catalogResult
                    <| function
                        | None -> p { "Loading..." }
                        | Some catalog ->
                            div {
                                div {
                                    attr.``class`` "catalog-items"
                                    forEach catalog.Data <| fun item -> comp<CatalogListItem> { "Item" => item }
                                }

                                div {
                                    attr.``class`` "page-links"

                                    forEach (GetVisiblePageIndexes (catalog, pageSize))
                                    <| fun pageIndex ->

                                        navLink NavLinkMatch.All {
                                            "ActiveClass" => "active-page"
                                            attr.href $"{PageIndexToUri (this.Nav, pageIndex)}"
                                            text $"{pageIndex}"
                                        }

                                }

                            }


                }
            }
