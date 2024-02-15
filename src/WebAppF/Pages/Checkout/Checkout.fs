namespace WebAppF.Pages

open System
open Bolero
open Microsoft.AspNetCore.Components
open eShop.WebAppComponents.Services
open eShop.WebApp.Services
open eShop.WebAppComponents.Catalog
open WebAppF.Pages.Pages
open Bolero.Html
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Http
open WebAppF.Components
open Hlp.Html
open System.Collections.Generic
open System.Linq
open Microsoft.AspNetCore.Authorization
open System.ComponentModel.DataAnnotations
open System.Globalization
open Microsoft.FSharp.Linq.RuntimeHelpers
open Microsoft.AspNetCore.Components.CompilerServices


[<RequireQualifiedAccess>]
module CheckoutPage=

    [<Route("/checkout")>]
    [<Authorize()>]
    type Page() =
        inherit Component()

        let mutable  editContext:EditContext = null
        let mutable  extraMessages:ValidationMessageStore  = null



        
        

        member inline private  this.ParseExpiryDate( mmyy:string) =
                                        match  DateTime.TryParseExact($"01/{mmyy}", "dd/MM/yy", null, DateTimeStyles.None)  with 
                                            | true, v -> Nullable(v.ToUniversalTime())
                                            | _ -> Nullable()


        override this.CssScope = CssScopes.Checkout

        

        [<SupplyParameterFromForm>]
        member val   Info:BasketCheckoutInfo =null with get, 
                                                        set



        [<Required>]
        [<SupplyParameterFromForm>]
        member inline this.CardExpirationString
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
                  concat {
                      addPageTitle "Checkout | Northern Mountains"
                      addPageHeaderTitle "Checkout"
                      div {
                          attr.``class``  "checkout"
                          comp<EditForm> {
                              attr.``class`` "checkout-form"
                              "FormName" => "checkout"
                              "EditContext" => editContext
                              "data-enhance" => true
                              "OnSubmit" => RuntimeHelpers.TypeCheck<EventCallback<Forms.EditContext>>(EventCallback.Factory.Create<Forms.EditContext>(this,System.Func<System.Threading.Tasks.Task>(fun () -> this.HandleSubmitAsync() )))

                              attr.fragmentWith "ChildContent" (fun (editContext: EditContext) ->
                                 concat {
                                        
                                  comp<DataAnnotationsValidator>{attr.empty()}
                                  div {
                                        attr.``class`` "form"
                                        div{
                                            attr.``class`` "form-section"
                                            h2 {
                                                text("Shipping address")
                                            }
                                            label{
                                                "Address"
                                                comp<InputText>{
                                                    InputText.attr.bindValue (this,(fun ()->this.Info.Street),(fun (v ) -> this.Info.Street <- v ))
                                                    }

                                                comp<ValidationMessage<String>>
                                                    {
                                                        
                                                        ValidationMessage.attr.forField(fun () -> this.Info.Street)
                                                    }
                                            }
                                            div {
                                                attr.``class`` "form-group"
                                                div {
                                                    attr.``class`` "form-group-item"
                                                    label {
                                                         text ("City")
                                                         comp<InputText>{
                                                                InputText.attr.bindValue (this,(fun ()->this.Info.City ),(fun (v ) -> this.Info.City <- v ))
                                                         }
                                                         comp<ValidationMessage<String>>
                                                             {      
                                                                    ValidationMessage.attr.forField(fun () -> this.Info.City)
                                                             }

                                                    }
                                                }
                                                div {
                                                    attr.``class`` "form-group-item"
                                                    label {
                                                         text ("State")
                                                         comp<InputText>{
                                                                InputText.attr.bindValue (this,(fun ()->this.Info.State ),(fun (v ) -> this.Info.State <- v ))

                                                         }
                                                         comp<ValidationMessage<String>>
                                                             {
                                                                ValidationMessage.attr.forField(fun () -> this.Info.State)
                                                             }

                                                    }
                                                }

                                                div {
                                                     attr.``class`` "form-group-item"
                                                     
                                                     label {
                                                            text ("Zip code")
                                                            comp<InputText>{
                                                                    InputText.attr.bindValue (this,(fun ()->this.Info.ZipCode ),(fun (v ) -> this.Info.ZipCode <- v ))

                                                            }
                                                            comp<ValidationMessage<String>>
                                                                {
                                                                   ValidationMessage.attr.forField(fun () -> this.Info.ZipCode)
                                                                }

                                                     }
                                                                                                      

                                                }
                                                }    
                                            div {
                                                label {
                                                    text ("Country")
                                                    comp<InputText>{
                                                            InputText.attr.bindValue (this,(fun ()->this.Info.Country ),(fun (v ) -> this.Info.Country <- v ))

                                                    }
                                                    comp<ValidationMessage<String>>
                                                        {
                                                           ValidationMessage.attr.forField(fun () -> this.Info.Country)
                                                    }
                                                }
                                            }    
                                        }
                                        div {
                                            attr.``class`` "form-section"
                                            h2 {
                                                text("Payment method")
                                            }
                                            label {
                                                text ("CardHolder name")
                                                comp<InputText>{
                                                        InputText.attr.bindValue (this,(fun ()->this.Info.CardHolderName ),(fun (v ) -> this.Info.CardHolderName <- v ))

                                                    }
                                                comp<ValidationMessage<String>>
                                                    {
                                                        ValidationMessage.attr.forField(fun () -> this.Info.CardHolderName)
                                                    }

                                                }

                                            div {
                                                attr.``class`` "form-group"
                                             
                                                div {
                                                        attr.``class`` "form-group-item"
                                                        label {
                                                            text ("Card number")
                                                            comp<InputText>{
                                                                    InputText.attr.bindValue (this,(fun ()->this.Info.CardNumber ),(fun (v ) -> this.Info.CardNumber <- v ))
                                                            }
                                                            comp<ValidationMessage<String>>
                                                                {
                                                                    ValidationMessage.attr.forField(fun () -> this.Info.CardNumber)
                                                                }

                                                        }
                                                    }

                                                div {
                                                        attr.``class`` "form-group-item"
                                                        label {
                                                            text ("Expiration date")
                                                            comp<InputText>{
                                                                    InputText.attr.bindValue (this,(fun ()->this.CardExpirationString ),(fun (v ) -> this.CardExpirationString <- v ))
                                                            }
                                                            comp<ValidationMessage<String>>
                                                                {
                                                                    ValidationMessage.attr.forField(fun () -> this.CardExpirationString)
                                                                }

                                                        }
                                                    }
                                                div {
                                                        attr.``class`` "form-group-item"
                                                        label {
                                                            text ("Security code")
                                                            comp<InputText>{
                                                                    InputText.attr.bindValue (this,(fun ()->this.Info.CardSecurityNumber ),(fun (v ) -> this.Info.CardSecurityNumber <- v ))
                                                            }                                                        
                                                            comp<ValidationMessage<String>>
                                                                {
                                                                    ValidationMessage.attr.forField(fun () -> this.Info.CardSecurityNumber) 
                                                                }
                                                        }
                                                    }
                                                }
                                        }                                  
                                        div {
                                            attr.``class`` "form-section"
                                            div {
                                                attr.``class`` "form-buttons"
                                                a {
                                                    attr.href  "chart"
                                                    attr.``class`` "button button-secondary"
                                                    img {
                                                        "role" => "presentation"
                                                        attr.src "icons/arrow-left.svg"

                                                    }
                                                    text "Back to shopping cart" 
                                                }
                                                button {
                                                    attr.``class`` "button button-primary"
                                                    attr.``type`` "submit"
                                                    text "Place order"
                                                }
                                            }
                                        }
                                        }
                                  comp<ValidationSummary>{
                                                  attr.empty()
                                              }
                                    })
                                  }
                              
                              }


                                

                            }
                    






