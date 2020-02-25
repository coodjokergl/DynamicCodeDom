# 动态编码

## 项目介绍

本项目是一个exe命令行工具，可以通过配置编译命令，提取目标程序集中的动态编码，自动生成DLL。适用场景：大量样板代码编写，重复的类包装。

## 如何使用

### 引入程序集特性

在自己的项目上引入特性*CodeDomAssemblyAttribute*。

```Csharp
[assembly: CodeDomAssemblyAttribute]
```

### 动态编码接口

编制一个公共类，实现接口*DataUpgrade.CodeDom.CodeBuilder.Interfaces.ICodBuilder*

### 添加编译命令

```cli
"$(TargetDir)DataUpgrade.CodeDom.exe" "MTIzNDU2Z2w=" "$(TargetPath)" "$(TargetDir)"
```

### 使用


在输出目录找到改动态的DLL，引入项目即可。