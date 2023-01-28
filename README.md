# ServerStatus-CSharpClient

框架：.Net 7.0

使用C#写的ServerStatus客户端

基于Cokemine大大的[Golang版](https://github.com/cokemine/ServerStatus-goclient)重构而成

## Build

编译前，您需安装`.Net 7 SDK`，这里以Ubuntu 20.04安装作为示范

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update &&  sudo apt-get install -y dotnet-sdk-7.0
```

开始编译

```
git clone https://github.com/XiaoSong0919/ServerStatus-CSharpClient.git
cd ServerStatus-CSharpClient
dotnet build
```

可执行文件输出路径在`bin/Debug/net7.0/`

等待片刻即可完成编译

### Notice：您可以使用低于7.0的.Net SDK进行编译，但这会导致可执行文件在Linux上抛出异常

## Run

运行前需安装`.Net 7 Runtime`，这里以Ubuntu 20.04安装作为示范

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update &&  sudo apt-get install -y dotnet-runtime-7.0
```

运行时需传入客户端对应参数。

假设你的服务端地址是`yourip`，客户端用户名`username`，密码`password`

端口号`35601`

你可以这样运行

```bash
chmod +x status-CSharpClient
./status-CSharpClient -dsn "username:password@yourip:35601"
```

即用户名密码以`:`分割，登录信息和服务器信息以`@`分割，地址与端口号以`:`分割。

默认端口号是35601，所以你可以忽略端口号不写，即直接写`username:password@yourip`

## Usage

### 如何指定NIC

如需上报特定NIC的信息，您可以使用`-i [NIC id]`参数

使用该参数前，需使用`-i list`获取该NIC的ID

```
>-i list
---------------------------------------------------------
NICs                                     NIC id
本地连接* 1                              {1743FAED-94B7-4A91-XXXX-C4F86968B3DE}
```
由此可以获得该NIC的id`1743FAED-94B7-4A91-XXXX-C4F86968B3DE`，接下来只需在运行时加入`-i 1743FAED-94B7-4A91-XXXX-C4F86968B3DE`参数即可上报此网卡的信息

如果没有使用`-i`参数，那么ServerStatus-CSharpClient将会对所有NIC进行求和并上报

```
  -dsn string
        Input DSN, format: username:password@host:port
        eg: use IPv4   -dsn "Test:doub.io@127.0.0.1:35601" or -dsn "Test:doub.io@127.0.0.1"
            use IPv6   -dsn "Test:doub.io@[::1]:35601" or -dsn "Test:doub.io@[::1]"
            
  -i [NIC id]                                 Report specified Network Interface Card(NIC)");
        eg: Print Network Interface Card List  -i list\n");
        
  -interval float
        Input the INTERVAL (default 2.0)
  -h | --help
```




