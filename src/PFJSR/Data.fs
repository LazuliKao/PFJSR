namespace PFJSR
open System
open Newtonsoft.Json
open System.IO
module Data=
    type ConfigVanillaScriptsModel() =
        let mutable _Enable : bool  = false
        let mutable _Path : string  = "scripts"
        member _this.Enable with get() =_Enable and set value  =_Enable<-value
        member _this.Path with get() =_Path and set value  =_Path<-value
    type ConfigJSRModel() = 
        let mutable _Enable : bool  = true
        let mutable _Path : string  = "PFJS"
        let mutable _CheckUUID : bool  = true
        let mutable _HotReload : bool  = true
        let mutable _ReloadCommand : string  = "pfjsr reload"
        member _this.Enable with get() =_Enable and set value  =_Enable<-value
        member _this.Path with get() =_Path and set value  =_Path<-value
        member _this.CheckUuid with get() =_CheckUUID and set value  =_CheckUUID<-value
        member _this.HotReload with get() =_HotReload and set value  =_HotReload<-value
        member _this.ReloadCommand with get() =_ReloadCommand and set value  =_ReloadCommand<-value
    type ConfigModel() =
        let mutable _JSR= new ConfigJSRModel()
        let mutable _VanillaScripts= new ConfigVanillaScriptsModel()
        member _this.JSR with get() =_JSR and set value  =_JSR<-value
        member _this.VanillaScripts with get() =_VanillaScripts and set value  =_VanillaScripts<-value
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