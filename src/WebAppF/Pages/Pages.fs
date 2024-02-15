namespace WebAppF.Pages

open Bolero.Html
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Sections

module Pages =

    type PageDescription =
        { pageTitle : string
          page_header_title : string
          page_header_subtitle : string }



    let addPageHeaderTitle txt =
        comp<SectionContent> {
            attr.fragment "ChildContent" (text txt)
            "SectionName" => "page-header-title"
        }

    let addPageTitle txt =
        comp<PageTitle> { attr.fragment "ChildContent" (text txt) }

    let addPageDescriptionComp (pageDescription : PageDescription) =

        concat {
            addPageTitle pageDescription.pageTitle

            addPageHeaderTitle pageDescription.page_header_title


            comp<SectionContent> {
                attr.fragment "ChildContent" (text pageDescription.page_header_subtitle)

                "SectionName" => "page-header-subtitle"
            }


        }
