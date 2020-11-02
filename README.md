# Silancer
让大家一起快乐地搅浑水，成为无忧无虑地乐子人吧

**该项目打击对象仅限于发表或支持损害中国任何利益的主播及其相关主播，目的仅限于破坏打击对象直播间的正常互动秩序，除此以外任何其他使用途径均非该项目创立意图**

**请勿在违反任何地区法律的途径上使用，请使用者不要以违法甚至犯罪的途径表达自己合情合理合法的情感**

>**敬告：下载、使用、拷贝本项目源代码及可执行文件，即视为您无条件支持中华人民共和国民众及中华人民共和国政府的一切意向与决策，包括但不限于领土问题、主权问题、民族问题等**

>**敬告：下載、使用、拷貝本項目源代碼及可執行文件，即視為您無條件支持中華人民共和國民眾及中華人民共和國政府的一切意向與決策，包括但不限於領土問題、主權問題、民族問題等**

>**Warning: Downloading, using, and copying the source code or executable files of this project is deemed to be your unconditional support for all the intentions and decisions of both the people and the government of the People’s Republic of China, including but not limited to territorial issues, sovereignty issues, ethnic issues, etc.**

另外，桐生可可必死。

## 环境需求
Silancer基于.net Core 3.1完成，因此在您需要运行Silancer的设备上必须妥善安装.net Core 3.1运行时

对于Windows系统，请参照 [在 Windows 上安装 .NET Core](https://docs.microsoft.com/zh-cn/dotnet/core/install/windows?tabs=netcore31)

对于MacOS系统，请参照 [在 macOS 上安装 .NET Core](https://docs.microsoft.com/zh-cn/dotnet/core/install/macos)

对于Linux的各种发行版系统，请参照 [在 Linux 上安装 .NET Core](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux)

## 快速开始
1. 启动命令行
    - **Windows**
    
    在Windows下可以直接运行silancer.exe启动命令行
    
    - **Linux**
    
    使用dotnet silancer.dll

2. 配置文件
    - **主配置文件**

    主配置文件必须位于可执行文件运行时的工作目录下，且文件名必须为`settings.json`
    
    该文件应该以json字符串形式存储，json字符串中应该包含一个字典，其中包括名为`Lancers_Json_Path`、`Enemies_Json_Path`、`Ammos_Folder`的三个元素，分别用于存储Lancers配置文件的位置，Enemies配置文件的位置和Ammos文件夹的位置
    
    主配置文件用于指示程序从哪些位置读取Lancer、Enemy和Ammos列表
    
    - **Lancers配置文件**
    
    Lancers配置文件中应该以json字符串形式存储，其中内容将被直接逆序列化为一个List<Dictionary<string,string>>并且随后逐个元素被解析为Lancer
    
    每个列表元素都应该是一个字典，并且每个字典中应该包含以下元素：
    
        - **Name**
        
        用于在程序内辨识该Lancer，如果发生重复，则会自动添加后缀
        
        - **Key**
        
        谷歌下发的固定令牌，一般不发生改变，可以从发送消息的包中获得
        
        - **Cookie**
        
        当前帐号的Cookie，可以从发送消息的包Headers中获得
        
        - **Onbehalfofuser**
        
        当使用非主频道时，发送消息的包负载中会出现该参数，目前该参数是否必需仍需观察
    
    - **Enemies配置文件**
       
    Enemies配置文件中应该以json字符串形式存储，其中内容将被直接逆序列化为一个List<Dictionary<string,string>>并且随后逐个元素被解析为Enemy
    
    每个列表元素都应该是一个字典，并且每个字典中应该包含以下元素：
    
        - **Name**
        
        用于在程序内辨识该Enemy，Name应该唯一
        
        - **ChannelID**
        
        ChannelID是YTB用于区分Youtuber的字符串，可以从Youtuber的首页URL中获取，该参数一般不会改变
        
        - **LiveID**
        
        LiveID是每次直播的唯一标识符，可以从直播页面URL中获取，每次直播该参数都会改变

    - **Ammos文件夹**
    
    Ammos文件夹用于存储弹药，所有弹药应该以文本文件存储，每个文件都会被Servant自动化分为一个库，每个文件内的每一非空行都被视为一发弹药

3. 发布攻击命令
使用命令CREATE Interval LancerName EnemyName AmmosMode \[AmmosListName\]来使一个Lancer发起攻击，各参数详细释义已在下方给出：

    - **CREATE**

    命令关键词，大小写不敏感

    - **Interval**

    发送评论间隔，单位为毫秒

    - **LancerName**

    此次分配的Lancer名，该名对应Lancers配置文件中的Name项，大小写敏感

    - **EnemyName**

    让该Lancer发动攻击的目标Enemy名，改名对应Enemies配置文件中的Name项，大小写敏感

    - **AmmosMode**

    装填弹药的模式，使用Random表示随机，Loop表示顺序循环，大小写不敏感

    - **AmmosListName**

    可选参数，指定弹药库，改名对应Ammos文件夹下的文件名
