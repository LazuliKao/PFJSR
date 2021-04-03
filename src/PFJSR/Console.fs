namespace PFJSR
open System.Drawing
open System
module Console=
    let PluginName="PFJSR"
    let ConsoleMode=
        try
            Console.WindowHeight|>ignore
            true
        with _-> false
    let WriteLineT(subtype:obj)(content :obj)=
        match ConsoleMode with
        | true->
            printfn "\x1b[38;2;176;196;222m[\x1b[38;2;169;169;169mINFO\x1b[38;2;176;196;222m][\x1b[38;2;167;132;239m%s\x1b[38;2;176;196;222m]\x1b[38;2;0;255;127m|\x1b[38;2;255;0;255m%s\x1b[38;2;0;255;127m|\x1b[38;2;250;250;210m%s\x1b[0m" 
                PluginName 
                (content.ToString()) 
                (subtype.ToString())
        |_->
            printfn "[INFO][%s]|%s|%s" 
                PluginName 
                (content.ToString()) 
                (subtype.ToString())
    let WriteLine(content :obj)=
        match ConsoleMode with
        | true->
            printfn "\x1b[38;2;176;196;222m[\x1b[38;2;169;169;169mINFO\x1b[38;2;176;196;222m][\x1b[38;2;167;132;239m%s\x1b[38;2;176;196;222m]\x1b[38;2;250;250;210m%s\x1b[0m"
                PluginName
                (content.ToString())
        | _->
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
        match ConsoleMode with
        | true->
            printfn "\x1b[38;2;240;128;128m[\x1b[38;2;253;99;71mWARN\x1b[38;2;240;128;128m][\x1b[38;2;167;132;239m%s\x1b[38;2;240;128;128m]\x1b[38;2;220;138;138m%s\x1b[38;2;152;201;120m-->\x1b[4m\x1b[38;2;127;255;1m\x1b[48;2;25;25;112m%s\x1b[0m"
                PluginName
                (content.ToString())
                (tip.ToString())
        | _->
            printfn "[WARN][%s]%s-->%s"
                PluginName
                (content.ToString())
                (tip.ToString())
    let log(content:string)=
        printfn "%s" content
    let WriteLineErr(content:obj,ex:exn)=
        match ConsoleMode with
        | true->
            printfn "\x1b[93m\x1b[41m[\x1b[0m\x1b[101m\x1b[4mERROR\x1b[0m\x1b[93m\x1b[41m]\x1b[0m\x1b[38;2;138;143;226m[\x1b[38;2;167;132;239m%s\x1b[38;2;138;143;226m]\x1b[38;2;234;47;39m%s\r\n\x1b[38;2;147;147;119m%s\x1b[0m"
                PluginName
                (content.ToString())
                (ex.ToString())
        | _->
            printfn "[ERROR][%s]%s\r\n%s"
                PluginName
                (content.ToString())
                (ex.ToString())
    let WriteLineErrEx(content:obj)(ex:exn)(name:string)=
        match ex with
            //| :? API.PFJsrException -> WriteLineWarn(content,ex.Message)
            | :? Esprima.ParserException as jex-> 
                try
                    let a=List.tryFind (fun x->(x:>API.ScriptItemModel).Type=API.ScriptType.JSR && (x:>API.ScriptItemModel).Name=name) API.LoadedScripts
                    let mutable Line=a.Value.Content.Split('\n').[jex.LineNumber-1]
                    let c=jex.Column-1
                    let endM=Text.RegularExpressions.Regex.Match(Line.[c+1..],@"[\p{P}\p{Z}]")
                    if endM.Success then 
                        let endIndex = c + endM.Index
                        if c=0 then
                            if ConsoleMode then Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[..endIndex]+"\x1b[0m\x1b[2m"+Line.[endIndex+1..]
                        else
                            if ConsoleMode then Line<-Line.[..c-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[c..endIndex]+"\x1b[0m\x1b[2m"+Line.[endIndex+1..]
                    else
                        if c=0 then
                        ////单个字符
                        //    Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[..0]+"\x1b[0m\x1b[2m"+Line.[1..]
                        //else
                        //    Line<-Line.[..c-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[c..c]+"\x1b[0m\x1b[2m"+Line.[c+1..]
                            if ConsoleMode then Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line+"\x1b[0m\x1b[2m"
                        else
                            if ConsoleMode then Line<-Line.[..c-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[c..]+"\x1b[0m\x1b[2m"
                    if ConsoleMode then 
                        printfn "\x1b[93m\x1b[41m[\x1b[0m\x1b[101m\x1b[4mERROR\x1b[0m\x1b[93m\x1b[41m]\x1b[0m\x1b[38;2;138;143;226m[\x1b[38;2;167;132;239m%s\x1b[38;2;138;143;226m]\x1b[38;2;234;47;39m%s\r\n\x1b[38;2;131;150;225m\t%s \x1b[38;2;2;250;250m>>>\r\n\t\x1b[38;2;175;238;238m信息:\t\x1b[38;2;147;147;119m%s\r\n\x1b[38;2;175;238;238m\t位于:\t\x1b[38;2;147;147;119m第 \x1b[38;2;147;147;119m\x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 行  \x1b[38;2;147;147;119m\x1b[38;2;147;147;119m第 \x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 列\r\n\x1b[38;2;175;238;238m\t原文:\t\x1b[0m\x1b[2m%s\x1b[0m"
                            PluginName
                            (content.ToString())
                            "Esprima.ParserException"
                            (jex.Description)
                            (jex.LineNumber)
                            (c)
                            (Line.Trim())
                    else
                        printfn "[ERROR][%s]%s\r\n\t%s >>>\r\n\t信息:\t%s\r\n\t位于:\t第 %d 行  第 %d 列\r\n\t原文:\t%s"
                            PluginName
                            (content.ToString())
                            "Esprima.ParserException"
                            (jex.Description)
                            (jex.LineNumber)
                            (c)
                            (Line.Trim())
                with _->WriteLineErr(content,ex)
            | :? Jint.Runtime.JavaScriptException as jex -> 
               try
                    let a=List.tryFind (fun x->(x:>API.ScriptItemModel).Type=API.ScriptType.JSR&&  (x:>API.ScriptItemModel).Name=name) API.LoadedScripts
                    let mutable Line=a.Value.Content.Split('\n').[jex.LineNumber-1]
                    let endM=Text.RegularExpressions.Regex.Match(Line.[jex.Column+1..],@"[\p{P}\p{Z}]")
                    if endM.Success then 
                        let endIndex = jex.Column + endM.Index
                        if jex.Column=0 then
                            if ConsoleMode then Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[..endIndex]+"\x1b[0m\x1b[2m"+Line.[endIndex+1..]
                        else
                            if ConsoleMode then Line<-Line.[..jex.Column-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[jex.Column..endIndex]+"\x1b[0m\x1b[2m"+Line.[endIndex+1..]
                    else
                        if jex.Column=0 then
                        ////单个字符
                        //    Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[..0]+"\x1b[0m\x1b[2m"+Line.[1..]
                        //else
                        //    Line<-Line.[..jex.Column-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[jex.Column..jex.Column]+"\x1b[0m\x1b[2m"+Line.[jex.Column+1..]                        //if jex.Column=0 then
                            if ConsoleMode then Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line+"\x1b[0m\x1b[2m"
                        else
                            if ConsoleMode then Line<-Line.[..jex.Column-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[jex.Column..]+"\x1b[0m\x1b[2m"
                    if jex.Message.StartsWith(API.PFJsrExceptionStart) then
                        match ConsoleMode with
                        | true->
                            printfn "\x1b[38;2;240;128;128m[\x1b[38;2;253;99;71mWARN\x1b[38;2;240;128;128m][\x1b[38;2;167;132;239m%s\x1b[38;2;240;128;128m]\x1b[38;2;234;47;39m%s\r\n\x1b[38;2;255;153;144m\t%s \x1b[38;2;2;250;250m>>>\r\n\t\x1b[38;2;175;238;238m信息:\t\x1b[4m\x1b[38;2;127;255;1m\x1b[48;2;25;25;112m%s\x1b[0m\r\n\x1b[38;2;175;238;238m\t位于:\t\x1b[38;2;147;147;119m第 \x1b[38;2;147;147;119m\x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 行  \x1b[38;2;147;147;119m\x1b[38;2;147;147;119m第 \x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 列\r\n\x1b[38;2;175;238;238m\t原文:\t\x1b[0m\x1b[2m%s\x1b[0m"
                                    PluginName
                                    (content.ToString())
                                    "PFJsrException"
                                    (jex.Message.Substring(API.PFJsrExceptionStart.Length))
                                    (jex.LineNumber)
                                    (jex.Column)
                                    (Line.Trim())
                        |_->
                            printfn "[WARN][%s]%s\r\n\t%s >>>\r\n\t信息:\t%s\r\n\t位于:\t第 %d 行  第 %d 列\r\n\t原文:\t%s"
                                    PluginName
                                    (content.ToString())
                                    "PFJsrException"
                                    (jex.Message.Substring(API.PFJsrExceptionStart.Length))
                                    (jex.LineNumber)
                                    (jex.Column)
                                    (Line.Trim())
                    else
                        match ConsoleMode with
                        | true->
                            printfn "\x1b[93m\x1b[41m[\x1b[0m\x1b[101m\x1b[4mERROR\x1b[0m\x1b[93m\x1b[41m]\x1b[0m\x1b[38;2;138;143;226m[\x1b[38;2;167;132;239m%s\x1b[38;2;138;143;226m]\x1b[38;2;234;47;39m%s\r\n\x1b[38;2;238;34;175m\t%s \x1b[38;2;2;250;250m>>>\x1b[0m\r\n\t\x1b[38;2;175;238;238m信息:\t\x1b[38;2;147;147;119m%s\r\n\x1b[38;2;175;238;238m\t位于:\t\x1b[38;2;147;147;119m第 \x1b[38;2;147;147;119m\x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 行  \x1b[38;2;147;147;119m\x1b[38;2;147;147;119m第 \x1b[4m%d\x1b[0m\x1b[38;2;147;147;119m 列\r\n\x1b[38;2;175;238;238m\t原文:\t\x1b[0m\x1b[2m%s\x1b[0m"
                                PluginName
                                (content.ToString())
                                "Runtime.JavaScriptException"
                                (jex.Message)
                                (jex.LineNumber)
                                (jex.Column)
                                (Line.Trim())
                        |_->
                            printfn "[ERROR][%s]%s\r\n\t%s >>>\r\n\t信息:\t%s\r\n\t位于:\t第 %d 行  第 %d 列\r\n\t原文:\t%s"
                                PluginName
                                (content.ToString())
                                "Runtime.JavaScriptException"
                                (jex.Message)
                                (jex.LineNumber)
                                (jex.Column)
                                (Line.Trim())
                with _->WriteLineErr(content,ex)
            | :? API.VanillaScriptException as vex->
                try
                    let mutable tempI=0
                    let stackStr=String.concat "\r\n\t" [
                        for (raw,funName,tp,l,c) in vex.stack do
                            if raw.Equals("at Anonymous function (CSR_tmpscript_0:14:9)")|>not then
                                //$"  方法:{funName}|类型:{t}|行:{l}|列{c}"
                                let a=List.tryFind (fun x->(x:>API.ScriptItemModel).Type=API.ScriptType.VJS && (x:>API.ScriptItemModel).Name=name) API.LoadedScripts
                                let mutable Line=a.Value.Content.Split('\n').[l-1]
                                let c=c-1
                                let endM=Text.RegularExpressions.Regex.Match(Line.[c+1..],@"[\p{P}\p{Z}]")
                                if endM.Success then 
                                    let endIndex = c + endM.Index
                                    if c=0 then
                                        if ConsoleMode then Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[..endIndex]+"\x1b[0m\x1b[2m"+Line.[endIndex+1..]
                                    else
                                        if ConsoleMode then Line<-Line.[..c-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[c..endIndex]+"\x1b[0m\x1b[2m"+Line.[endIndex+1..]
                                else
                                    if c=0 then
                                    ////单个字符
                                    //    Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[..0]+"\x1b[0m\x1b[2m"+Line.[1..]
                                    //else
                                    //    Line<-Line.[..c-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[c..c]+"\x1b[0m\x1b[2m"+Line.[c+1..]
                                        if ConsoleMode then Line<-"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line+"\x1b[0m\x1b[2m"
                                    else
                                        if ConsoleMode then Line<-Line.[..c-1]+"\x1b[0m\x1b[4m\x1b[40m\x1b[33m\x1b[1m"+Line.[c..]+"\x1b[0m\x1b[2m"
                                let sp=String.concat "" [for _=1 to tempI do "  "]
                                tempI<-tempI+1
                                let ed=if tempI=vex.stack.Length-1 then "┗" else "┣"
                                let t="\t"
                                if ConsoleMode then 
                                    let oc="\x1b[38;2;232;147;119m"
                                    let oic="\x1b[38;2;120;147;249m"
                                    let tc="\x1b[38;2;82;137;99m"
                                    let ic="\x1b[38;2;167;167;139m"
                                    let bc="\x1b[38;2;175;238;238m"
                                    let dc="\x1b[0m"
                                    let d2="\x1b[2m"
                                    let d4="\x1b[4m"
                                    $"""{oc}{sp}┗ {oic}[{tempI}]
{t}{sp}{oc}  ┣ {bc}方法:{t}{ic}{funName}{tc}[:{tp}]
{t}{sp}{oc}  ┣ {bc}位于:{t}{ic}第 {ic}{d4}%d{l}{dc}{ic} 行  {ic}{ic}第 {d4}%d{c}{dc}{ic} 列
{t}{sp}{oc}  {ed} {bc}原文:{t}{dc}{d2}{Line.Trim()}{dc}"""
                                else
                                    $"""{sp}[{tempI}]:
{t}{sp}方法:{t}{funName}[:{tp}]
{t}{sp}位于:{t}第 %d{l} 行  第 %d{c} 列
{t}{sp}原文:{t}{Line.Trim()}"""
                        ]
                    if ConsoleMode then
                        printfn "\x1b[38;2;240;128;128m[\x1b[38;2;253;99;71mWARN\x1b[38;2;240;128;128m][\x1b[38;2;167;132;239m%s\x1b[38;2;240;128;128m]\x1b[38;2;254;197;189m%s\r\n\t\x1b[38;2;255;153;144m[%s]\x1b[38;2;2;250;250m>>>\r\n\t\x1b[38;2;232;147;119m┣ \x1b[38;2;175;238;238m信息:\t\x1b[38;2;190;200;200m%s\r\n\t%s"
                            PluginName
                            ("[VanillaScriptException]\x1b[38;2;234;107;99m"+content.ToString())
                            (vex.scriptName+".js")
                            vex.message
                            stackStr
                    else
                        printfn "[WARN][%s]%s\r\n[%s]>>>%s\r\n%s"
                            PluginName
                            ("[VanillaScriptException]"+content.ToString())
                            (vex.scriptName+".js")
                            vex.message
                            stackStr
                with _->WriteLineErr(content,ex)
            | _ -> WriteLineErr(content,ex)
    let Setup()=
        try
            if ConsoleMode then PFJSRBDSAPI.Ex.FixConsole()
        with _->()