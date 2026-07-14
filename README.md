# SSO
Architecture CQRS Full (.NET 10.0)

#### Migrations

Go to "SSO.Infrastructures.Data" project folder and open cmd
> cd src/SSO.Infrastructures.Data

Add migration ex: InitialMigrationDefaultDbContext (--verbose for more details)
> dotnet ef --startup-project ../SSO.Web.Api migrations add [Name of the migration]DefaultDbContext -c DefaultDbContext -o Default/Migrations


#### dotnet-ef install
> dotnet tool install --global dotnet-ef

#### dotnet-ef update
> dotnet tool update --global dotnet-ef
