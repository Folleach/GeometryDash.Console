**GeometryDash.Console** is CLI Tool for Geometry Dash

Install from [NuGet](https://www.nuget.org/packages/GeometryDash.Console)

```
dotnet tool install --global GeometryDash.Console --version 1.0.2
```

# Commands

## pack

**Unpacking .dat file to .xml (.plist)**

```cmd
gd unpack CCLocalLevels.dat levels.xml
```

After executing the command, a file will be created (or overwritten): `levels.xml`

## unpack

**Packing .xml file to .dat**

```
gd pack levels.xml CCLocalLevels.dat
```

After executing, a file will be created (or overwritten): `CCLocalLevels.dat`

## featured

**Lists the featured levels**

```
gd featured --page 1
```

![image](https://github.com/Folleach/GeometryDash.Console/assets/32067915/c234a0d0-baeb-4497-a1de-8d91218d5c1a)

Or in json format

```
gd featured --page 1 --json
```

<img src="https://github.com/Folleach/GeometryDash.Console/assets/32067915/7ec8dbf3-0c2f-40c1-9c5a-50995bb64cf2" width="400px" />
