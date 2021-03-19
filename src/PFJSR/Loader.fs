namespace PFJSR
open PFJSR
open System.Collections
open System.IO
open System.Threading.Tasks
open System.Threading
module Loader=
    let LoadJSRScript(filePath:string)=
        let scriptName=filePath|>Path.GetFileNameWithoutExtension
        try
            let engine = JSR.CreateEngine(scriptName)
            let jsContent=filePath|>File.ReadAllText
            API.LoadedScripts<-new API.ScriptItemModel(API.ScriptType.JSR,scriptName, filePath,jsContent)::API.LoadedScripts
            (engine,jsContent)|>JSR.InitScript|>ignore
            scriptName+"加载完成！"|>Console.WriteLine
        with ex->Console.WriteLineErrEx
                            $"\"{scriptName}\"加载失败！"
                            ex scriptName
    let LoadVanillaScript(filePath:string)=
        let scriptName=filePath|>Path.GetFileNameWithoutExtension
        try
            let scriptContent=filePath|>File.ReadAllText
            let m=API.ScriptItemModel(API.ScriptType.VJS,scriptName,filePath,scriptContent)
            m|>VanillaScripts.AppendScript
            API.LoadedScripts<-m::API.LoadedScripts
        with ex->($"\"{scriptName}\"导入失败！",ex)|>Console.WriteLineErr