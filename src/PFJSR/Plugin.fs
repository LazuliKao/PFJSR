namespace PFJSR
open CSR
//open Colorful
open System.IO
module PluginMain=
    let LoadJSRScripts()=
        if Data.Config.JSR.Enable then
            let DirPath:string=Data.Config.JSR.Path|>Path.GetFullPath
            if DirPath|>Directory.Exists|>not then
                DirPath|>Directory.CreateDirectory|>ignore
                "创建JSR插件目录:"+Data.Config.JSR.Path|>Console.WriteLine
            for file in (Seq.choose (fun x->
                match x:string with
                | jsfile when x.ToLower().EndsWith(".js") ->Some(jsfile)
                | _->None
                ) 
                        (DirPath|>Directory.GetFiles)) do file|>Loader.LoadJSRScript
    let LoadVanillaScripts()=
        if Data.Config.VanillaScripts.Enable then
            let DirPath:string=Data.Config.VanillaScripts.Path|>Path.GetFullPath
            if DirPath|>Directory.Exists|>not then
                DirPath|>Directory.CreateDirectory|>ignore
                "创建NativeScripts目录:"+Data.Config.VanillaScripts.Path|>Console.WriteLine
            for file in (Seq.choose (fun x->
                match x:string with
                | jsfile when x.ToLower().EndsWith(".js") ->Some(jsfile)
                | _->None
                )    
                        (DirPath|>Directory.GetFiles)) do file|>Loader.LoadJSRScript
    let LoadcsrAssembly()=
        API.csrAssemblyList <- List.choose 
            ( fun x -> match x with
                            | null -> None
                            | _ -> Some(x)
            )
            ( 
                List.map (fun x ->
                    try
                        System.Reflection.Assembly.LoadFile(x)
                    with ex->
                        //Console.WriteLineErr($"加载外部程序集\".\\csr\\{Path.GetFileName(x)}\"出错",ex)
                        null
                ) [for x in "csr"|>Path.GetFullPath|>Directory.GetFiles do if x.EndsWith(".dll") then x]
            )
        (String.concat "|" 
            ($"已将 {API.csrAssemblyList.Length} 个来自CSR插件目录的程序集加载到脚本引擎"
            ::[for x in API.csrAssemblyList->x.GetName().Name])
        )|>Console.WriteLine 
    let Init(_api:MCCSAPI) =
        API.api <- _api
        Console.Setup()
        if Data.Config.LoadCSRAssembly then LoadcsrAssembly()
        LoadJSRScripts()
        LoadVanillaScripts()
        if Data.Config.JSR.HotReloadEnabled then
            API.api.addBeforeActListener(EventKey.onServerCmd,fun _e->
                try
                    let e=ServerCmdEvent.getFrom(_e)
                    if e.cmd.Trim() = Data.Config.JSR.HotReloadCommand then
                        "正在重载..."|>Console.WriteLine
                        let mutable scriptCount=0
                        let mutable ListenerCount=0
                        for runner in JSR.RunnerList do
                            for (_,name,func,_) in runner.core.BeforeActListeners do
                                API.api.removeBeforeActListener(name,func)|>ignore
                                ListenerCount<-ListenerCount+1
                            runner.core.BeforeActListeners.Clear()
                            for (_,name,func,_) in runner.core.AfterActListeners do
                                API.api.removeAfterActListener(name,func)|>ignore
                                ListenerCount<-ListenerCount+1
                            runner.core.AfterActListeners.Clear()
                            scriptCount<-scriptCount+1
                        JSR.RunnerList<-[]
                        NativeFunc.Reset()
                        LoadJSRScripts()
                        $"重载成功：已删除来自 {scriptCount} 个脚本的 {ListenerCount} 个监听"|>Console.WriteLine
                        false
                        //match  with
                        //|  -> 
                        //| _ -> false
                    else true
                with ex->("重载失败：",ex)|>Console.WriteLineErr;true
            )|>ignore      
        

        //API.api.addBeforeActListener(EventKey.onMobHurt,fun _e->
        //    (_e|>MobHurtEvent.getFrom|>Newtonsoft.Json.Linq.JObject.FromObject).ToString()|>Console.WriteLine
        //    true
        //)
        //System.Threading.Tasks.Task.Run(fun ()->
        //    let script: string=System.IO.File.ReadAllText("Logging.js")
        //    System.Threading.Thread.Sleep(5000)
        //    api.JSErunScript(script,fun _ -> ())
        //)|>ignore
        //api.addAfterActListener(EventKey.onScriptEngineInit, MCCSAPI.EventCab(fun _e -> 
        //    let e= ScriptEngineInitEvent.getFrom _e
        //    //printfn "%s攻击了%s"  e.playername  e.actortype
        //    e|>Newtonsoft.Json.JsonConvert.SerializeObject|>Console.WriteLine
        //    //ScriptEngine::shutdown() -> bool
        //    true
        //)) |> ignore 
