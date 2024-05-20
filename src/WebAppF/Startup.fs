module WebAppF.Program

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero
open Bolero.Server
open WebAppF
open Bolero.Templating.Server

open eShop.ServiceDefaults

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder (args)

    builder.Services
        .AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents ()
    |> ignore

    builder.Services.AddServerSideBlazor () |> ignore
    builder.Services.AddBoleroComponents () |> ignore
#if DEBUG
    builder.Services.AddHotReload (templateDir = __SOURCE_DIRECTORY__ + "/")
    |> ignore
#endif




    let appAdapter =
        { new IAdapter with
            member this.ClientID = "webappf" }

    builder.AddServiceDefaults () |> ignore
    builder.Services.AddHttpForwarder () |> ignore


    builder.AddApplicationServices (appAdapter)


    let app = builder.Build ()

    if app.Environment.IsDevelopment () then
        app.UseWebAssemblyDebugging ()

    app
        .UseAuthentication()
        .UseStaticFiles()
        .UseRouting()
        .UseAuthorization()
        .UseAntiforgery ()
    |> ignore

#if DEBUG
    app.UseHotReload ()
#endif
    app
        .MapRazorComponents<Index.Page>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode ()
    |> ignore


    app.MapForwarder ("/product-images/{id}", "http://catalog-api", "/api/catalog/items/{id}/pic")
    |> ignore

    app.Run ()
    0
