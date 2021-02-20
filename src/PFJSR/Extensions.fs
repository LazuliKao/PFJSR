namespace PFJSR
open System
open System.Text

module Extensions=
    let Localrequest(u:string)(m:string)(p:string):string=
        let mode:string=if m="POST"then m else "GET"
        let mutable url:string = u;
        if (mode = "GET") then
            if p|>String.IsNullOrEmpty|>not then
                url<-url + "?" + p
        let req = System.Net.WebRequest.Create(url)
        req.Method <- mode
        if mode = "POST"then
            req.ContentType<-"application/x-www-form-urlencoded;charset=UTF-8"
            if p|>String.IsNullOrEmpty|>not then
                let payload:byte[]=Encoding.UTF8.GetBytes(p)
                req.ContentLength<-int64 payload.Length
                let writer = req.GetRequestStream()
                writer.Write(payload, 0, payload.Length)
                writer.Dispose()
                writer.Close()
        let resp=req.GetResponse()
        let stream=resp.GetResponseStream()
        let reader=new System.IO.StreamReader(stream)// 初始化读取器
        let result:string=reader.ReadToEnd()// 读取，存储结果
        reader.Close()// 关闭读取器，释放资源
        stream.Close()// 关闭流，释放资源
        result
    

