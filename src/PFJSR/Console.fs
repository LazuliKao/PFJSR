namespace PFJSR
open System.Drawing
open System
module Console=
    let PluginName="PFJSR"
    let WriteLineT(subtype:obj)(content :obj)=
        printfn "\x1b[38;2;176;196;222m[\x1b[38;2;169;169;169mINFO\x1b[38;2;176;196;222m][\x1b[38;2;167;132;239m%s\x1b[38;2;176;196;222m]\x1b[38;2;0;255;127m|\x1b[38;2;255;0;255m%s\x1b[38;2;0;255;127m|\x1b[38;2;250;250;210m%s\x1b[0m" 
            PluginName 
            (content.ToString()) 
            (subtype.ToString())
    let WriteLine(content :obj)=
        printfn "\x1b[38;2;176;196;222m[\x1b[38;2;169;169;169mINFO\x1b[38;2;176;196;222m][\x1b[38;2;167;132;239m%s\x1b[38;2;176;196;222m]\x1b[38;2;250;250;210m%s\x1b[0m"
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
        printfn "\x1b[38;2;240;128;128m[\x1b[38;2;253;99;71mWARN\x1b[38;2;240;128;128m][\x1b[38;2;167;132;239m%s\x1b[38;2;240;128;128m]\x1b[38;2;220;138;138m%s\x1b[38;2;152;201;120m-->\x1b[4m\x1b[38;2;127;255;1m\x1b[48;2;25;25;112m%s\x1b[0m"
            PluginName
            (content.ToString())
            (tip.ToString())
    let WriteLineErr(content:obj,ex:exn)=
        match ex with
            | :? API.PFJsrException -> WriteLineWarn(content,ex.Message)
            | :? Jint.Runtime.JavaScriptException as jex -> 
                printfn "\x1b[93m\x1b[41m[\x1b[0m\x1b[101m\x1b[4mERROR\x1b[0m\x1b[93m\x1b[41m]\x1b[0m\x1b[38;2;138;143;226m[\x1b[38;2;167;132;239m%s\x1b[38;2;138;143;226m]\x1b[38;2;234;47;39m%s\n\x1b[38;2;175;238;238m\tJavaScriptException >>>\n\t信息:\t\x1b[38;2;147;147;119m%s\n\x1b[38;2;175;238;238m\t位于:\t\x1b[38;2;147;147;119m第 \x1b[38;2;147;147;119m\x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 行  \x1b[38;2;147;147;119m\x1b[38;2;147;147;119m第 \x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 列\x1b[0m"
                        PluginName
                        (content.ToString())
                        (jex.Message)
                        (jex.LineNumber)
                        (jex.Column)
            | _ -> printfn "\x1b[93m\x1b[41m[\x1b[0m\x1b[101m\x1b[4mERROR\x1b[0m\x1b[93m\x1b[41m]\x1b[0m\x1b[38;2;138;143;226m[\x1b[38;2;167;132;239m%s\x1b[38;2;138;143;226m]\x1b[38;2;234;47;39m%s\n\x1b[38;2;147;147;119m%s\x1b[0m"
                        PluginName
                        (content.ToString())
                        (ex.ToString())

    let log(content:string)=
        printfn "%s" content
