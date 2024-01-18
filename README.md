# IntellisenseTranslator
这是一个汉化 Visual Studio 智能感知的工具
从 [dsjzazs/IntellisenseTranslator](https://github.com/dsjzazs/IntellisenseTranslator) 修改而来


## 翻译效果
![image](https://user-images.githubusercontent.com/13758552/185071382-cc7d314f-ccfc-40ab-93ed-b78b77c9e093.png)
![image](https://user-images.githubusercontent.com/13758552/185071408-8fedf271-ee1e-4a75-8378-45f27794eb87.png)
![image](https://user-images.githubusercontent.com/13758552/185071468-f4b38b3a-4090-4f66-87f2-d33b2e6f7897.png)
![image](https://user-images.githubusercontent.com/13758552/185071552-22772b11-d2ff-4a5e-a620-9b73f6bfe498.png)
### 同时显示翻译的中文和原文


## 如何更新字典并翻译？
1. 使用管理员身份运行 IntellisenseTranslator.exe
2. 选择需要汉化的目录，会自动搜索所有子目录中的未翻译文件
3. 选择是否使用本地字典汉化，否在使用在线翻译
4. 选择是否汉化完成后自动替换目标文件并在目标文件夹中备份，否则手动从软件所在目录的translate文件夹中复制到目标并替换
5. 是否更新本地字典，更新完成后会自动创建一个[yyyyMMddHHmmssfff].json的字典文件，如果你愿意分享，请提交你的文件到原项目。
6. 点击翻译，如手动替换参见4. 
7. 重启Visual Studio。







## 当出现未翻译的内容如何处理？
![image](https://user-images.githubusercontent.com/13758552/184807374-ad6a54d3-e1f8-4013-b0b6-0b1e661a7f88.png)
可以右键转到定义，并将滚动条拉到文件最上方，复制所引用的dll文件夹路径：
![image](https://user-images.githubusercontent.com/13758552/184807607-d1f48153-86a2-4e89-a905-9c5870ea07a7.png)
打开这个文件夹路径后，可以看到xml的文件：
![image](https://user-images.githubusercontent.com/13758552/184807756-7c5982a5-4119-4c71-b4d8-69f1db080f97.png)

此时你可以通过翻译工具，对这个目录进行翻译，并将翻译后的xml文件，复制到此目录替换。
你也可以直接跳转到packages文件夹，对所有nuget缓存的xml文件进行翻译：

![image](https://user-images.githubusercontent.com/13758552/184807980-d0b6f696-b508-4d73-b87a-0db47124f036.png)
