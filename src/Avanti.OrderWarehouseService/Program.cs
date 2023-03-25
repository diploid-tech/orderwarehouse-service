// CA1852 Type 'Program' can be sealed because it has no subtypes in its containing assembly and is not externally visible
#pragma warning disable CA1852

// Global Usings
global using Akka.Actor;
global using Avanti.Core.Microservice;

// Main Entry
using Avanti.OrderWarehouseService;
Service.Run<ServiceSettings>(new ExtendedConfiguration());
