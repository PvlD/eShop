namespace eShop.WebApp.Chatbot



open System.Text;
open System.Text.Encodings.Web;
open System.Text.RegularExpressions;
open Microsoft.AspNetCore.Components;



module MessageProcessor =

    let  AllowImages(message_:string ) =
    
        // Having to process markdown and deal with HTML encoding isn't ideal. If the language model could return
        // search results in some defined format like JSON we could simply loop over it in .razor code. This is
        // fine for now though.

        let  result = new StringBuilder();
        let mutable prevEnd = 0;
        let message = message_.Replace("&lt;", "<").Replace("&gt;", ">");

        for match_ in Regex.Matches(message_, @"\!?\[([^\]]+)\]\((http[^\)]+)\)") do
            let contentToHere = message.Substring(prevEnd, match_.Index - prevEnd);
            result.Append(HtmlEncoder.Default.Encode(contentToHere)) |> ignore
            result.Append($"""<img title=\"{(HtmlEncoder.Default.Encode(match_.Groups[1].Value))}\" src=\"{(HtmlEncoder.Default.Encode(match_.Groups[2].Value))}\" />""") |> ignore
            prevEnd <- match_.Index + match_.Length;

        result.Append(HtmlEncoder.Default.Encode(message.Substring(prevEnd)))|> ignore

        MarkupString(result.ToString())
    






