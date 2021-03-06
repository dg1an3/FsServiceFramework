﻿open System

open FsServiceFramework

open Trending.Contracts
open Trending.Services

open Worklist.Contracts
open Worklist.Services

open Import.Contracts
open Import.Services.Impl

open Unity
open Unity.Interception.ContainerIntegration
open Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception


[<EntryPoint>]
let main argv = 

    //let da = { new ITrendingDataAccess with
    //            member this.GetTrendingSeries(seriesId: int): SiteTrendingSeries = 
    //                raise (System.NotImplementedException())
    //            member this.UpdateTrendingSeries(series: SiteTrendingSeries): unit = 
    //                raise (System.NotImplementedException()) }

    //let container = new UnityContainer()
    //container.RegisterType(typedefof<ITrendingDataAccess>, da.GetType(),
    //        Interceptor<InterfaceInterceptor>(),
    //        (Utility.unityInterceptionBehavior (fun input inner -> 
    //            use opId = { new IDisposable with member this.Dispose() = () }
    //            inner input) |> InterceptionBehavior)) |> ignore

    //let getDa = container.Resolve<ITrendingDataAccess>()

    // id value for testing
    let seriesId = 1    

    // create an identity matrix
    let diag i = if i/4 = i%4 then 1. else 0.
    let matrix = Array.init 16 diag

    // function to create a shift that correlates with the id
    let shiftForId (id:int) = Array.init 3 (fun x -> float(x+id))

    let createAndPopulateRepository () =
        // construct a repository for test data
        let repository = 
            VolatileRepository<int, SiteTrendingSeries>(fun sts -> sts.Id) 
                :> IRepository<int, SiteTrendingSeries>

        // helper to create STS record for a given index
        let createSiteTrendingSeriesForIndex i =
            SiteTrendingSeries(Id = i,
                Label = i.ToString(),
                Protocol = TrendingProtocol(Algorithm = "trend", Tolerance = 1.0 ),
                SeriesItems = [ { AllResults = []; SelectedResult = {Label=""; Matrix=matrix} };
                                { AllResults = []; SelectedResult = {Label=""; Matrix=matrix} } ],
                Shift = (shiftForId i))

        // add some test record
        { seriesId..seriesId+2 }
        |> Seq.map createSiteTrendingSeriesForIndex            
        |> Seq.iter (fun sts -> repository.Create(sts) |> ignore)
        repository

    Log.Out(Debug "Creating test repository...")
    let repository = createAndPopulateRepository()

    // create standard hosting container
    // TODO: figure out how to dispose proxy manager better
    let container = Hosting.createHostContainer() 
    container
    |> ComponentRegistration.registerService_ typedefof<ITrendingManager> typedefof<TrendingManagerService>
    |> ComponentRegistration.registerService_ typedefof<ITrendingEngine> typedefof<TrendingEngineService>
    |> ComponentRegistration.registerService_ typedefof<ITrendingDataAccess> typedefof<TrendingDataAccess>
    |> ComponentRegistration.registerFunction<ITrendCalculationFunction, TrendCalculation>
    |> ComponentRegistration.registerRepositoryInstance<int, SiteTrendingSeries>(repository)
    |> ComponentRegistration.registerService_ typedefof<IWorklistManager> typedefof<WorklistManagerService>
    |> ComponentRegistration.registerService_ typedefof<IWorklistEngine> typedefof<WorklistEngineService>
    |> ComponentRegistration.registerService_ typedefof<IImportManager> typedefof<ImportManager>
    |> Hosting.startServices
    Console.ReadLine() |> ignore

    Log.Out(Debug "closing services")
    Hosting.stopServices container

    0 // return 0 for OK
