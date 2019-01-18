﻿namespace FsServiceFramework

open System
open System.Runtime.Serialization
open System.ServiceModel

open Unity

[<DataContract>]
[<CLIMutable>]
type DurableInstanceContex = {
    [<DataMember>] DurableInstanceId : Guid
    [<DataMember>] InstanceCreation : DateTime }

module Instance =

    (* TODO: why is this here? *)
    type StorageProviderInstanceContextExtension(container:IUnityContainer) =
        interface IExtension<InstanceContext> with
            member this.Attach (owner:InstanceContext) = ()
            member this.Detach (owner:InstanceContext) = ()
            