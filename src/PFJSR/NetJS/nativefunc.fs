namespace PFJSR
open API
open CSR
open type Newtonsoft.Json.JsonConvert
open Newtonsoft.Json.Linq
open PFJSR
open Jint.Native
open System
open System.Threading.Tasks
open System.Threading
open System.IO

module NativeFunc=
    module Basic=
        let shares  = new Collections.Generic.Dictionary<string,JsValue>()
        type mkdir_delegate = delegate of string-> bool
        let mkdir=
           mkdir_delegate(fun dirname ->
                   let mutable dir :DirectoryInfo= null;
                   if not (dirname|>isNull) then
                       try
                           dir <- Directory.CreateDirectory(dirname)
                       with _->()
                   not (dir|>isNull)
               )
        type log_delegate = delegate of string -> unit
        let log=log_delegate(fun e-> 
                //if e.[0]<>'#' then
                    Console.log(e)
                //else
                //    if e.StartsWith("#Create ") then
                //        Console.log("尝试自动创建目录："+e.Substring(9))
            )
        type fileReadAllText_delegate = delegate of string -> string
        let fileReadAllText=
            fileReadAllText_delegate (fun e->  
                try
                    e|>File.ReadAllText 
                with _ -> null
                )
        type fileWriteAllText_delegate = delegate of string*string -> bool
        let fileWriteAllText=
            fileWriteAllText_delegate(fun f c ->
                try
                    (f,c)|>File.WriteAllText
                    true
                with _ -> false
                )
        type fileWriteLine_delegate = delegate of string*string-> bool
        let fileWriteLine=
            fileWriteLine_delegate(fun f c ->
                try
                    (f,[c])|>File.AppendAllLines
                    true
                with _ -> false
                )
        type TimeNow_delegate = delegate of unit -> string
        let TimeNow=
            TimeNow_delegate(fun _ ->
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                )
        type setShareData_delegate = delegate of string*JsValue-> unit
        let setShareData=
            setShareData_delegate(fun k o ->
                    if shares.ContainsKey(k) then shares.[k] <- o else shares.Add(k,o)
                )
        type getShareData_delegate = delegate of string -> JsValue
        let getShareData=
            getShareData_delegate(fun k ->
                    if shares.ContainsKey(k) then shares.[k] else Jint.Native.Undefined.Instance 
                    )
        type removeShareData_delegate = delegate of string -> JsValue
        let removeShareData=
            removeShareData_delegate(fun k->
                    if shares.ContainsKey(k) then 
                        let o=shares.[k]
                        k|> shares.Remove|>ignore
                        o
                    else
                        Jint.Native.Undefined.Instance
                )
        type getWorkingPath_delegate = delegate of unit-> string
        let getWorkingPath=
            getWorkingPath_delegate(fun _ ->
                    AppDomain.CurrentDomain.BaseDirectory
                )
    module Core =
        type addBeforeActListener_delegate = delegate of string*Func<string,Object> -> int
        type addAfterActListener_delegate = delegate of string*Func<string,Object> -> int
        type removeBeforeActListener_delegate = delegate of string*int -> unit
        type removeAfterActListener_delegate = delegate of string*int -> unit
        type setCommandDescribe_delegate = delegate of string*string -> unit
        type runcmd_delegate = delegate of string -> bool
        type logout_delegate = delegate of string -> unit
        type getOnLinePlayers_delegate = delegate of unit -> string
        type getStructure_delegate = delegate of int*string*string*bool*bool -> string
        type setStructure_delegate = delegate of string*int*string*byte*bool*bool -> bool
        type setServerMotd_delegate = delegate of string*bool -> bool
        type JSErunScript_delegate = delegate of string*Action<bool> -> unit
        type JSEfireCustomEvent_delegate = delegate of string*string*Action<bool> -> unit
        type reNameByUuid_delegate = delegate of string*string -> bool
        type getPlayerAbilities_delegate = delegate of string -> string
        type setPlayerAbilities_delegate = delegate of string*string -> bool
        type getPlayerAttributes_delegate = delegate of string -> string
        type setPlayerTempAttributes_delegate = delegate of string*string -> bool
        type getPlayerMaxAttributes_delegate = delegate of string -> string
        type setPlayerMaxAttributes_delegate = delegate of string*string -> bool
        type getPlayerItems_delegate = delegate of string -> string
        type getPlayerSelectedItem_delegate = delegate of string -> string
        type setPlayerItems_delegate = delegate of string*string -> bool
        type addPlayerItemEx_delegate = delegate of string*string -> bool
        type addPlayerItem_delegate = delegate of string*int*int16*byte -> bool
        type getPlayerEffects_delegate = delegate of string -> string
        type setPlayerEffects_delegate = delegate of string*string -> bool
        type setPlayerBossBar_delegate = delegate of string*string*float32 -> bool
        type removePlayerBossBar_delegate = delegate of string -> bool
        type selectPlayer_delegate = delegate of string -> string
        type transferserver_delegate = delegate of string*string*int -> bool
        type teleport_delegate = delegate of string*float32*float32*float32*int -> bool
        type talkAs_delegate = delegate of string*string -> bool
        type runcmdAs_delegate = delegate of string*string -> bool
        type sendSimpleForm_delegate = delegate of string*string*string*string -> uint
        type sendModalForm_delegate = delegate of string*string*string*string*string -> uint
        type sendCustomForm_delegate = delegate of string*string -> uint
        type releaseForm_delegate = delegate of uint -> bool
        type setPlayerSidebar_delegate = delegate of string*string*string -> bool
        type removePlayerSidebar_delegate = delegate of string -> bool
        type getPlayerPermissionAndGametype_delegate = delegate of string -> string
        type setPlayerPermissionAndGametype_delegate = delegate of string*string -> bool
        type disconnectClient_delegate = delegate of string*string -> bool
        type sendText_delegate = delegate of string*string -> bool
        type getscoreboard_delegate = delegate of string*string -> int
        type setscoreboard_delegate = delegate of string*string*int -> bool
        type getPlayerIP_delegate = delegate of string -> string
        type request_delegate = delegate of string*string*string*Action<obj> -> unit
        type setTimeout_delegate = delegate of JsValue*int -> unit
        type runScript_delegate = delegate of JsValue -> unit
        type postTick_delegate = delegate of JsValue -> unit
        type Instance(scriptName:string,engine:Jint.Engine) =
            let CheckUuid(uuid:string)=
                if String.IsNullOrWhiteSpace(uuid) then
                    let funcname = (new Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name
                    let err = $"在脚本\"{scriptName}\"调用\"{funcname.Remove(funcname.Length-4)}\"方法时使用了空的uuid！"
                    err|>failwith
                if Data.Config.JSR.CheckUuid then
                    if (uuid,"^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$")|>Text.RegularExpressions.Regex.IsMatch|>not then
                        let funcname = (new Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name
                        let err = $"在脚本\"{scriptName}\"调用\"{funcname.Remove(funcname.Length-4)}\"方法时使用了无效的uuid:\"{uuid}\"！"
                        err|>failwith
            let AssertCommercial()=
                if not api.COMMERCIAL then
                    let fn = (new Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name
                    let err = $"获取方法\"{fn.Remove(fn.Length-4)}\"失败，社区版不支持该方法！"
                    err|>Console.WriteLine
                    err|>failwith 
            let InvokeRemoveFailed(a1:string)= 
                ("在脚本\""+scriptName+"\"执行\"remove"+a1+"ActListener\"无效",new exn( "参数2的值仅可以通过\"add"+a1+"ActListener\"结果获得"))|>Console.WriteLineErr
            let _BeforeActListeners =new Collections.Generic.List<(int*string*MCCSAPI.EventCab)>()
            let _AfterActListeners =new Collections.Generic.List<(int*string*MCCSAPI.EventCab)>()
            let setTimeout_fun(o:JsValue)(ms:int)= 
                if not (o|>isNull) then
                    Task.Run(fun _->
                    (
                        try
                            ms|>Thread.Sleep
                            if o.IsString() then
                                engine.Execute(o.ToString())|>ignore
                            else
                                o.Invoke()|>ignore
                        with ex->
                        (
                            ($"在脚本\"{scriptName}\"执行\"setTimeout时遇到错误：",ex)|>Console.WriteLineErr
                        ) 
                    ))|>ignore
            let runScript_fun(o:JsValue)=
                if not (o|>isNull) then
                    try
                        if o.IsString() then
                            engine.Execute(o.ToString())|>ignore
                        else
                            o.Invoke()|>ignore
                    with ex->
                        ($"在脚本\"{scriptName}\"执行\"runScript时遇到错误：",ex)|>Console.WriteLineErr
            let request_fun(u)(m)(p)(f:Action<obj>)=
                Task.Run(fun ()-> 
                        try
                            let mutable ret:string = null;
                            try
                                    ret <- PFJSRBDSAPI.Ex.Localrequest(u, m, p)
                            with _-> ()
                            if f|>isNull|>not then
                                try
                                    ret|>f.Invoke
                                with ex->
                                (
                                    ($"在脚本\"{scriptName}\"执行\"[request]回调时遇到错误：",ex)|>Console.WriteLineErr
                                ) 
                        with _-> ()
                    )|>ignore
            let addBeforeActListener_fun(k)(f:Func<string,Object>)=
                let fullFunc=MCCSAPI.EventCab(fun e->
                        try
                            e|>BaseEvent.getFrom|>SerializeObject|>f.Invoke|>false.Equals|>not
                            //let got=e|>BaseEvent.getFrom
                            //let e= (got|>Newtonsoft.Json.Linq.JObject.FromObject)
                            //e.Add("result",new JValue( got.RESULT):>JToken)
                            //e.ToString()|>f.Invoke|>false.Equals|>not
                        with ex->
                            try
                            ("在脚本\""+scriptName+"\"执行\""+(int e.``type``|>enum<EventType>).ToString()+"\"BeforeAct回调时遇到错误：",ex)|>Console.WriteLineErr
                            with _->()
                            true
                    )
                let funcHash=f.Method.GetHashCode()
                _BeforeActListeners.Add(funcHash,k,fullFunc)
                (k,fullFunc)|>api.addBeforeActListener|>ignore
                funcHash
            let addAfterActListener_fun(k)(f:Func<string,Object>)=
                let fullFunc=MCCSAPI.EventCab(fun e->
                        try
                            let got=e|>BaseEvent.getFrom
                            let e= (got|>Newtonsoft.Json.Linq.JObject.FromObject)
                            e.Add("result",new JValue(got.RESULT):>JToken)
                            e.ToString(Newtonsoft.Json.Formatting.None)|>f.Invoke|>false.Equals|>not
                        with ex->
                            try
                            ("在脚本\""+scriptName+"\"执行\""+(int e.``type``|>enum<EventType>).ToString()+"\"AfterAct回调时遇到错误：",ex)|>Console.WriteLineErr
                            with _->()
                            true
                    )
                let funcHash=f.Method.GetHashCode()
                _AfterActListeners.Add(funcHash,k,fullFunc)
                (k,fullFunc)|>api.addAfterActListener|>ignore
                funcHash
            let removeBeforeActListener_fun(k)(fhash)=
                try
                    let index=_BeforeActListeners.FindIndex(fun (hash,_,_)->hash=fhash)
                    if index <> -1 then
                        let item=_BeforeActListeners.[index]
                        let (_,_,getFunc)=item
                        (k, getFunc )|>api.removeBeforeActListener|>ignore
                        _BeforeActListeners.Remove(item)|>ignore
                    else
                        InvokeRemoveFailed("Before")
                with _->  InvokeRemoveFailed("Before")
            let removeAfterActListener_fun(k)(fhash)=
                try
                     let index=_AfterActListeners.FindIndex(fun (hash,_,_)->hash=fhash)
                     if index <> -1 then
                            let item=_AfterActListeners.[index]
                            let (_,_,getFunc)=item
                            (k, getFunc )|>api.removeAfterActListener|>ignore
                            _AfterActListeners.Remove(item)|>ignore
                     else
                            InvokeRemoveFailed("After")
                with _->InvokeRemoveFailed("After")
            let setCommandDescribe_fun(c)(s)=(c,s)|>api.setCommandDescribe
            let runcmd_fun(cmd)=cmd|>api.runcmd
            let logout_fun(l)=l|>api.logout
            let getOnLinePlayers_fun()= 
                let result=api.getOnLinePlayers()
                if result|>String.IsNullOrEmpty then "[]" else result
            let getStructure_fun(did)(posa)(posb)(exent)(exblk)=
                AssertCommercial()
                (did,posa,posb,exent,exblk)|>api.getStructure
            let setStructure_fun(jdata)(did)(jsonposa)(rot)(exent)(exblk)=
                AssertCommercial()
                (jdata,did,jsonposa,rot,exent,exblk)|>api.setStructure
            let setServerMotd_fun(motd)(isShow)=(motd, isShow)|>api.setServerMotd
            let JSErunScript_fun(js)(cb:Action<bool>)=
                let fullFunc=MCCSAPI.JSECab(fun result->
                    try
                        cb.Invoke(result)
                    with ex->
                        try
                        ($"在脚本\"{scriptName}\"执行\"JSErunScript回调时遇到错误：",ex)|>Console.WriteLineErr
                        with _->()
                )
                (js,fullFunc)|>api.JSErunScript
            let JSEfireCustomEvent_fun(ename)(jdata)(cb:Action<bool>)=
                let fullFunc=MCCSAPI.JSECab(fun result->
                    try
                        cb.Invoke(result)
                    with ex->
                        (
                        try
                        ($"在脚本\"{scriptName}\"执行\"JSErunScript回调时遇到错误：",ex)|>Console.WriteLineErr
                        with _->()
                        )
                    )
                (ename, jdata,fullFunc)|>api.JSEfireCustomEvent
            let reNameByUuid_fun(uuid)(name)=uuid|>CheckUuid;(uuid,name)|>api.reNameByUuid
            let getPlayerAbilities_fun(uuid:string)= 
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.getPlayerAbilities
            let setPlayerAbilities_fun(uuid)(a)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,a)|>api.setPlayerAbilities
            let getPlayerTempAttributes_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.getPlayerAttributes
            let setPlayerTempAttributes_fun(uuid)(a)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,a)|>api.setPlayerTempAttributes
            let getPlayerMaxAttributes_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.getPlayerMaxAttributes
            let setPlayerMaxAttributes_fun(uuid)(a)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,a)|>api.setPlayerMaxAttributes
            let getPlayerItems_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.getPlayerItems
            let getPlayerSelectedItem_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.getPlayerSelectedItem
            let setPlayerItems_fun(uuid)(a)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,a)|>api.setPlayerItems
            let addPlayerItemEx_fun(uuid)(a)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,a)|>api.addPlayerItemEx
            let addPlayerItem_fun(uuid)(id)(aux)(count)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,id,aux,count)|>api.addPlayerItem
            let getPlayerEffects_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.getPlayerEffects
            let setPlayerEffects_fun(uuid)(a)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,a)|>api.setPlayerEffects
            let setPlayerBossBar_fun(uuid)(title)(percent)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,title,percent)|>api.setPlayerBossBar
            let removePlayerBossBar_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.removePlayerBossBar
            let selectPlayer_fun(uuid)=uuid|>api.selectPlayer
            let transferserver_fun(uuid)(addr)(port)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid, addr, port)|>api.transferserver
            let teleport_fun(uuid)(x)(y)(z)(did)=
                uuid|>CheckUuid
                //try
                //    {
                //        const string key = ;
                //        IntPtr ptr = CsApi.getSharePtr(key);
                //        if (ptr == IntPtr.Zero) { GetPFEssentialsApiFailedTips(key); }
                //        else
                //        {
                //            Action<string,string> org = (Action<string,string>)Runtime.InteropServices.Marshal.GetObjectForIUnknown(ptr);
                //            org.Invoke(name,cmd);
                //        }
                //}
                //catch { }
                if  api.COMMERCIAL then
                    (uuid,x,y,z,did)|>api.teleport
                else
                    let ptr:IntPtr=api.getSharePtr("PFEssentials.PublicApi.V2.Teleport") 
                    let mutable result=false
                    if ptr <> IntPtr.Zero then
                        let org = Runtime.InteropServices.Marshal.GetObjectForIUnknown(ptr):?>Func<string,single,single,single,int,bool>
                        result<- ((uuid,x,y,z,did)|>org.Invoke )
                    if result|>not then
                        AssertCommercial()
                        (uuid,x,y,z,did)|>api.teleport
                    else result
            let setPlayerSidebar_fun(uuid)(title)(list)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,title,list)|>api.setPlayerSidebar
            let removePlayerSidebar_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.removePlayerSidebar
            let getPlayerPermissionAndGametype_fun(uuid)=
                uuid|>CheckUuid;AssertCommercial()
                uuid|>api.getPlayerPermissionAndGametype
            let setPlayerPermissionAndGametype_fun(uuid)(a)=
                uuid|>CheckUuid;AssertCommercial()
                (uuid,a)|>api.setPlayerPermissionAndGametype
            let disconnectClient_fun(uuid)(msg)=uuid|>CheckUuid;(uuid,msg)|>api.disconnectClient
            let sendText_fun(uuid)(msg)=uuid|>CheckUuid;(uuid,msg)|>api.sendText
            let getscoreboard_fun(uuid)(a)=uuid|>CheckUuid;(uuid,a)|>api.getscoreboard
            let setscoreboard_fun(uuid)(sname)(value)=uuid|>CheckUuid;(uuid,sname,value)|>api.setscoreboard
            let getPlayerIP_fun(uuid)=
                uuid|>CheckUuid;
                let mutable result=String.Empty
                let data = api.selectPlayer(uuid)
                if data|>String.IsNullOrEmpty|>not then
                    let pinfo=Newtonsoft.Json.Linq.JObject.Parse(data)
                    if pinfo.ContainsKey("playerptr") then
                        let mutable ptr = pinfo.["playerptr"]|>Convert.ToInt64|>IntPtr
                        if ptr <> IntPtr.Zero then
                            let ipport=(new CsPlayer(api, ptr)).IpPort
                            Console.WriteLine(ipport)
                            result<-ipport.Substring(0, ipport.IndexOf('|'))
                result
            let postTick_fun(o:JsValue)=
                if not (o|>isNull) then
                    fun ()->
                        try
                            o.Invoke()|>ignore
                        with ex->
                            try
                                ($"在脚本\"{scriptName}\"执行\"postTick时遇到错误：",ex)|>Console.WriteLineErr
                            with _->()
                   |>api.postTick 
            member _this.BeforeActListeners with get()=_BeforeActListeners
            member _this.AfterActListeners with get()=_AfterActListeners
            member _this.setTimeout=setTimeout_delegate(setTimeout_fun)
            member _this.runScript=runScript_delegate(runScript_fun)
            member _this.request=request_delegate(request_fun)
            member _this.addBeforeActListener=addBeforeActListener_delegate(addBeforeActListener_fun)      
            member _this.setBeforeActListener=_this.addBeforeActListener
            member _this.addAfterActListener=addAfterActListener_delegate(addAfterActListener_fun)  
            member _this.setAfterActListener=_this.addAfterActListener
            member _this.removeBeforeActListener=removeBeforeActListener_delegate(removeBeforeActListener_fun)   
            member _this.removeAfterActListener=removeAfterActListener_delegate(removeAfterActListener_fun)
            member _this.setCommandDescribe=setCommandDescribe_delegate(setCommandDescribe_fun)
            member _this.runcmd=runcmd_delegate(runcmd_fun)
            member _this.logout=logout_delegate(logout_fun)
            member _this.getOnLinePlayers=getOnLinePlayers_delegate(getOnLinePlayers_fun)
            member _this.getStructure =getStructure_delegate(getStructure_fun)
            member _this.setStructure =setStructure_delegate(setStructure_fun)
            member _this.setServerMotd=setServerMotd_delegate(setServerMotd_fun)
            member _this.JSErunScript=JSErunScript_delegate(JSErunScript_fun)
            member _this.JSEfireCustomEvent=JSEfireCustomEvent_delegate(JSEfireCustomEvent_fun)
            member _this.reNameByUuid=reNameByUuid_delegate(reNameByUuid_fun)
            member _this.getPlayerAbilities =getPlayerAbilities_delegate(getPlayerAbilities_fun)
            member _this.setPlayerAbilities =setPlayerAbilities_delegate(setPlayerAbilities_fun)
            member _this.getPlayerAttributes =getPlayerAttributes_delegate(getPlayerTempAttributes_fun)
            member _this.setPlayerTempAttributes =setPlayerTempAttributes_delegate(setPlayerTempAttributes_fun)
            member _this.getPlayerMaxAttributes =getPlayerMaxAttributes_delegate(getPlayerMaxAttributes_fun)
            member _this.setPlayerMaxAttributes =setPlayerMaxAttributes_delegate(setPlayerMaxAttributes_fun)
            member _this.getPlayerItems =getPlayerItems_delegate(getPlayerItems_fun)
            member _this.getPlayerSelectedItem =getPlayerSelectedItem_delegate(getPlayerSelectedItem_fun)
            member _this.setPlayerItems =setPlayerItems_delegate(setPlayerItems_fun)
            member _this.addPlayerItemEx =addPlayerItemEx_delegate(addPlayerItemEx_fun)
            member _this.addPlayerItem =addPlayerItem_delegate(addPlayerItem_fun)
            member _this.getPlayerEffects =getPlayerEffects_delegate(getPlayerEffects_fun)
            member _this.setPlayerEffects=setPlayerEffects_delegate(setPlayerEffects_fun)
            member _this.setPlayerBossBar=setPlayerBossBar_delegate(setPlayerBossBar_fun)
            member _this.removePlayerBossBar=removePlayerBossBar_delegate(removePlayerBossBar_fun)
            member _this.selectPlayer=selectPlayer_delegate(selectPlayer_fun)
            member _this.transferserver=transferserver_delegate(transferserver_fun)
            member _this.teleport=teleport_delegate(teleport_fun)
            member _this.talkAs=talkAs_delegate(fun uuid a->uuid|>CheckUuid;(uuid,a)|>api.talkAs)
            member _this.runcmdAs=runcmdAs_delegate(fun uuid a->uuid|>CheckUuid;(uuid,a)|>api.runcmdAs)
            member _this.sendSimpleForm=sendSimpleForm_delegate(fun uuid title content buttons->uuid|>CheckUuid;(uuid,title,content,buttons)|>api.sendSimpleForm)
             member _this.sendModalForm=sendModalForm_delegate(fun uuid title content button1 button2->uuid|>CheckUuid;(uuid,title,content,button1,button2)|>api.sendModalForm)
            member _this.sendCustomForm=sendCustomForm_delegate(fun uuid json->uuid|>CheckUuid;(uuid,json)|>api.sendCustomForm)
            member _this.releaseForm=releaseForm_delegate(fun formid->formid|>api.releaseForm)
            member _this.setPlayerSidebar=setPlayerSidebar_delegate(setPlayerSidebar_fun)
            member _this.removePlayerSidebar=removePlayerSidebar_delegate(removePlayerSidebar_fun)
            member _this.getPlayerPermissionAndGametype =getPlayerPermissionAndGametype_delegate(getPlayerPermissionAndGametype_fun)
            member _this.setPlayerPermissionAndGametype=setPlayerPermissionAndGametype_delegate(setPlayerPermissionAndGametype_fun)
            member _this.disconnectClient=disconnectClient_delegate(disconnectClient_fun)
            member _this.sendText=sendText_delegate(sendText_fun)
            member _this.getscoreboard=getscoreboard_delegate(getscoreboard_fun)
            member _this.setscoreboard=setscoreboard_delegate(setscoreboard_fun)
            member _this.getPlayerIP=getPlayerIP_delegate(getPlayerIP_fun)
            member _this.postTick=postTick_delegate(postTick_fun)

