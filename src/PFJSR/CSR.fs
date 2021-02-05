namespace CSR
open CSR
  type public Plugin()=
        static let mutable mapi:MCCSAPI =null
        //插件主入口 请勿随意更改(由gxh翻译自CSRAPI)
        static member public onServerStart(pathandversion:string):int =
            printfn "%s" pathandversion
            let mutable result:int = -1
            let pav:string[] =pathandversion.Split(",".ToCharArray())
            if  pav.Length > 1 then
                let path:string = pav.GetValue(0).ToString()
                let Version:string = pav.GetValue(1).ToString()
                let commercial:bool = pav.GetValue(pav.Length - 1).Equals("1")
                mapi <- MCCSAPI(path, Version, commercial)
                if not (isNull(mapi))  then
                     printfn "[F#] plugin is Loading."
                     System.GC.KeepAlive(mapi);
                     Plugin.onStart(mapi)|>ignore
                     printfn "[F#] plugin Load Success."
                     result <- 0
            if result = -1 then  printf "[F#] plugin Load failed."
            result
        static member  public  onStart(api:MCCSAPI)=
            //TODO  此处需要自行实现
            PFJSR.PluginMain.Init(api)