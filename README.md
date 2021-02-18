<div align="center">
<h1>PFJSR</h1>
  
### [Minebbs](https://www.minebbs.com/resources/2105/) | [最新构建](https://github.com/littlegao233/PFJSR/releases/tag/v1-AzurePipelineBuild)
</div>

> ### 仅供学习交流使用
> ### 核心功能参考自[NetJSR](https://github.com/zhkj-liuxiaohua/BDSJSR2)
>
> ### 感谢@liuxiaohua维护的BDSNetRunner,极大地降低了插件开发成本


---
### 相比[NetJSR](https://github.com/zhkj-liuxiaohua/BDSJSR2)变更或新增的方法：
- #### [变更] `add*ActListener`的返回值：(返回已添加的回调ID，用于`remove*ActListener`)
     >```js
     >let id = add*ActListener("onLoadName",e => {});
     >```
- #### [变更] `remove*ActListener`方法重载(由`add*ActListener`返回值获取)：
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
     >//参数类型：字符串
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


