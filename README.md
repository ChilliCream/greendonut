![GreenDonut](https://cdn.rawgit.com/ChilliCream/greendonut-logo/master/img/greendonut-banner-light.svg)

[![GitHub release](https://img.shields.io/github/release/chillicream/GreenDonut.svg)](https://github.com/ChilliCream/greendonut/releases) [![NuGet Package](https://img.shields.io/nuget/v/greendonut.svg)](https://www.nuget.org/packages/GreenDonut/) [![License](https://img.shields.io/github/license/ChilliCream/greendonut.svg)](https://github.com/ChilliCream/greendonut/releases) [![Build](https://ci.appveyor.com/api/projects/status/fm01y9pt10f84145/branch/master?svg=true)](https://ci.appveyor.com/project/rstaib/greendonut) [![Tests](https://img.shields.io/appveyor/tests/rstaib/greendonut/master.svg)](https://ci.appveyor.com/project/rstaib/greendonut) [![Coverage Status](https://sonarcloud.io/api/project_badges/measure?project=GreenDonut&metric=coverage)](https://sonarcloud.io/dashboard?id=GreenDonut) [![Quality](https://sonarcloud.io/api/project_badges/measure?project=GreenDonut&metric=alert_status)](https://sonarcloud.io/dashboard?id=GreenDonut) [![BCH compliance](https://bettercodehub.com/edge/badge/ChilliCream/greendonut?branch=master)](https://bettercodehub.com/)

---

**Green Donut is a DataLoader implementation for _.net core_ and _classic_**

Here is a short sentence how _facebook_ describes _DataLoaders_.

> DataLoader is a generic utility to be used as part of your application's data fetching layer to
> provide a consistent API over various backends and reduce requests to those backends via batching
> and caching. -- facebook

_DataLoaders_ are a perfect fit for client-side and server-side scenarios. They decouple any kind of
request to a back-end component or resource. This will reduce in general the traffic (round-trips)
to e.g. a _GraphQL API_, _REST API_, _DB_, or something completly else.

## Getting Started

Before we start let us install the package via _NuGet_.

For _.net core_ we could use the dotnet CLI. Which is perhaps the preferred way doing this.

```powershell
dotnet add package GreenDonut
```

And for _.net classic_ we still could use the following line.

```powershell
Install-Package GreenDonut
```

Of course there are more ways to install a package. However, I just focused here on the most common
console way for both _.net core_ and _classic_.

After we have installed the package, we should probably start using it, right. We really tried to
keep the _API_ of _DataLoaders_ congruent to the
[original facebook implementation which is written in JavaScript](https://github.com/facebook/dataloader),
but without making the experience for us .net developers weird.

```csharp
var userLoader = new DataLoader<string, User>(keys => FetchUsers(keys));
```

In order to change the default behavior of a `DataLoader`, we have to create a new instance of
`DataLoaderOptions` and pass it right into the `DataLoader`. Let us see how it would look like.

```csharp
var options = new DataLoaderOptions<string>
{
    SlidingExpiration = TimeSpan.FromHours(1)
};
var userLoader = new DataLoader<string, User>(keys => FetchUsers(keys), options);
```

So, what we see here is that we have changed the `SlidingExpiration` from its default value, which
is `0` to `1 hour`. `0` means the cache entries will live forever in the cache as long as the max
cache size does not exceed. Whereas `1 hour` means a single cache entry will stay in the cache as
long as the entry gets touched within one hour. This is an additional feature that does not exist in
the original _facebook_ implementation.

### API

| Methods           | Description                                                                                                                                                                                                                     |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Clear()`         | Empties the complete cache.                                                                                                                                                                                                     |
| `DispatchAsync()` | Dispatches one or more batch requests. In case of auto dispatching we just trigger an implicit dispatch which could mean to interrupt a wait delay. Whereas in a manual dispatch scenario it could mean to dispatch explicitly. |
| `LoadAsync(key)`  | Loads a single value by key. This call may return a cached value or enqueues this single request for bacthing if enabled.                                                                                                       |
| `LoadAsync(keys)` | Loads multiple values by keys. This call may return cached values and enqueues requests which were not cached for bacthing if enabled.                                                                                          |
| `Remove(key)`     | Removes a single entry from the cache.                                                                                                                                                                                          |
| `Set(key, value)` | Adds a new entry to the cache if not already exists.                                                                                                                                                                            |

### Best Practise

- Be careful when and how to create `DataLoader` instances, becuase sometimes users have different
  privileges. That implies perhaps a `DataLoader` on a per request base. However, it really depends
  on your application logic and the specific case you try to find a perfect solution for.

## Documentation

For more examples and a detailed documentation click [here](http://greendonut.io).
