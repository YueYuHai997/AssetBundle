# AssetBundle
AssetBundle学习

#1.0
加载策略分为本地AB包、远程AB包，如果本地有AB包则从本地加载
而且认为本地AB包是不会变化的即使远程AB文件与本地文件不同也是默认加载本地AB包。
AssetBundle下载、版本管理、加载/卸载,能够根据不同平台实现加载不同AB包
实现场景、模型的加载卸载

特殊代码：
ABMainName 主包名
LoadSavePath 下载本地位置
DefaultPath 默认AB包位置
TempPath 临时下载路径 临时下载路径 每次都默认下载对比文件，然后比较临时对比文件和本地位置  
ServerPath 服务器路径

使用指南：
使用IIS服务器实例
  服务器存放远程AB包资源文件，ServerPath写入地址。
  LoadSavePath、TempPath位置不改变。
  DefaultPath存放首包。
  每次重新生成AB包文件后通过工具【AB包工具=>Temp对比资源生成】生成对比文件，然后上传到服务器。
  下载失败后会重新下载5次
  注：如果本地没有AB包，首包里也没有AB包。则会报错找不到AB包