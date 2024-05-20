#nowarn "0020"

open System
open System.Reflection
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

open eShop.ServiceDefaults

let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())

builder.AddServiceDefaults () |> ignore

let services = builder.Services

services.AddRazorComponents().AddInteractiveServerComponents()
services.AddServerSideBlazor(fun x -> x.RootComponents.RegisterCustomElementForFunBlazor(Assembly.GetExecutingAssembly()))
services.AddFunBlazorServer()



let appAdapter =
    { new IAdapter with
        member this.ClientID = "webappfn" }



builder.AddApplicationServices (appAdapter)



let app = builder.Build()

app.UseStaticFiles()
app.UseAntiforgery()

app.MapRazorComponentsForSSR(Assembly.GetExecutingAssembly())
app.MapCustomElementsForSSR(Assembly.GetExecutingAssembly())

app.MapRazorComponents<WebAppFn.View.App>().AddInteractiveServerRenderMode()


app.MapForwarder ("/product-images/{id}", "http://catalog-api", "/api/catalog/items/{id}/pic")
|> ignore


app.Run()
