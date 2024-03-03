namespace WebAppFn.View.Pages
open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Fun.Blazor
open eShop.WebAppComponents.Services
open eShop.WebAppComponents.Catalog
open Microsoft.AspNetCore.Components.Routing






[<Route "/">]
type Catalog() =
    inherit FunComponent()

    
    let mutable   catalogResult:CatalogResult=null;

    let pageSize= 9

    let GetVisiblePageIndexes (result : CatalogResult, pageSize : int) =

        let maxP =
            (result.Count / pageSize, result.Count % pageSize)
            |> function
                | (0, 0) -> 0
                | (0, _) -> 1
                | (d, 0) -> d
                | (d, _) -> d + 1


        seq { 1..maxP }




    let  PageIndexToUri (nav : NavigationManager, pageIndex : int) =
        nav.GetUriWithQueryParameter (
            "page",
            pageIndex
            |> function
                | 1 -> Nullable ()
                | _ -> Nullable pageIndex
        )


    [<Inject>]
    member val CatalogService = Unchecked.defaultof<CatalogService> with get, set

    [<Inject>]
    member val Nav = Unchecked.defaultof<NavigationManager> with get, set

    [<SupplyParameterFromQuery>]
    member val   ReturnUrl = null: string with  get, set 



    [<SupplyParameterFromQuery>]
    member val Page:Nullable<int> = Nullable()   with  get, set 

    [<SupplyParameterFromQuery(Name = "brand")>]
    member val BrandId:Nullable<int> = Nullable()   with  get, set 


    [<SupplyParameterFromQuery(Name = "type")>]
    member val ItemTypeId:Nullable<int> = Nullable()   with  get, set 

    override this.OnInitializedAsync() = 
        task {
            let! catalogResult_ =
                this.CatalogService.GetCatalogItems (
                    ((1, this.Page |> Option.ofNullable ) ||> Option.defaultValue) - 1,
                    pageSize,
                    this.BrandId ,
                    this.ItemTypeId 
                )

            catalogResult <- catalogResult_
            return ()
        }


    override this.Render() =
        html.fragment [| 

            stylesheet "View/Pages/Catalog/Catalog.fs.css"


            PageTitle'() { "Northern Mountains" }
            SectionContent'() {
                SectionName "page-header-title"
                "Ready for a new adventure?"
            }
            SectionContent'() {
               SectionName "page-header-subtitle"
               "Start the season with the latest in clothing and equipment."
            }
            div {
                    class'  "catalog" 
                    html.blazor (
                    ComponentAttrBuilder<CatalogSearch>()
                        .Add((fun x -> x.BrandId), this.BrandId)
                        .Add((fun x -> x.ItemTypeId), this.ItemTypeId)
                    )

                    if not (isNull catalogResult) then
                        div {
                            div {
                                class' "catalog-items"
                                for item in catalogResult.Data do
                                    html.blazor(
                                        ComponentAttrBuilder<CatalogListItem>()
                                            .Add((fun x -> x.Item ), item)
                                    )
                                }
                            div {
                                 class' "page-links"
                                 for pageIndex in GetVisiblePageIndexes(catalogResult, pageSize) do
                                  NavLink'() {
                                    ActiveClass "active-page"
                                    Match NavLinkMatch.All
                                    AdditionalAttributes (Map [("href",$"{PageIndexToUri (this.Nav, pageIndex)}");("class","catalog")])
                                    $"{pageIndex}"
                                  }

                                }
                        }
                    else 
                        p { "Loading..." }

                }
        |]


