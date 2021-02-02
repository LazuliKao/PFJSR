namespace PFJSR
open CSR
open Colorful
open System.IO
module PluginMain=
    let Init(_api:MCCSAPI) =
        API.api <- _api
        if Data.Config.JSR.Enable then
            let DirPath:string=Data.Config.JSR.Path|>Path.GetFullPath
            if not(DirPath|>Directory.Exists) then
                DirPath|>Directory.CreateDirectory|>ignore
                "创建插件目录:"+Data.Config.JSR.Path|>Console.WriteLine
            for file in (DirPath|>Directory.GetFiles) do
                 //if file.ToLower().EndsWith(".js") then file|>ScriptFiles.Add
                 if file.ToLower().EndsWith(".js") then file|>Loader.LoadJSRScript
                    

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
