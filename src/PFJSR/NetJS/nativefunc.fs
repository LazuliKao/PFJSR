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
open System.Net
open System.Text
open System.Collections
module NativeFunc=
    let mutable cid:int=Int32.MinValue
    let NextID()=cid<-Interlocked.Increment(ref cid);cid
    module Basic=
        type mkdir_delegate = delegate of string-> bool
        type log_delegate = delegate of string -> unit
        type fileReadAllText_delegate = delegate of string -> string
        type fileWriteAllText_delegate = delegate of string*string -> bool
        type fileWriteLine_delegate = delegate of string*string-> bool
        type TimeNow_delegate = delegate of unit -> string
        type setShareData_delegate = delegate of string*JsValue-> unit
        type getShareData_delegate = delegate of string -> JsValue
        type removeShareData_delegate = delegate of string -> JsValue
        type getWorkingPath_delegate = delegate of unit-> string
        type startLocalHttpListen_delegate = delegate of int*Func<string,string>->int
        type stopLocalHttpListen_delegate = delegate of int ->bool
        type resetLocalHttpListener_delegate = delegate of int*Func<string,string>->bool
        let shares=new Collections.Generic.Dictionary<string,JsValue>()
        // 本地侦听器
        let httplis=new System.Collections.Generic.Dictionary<int,HttpListener>()
        // 侦听函数
        let httpfuncs:Hashtable = new Hashtable();
        type Model()=
            let mkdir_fun=
               mkdir_delegate(fun dirname ->
                       let mutable dir :DirectoryInfo= null;
                       if not (dirname|>isNull) then
                           try
                               dir <- Directory.CreateDirectory(dirname)
                           with _->()
                       not (dir|>isNull)
                   )
            let log_fun=log_delegate(fun e-> 
                    //if e.[0]<>'*' then
                        Console.log(e)
                    //else
                        //if e.StartsWith("*") then
                        //    Console.log("尝试自动创建目录："+e.Substring(1))
                )
            let fileReadAllText_fun=
                fileReadAllText_delegate (fun e->  
                    try
                        e|>File.ReadAllText 
                    with _ -> null
                    )
            let fileWriteAllText_fun=
                fileWriteAllText_delegate(fun f c ->
                    try
                        (f,c)|>File.WriteAllText
                        true
                    with _ -> false
                    )
            let fileWriteLine_fun=
                fileWriteLine_delegate(fun f c ->
                    try
                        (f,[c])|>File.AppendAllLines
                        true
                    with _ -> false
                    )
            let TimeNow_fun=
                TimeNow_delegate(fun _ ->
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    )
            let setShareData_fun=
                setShareData_delegate(fun k o ->
                        if shares.ContainsKey(k) then shares.[k] <- o else shares.Add(k,o)
                    )
            let getShareData_fun=
                getShareData_delegate(fun k ->
                        if shares.ContainsKey(k) then shares.[k] else Jint.Native.Undefined.Instance 
                        )
            let removeShareData_fun=
                removeShareData_delegate(fun k->
                        if shares.ContainsKey(k) then 
                            let o=shares.[k]
                            k|> shares.Remove|>ignore
                            o
                        else
                            Jint.Native.Undefined.Instance
                    )
            let getWorkingPath_fun=
                getWorkingPath_delegate(fun _ ->
                        AppDomain.CurrentDomain.BaseDirectory
                    )
            let readStream(s:Stream,c:Encoding):string=
                try
                    let byteList= new System.Collections.Generic.List<byte>()
                    let byteArr:System.Byte[] =Array.zeroCreate 2048  //[| for _ in 0 .. 2048 -> byte 0 |]
                    let mutable readLen:int = 0
                    let mutable len:int = 0
                    while readLen <> 0 do
                        readLen <- s.Read(byteArr, 0, byteArr.Length)
                        len<-len+readLen
                        byteList.AddRange(byteArr)
                    c.GetString(byteList.ToArray(), 0, len)
                with ex-> 
                    Console.WriteLineErr("readStream",ex)
                    String.Empty
            let readQueryString(q:System.Collections.Specialized.NameValueCollection,url:string):JArray=
                if q|>isNull|>not then
                    let ol=new JArray()
                    if q.Count > 0 then
                        let d = System.Web.HttpUtility.ParseQueryString(url.Substring(url.IndexOf('?')));
                        if d|>isNull|>not && d.Count > 0 then
                            for k:string in d.Keys do
                                ol.Add(new JObject[|
                                new JProperty("Key",k)
                                new JProperty("Value", d.[k])
                                |])//new KeyValuePair<string, object>(k, q[k]))
                    ol
                else null
            let makeReqCallback(f:Func<string,string>):AsyncCallback=
                let cb:AsyncCallback=new AsyncCallback(fun x->
                    let listener:HttpListener=x.AsyncState:?>HttpListener
                    try
                        //If we are not listening this line throws a ObjectDisposedException.
                        let context:HttpListenerContext=listener.EndGetContext(x)
                        if f|>isNull|>not then// 此处处理自定义方法
                            let req = context.Request
                            let resp = context.Response
                            try
                                let ret=f.Invoke(Newtonsoft.Json.JsonConvert.SerializeObject(new JObject[|
                                    new JProperty("AcceptTypes",req.AcceptTypes)
                                    new JProperty("ContentEncoding",JObject.FromObject(req.ContentEncoding))
                                    new JProperty("ContentLength64",req.ContentLength64)
                                    new JProperty("ContentType",req.ContentType)
                                    new JProperty("Cookies",req.Cookies)
                                    new JProperty("HasEntityBody",req.HasEntityBody)
                                    new JProperty("Headers",req.Headers)
                                    new JProperty("HttpMethod",req.HttpMethod)
                                    new JProperty("InputStream",readStream(req.InputStream, req.ContentEncoding))
                                    new JProperty("IsAuthenticated",req.IsAuthenticated)
                                    new JProperty("IsLocal",req.IsLocal)
                                    new JProperty("IsSecureConnection",req.IsSecureConnection)
                                    new JProperty("IsWebSocketRequest",req.IsWebSocketRequest)
                                    new JProperty("KeepAlive",req.KeepAlive)
                                    new JProperty("LocalEndPoint",new JObject[|
                                        new JProperty("Address",req.LocalEndPoint.Address.ToString())
                                        new JProperty("AddressFamily",req.LocalEndPoint.AddressFamily)
                                        new JProperty("Port",req.LocalEndPoint.Port)
                                        |])
                                    new JProperty("ProtocolVersion",JObject.FromObject(req.ProtocolVersion))
                                    new JProperty("QueryString",readQueryString(req.QueryString,req.RawUrl))
                                    new JProperty("RawUrl",req.RawUrl)
                                    new JProperty("RemoteEndPoint",new JObject[|
                                        new JProperty("Address",req.RemoteEndPoint.Address.ToString())
                                        new JProperty("AddressFamily",req.RemoteEndPoint.AddressFamily)
                                        new JProperty("Port",req.RemoteEndPoint.Port)
                                        |])
                                    new JProperty("RequestTraceIdentifier",req.RequestTraceIdentifier.ToString())
                                    new JProperty("ServiceName",req.ServiceName)
                                    new JProperty("TransportContext",JObject.FromObject(req.TransportContext))
                                    new JProperty("Url",req.Url)
                                    new JProperty("UrlReferrer",req.UrlReferrer)
                                    new JProperty("UserAgent",req.UserAgent)
                                    new JProperty("UserHostAddress",req.UserHostAddress)
                                    new JProperty("UserHostName",req.UserHostName)
                                    new JProperty("UserLanguages",req.UserLanguages)
                                    |]))
                                if ret|>isNull|>not then
                                    resp.ContentType <- "text/plain;charset<-UTF-8"//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
                                    resp.AddHeader("Access-Control-Allow-Origin", "*")
                                    resp.AddHeader("Content-type", "text/plain")//添加响应头信息
                                    resp.ContentEncoding <- Encoding.UTF8
                                    resp.StatusDescription <- "200"//获取或设置返回给客户端的 HTTP 状态代码的文本说明。
                                    resp.StatusCode <- 200// 获取或设置返回给客户端的 HTTP 状态代码。
                                    let d = Encoding.UTF8.GetBytes(ret)
                                    resp.OutputStream.Write(d, 0, d.Length)
                                else resp.StatusDescription <- "404";resp.StatusCode <- 404
                            with
                                | :? HttpListenerException ->resp.StatusDescription <- "404";resp.StatusCode <- 404
                                | ex-> 
                                    Console.WriteLineErr("makeReqCallback.1",ex)
                                    resp.StatusDescription <- "404";resp.StatusCode <- 404
                        context.Response.Close();
                    with
                        | :? HttpListenerException->()
                        | ex->
                            Console.WriteLineErr("makeReqCallback.2",ex)
                    let mcb = httpfuncs.[listener] :?> AsyncCallback;
                    if mcb|>isNull|>not then
                        listener.BeginGetContext(mcb, listener)|>ignore
                )
                cb
            let startLocalHttpListen_fun=startLocalHttpListen_delegate(fun port f->
                    let mutable hid:int= -1
                    try
                        let h:HttpListener = new HttpListener()
                        let local1:string = "localhost:"
                        let local2:string= "127.0.0.1:"
                        let head:string = "http://"
                        let iport:int = port
                        h.Prefixes.Add(head + local1 + iport.ToString() + "/")
                        h.Prefixes.Add(head + local2 + iport.ToString() + "/")
                        h.Start()
                        let cb:AsyncCallback = makeReqCallback(f)
                        httpfuncs.[h] <- cb
                        h.BeginGetContext(cb, h)|>ignore
                        hid<-((new Random()).Next())
                        httplis.Add(hid,h)
                    with 
                        | :? HttpListenerException -> ()
                        | ex-> ("err",ex)|>Console.WriteLineErr 
                    hid
            )
            let stopLocalHttpListen_fun=stopLocalHttpListen_delegate(fun i->
                if httplis.ContainsKey(i) then
                    let h = httplis.[i]
                    if h|>isNull|>not then
                        try
                            httpfuncs.Remove(h)
                            let r=httplis.Remove(i)
                            h.Stop()
                            r
                        with _->false
                    else false
                else false
            )
            let resetLocalHttpListener_fun=resetLocalHttpListener_delegate(fun i f->
                if httplis.ContainsKey(i) then
                    let h = httplis.[i]
                    if h|>isNull|>not then
                        try
                            let cb:AsyncCallback = makeReqCallback(f);
                            httpfuncs.[h] <- cb;
                            true
                        with _->false
                    else false
                else false
            )
            member _this.mkdir=mkdir_fun
            member _this.log=log_fun
            member _this.fileReadAllText=fileReadAllText_fun
            member _this.fileWriteAllText=fileWriteAllText_fun
            member _this.fileWriteLine=fileWriteLine_fun
            member _this.TimeNow=TimeNow_fun
            member _this.setShareData=setShareData_fun
            member _this.getShareData=getShareData_fun
            member _this.removeShareData=removeShareData_fun
            member _this.getWorkingPath=getWorkingPath_fun
            member _this.startLocalHttpListen=startLocalHttpListen_fun
            member _this.stopLocalHttpListen=stopLocalHttpListen_fun
            member _this.resetLocalHttpListener=resetLocalHttpListener_fun
        let Instance=new Model()
    module Core=
        type addBeforeActListener_delegate = delegate of string*JsValue -> int
        type addAfterActListener_delegate = delegate of string*JsValue -> int
        type removeBeforeActListener_delegate = delegate of JsValue*JsValue -> JsValue
        type removeAfterActListener_delegate = delegate of JsValue*JsValue -> JsValue
        type setCommandDescribe_delegate = delegate of string*string -> unit
        type runcmd_delegate = delegate of string -> bool
        type logout_delegate = delegate of string -> unit
        type getOnLinePlayers_delegate = delegate of unit -> string
        type getStructure_delegate = delegate of int*string*string*bool*bool -> string
        type setStructure_delegate = delegate of string*int*string*byte*bool*bool -> bool
        type setServerMotd_delegate = delegate of string*JsValue -> bool
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
        type disconnectClient_delegate = delegate of string*string->bool
        type sendText_delegate = delegate of string*string->bool
        type getscoreboard_delegate = delegate of string*string->int
        type setscoreboard_delegate = delegate of string*string*int->bool
        type getPlayerIP_delegate = delegate of string->string
        type request_delegate = delegate of string*string*string*Action<obj> -> unit
        type setTimeout_delegate = delegate of JsValue*int->int
        type clearTimeout_delegate = delegate of int->unit
        type setInterval_delegate = delegate of JsValue*int->int
        type clearInterval_delegate = delegate of int->unit
        type runScript_delegate = delegate of JsValue->unit
        type postTick_delegate = delegate of JsValue->unit
        type getscoreById_delegate = delegate of int64*string->int
        type setscoreById_delegate = delegate of int64*string*int->int
        type getAllScore_delegate = delegate of unit -> string
        type setAllScore_delegate = delegate of string->bool
        type getMapColors_delegate = delegate of int*int*int*int->string
        type exportPlayersData_delegate = delegate of unit->string
        type importPlayersData_delegate = delegate of string->bool
        let timerList=new System.Collections.Generic.Dictionary<int,System.Timers.Timer>()
        type Model(scriptName:string,engine:Jint.Engine)=
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
                    for item in (new Diagnostics.StackTrace()).GetFrames() do
                        item.ToString()|>Console.WriteLine
                    let err = $"获取方法\"{fn.Remove(fn.Length-4)}\"失败，社区版不支持该方法！"
                    err|>Console.WriteLine
                    err|>failwith 
            let mutable ActCid:int=Int32.MinValue
            let NextActID():int=ActCid<-Interlocked.Increment(ref ActCid);ActCid
            let _BeforeActListeners =new Collections.Generic.List<(int*string*MCCSAPI.EventCab*JsValue)>()
            let _AfterActListeners =new Collections.Generic.List<(int*string*MCCSAPI.EventCab*JsValue)>()
            let setTimeout_fun(o:JsValue)(ms:int):int= 
                let id=NextID()
                if o|>isNull|>not then
                    let t=new System.Timers.Timer(float ms,AutoReset=false)
                    timerList.Add(id,t)
                    //let c=SynchronizationContext.Current
                    //c.Post(SendOrPostCallback(fun _->Console.WriteLine(Thread.CurrentThread.ManagedThreadId)),null)
                    //SynchronizationContext.Current 为获取当前线程的同步上下文，拿到线程的上下文之后可以通过调用Send（同步）和Post （异步）将消息分派到同步上下文，以此实现在指定线程执行！！！
                    t.Elapsed.AddHandler(fun _ _->
                        //c.Post(SendOrPostCallback(fun _->
                            try
                                if o.IsString() then engine.Execute(o.ToString())|>ignore
                                else o.Invoke()|>ignore
                            with ex->
                                ($"在脚本\"{scriptName}\"执行\"setTimeout时遇到错误：",ex)|>Console.WriteLineErr
                            if timerList.ContainsKey(id) then
                                t.Dispose()
                                timerList.Remove(id)|>ignore
                        //),null)
                    )
                    t.Start()
                id
            let clearTimeout_fun(key:int)=
                match timerList.TryGetValue key with
                | true, t -> 
                    try if t.Enabled then t.Stop() with _->()
                    try t.Dispose() with _->()
                    timerList.Remove(key)|>ignore
                | _ -> ("在脚本\""+scriptName+"\"执行\"clearTimeout\"无效",new exn($"未找到id:{id}对应的Timer实例"))|>Console.WriteLineErr
            let setInterval_fun(o:JsValue)(ms:int)=
                let id=NextID()
                if o|>isNull|>not then
                    let t=new System.Timers.Timer(float ms,AutoReset=true)
                    timerList.Add(id,t)
                    if o.IsString() then
                        t.Elapsed.AddHandler(fun _ _->
                            try engine.Execute(o.ToString())|>ignore
                            with ex->($"在脚本\"{scriptName}\"执行\"setInterval时遇到错误：",ex)|>Console.WriteLineErr
                        )
                    else
                        t.Elapsed.AddHandler(fun _ _->
                            try o.Invoke()|>ignore
                            with ex->($"在脚本\"{scriptName}\"执行\"setInterval时遇到错误：",ex)|>Console.WriteLineErr
                        )
                    t.Start()
                id
            let clearInterval_fun(key:int)=
                match timerList.TryGetValue key with
                | true, t -> 
                    try if t.Enabled then t.Stop() with _->()
                    try t.Dispose() with _->()
                    timerList.Remove(key)|>ignore
                | _ -> ("在脚本\""+scriptName+"\"执行\"clearInterval\"无效",new exn($"未找到id:{id}对应的Timer实例"))|>Console.WriteLineErr
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
                                    ret <- Extensions.Localrequest u m p
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
            //let mutable oldf=JsValue.FromObject(engine,false:>obj)
            let addBeforeActListener_fun(k)(f:JsValue)=
                //Console.WriteLine(f.Type.ToString())
                //Console.WriteLine(f.GetHashCode().ToString())
                //Console.WriteLine(f.Equals(oldf))
                //oldf<-f
                let fullFunc=MCCSAPI.EventCab(fun e->
                        try
                            //(engine,e|>BaseEvent.getFrom|>SerializeObject)|>JsValue.FromObject|>f.Invoke|>false.Equals|>not
                            let result=f.Invoke(new JsString(e|>BaseEvent.getFrom|>SerializeObject))
                            //false|>result.Equals|>not
                            if isNull(result) then true else
                                if result.Type=Jint.Runtime.Types.Boolean then
                                    Jint.Runtime.TypeConverter.ToBoolean(result) 
                                else true
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
                let fid=NextActID()
                _BeforeActListeners.Add(fid,k,fullFunc,f)
                (k,fullFunc)|>api.addBeforeActListener|>ignore
                fid
            let addAfterActListener_fun(k)(f:JsValue)=
                let fullFunc=MCCSAPI.EventCab(fun basee->
                        try
                            let got=basee|>BaseEvent.getFrom
                            let e=got|>Newtonsoft.Json.Linq.JObject.FromObject
                            e.Add("result",new JValue(got.RESULT):>JToken)
                            let result=f.Invoke(new JsString(e.ToString Newtonsoft.Json.Formatting.None))
                            if isNull(result) then true else
                                if result.Type=Jint.Runtime.Types.Boolean then
                                    Jint.Runtime.TypeConverter.ToBoolean(result) 
                                else true
                        with ex->
                            try
                            ("在脚本\""+scriptName+"\"执行\""+(int basee.``type``|>enum<EventType>).ToString()+"\"AfterAct回调时遇到错误：",ex)|>Console.WriteLineErr
                            with _->()
                            true
                    )
                let fid=NextActID()
                _AfterActListeners.Add(fid,k,fullFunc,f)
                (k,fullFunc)|>api.addAfterActListener|>ignore
                fid
            let InvokeRemoveFailed(a1:string,a2:exn)=("在脚本\""+scriptName+"\"执行\"remove"+a1+"ActListener\"无效",a2)|>Console.WriteLineErr
            let removeBeforeActListener_fun(k:JsValue)(f:JsValue):JsValue=
                let mutable result=JsValue.Undefined
                try
                    if f|>isNull then 
                        if k.IsNumber() then
                            let fid=Jint.Runtime.TypeConverter.ToInt32(k)
                            let index=_BeforeActListeners.FindIndex(fun (hash,_,_,_)->hash=fid)
                            if index <> -1 then
                                let item=_BeforeActListeners.[index]
                                let (_,cbtype,getFunc,_)=item
                                (cbtype, getFunc)|>api.removeBeforeActListener|>ignore
                                _BeforeActListeners.Remove(item)|>ignore
                            else
                                InvokeRemoveFailed("Before",new NullReferenceException($"通过参数1的ID({fid})值未找到对应方法"))
                        else
                            let all=if k.IsString() then _BeforeActListeners.FindAll(fun (_,cbtype,_,_)->cbtype=k.ToString())
                                                               else _BeforeActListeners.FindAll(fun (_,_,_,jv)->k.Equals jv)
                            if all.Count <> 0 then
                                for item in all do
                                    let (_,cbtype,getFunc,_)=item
                                    (cbtype, getFunc )|>api.removeBeforeActListener|>ignore
                                    _BeforeActListeners.Remove(item)|>ignore
                                result<-new JsNumber(all.Count)
                            else
                                InvokeRemoveFailed("Before",new NullReferenceException("通过参数1的值未找到对应非匿名方法"))
                    else
                        if f.IsNumber() then
                            let fid=Jint.Runtime.TypeConverter.ToInt32(f)
                            let index=_BeforeActListeners.FindIndex(fun (hash,cbtype,_,_)->hash=fid && cbtype=k.ToString())
                            if index <> -1 then
                                let item=_BeforeActListeners.[index]
                                let (_,cbtype,getFunc,_)=item
                                (cbtype, getFunc)|>api.removeBeforeActListener|>ignore
                                _BeforeActListeners.Remove(item)|>ignore
                            else
                                InvokeRemoveFailed("Before",new NullReferenceException($"通过参数2的ID({fid})值未找到对应方法"))
                        else
                            let all=_BeforeActListeners.FindAll(fun (_,cbtype,_,jv)->jv.Equals f && cbtype=k.ToString())
                            if all.Count <> 0 then
                                for item in all do
                                    let (_,cbtype,getFunc,_)=item
                                    (cbtype, getFunc )|>api.removeBeforeActListener|>ignore
                                    _BeforeActListeners.Remove(item)|>ignore
                                result<-new JsNumber(all.Count)
                            else
                                InvokeRemoveFailed("Before",new NullReferenceException("通过参数2的值未找到对应非匿名方法"))
                with ex->  InvokeRemoveFailed("Before",ex)
                result
            let removeAfterActListener_fun(k:JsValue)(f:JsValue):JsValue=
                let mutable result=JsValue.Undefined
                try
                    if f|>isNull then 
                        if k.IsNumber() then
                            let fid=Jint.Runtime.TypeConverter.ToInt32(k)
                            let index=_AfterActListeners.FindIndex(fun (hash,_,_,_)->hash=fid)
                            if index <> -1 then
                                let item=_AfterActListeners.[index]
                                let (_,cbtype,getFunc,_)=item
                                (cbtype, getFunc)|>api.removeAfterActListener|>ignore
                                _AfterActListeners.Remove(item)|>ignore
                            else
                                InvokeRemoveFailed("After",new NullReferenceException($"通过参数1的ID({fid})值未找到对应方法"))
                        else
                            let all=if k.IsString() then _AfterActListeners.FindAll(fun (_,cbtype,_,_)->cbtype=k.ToString())
                                                               else _AfterActListeners.FindAll(fun (_,_,_,jv)->k.Equals jv)
                            if all.Count <> 0 then
                                for item in all do
                                    let (_,cbtype,getFunc,_)=item
                                    (cbtype, getFunc )|>api.removeAfterActListener|>ignore
                                    _AfterActListeners.Remove(item)|>ignore
                                result<-new JsNumber(all.Count)
                            else
                                InvokeRemoveFailed("After",new NullReferenceException("通过参数1的值未找到对应非匿名方法"))
                    else
                        if f.IsNumber() then
                            let fid=Jint.Runtime.TypeConverter.ToInt32(f)
                            let index=_AfterActListeners.FindIndex(fun (hash,cbtype,_,_)->hash=fid && cbtype=k.ToString())
                            if index <> -1 then
                                let item=_AfterActListeners.[index]
                                let (_,cbtype,getFunc,_)=item
                                (cbtype, getFunc)|>api.removeAfterActListener|>ignore
                                _AfterActListeners.Remove(item)|>ignore
                            else
                                InvokeRemoveFailed("After",new NullReferenceException($"通过参数2的ID({fid})值未找到对应方法"))
                        else
                            let all=_AfterActListeners.FindAll(fun (_,cbtype,_,jv)->jv.Equals f && cbtype=k.ToString())
                            if all.Count <> 0 then
                                for item in all do
                                    let (_,cbtype,getFunc,_)=item
                                    (cbtype, getFunc )|>api.removeAfterActListener|>ignore
                                    _AfterActListeners.Remove(item)|>ignore
                                result<-new JsNumber(all.Count)
                            else
                                InvokeRemoveFailed("After",new NullReferenceException("通过参数2的值未找到对应非匿名方法"))
                with ex->  InvokeRemoveFailed("After",ex)
                result
            let setCommandDescribe_fun(c)(s)=(c,s)|>api.setCommandDescribe
            let runcmd_fun(cmd:string)=
                if cmd.StartsWith("system ") then
                    let cli = new Diagnostics.Process()
                    cli.StartInfo.FileName <- "cmd"
                    cli.StartInfo.WorkingDirectory<-Basic.Instance.getWorkingPath.Invoke()
                    cli.StartInfo.Arguments <- $"/C \"{cmd.Substring(7)}\""
                    //cli.StartInfo.RedirectStandardOutput <- true
                    //cli.StartInfo.RedirectStandardInput <- true
                    //cli.StartInfo.RedirectStandardError <- true
                    cli.StartInfo.UseShellExecute <- false
                    cli.StartInfo.CreateNoWindow <- false
                    //cli.OutputDataReceived.AddHandler(fun _ e -> "[System CMD Inside]"+e.Data|>Console.WriteLine)
                    //cli.ErrorDataReceived.AddHandler(fun _ e -> "[System CMD Inside][Error]"+e.Data|>Console.WriteLine)
                    cli.Exited.AddHandler(fun _ e -> 
                        //Console.WriteLine(cli.StandardOutput.ReadToEnd())
                        cli.Dispose()
                    )
                    cli.Start()|>ignore
                        //Console.WriteLine(cli.StandardOutput.ReadToEnd())
                        //Diagnostics.Process.Start("\"%windir%\system32\cmd.exe\" /C \"cmd.exe\"")|>ignore
                    //cli.Execute(cmd.Substring(7),false)
                    true
                else
                    cmd|>api.runcmd
            let logout_fun(l:string)=
                if l.[0]<>'*' then
                    l|>api.logout
                else
                    Console.log("[Folder Creator Inside]尝试自动创建目录："+l.Substring(1))
                    Basic.Instance.mkdir.Invoke(l.Substring(1))|>ignore
            let getOnLinePlayers_fun()= 
                let result=api.getOnLinePlayers()
                if result|>String.IsNullOrEmpty then "[]" else result
            let getStructure_fun(did)(posa)(posb)(exent)(exblk)=
                AssertCommercial()
                (did,posa,posb,exent,exblk)|>api.getStructure
            let setStructure_fun(jdata)(did)(jsonposa)(rot)(exent)(exblk)=
                AssertCommercial()
                (jdata,did,jsonposa,rot,exent,exblk)|>api.setStructure
            let setServerMotd_fun(motd)(isShow:JsValue)=
                let s = if isNull(isShow) then true else 
                                if isShow.Type=Jint.Runtime.Types.Boolean then
                                    Jint.Runtime.TypeConverter.ToBoolean(isShow) 
                                else true
                (motd, s)|>api.setServerMotd
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
                        try
                        ($"在脚本\"{scriptName}\"执行\"JSErunScript回调时遇到错误：",ex)|>Console.WriteLineErr
                        with _->()
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
            let talkAs_fun(uuid)(a)=uuid|>CheckUuid;(uuid,a)|>api.talkAs
            let runcmdAs_fun(uuid)(a)=uuid|>CheckUuid;(uuid,a)|>api.runcmdAs
            let sendSimpleForm_fun(uuid)(title)(content)(buttons)=uuid|>CheckUuid;(uuid,title,content,buttons)|>api.sendSimpleForm
            let sendModalForm_fun(uuid)(title)(content)(button1)(button2)=uuid|>CheckUuid;(uuid,title,content,button1,button2)|>api.sendModalForm
            let sendCustomForm_fun(uuid)(json)=uuid|>CheckUuid;(uuid,json)|>api.sendCustomForm
            let releaseForm_fun(id)=id|>api.releaseForm
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
            let getAllScore_fun()=
                AssertCommercial()
                api.getAllScore()
            let setAllScore_fun(data) =
                AssertCommercial()
                data|>api.setAllScore
            let getscoreById_fun(id)(objname) =(id,objname)|>api.getscoreById
            let setscoreById_fun(id)(objname)(value) =(id,objname,value)|>api.setscoreById
            let getMapColors_fun(x)(y)(z)(d)=
                AssertCommercial()
                (x,y,z,d)|>api.getMapColors
            let exportPlayersData_fun()=
                AssertCommercial()
                api.exportPlayersData()
            let importPlayersData_fun(data:string)=
                AssertCommercial()
                data|>api.importPlayersData
            member _this.BeforeActListeners with get()=_BeforeActListeners
            member _this.AfterActListeners with get()=_AfterActListeners
            member _this.setTimeout=setTimeout_delegate(setTimeout_fun)
            member _this.clearTimeout=clearTimeout_delegate(clearTimeout_fun)
            member _this.setInterval=setInterval_delegate(setInterval_fun)
            member _this.clearInterval=clearInterval_delegate(clearInterval_fun)
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
            member _this.talkAs=talkAs_delegate(talkAs_fun)
            member _this.runcmdAs=runcmdAs_delegate(runcmdAs_fun)
            member _this.sendSimpleForm=sendSimpleForm_delegate(sendSimpleForm_fun)
            member _this.sendModalForm=sendModalForm_delegate(sendModalForm_fun)
            member _this.sendCustomForm=sendCustomForm_delegate(sendCustomForm_fun)
            member _this.releaseForm=releaseForm_delegate(releaseForm_fun)
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
            member _this.getscoreById=getscoreById_delegate(getscoreById_fun)
            member _this.setscoreById=setscoreById_delegate(setscoreById_fun)
            member _this.getAllScore=getAllScore_delegate(getAllScore_fun)
            member _this.setAllScore=setAllScore_delegate(setAllScore_fun)
            member _this.getMapColors=getMapColors_delegate(getMapColors_fun)
            member _this.exportPlayersData=exportPlayersData_delegate(exportPlayersData_fun)
            member _this.importPlayersData=importPlayersData_delegate(importPlayersData_fun)
    let Reset()=
        for t in Core.timerList.Values do
            try if t.Enabled then t.Stop() with _->()
            try t.Dispose() with _->()
        Core.timerList.Clear()
        try Basic.shares.Clear() with ex->("重置ShareData出错",ex)|>Console.WriteLineErr
        for h in Basic.httplis do
            try 
                h.Value.Stop()
                Basic.httpfuncs.Remove(h)
            with ex->("http侦听器终止出错",ex)|>Console.WriteLineErr
        try Basic.httplis.Clear() with ex->("重置http侦听器出错",ex)|>Console.WriteLineErr
        try Basic.httpfuncs.Clear() with ex->("重置http回调出错",ex)|>Console.WriteLineErr