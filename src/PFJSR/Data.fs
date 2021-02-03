namespace PFJSR

open System
open Newtonsoft.Json
open System.IO

module Data=
    type ConfigNativeScriptsModel() =
       member this.Enable : bool = true
       member this.Path : string = "scripts"
    type ConfigJSRModel() = 
        let Enable : bool = true
        let Path : string = "NetJS"
    type ConfigModel() =
        //member this.NativeScripts  : ConfigNativeScriptsModel=
        //    new ConfigNativeScriptsModel()
        member this.JSR:ConfigJSRModel =
            new ConfigJSRModel()
    let mutable private _config: ConfigModel = new ConfigModel()
    let mutable private _hasLoaded: bool = false
    let public Config:ConfigModel = 
        if _hasLoaded then 
            _config
        else
            let configPtah:string="plugins\\PFJSR\\config.json"|>Path.GetFullPath 
            printf "a"
            let configDir:string=configPtah|>Path.GetDirectoryName 
            printf "a"
            if configPtah|>File.Exists then
                printf "a"
                let readedStr:string=File.ReadAllText configPtah
                printf "a"
                _config <- JsonConvert.DeserializeObject<ConfigModel>readedStr
                printf "%s" (_config.JSR.Enable.ToString())
            elif not (configDir|>Directory.Exists) then
                configDir|>Directory.CreateDirectory|>ignore
            File.WriteAllText(configPtah,(_config,Formatting.Indented)|>JsonConvert.SerializeObject )
            _hasLoaded<-true
            _config
     //let Config:ConfigModel = new ConfigModel()