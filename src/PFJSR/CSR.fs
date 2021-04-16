namespace CSR
open CSR
  type public Plugin()=
        //static let _a=
        //    printfn "[PFJSR]模块导入..."
        static let mutable mapi:MCCSAPI =null
        //插件主入口 请勿随意更改(由gxh翻译自CSRAPI)
        static member public onServerStart(pathandversion:string):int =
            #if DEBUG
            "%s" pathandversion
            #endif
            let mutable result:int = -1
            let pav:string[] =pathandversion.Split(",".ToCharArray())
            if  pav.Length > 1 then
                let path:string = pav.GetValue(0).ToString()
                let Version:string = pav.GetValue(1).ToString()
                let commercial:bool = pav.GetValue(pav.Length - 1).Equals("1")
                mapi <- MCCSAPI(path, Version, commercial)
                if not (isNull(mapi))  then
                     printfn $"[F#-PFJSR] version : {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}. author : littlegao233."
                     System.GC.KeepAlive(mapi);
                     Plugin.onStart(mapi)|>ignore
                     result <- 0
            if result = -1 then  printf "[F#-PFJSR] plugin Load failed."
            result
        static member public onStart(api:MCCSAPI)=
            //TODO  此处需要自行实现
            PFJSR.PluginMain.Init(api)