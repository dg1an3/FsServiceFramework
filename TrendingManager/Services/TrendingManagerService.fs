﻿namespace TrendingManager

open System
open System.ServiceModel
open Infrastructure

open MathNet.Numerics.LinearAlgebra

open TrendingManager.Contracts

[<ProvidedInterface(typedefof<ITrendingManager>)>]
[<RequiredInterface(typedefof<ITrendingEngine>)>]
[<ServiceBehavior(IncludeExceptionDetailInFaults=true)>]
type TrendingManagerService(pm:IProxyManager) =
    interface ITrendingManager with
        member this.GetSeries siteId =
            let protocol = { 
                Algorithm = "trend"; 
                Tolerance = 1.0 } 
            let items = 
                [ { AllResults = []; SelectedResult = {Label=""; Matrix=[|1.0;0.0;0.0;0.0;1.0;0.0;0.0;0.0;1.0;0.0;0.0;0.0;1.0;0.0;0.0;0.0;|] } };
                  { AllResults = []; SelectedResult = {Label=""; Matrix=[|1.0;0.0;0.0;0.0;1.0;0.0;0.0;0.0;1.0;0.0;0.0;0.0;1.0;0.0;0.0;0.0;|] } } ]
            { Label = siteId.ToString();
              Protocol = protocol;
              SeriesItems = items;
              Shift = [| 1.0; 2.0; 3.0 |] }
        member this.UpdateSeries series =
            let proxy = pm.GetProxy<ITrendingEngine>()
            proxy.CalculateTrendForSeries series
        member this.UpdateSiteOffset series =
            let proxy = pm.GetProxy<ITrendingEngine>()
            proxy.UpdateSiteOffset series