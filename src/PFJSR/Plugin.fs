namespace PFJSR
open CSR
module PluginMain=
    let mutable api:MCCSAPI = null
    let Init(_api:MCCSAPI) =  
        api <- _api
        let addResult = api.addAfterActListener(EventKey.onScriptEngineInit, MCCSAPI.EventCab(fun _e -> 
            let e= ScriptEngineLogEvent.getFrom _e
            //printfn "%s攻击了%s"  e.playername  e.actortype
            Console.WriteLine ""
            //ScriptEngine::shutdown() -> bool
            true
        ))// |> ignore 
        if addResult then printfn "[F#] addAfterActListener注册成功"
    let TEST() = 0   