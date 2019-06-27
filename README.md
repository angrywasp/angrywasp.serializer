# AngryWasp.Serializer

Provides strongly typed serialization of classes to/from XML

## Building

Requires .NET Core

`dotnet restore && dotnet build -c Release`

## How-To

``` cs
Serializer.Initialize();
public Foobar fb = new Foobar();  
XDocument xd = new ObjectSerializer().Serialize(fb);  
Foobar fb2 = new ObjectSerializer().Deserialize<Foobar>(xd); 
```
