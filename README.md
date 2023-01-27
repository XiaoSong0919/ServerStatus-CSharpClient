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

```
  -dsn string
        Input DSN, format: username:password@host:port
  -interval float
        Input the INTERVAL (default 2.0)
  
```




