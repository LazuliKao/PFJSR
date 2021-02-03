namespace PFJSR

open System
open Newtonsoft.Json
open System.IO

module Data=
    type ConfigNativeScriptsModel() =
        let mutable _Enable : bool  = true
        let mutable _Path : string  = "scripts"
        member this.Enable with get() =_Enable and set value  =_Enable<-value
        member this.Path with get() =_Path and set value  =_Path<-value
    type ConfigJSRModel() = 
        let mutable _Enable : bool  = true
        let mutable _Path : string  = "PFJS"
        member this.Enable with get() =_Enable and set value  =_Enable<-value
        member this.Path with get() =_Path and set value  =_Path<-value
    type ConfigModel() =
        let mutable _JSR= new ConfigJSRModel()
        //let mutable _NativeScripts= new ConfigNativeScriptsModel()
        member this.JSR with get() =_JSR and set value  =_JSR<-value
        //member this.NativeScripts with get() =_NativeScripts and set value  =_NativeScripts<-value
    let mutable private _config: ConfigModel = new ConfigModel()
    let mutable private _hasLoaded: bool = false
    let public Config:ConfigModel = 
        if _hasLoaded then 
            _config
        else
            let configPtah:string="plugins\\PFJSR\\config.json"|>Path.GetFullPath 
            let configDir:string=configPtah|>Path.GetDirectoryName 
            if configPtah|>File.Exists then
                let readedStr:string=File.ReadAllText configPtah
                _config <- JsonConvert.DeserializeObject<ConfigModel>readedStr
            elif not (configDir|>Directory.Exists) then
                configDir|>Directory.CreateDirectory|>ignore
            File.WriteAllText(configPtah,(_config,Formatting.Indented)|>JsonConvert.SerializeObject )
            _hasLoaded<-true
            _config
     //let Config:ConfigModel = new ConfigModel()