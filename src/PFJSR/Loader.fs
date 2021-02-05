namespace PFJSR
open PFJSR
open System.Collections
open System.IO
open System.Threading.Tasks
open System.Threading
module Loader=
    type ScriptItemModel(n: string, p: string) =
        member _this.Name :string=n
        member _this.Path :string=p
    let mutable LoadedScripts:list<ScriptItemModel>=[] 
    let LoadJSRScript(filePath:string)=
        let scriptName=filePath|>Path.GetFileNameWithoutExtension
        try
            let options=new Jint.Options()
            options.AllowClr()|>ignore
            options.AllowClr(typeof<FSharp.Reflection.FSharpType>.Assembly)|>ignore
            options.AllowClr(typeof<Colorful.Console>.Assembly)|>ignore
            options.AllowClr(typeof<Jint.Engine>.Assembly)|>ignore
            options.AllowClr(typeof<Newtonsoft.Json.JsonConvert>.Assembly)|>ignore
            let engine=new Jint.Engine(options)
            (
                engine,
                (filePath|>File.ReadAllText),
                new NativeFunc.Core.Instance(scriptName,engine)
            )|>JSR.CreateEngine|>ignore
            LoadedScripts<-new ScriptItemModel(scriptName, filePath)::LoadedScripts
            scriptName+"加载完成！"|>Console.WriteLine
        with ex->($"\"{scriptName}\"加载失败！",ex)|>Console.WriteLineErr
    let LoadNativeScript(filePath:string,startTime:int)=
        let scriptName=filePath|>Path.GetFileNameWithoutExtension
        try
            let scriptContent=filePath|>File.ReadAllText
            API.api.addAfterActListener(CSR.EventKey.onScriptEngineInit,fun e->
                try
                    Task.Run(fun _->
                        try
                            Thread.Sleep(startTime)
                            //NativeScripts.LoadScript(scriptName,scriptContent)
                            //NativeScripts.LoadScript(scriptName,scriptContent)
                            (scriptContent,fun result->
                                      if result then
                                         $"\"{scriptName}\"载入成功！(返回值{result})"|>Console.WriteLine
                                      else
                                         $"\"{scriptName}\"载入失败！(返回值{result})"|>Console.WriteLine
                                  )|>API.api.JSErunScript
                        with ex ->($"\"{scriptName}\"加载失败！",ex)|>Console.WriteLineErr
                    )|>ignore  
                with ex ->($"\"{scriptName}\"添加失败！",ex)|>Console.WriteLineErr
                true
            )|>ignore
        with ex->($"\"{scriptName}\"导入失败！",ex)|>Console.WriteLineErr