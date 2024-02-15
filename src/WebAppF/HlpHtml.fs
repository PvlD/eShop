module Hlp.Html 
open System
open Bolero
open System.Linq.Expressions
open Microsoft.AspNetCore.Components.CompilerServices
open Microsoft.AspNetCore.Components


let inline addNamedEvent (eventType:string , assignedName:string )
                                    = Node(fun _ tb i ->
                                            tb.AddNamedEvent(eventType, assignedName)
                                            i )


let inline formWithEvents(formName:string )
                                    = fun  (eventType: String )  ->
                                                         addNamedEvent (eventType,formName)    

                                                


let inline formWithOnsubmit(formName:string )  
                                    = formWithEvents formName  "onsubmit"    




module ValidationMessage =
    type  attr =
        static member  forField<'T> (e: Expression<Func<'T>>)=
                    Attr (fun _ tb i -> 
                        tb.AddAttribute(i,"For", e ) 
                        i + 1
                        )

module InputText =
        type  attr =
            static member  bindValue<'T> ((target :'T), getter: Expression<Func<String>>,(setter:string->unit) )=
                        Attr (fun _ tb i -> 


                            let getter_= getter.Compile()
                            tb.AddAttribute(i,"Value", getter_.Invoke())

                            tb.AddAttribute(i + 1 ,"ValueExpression", getter)
                            tb.AddAttribute(i + 2 ,"ValueChanged", RuntimeHelpers.TypeCheck<EventCallback<String>>(EventCallback.Factory.Create<String>(target,RuntimeHelpers.CreateInferredEventCallback
                                                                    (target , Action<string>( setter ) , getter_.Invoke() )) ) 
                                                                    )
                            i + 3
                            )




