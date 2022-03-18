<div class="d-flex align-items-center">
    <img src="https://blazorstrap.io/logo5.svg" alt="BootStrapV5 Logo" style="height: 100px"/>
    <h1>BlazorStrap 5</h1>
</div>

## Install

*We do not include bootstrap.min.css in the package. This already exists in the blazor wasm and blazor server side templates. Ensure that you are using Bootstrap v5 CSS or newer.*

Install the BlazorStrap v5.x package from nuget: [![nuget](https://img.shields.io/badge/nuget-Download%205.x-blue)](https://www.nuget.org/packages/BlazorStrap)

### Blazor WebAssembly

1. Modify your index.html with the following.
   1. Inside the `<head>` add:
      1. `<link href="YourAssemblyName.styles.css" rel="stylesheet" />`
      2. `<link href="path_to_bootstrap.min.css" rel="stylesheet" integrity="if_needed" />"`
   2. At the end of the `<body>` add:
      1. `<script src="_content/BlazorStrap/popper.min.js"></script>`
      2. `<script src="_content/BlazorStrap/blazorstrap.js"></script>`
2. In `Program.cs` add:
   1. `builder.Services.AddBlazorStrap();` to your `WebApplication` builder pipeline
3. In `_Imports.razor` add:
   1. `@using BlazorStrap`

### Blazor Server Side

1. Modify your _Host.cshtml or _Layout.cshtml with the following:
   1. Inside the `<head>` add:
      1. `<link href="YourAssemblyName.styles.css" rel="stylesheet" />`
      2. `<link href="path_to_bootstrap.min.css" rel="stylesheet" integrity="if_needed" />"`
   2. At the end of the `<body>` add:
      1. `<script src="_content/BlazorStrap/popper.min.js"></script>`
      2. `<script src="_content/BlazorStrap/blazorstrap.js"></script>`
2. In `Program.cs` or `Startup.cs` add:
   1. `builder.Services.AddBlazorStrap();` to your `WebApplication` builder pipeline
3. In `_Imports.razor` add:
   1. `@using BlazorStrap`
   