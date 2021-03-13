namespace PFJSR
open System
open Newtonsoft.Json
open System.IO
module Data=
    type ConfigVanillaScriptsModel() =
        let mutable _Enable : bool = false
        let mutable _Path : string = "scripts"
        member _this.Enable with get() =_Enable and set value  =_Enable<-value
        member _this.Path with get() =_Path and set value  =_Path<-value
    type ConfigJSRModel() = 
        let mutable _Enable : bool = true
        let mutable _Path : string = "PFJS"
        let mutable _CheckUUID : bool = true
        let mutable _HotReloadEnabled : bool = true
        let mutable _SystemCmdEnabled : bool = false
        let mutable _HotReloadCommand : string = "pfjsr reload"
        member _this.Enable with get() =_Enable and set value  =_Enable<-value
        member _this.Path with get() =_Path and set value  =_Path<-value
        member _this.CheckUuid with get() =_CheckUUID and set value  =_CheckUUID<-value
        member _this.HotReloadEnabled with get() =_HotReloadEnabled and set value  =_HotReloadEnabled<-value
        member _this.SystemCmdEnabled with get() =_SystemCmdEnabled and set value  =_SystemCmdEnabled<-value
        member _this.HotReloadCommand with get() =_HotReloadCommand and set value  =_HotReloadCommand<-value
    type ConfigModel() =
        let mutable _JSR= new ConfigJSRModel()
        let mutable _VanillaScripts= new ConfigVanillaScriptsModel()
        member _this.JSR with get() =_JSR and set value  =_JSR<-value
        member _this.VanillaScripts with get() =_VanillaScripts and set value  =_VanillaScripts<-value
        //member val JSR = new ConfigJSRModel() with get, set
        //member val VanillaScripts = new ConfigVanillaScriptsModel() with get, set
        member val LoadCSRAssembly=true with get, set
    let configPathRaw:string="plugins\\PFJSR\\config.json"
    let mutable private _config: ConfigModel = new ConfigModel()
    let mutable private _hasLoaded: bool = false
    let public Config:ConfigModel = 
        if _hasLoaded then 
            _config
        else
            let configPtah:string=configPathRaw|>Path.GetFullPath 
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