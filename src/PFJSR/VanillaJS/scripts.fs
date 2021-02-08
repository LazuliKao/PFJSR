namespace PFJSR
open CSR
open System.Threading

module VanillaScripts=
    let mutable ScriptQueue:list<string*string>=[]
    let mutable HasSetup=false
    let SetupEngine()=
        HasSetup<-API.api.addAfterActListener(CSR.EventKey.onScriptEngineInit,fun e->
            try
                Tasks.Task.Run(fun ()->
                    try
                        Thread.Sleep(5000)
                        let id="pfjsrvs"+System.Guid.NewGuid().GetHashCode().ToString()
                        ($"server.registerSystem(0, 0).listenForEvent('pfjsr:{id}', (e_{id})=>{{try{{eval(e_{id}.data);}}catch(err){{console.log(err);}}}});",
                            fun e->
                                try
                                    Tasks.Task.Run(fun ()->
                                        if e then  
                                            $"[VanillaScripts]脚本入口载入成功！(返回值{e})"|>Console.WriteLine
                                            for (name,script) in ScriptQueue do
                                                let mutable loading=true
                                                try
                                                    Thread.Sleep(100)
                                                    $"[VanillaScripts]尝试载入{name}"|>Console.WriteLine
                                                    ($"pfjsr:{id}",
                                                        script,
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
    let AppendScript(scriptName:string,scriptContent:string)=
        if HasSetup|>not then SetupEngine();HasSetup<-true
        ScriptQueue<-(scriptName,scriptContent)::ScriptQueue
        //(scriptContent,fun result->
        //    if result then
        //       $"\"{scriptName}\"载入成功！(返回值{result})"|>Console.WriteLine
        //    else
        //       $"\"{scriptName}\"载入失败！(返回值{result})"|>Console.WriteLine
        //)|>API.api.JSErunScript
    //let ApplyScripts=()

