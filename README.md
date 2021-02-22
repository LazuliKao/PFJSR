<div align="center">
<h1>PFJSR</h1>
  
### [Minebbs](https://www.minebbs.com/resources/2105/) | [最新构建](https://github.com/littlegao233/PFJSR/releases/tag/v1-AzurePipelineBuild)
</div>

> ### 仅供学习交流使用
> ### 核心功能参考自[NetJSR](https://github.com/zhkj-liuxiaohua/BDSJSR2)
>
> ### 感谢@liuxiaohua维护的BDSNetRunner,极大地降低了插件开发成本


---
<details>
<summary><b>相比<a href="https://github.com/zhkj-liuxiaohua/BDSJSR2">NetJSR</a>变更或新增的方法：<b></summary>

- #### [变更] `add*ActListener`的返回值：(返回已添加的回调ID，可用于`remove*ActListener`)
     >```js
     >let id = add*ActListener("onLoadName",e => {});
     >```
- #### [变更] `remove*ActListener`方法重载：
     >```js
     >//示例功能：移除当前脚本的所有"onLoadName"监听
     >//参数个数：1个
     >//参数类型：字符串
     >//参数释义：key - 注册用关键字
     >//返回值：移除的监听数量
     >//备注：现NetJsr方法，但返回值与现Netjsr不同
     >let count = remove*ActListener("onLoadName");
     >```
     >```js
     >function aListener(e){//定义一个非匿名函数
     >    log(e);
     >}
     >add*ActListener("onLoadName",aListener);//添加监听
     >
     >//示例功能：通过非匿名函数移除一个"onLoadName"监听
     >//参数个数：2个
     >//参数类型：字符串;方法
     >//参数释义：key - 注册用关键字;fun - 监听器的非匿名函数
     >//返回值：移除的监听数量
     >//备注：不兼容最新netjsr,兼容旧的NetJSR或原JSR
     >let count = remove*ActListener("onLoadName",aListener);
     >```
     >```js
     >let id = add*ActListener("onLoadName",e => {});
     >
     >//示例功能：通过id移除一个"onLoadName"监听
     >//参数个数：1个
     >//参数类型：字符串
     >//参数释义：id - add*ActListener得到的ID
     >//备注：不兼容netjsr
     >remove*ActListener(id);
     >```
     >```js
     >//另有(不再赘述)：
     >//#1
     >let count = remove*ActListener(aListener);
     >//#2
     >remove*ActListener("onLoadName",id);
     >```
 - #### [变更] `setTimeout`的返回值：
     >```js
     >//创建一个Timeout，把该timeout的id存入变量
     >let id = setTimeout("xxx();",1000);
     >log("当前Timeout的ID：" + id);
     >```
- #### [增加] `clearTimeout`方法：
     >```js
     >//取消一个TimeOut
     >clearTimeout(id);
     >```
- #### [增加] `setInterval`方法，设置循环计时器
    - 参数个数：2个
    - 参数类型：字符串/函数，整型
    - 参数详解：code - 待延时执行的指令字符串/函数对象，millisec - 循环毫秒数
     >```js
     >//创建一个计时器，把该计时器的id存入变量
     >let id = setInterval("xxx();",1000);
     >log("当前计时器的ID：" + id);
     >```
- #### [增加] `clearInterval`方法：
     >```js
     >//清除一个计时器
     >clearInterval(id);
     >```
</details>
<details>
<summary><b>更多增加功能<b></summary>

 - #### 导入`.Net Framework`命名空间
     - 示例
       >```js
       >const Console = importNamespace("System").Console;
       >Console.Clear();
       >Console.WriteLine("已清屏");
       >```
 - #### 导入其他CSR目录下的程序集的命名空间
     - 比如csr目录下有个PFEssentials.csr.dll，那么可以通过如下的方式调用程序集内部的方法
       >```js
       >const PFConsole = importNamespace("PFEssentials").Console//导入PFEssentials命名空间的静态类Console
       >PFConsole.SharedWriteLine("测试插件", "输出内容")//调用SharedWriteLine方法
       >const pfessApi = importNamespace("PFEssentials.PublicApi").V2;
       >//导入"PFEssentials"的命名空间("PublicApi"是静态类名，当命名空间导入),V2是类名
       >//其他程序集如何导入需要具体分析
       >addAfterActListener("onInputCommand", function (_e) {
       >    const e = JSON.parse(_e);
       >    if (e.cmd.toLowerCase().trim() === "/querymoney") {//匹配命令
       >        const money = pfessApi.GetMoney(e.playername);//调用已经导入的类的静态方法来获取money
       >        pfessApi.FeedbackTellraw(e.playername, "你的Money:" + money);
       >    }
       >  /*参考(具体方法可以通过反编译程序集来查看)
       >  pfessApi.AddMoney
       >  pfessApi.RemoveMoney
       >  pfessApi.GetMoney
       >  pfessApi.GetUUID
       >  pfessApi.HasOpPermission
       >  pfessApi.FeedbackTellraw
       >  pfessApi.SendActionbar
       >  pfessApi.AddCommandDescribe
       >  pfessApi.DelCommandDescribe
       >  pfessApi.ExecuteCmd
       >  pfessApi.ExecuteCmdAs"
       >  */
       >});
       >```

</details>