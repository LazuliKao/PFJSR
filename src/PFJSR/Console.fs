namespace PFJSR
open System.Drawing
open System
module Console=
    let PluginName="PFJSR"
    let WriteLineT(subtype:obj)(content :obj)=
        printfn "[INFO][%s]|%s|%s" 
            PluginName 
            (content.ToString()) 
            (subtype.ToString())
    let WriteLine(content :obj)=
        printfn "[INFO][%s]%s"
             PluginName
             (content.ToString())
        //Colorful.Console.WriteLineFormatted("{0}{3}{1}{0}{2}{1}{4}\t", Console.ForegroundColor,
        //   new Formatter("[", Color.LightSteelBlue),
        //   new Formatter("]", Color.LightSteelBlue),
        //   new Formatter("PFJSR", Color.LightGreen),
        //   new Formatter("INFO", Color.Gray),
        //   new Formatter(content, Color.LightGoldenrodYellow)
        //)
        //Console.ForegroundColor<-Color.White
    let WriteLineWarn(content:obj,tip:string)=
        printfn "[INFO][%s]%s --> %s"
            PluginName
            (content.ToString())
            (tip.ToString())
    let WriteLineErr(content:obj,ex:exn)=
        if typeof<API.PFJsrException> = ex.GetType() then
            printfn "[INFO][%s]%s --> %s"
                PluginName
                (content.ToString())
                ex.Message
        else
            printfn "[INFO][%s]%s\n%s"
                PluginName
                (content.ToString())
                (ex.ToString())
        //Colorful.Console.WriteLineFormatted("{0}{2} {3}{1}{4}\n\t{5}\n{6}\t", Console.ForegroundColor,
        //   new Formatter("[", Color.LightGoldenrodYellow),
        //   new Formatter("]", Color.LightGoldenrodYellow),
        //   new Formatter("PFJSR", Color.Cyan),
        //   new Formatter("ERROR", Color.MediumVioletRed),
        //   new Formatter(content, Color.OrangeRed),
        //   new Formatter(ex.Message, Color.Orange),
        //   new Formatter(ex.StackTrace, Color.Gray)
        //)
        //Console.ForegroundColor<-Color.White
    let log(content:string)=
        printfn "%s" content
        //(content,Color.LightYellow)|>Colorful.Console.WriteLine
