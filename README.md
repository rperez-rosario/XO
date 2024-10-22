# Introduction 
An e-commerce web application built using SQL Server, EF, ASP. NET Core MVC, C#, Javascript and an assortment of other libraries and web services.

Data Entity Relationship Diagram
<img width="2003" alt="XO Skin Web Commerce App ER Diagram" src="https://github.com/rperez-rosario/XO/assets/24212098/d28323fe-9f93-4088-b1fe-fd4ee1752e6e">
<br/><br/>
Application Class Diagram
<img width="1825" alt="XOSkinWebApp Dependency Graph" src="https://github.com/rperez-rosario/XO/assets/24212098/ad4adbdc-24ef-464a-90ea-638d1bc0ea97">

# Getting Started
1.	Installation process:

(See "Build and Test" section of this document.)

2.	Software dependencies:

<ul>
<li>ASP.NET Core 5.0 MVC</li>
<li>Entity Framework Core</li>
<li>C#</li>
<li>Javascript</li>
<li>JQuery</li>
<li>Bootstrap</li>
<li>DataTables</li>
<li>Razor</li>
<li>ShipEngine Web API</li>
<li>TaxJar Web API</li>
<li>Shopify Web API</li>
<li>SendGrid Web API</li>
<li>Stripe Payment Gateway Web API</li>
</ul>

3.	Web API, languages and technology stack references:

<ul>
<li>https://docs.microsoft.com/en-us/dotnet/csharp/</li>
<li>https://docs.microsoft.com/en-us/ef/</li>
<li>https://getbootstrap.com/</li>
<li>https://www.javascript.com/</li>
<li>https://jquery.com/</li>
<li>https://datatables.net/</li>
<li>https://dotnet.microsoft.com/apps/aspnet/mvc</li>
<li>https://www.microsoft.com/en-us/sql-server/sql-server-2019</li>
<li>https://www.iis.net/</li>
<li>https://azure.microsoft.com/en-us/</li>
<li>https://www.shipengine.com/docs/getting-started/</li>
<li>https://www.taxjar.com/product/api</li>
<li>https://shopify.dev/api</li>
<li>https://stripe.com/docs/api</li>
<li>https://docs.sendgrid.com/</li>
</ul>

# Build and Test
1. Create and configure a private application within the target Shopify store.
2. Configure additional external services (ShipEngine, SendGrid, TaxJar and Stripe.) 
3. Extract and deploy current .dacpac to SQL Server. Configure security, and business-domain 
entities as needed.
4. Web or folder deploy to IIS or Azure Cloud App (.NET Core 5.0 application pool), 
configure appsettings.json as needed.
5. Access using a web browser or build and execute Visual Studio solution.
