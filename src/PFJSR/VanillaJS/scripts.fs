namespace PFJSR
open CSR
open System.Threading
open Newtonsoft.Json.Linq
open System.Text.RegularExpressions

module VanillaScripts=
    let mutable ScriptQueue:list<API.ScriptItemModel*Jint.Engine>=[]
    let mutable HasSetup=false
    let SetupEngine()=
        if HasSetup|>not then 
            API.api.addBeforeActListener(CSR.EventKey.onScriptEngineCmd,fun _e->
                try
                    let e=CSR.ScriptEngineCmdEvent.getFrom(_e)
                    if e.cmd.StartsWith("pfjsr ") then
                        if e.cmd.StartsWith("pfjsr RaiseError ") then
                            let info=JObject.Parse(e.cmd.Substring("pfjsr RaiseError ".Length))
                            let scriptName=info.Value<string>("script")
                            let exname=info.Value<string>("exname")
                            let message=info.Value<string>("message")
                            let stack=info.Value<string>("stack")
                            let stackmatch=Regex(@"at(\s.*?)\s\((.*?):(\d+):(\d+)\)").Matches(stack)
                            //Console.WriteLineErrEx 
                            //    "VanillaScriptException"
                            //    (API.VanillaScriptException())
                            //    scriptName
                            Console.WriteLineErrEx
                                $"VanillaScriptException from {scriptName}"
                                (exn($"{exname}:{message}\n{stack}"))
                                scriptName
                            false
                        else if e.cmd.StartsWith("pfjsr JSRErunScript ") then
                            let all=e.cmd.Substring("pfjsr JSRErunScript ".Length)
                            let nameIndex=all.IndexOf(':')
                            let name=all.Remove(nameIndex)
                            let script=all.Substring(nameIndex+1)
                            #if DEBUG
                            Console.WriteLine(name+">>>"+script)
                            #endif
                            false
                        else true
                    else true
                with _->true
            )|>ignore
            API.api.addBeforeActListener(CSR.EventKey.onScriptEngineLog,fun _e->
                try
                    let e=CSR.ScriptEngineLogEvent.getFrom(_e)
                    Console.log(e.log)
                    //if e.cmd.StartsWith("pfjsr ") then
                    //    if e.cmd.StartsWith("pfjsr JSRErunScript ") then
                    //        let script=e.cmd.Substring("pfjsr JSRErunScript ".Length)
                    //        #if DEBUG
                    //        Console.WriteLine(script)
                    //        #endif
                    //        false
                    //    else true
                    //else true
                with _->()
                true
            )|>ignore
        let rr=API.api.addAfterActListener(CSR.EventKey.onScriptEngineInit,fun _->
            try
                Tasks.Task.Run(fun _->
                    try
                        Thread.Sleep(3000)
                        //let id="pfjsrvs"
                        let id="pfjsrvs"+System.Guid.NewGuid().GetHashCode().ToString().Replace("-","")
                        #if DEBUG
                        Console.WriteLine(id)
                        #endif
                        (
                            $"""const System_{id}=server.registerSystem(0, 0);
let logs = System_{id}.createEventData('minecraft:script_logger_config');
logs.data.log_information = true;
logs.data.log_errors = true;
logs.data.log_warnings = true;
System_{id}.broadcastEvent('minecraft:script_logger_config', logs);

System_{id}.listenForEvent('pfjsr:{id}', (e_{id})=>{{
    const scriptData{id}=JSON.parse(e_{id}.data)
    function JSRErunScript(e){{
        System_{id}.executeCommand("pfjsr JSRErunScript "+e.toString()+":"+scriptData{id}.name,null);
    }}
    try{{
        eval(scriptData{id}.content);
    }}
    catch(err){{
        let info={{
                script:scriptData{id}.name,
                exname:err.name,
                message:err.message,
                stack:err.stack,
        }};
        System_{id}.executeCommand("pfjsr RaiseError "+JSON.stringify(info),null);
    }}
}});"""
                        //$"server.registerSystem(0, 0).listenForEvent('pfjsr:{id}', (e_{id})=>{{try{{eval(e_{id}.data);}}catch(err){{console.log(err);}}}});"
                        ,
                            fun e->
                                try
                                    Tasks.Task.Run(fun ()->
                                        if e then  
                                            $"[VanillaScripts]脚本入口载入成功！(返回值{e})"|>Console.WriteLine
                                            for (item,_) in ScriptQueue do
                                                let name=item.Name
                                                let script=item.Content
                                                let mutable loading=true
                                                try
                                                    Thread.Sleep(100)
                                                    $"[VanillaScripts]尝试载入{name}"|>Console.WriteLine
                                                    let sc=Newtonsoft.Json.Linq.JObject[|
                                                            Newtonsoft.Json.Linq.JProperty("name",name)
                                                            Newtonsoft.Json.Linq.JProperty("content",script)
                                                        |]
                                                    ($"pfjsr:{id}",
                                                        sc.ToString(Newtonsoft.Json.Formatting.None),
                                                        fun result->
                                                            try
                                                                if result then
                                                                    $"[VanillaScripts]脚本\"{name}\"载入成功！(返回值{result})"|>Console.WriteLine
                                                                else
                                                                    $"[VanillaScripts]脚本\"{name}\"载入失败！(返回值{result})"|>Console.WriteLine
                                                                loading<-false
                                                            with ex->($"[VanillaScripts]脚本\"{name}\"载入回调到错误！",ex)|>Console.WriteLineErr
                                                    )|>API.api.JSEfireCustomEvent
                                                with ex->($"[VanillaScripts]脚本\"{name}\"载入时遇到错误！",ex)|>Console.WriteLineErr
                                                while loading do
                                                    Thread.Sleep(100)
                                        else
                                            $"[VanillaScripts]脚本入口载入失败！(返回值{e})"|>Console.WriteLine
                                    )|>ignore
                                with ex->($"[VanillaScripts]脚本入口载入回调到错误！",ex)|>Console.WriteLineErr
                        )|>API.api.JSErunScript
                        //for (name,script) in ScriptQueue do  
                        //    let id="PFJSR_"+System.Guid.NewGuid().ToString().Replace("-","")
                        //    let content=script.Split([| '\r'; '\n' |],System.StringSplitOptions.RemoveEmptyEntries)
                        //    AllScripts.AppendLine($"function {id}(){{")|>ignore
                        //    AllScripts.AppendLine("}")|>ignore
                        //AllScripts.ToString()
                    with ex ->($"[VanillaScripts]加载入口遇到错误:",ex)|>Console.WriteLineErr
                    true
                )|>ignore
            with ex ->($"[VanillaScripts]加载遇到错误:",ex)|>Console.WriteLineErr
            true
        )
        HasSetup<-rr
    let AppendScript(m:API.ScriptItemModel)=
        if HasSetup|>not then SetupEngine();HasSetup<-true
        ScriptQueue<-(m,JSR.CreateEngine(m.Name))::ScriptQueue
        //(scriptContent,fun result->
        //    if result then
        //       $"\"{scriptName}\"载入成功！(返回值{result})"|>Console.WriteLine
        //    else
        //       $"\"{scriptName}\"载入失败！(返回值{result})"|>Console.WriteLine
        //)|>API.api.JSErunScript
    //let ApplyScripts=()

