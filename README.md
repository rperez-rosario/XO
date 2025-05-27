# Introduction 
An e-commerce web application built using SQL Server, EF, ASP. NET Core MVC, C#, Javascript and an assortment of other libraries and web services.

Data Entity Relationship Diagram
<img width="2003" alt="XO Skin Web Commerce App ER Diagram" src="https://github.com/rperez-rosario/XO/assets/24212098/d28323fe-9f93-4088-b1fe-fd4ee1752e6e">
<br/><br/>
Application Class Diagram
<img width="1825" alt="XOSkinWebApp Dependency Graph" src="https://github.com/rperez-rosario/XO/assets/24212098/ad4adbdc-24ef-464a-90ea-638d1bc0ea97">
<br/><br/>
<img width="836" alt="Web Interface" src="https://github.com/user-attachments/assets/1022fc50-d842-496d-b6ff-882c4598fdc9" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/ece3a66d-f8fe-4a0f-a9fd-cd0a10a6085f" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/8cd20b74-adfc-49af-8cd0-c5ea8c24fe71" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/364723a4-684b-4f72-8302-0364a6417b63" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/dcebac46-b37d-46da-9484-514d79dc31e7" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/06caa437-24a5-48b3-aaba-ebe957f6a212" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/fba851ed-b3d7-4b53-9955-e9d7cdfbe6d7" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/305d157f-66da-4e3c-8dc9-8743721b051e" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/0e85340c-9a91-4e12-887e-31a068c4fbc1" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/7ae93b51-4f99-43ba-af52-80783727bdb0" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/6644f6b5-f572-4fbc-b26e-bae81a1a8c15" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/39b097af-46c0-44c1-a167-7c60e723ea34" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/b99f8a96-84a7-48f8-9a7a-bef30dd81611" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/1fab3194-95dc-4845-9468-30ed5d468ec0" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/8b86fd4e-dbd5-45ae-9152-e89225bd512e" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/13558880-e858-417f-8de1-6e8c40e5cb63" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/0492a70e-ccda-45e7-8dd5-0bdd1cbc1ff5" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/4974ad35-2521-424a-9bbc-ad553e1d1bca" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/87ad2b3d-0ec8-4e18-8828-9d3fe6cc112a" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/38f4db1a-a1a8-4d55-94e5-635077fe50e4" />
<br/><br/>
<img width="836" alt="image" src="https://github.com/user-attachments/assets/557e94b3-5d42-406d-9f9f-6b87c3c4fc43" />


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
