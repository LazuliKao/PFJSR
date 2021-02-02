namespace PFJSR

open System
open Newtonsoft.Json
open System.IO

module Data=
    type ConfigNativeScriptsModel() =
       member this.Enable : bool = true
       member this.Path : string = "scripts"
    type ConfigJSRModel() = 
        member this.Enable : bool = true
        member this.Path : string = "NetJS"
    type ConfigModel() =
        member this.NativeScripts  : ConfigNativeScriptsModel=
            new ConfigNativeScriptsModel()
        member this.JSR:ConfigJSRModel =
            new ConfigJSRModel()
    let mutable private _config: ConfigModel = new ConfigModel()
    let mutable private _hasLoaded: bool = false
    let public Config:ConfigModel = 
        if _hasLoaded then 
            _config
        else
            let configPtah:string="plugin\\PFJSR\\config.js"|>Path.GetFullPath 
            let configDir:string=configPtah|>Path.GetDirectoryName 
            if configPtah|>File.Exists then
                let readedStr:string=File.ReadAllText configPtah
                _config <- JsonConvert.DeserializeObject<ConfigModel>readedStr
            elif not (configDir|>Directory.Exists) then
                configDir|>Directory.CreateDirectory|>ignore
            File.WriteAllText(configPtah,_config|>JsonConvert.SerializeObject )
            _hasLoaded<-true
            _config
     //let Config:ConfigModel = new ConfigModel()