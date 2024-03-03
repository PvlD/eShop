namespace WebAppFn.View.Pages
open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Fun.Blazor
open eShop.WebAppComponents.Services
open eShop.WebAppComponents.Catalog
open Microsoft.AspNetCore.Components.Routing
open eShop.WebApp.Services
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Components.Forms

open Microsoft.AspNetCore.Authorization
open System.Collections.Generic
open System.Linq
open System.ComponentModel.DataAnnotations
open System.Globalization





[<Route "/checkout">]
[<Authorize()>]
type Checkout() =
        inherit FunComponent()

        let mutable  editContext:EditContext = null
        let mutable  extraMessages:ValidationMessageStore  = null






        member inline private  this.ParseExpiryDate( mmyy:string) =
                                        match  DateTime.TryParseExact($"01/{mmyy}", "dd/MM/yy", null, DateTimeStyles.None)  with 
                                            | true, v -> Nullable(v.ToUniversalTime())
                                            | _ -> Nullable()


        



        [<SupplyParameterFromForm>]
        member val   Info:BasketCheckoutInfo =null with get, 
                                                        set



        [<Required>]
        [<SupplyParameterFromForm>]
        member  this.CardExpirationString
                with get () = this.Info.CardExpiration 
                                |> Option.ofNullable 
                                |> Option.map (fun v -> v.ToString("MM/yy")) 
                                |> Option.defaultValue null

                and set (value) = this.Info.CardExpiration <- this.ParseExpiryDate(value)


        [<Inject>]
        member val  Nav = Unchecked.defaultof<NavigationManager > with get, set
        [<Inject>]
        member val  Basket = Unchecked.defaultof<BasketState> with get, set


        [<CascadingParameter>]
        member val  HttpContext = Unchecked.defaultof<HttpContext> with get, set


        member  private this.PopulateFormWithDefaultInfo()=

               let ReadClaim( ``type``:string)=
                             this.HttpContext.User.Claims.FirstOrDefault(fun x -> x.Type = ``type``)
                             |> Option.ofObj
                             |> Option.map (fun v -> v.Value)
                             |> Option.defaultValue null

   
               this.Info <- new BasketCheckoutInfo
                                       (
                                           Street = ReadClaim("address_street"),
                                           City = ReadClaim("address_city"),
                                           State = ReadClaim("address_state"),
                                           Country = ReadClaim("address_country"),
                                           ZipCode = ReadClaim("address_zip_code"),
                                           CardNumber = ReadClaim("card_number"),
                                           CardHolderName = ReadClaim("card_holder"),
                                           CardSecurityNumber = ReadClaim("card_security_number"),
                                           RequestId = Guid.NewGuid()
                                       )

 

               this.CardExpirationString <- ReadClaim("card_expiration");

   

        member private this.HandleSubmitAsync() =
                task {
                    do! this.PerformCustomValidationAsync()

                    if editContext.Validate() then 
                        do! this.HandleValidSubmitAsync()
            
                }

        member private this.HandleValidSubmitAsync() =
                    task {
                           this.Info.CardTypeId <- 1
                           do! this.Basket.CheckoutAsync(this.Info)
                           this.Nav.NavigateTo("user/orders")
                    }

        member private this.PerformCustomValidationAsync() =
           task {
               extraMessages.Clear();
               if not <| this.Info.CardExpiration.HasValue then 
                   let identifier = FieldIdentifier.Create(fun () -> this.CardExpirationString) 
                   extraMessages.Add(&identifier, "Must be valid mm/yy")
                   editContext.NotifyValidationStateChanged()

               let! bitems  =  this.Basket.GetBasketItemsAsync()
               if bitems.Count = 0 then 
                   let identifier =  new FieldIdentifier(this.Info, "")
                   extraMessages.Add(&identifier, "Your cart is empty")
       
           }    


        override this.OnInitialized()=
                  if this.Info = null then 
                         this.PopulateFormWithDefaultInfo()
          
                  editContext <- new EditContext(this.Info);
                  extraMessages <- new ValidationMessageStore(editContext)




        override this.Render() =
                       html.fragment [|
                          stylesheet "View/Pages/Checkout/Checkout.fs.css"

                          PageTitle'() { "Checkout | Northern Mountains" }
                          SectionContent'() {
                              SectionName "page-header-title"
                              "Checkout"
                          }
                          div {
                              class' "checkout"
                              EditForm'() {
                                  FormName "checkout"
                                  EditContext editContext
                                  OnSubmit (fun _ -> this.HandleSubmitAsync())
                                  ChildContent  (fun (editContext: EditContext)  ->
                                    html.fragment [|
                                        html.blazor<DataAnnotationsValidator>()
                                        div {
                                                class' "form"
                                                div {
                                                    class' "form-section"
                                                    h2{
                                                    "Shipping Address"
                                                    }
                                                    label {
                                                    "Address"
                                                    InputText'() {
                                                        Value' (this.Info.Street,(fun v -> this.Info.Street <- v))
                                                        ValueExpression (fun () -> this.Info.Street)
                                                    }
                                                    ValidationMessage'() {
                                                        For' (fun () -> this.Info.Street)
                                                    }
                                                    }
                                                    div {
                                                        class' "form-group"
                                                        div {
                                                        class' "form-group-item"
                                                        label {
                                                            "City"
                                                            InputText'() {
                                                                Value' (this.Info.City,(fun v -> this.Info.City <- v))
                                                                ValueExpression (fun () -> this.Info.City)
                                                            }
                                                            ValidationMessage'() {
                                                                For' (fun () -> this.Info.City)
                                                            }
                                                        }
                                                        }
                                                        div {
                                                        class' "form-group-item"
                                                        label {
                                                            "State"
                                                            InputText'() {
                                                                Value' (this.Info.State,(fun v -> this.Info.State <- v))
                                                                ValueExpression (fun () -> this.Info.State)
                                                            }
                                                            ValidationMessage'() {
                                                                For' (fun () -> this.Info.State)
                                                            }
                                                        }
                                                        }
                                                        div {
                                                        class' "form-group-item"
                                                        label {
                                                            "Zip code"
                                                            InputText'() {
                                                                Value' (this.Info.ZipCode,(fun v -> this.Info.ZipCode <- v))
                                                                ValueExpression (fun () -> this.Info.ZipCode)
                                                            }
                                                            ValidationMessage'() {      
                                                                For' (fun () -> this.Info.ZipCode)
                                                            }

                                                        }
                                                        }
                                                    }
                                                    div {
                                                        label {
                                                            "Country"
                                                            InputText'() {
                                                                Value' (this.Info.Country,(fun v -> this.Info.Country <- v))
                                                                ValueExpression (fun () -> this.Info.Country)
                                                            }
                                                            ValidationMessage'() {
                                                                For' (fun () -> this.Info.Country)
                                                            }
                                                        }
                                                    }

                                                }
                                                div {
                                                    class' "form-section"
                                                    h2{
                                                        "Payment method"
                                                        label {
                                                            "Card name"
                                                            InputText'() {
                                                                Value' (this.Info.CardHolderName,(fun v -> this.Info.CardHolderName <- v))
                                                                ValueExpression (fun () -> this.Info.CardHolderName)
                                                            }
                                                            ValidationMessage'() {
                                                                For' (fun () -> this.Info.CardHolderName)
                                                            }


                                                        }
                                                        div {
                                                            class' "form-group"
                                                            div {
                                                                class' "form-group-item"
                                                                label {
                                                                    "Card number"
                                                                    InputText'() {
                                                                        Value' (this.Info.CardNumber,(fun v -> this.Info.CardNumber <- v))
                                                                        ValueExpression (fun () -> this.Info.CardNumber)
                                                                    }
                                                                    ValidationMessage'() {
                                                                        For' (fun () -> this.Info.CardNumber)
                                                                    }
                                                                }
                                                            }
                                                            div {
                                                                class' "form-group-item"
                                                                label {
                                                                    "Expiration date"
                                                                    InputText'() {
                                                                        Value' (this.CardExpirationString,(fun v -> this.CardExpirationString <- v))
                                                                        ValueExpression (fun () -> this.CardExpirationString)
                                                                    }
                                                                    ValidationMessage'() {
                                                                        For' (fun () -> this.CardExpirationString)
                                                                    }

                                                                }
                                                            }
                                                            div {
                                                                class' "form-group-item"
                                                                label {
                                                                    "Security code"        
                                                                    InputText'() {
                                                                        Value' (this.Info.CardSecurityNumber,(fun v -> this.Info.CardSecurityNumber <- v))
                                                                        ValueExpression (fun () -> this.Info.CardSecurityNumber)
                                                                    }
                                                                    ValidationMessage'() {
                                                                        For' (fun () -> this.Info.CardSecurityNumber)
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                div {
                                                    class' "form-section"
                                                    div {
                                                        class' "form-buttons"
                                                        a{
                                                            class' "button button-secondary"
                                                            href "cart"
                                                            img{
                                                                role "presentation"
                                                                src "icons/arrow-left.svg"
                                                            }
                                                            "Back to the shopping bag"
                                                        }
                                                        button{
                                                            type' "submit"
                                                            class' "button button-primary"
                                                            "Place order"
                                                        }
                                                    }
                                                }
                                            }
                                        html.blazor<ValidationSummary>() 
                                  |]
                                )
                              }
                          }

                |]
