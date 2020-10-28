# Silancer
让大家一起快乐地搅浑水，成为无忧无虑地乐子人吧

**该项目仅限正常使用，请勿在违反任何地区法律的途径上使用，请乐子人不要以违法甚至犯罪的途径表达自己合情合理合法的情感**

另外，桐生可可必死。

## Silancer.Lancer类
实例方法`Lancer.Command(AttackMode, string)`方法可以向该实例传达一次“命令”

其中首个参数`AttackMode`是一个枚举体，`AttackMode.Normal`表示普通发言，`AttackMode.MegaAttack`表示穿甲弹

## 配置文件
`enemies.json`是用于存储WEB参数的配置文件，该Json文件应该包含一个列表，并且列表元素均为字典，总体形如
>[
>  {
>    "Name": "",
>    "Key": "",
>    "Cookie": "",
>    "Authorization": "",
>    "Param": "",
>    "Onbehalfofuser": ""
>  }
>]

其中，Key、Cookie、Authorization、Param是构造请求的必选参数，Key、Cookie和Authorization来自于头文件，Param来自于负载段

Name是另一个必选参数，但它是用于本地识别的特殊参数，你可以使用相同的名字，但是这些名字在读入过程中会被附加尾缀