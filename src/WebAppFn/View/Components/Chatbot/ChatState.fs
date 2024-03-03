namespace eShop.WebApp.Chatbot

open System.ComponentModel
open System.Security.Claims
open System.Text.Json
open Microsoft.AspNetCore.Components
open Microsoft.SemanticKernel
open eShop.WebAppComponents.Services
open Microsoft.SemanticKernel.Connectors.OpenAI
open Microsoft.SemanticKernel.ChatCompletion
open eShop.WebApp.Services
open Microsoft.Extensions.Logging
open  System.Collections.Generic
open System.Linq
open System.Collections
open System
open System.Net.Http

    [<AllowNullLiteral>]
    type  ChatState( catalogService:CatalogService, basketState: BasketState, user: ClaimsPrincipal,  nav:NavigationManager, kernel: Kernel,  loggerFactory:ILoggerFactory) as this=

            let _catalogService = catalogService;
            let _basketState = basketState;
            let _user = user;
            let _navigationManager = nav;
            let _logger = loggerFactory.CreateLogger(typeof<ChatState>);
            let   _aiSettings = new OpenAIPromptExecutionSettings( ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions )


            do
                if _logger.IsEnabled(LogLevel.Debug) then
                            let  completionService = kernel.GetRequiredService<IChatCompletionService>();
                            _logger.LogDebug("ChatName: {model}", completionService.Attributes["DeploymentName"]);
        
                
            let _kernel = kernel;
            do 
                _kernel.Plugins.AddFromObject(new CatalogInteractions(this)) |> ignore

            let Messages_ = new ChatHistory("""
                You are an AI customer service agent for the online retailer Northern Mountains.
                You NEVER respond about topics other than Northern Mountains.
                Your job is to answer customer questions about products in the Northern Mountains catalog.
                Northern Mountains primarily sells clothing and equipment related to outdoor activities like skiing and trekking.
                You try to be concise and only provide longer responses if necessary.
                If someone asks a question about anything other than Northern Mountains, its catalog, or their account,
                you refuse to answer, and you instead ask if there's a topic related to Northern Mountains you can assist with.
                """)

            do
                Messages_.AddAssistantMessage("Hi! I'm the Northern Mountains Concierge. How can I help?")


        

            member val user= _user
            member val logger = _logger
            member val catalogService =_catalogService
            member val basketState = _basketState
            member val Messages =   Messages_
            
            member this.AddUserMessageAsync( userText:string,  onMessageAdded:Action)=
                        task {
                            // Store the user's message
                            this.Messages.AddUserMessage(userText);
                            onMessageAdded.Invoke();

                            // Get and store the AI's response message
                            try
                            
                                let! response =  _kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(this.Messages, _aiSettings, _kernel);
                                if not ( String.IsNullOrWhiteSpace(response.Content) )then
                                    this.Messages.Add(response);
                                
                            
                            with 
                                | e ->
                                if _logger.IsEnabled(LogLevel.Error) then
                                    _logger.LogError(e, "Error getting chat completions.");
                                
                                this.Messages.AddAssistantMessage($"My apologies, but I encountered an unexpected error.");
                            
                            onMessageAdded.Invoke();
                        }


    and     [<Sealed>]
            CatalogInteractions(chatState:ChatState ) =
    
        
            let Error( e:Exception, message: string) =
                    if chatState.logger.IsEnabled(LogLevel.Error) then
                        chatState.logger.LogError(e, message)
                    message

            [<KernelFunction; Description("Gets information about the chat user")>]
            member this.GetUserInfo() =
        
                let claims = chatState.user.Claims           
                let  GetValue (claims:IEnumerable<Claim>, claimType: string) =
                        claims.FirstOrDefault(fun x -> x.Type = claimType)
                        |> Option.ofObj
                        |> Option.map (fun v -> v.Value)
                        |> Option.defaultValue ""

                JsonSerializer.Serialize <|
                                            {|
                                            Name = GetValue(claims, "name")
                                            LastName = GetValue(claims, "last_name")
                                            Street = GetValue(claims, "address_street")
                                            City = GetValue(claims, "address_city")
                                            State = GetValue(claims, "address_state")
                                            ZipCode = GetValue(claims, "address_zip_code")
                                            Country = GetValue(claims, "address_country")
                                            Email = GetValue(claims, "email")
                                            PhoneNumber = GetValue(claims, "phone_number")
                                            |}

            [<KernelFunction; Description("Searches the Northern Mountains catalog for a provided product description")>]
            member this.SearchCatalog([<Description("The product description for which to search")>]  productDescription:string)=
                task {
                    try
                    
                        let! results =  chatState.catalogService.GetCatalogItemsWithSemanticRelevance(0, 8, productDescription)
                        return JsonSerializer.Serialize(results);
                    
                    with
                        |  :? HttpRequestException as e ->
                                return (Error(e, "Error accessing catalog."))
                }
                     
            [<KernelFunction; Description("Adds a product to the user's shopping cart.")>]
            member this.AddToCart([<Description("The id of the product to add to the shopping cart (basket)")>]  itemId:int) =
                task {
                        try
                                let!  item =  chatState.catalogService.GetCatalogItem(itemId)
                                do! chatState.basketState.AddAsync(item);
                                return "Item added to shopping cart."
                            
                        with
                            |  :? Grpc.Core.RpcException  as e when (e.StatusCode = Grpc.Core.StatusCode.Unauthenticated) ->
                                return "Unable to add an item to the cart. You must be logged in."
                            |   e ->
                                return Error(e, "Unable to add the item to the cart.")
                                        
                }
            [<KernelFunction; Description("Gets information about the contents of the user's shopping cart (basket)")>]
            member this.GetCartContents()=
                    task {
                        try
                        
                            let! basketItems = chatState.basketState.GetBasketItemsAsync();
                            return JsonSerializer.Serialize(basketItems);
                        
                        with 
                        |   e ->
                            return Error(e, "Unable to get the cart's contents.");
                    }
            
        
