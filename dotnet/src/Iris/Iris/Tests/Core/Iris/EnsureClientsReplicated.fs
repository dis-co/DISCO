namespace Iris.Tests

open System.IO
open System.Threading
open Expecto

open Iris.Core
open Iris.Service
open Iris.Client
open Iris.Client.Interfaces
open Iris.Service.Interfaces
open Iris.Raft
open Iris.Net

open Common

module EnsureClientsReplicated =

  let test =
    testCase "ensure connected clients are forwarded to leader" <| fun _ ->
      either {
        use electionDone = new AutoResetEvent(false)
        use addClientDone = new AutoResetEvent(false)
        use appendDone = new AutoResetEvent(false)
        use clientRegistered = new AutoResetEvent(false)
        use clientAppendDone = new AutoResetEvent(false)
        use updateDone = new AutoResetEvent(false)
        use pushDone = new AutoResetEvent(false)

        let! (project, zipped) = mkCluster 2

        let serverHandler id = function
          | IrisEvent.GitPush _                        -> pushDone.Set() |> ignore
          | IrisEvent.StateChanged(oldst, Leader)      -> electionDone.Set() |> ignore
          | IrisEvent.Append(Origin.Raft, AddClient _) -> addClientDone.Set() |> ignore
          | IrisEvent.Append(Origin.Raft, msg)         -> appendDone.Set() |> ignore
          | _ -> ()

        let handleClient = function
          | ClientEvent.Registered              -> clientRegistered.Set() |> ignore
          | ClientEvent.Update (AddCue _)       -> clientAppendDone.Set() |> ignore
          | ClientEvent.Update (AddPinGroup _)  -> clientAppendDone.Set() |> ignore
          | ClientEvent.Update (UpdateSlices _) -> updateDone.Set() |> ignore
          | _ -> ()

        //  ____                  _            _
        // / ___|  ___ _ ____   _(_) ___ ___  / |
        // \___ \ / _ \ '__\ \ / / |/ __/ _ \ | |
        //  ___) |  __/ |   \ V /| | (_|  __/ | |
        // |____/ \___|_|    \_/ |_|\___\___| |_|

        let mem1, machine1 = List.head zipped

        use! service1 = IrisService.create {
          Machine = machine1
          ProjectName = project.Name
          UserName = User.Admin.UserName
          Password = password Constants.ADMIN_DEFAULT_PASSWORD
          SiteId = None
        }

        use oobs1 = service1.Subscribe (serverHandler machine1.MachineId)

        do! service1.Start()

        //  ____                  _            ____
        // / ___|  ___ _ ____   _(_) ___ ___  |___ \
        // \___ \ / _ \ '__\ \ / / |/ __/ _ \   __) |
        //  ___) |  __/ |   \ V /| | (_|  __/  / __/
        // |____/ \___|_|    \_/ |_|\___\___| |_____|

        let mem2, machine2 = List.last zipped

        use! service2 = IrisService.create {
          Machine = machine2
          ProjectName = project.Name
          UserName = User.Admin.UserName
          Password = password Constants.ADMIN_DEFAULT_PASSWORD
          SiteId = None
        }

        use oobs2 = service2.Subscribe (serverHandler machine2.MachineId)

        do! service2.Start()
        do! waitOrDie "electionDone" electionDone

        //   ____ _ _            _     _
        //  / ___| (_) ___ _ __ | |_  / |
        // | |   | | |/ _ \ '_ \| __| | |
        // | |___| | |  __/ | | | |_  | |
        //  \____|_|_|\___|_| |_|\__| |_|

        let serverAddress1:IrisServer = {
          Port = mem1.ApiPort
          IpAddress = mem1.IpAddr
        }

        use client1 = ApiClient.create serverAddress1 {
          Id = Id.Create()
          Name = "Client 1"
          Role = Role.Renderer
          ServiceId = mem1.Id
          Status = ServiceStatus.Starting
          IpAddress = IpAddress.Localhost
          Port = port 12345us
        }

        use clobs1 = client1.Subscribe (handleClient)
        do! client1.Start()
        do! waitOrDie "clientRegistered" clientRegistered
        do! waitOrDie "addClientDone" addClientDone

        //   ____ _ _            _     ____
        //  / ___| (_) ___ _ __ | |_  |___ \
        // | |   | | |/ _ \ '_ \| __|   __) |
        // | |___| | |  __/ | | | |_   / __/
        //  \____|_|_|\___|_| |_|\__| |_____|

        let serverAddress2:IrisServer = {
          Port = mem2.ApiPort
          IpAddress = mem2.IpAddr
        }

        use client2 = ApiClient.create serverAddress2 {
          Id = Id.Create()
          Name = "Client 2"
          Role = Role.Renderer
          ServiceId = mem2.Id
          Status = ServiceStatus.Starting
          IpAddress = IpAddress.Localhost
          Port = port 12345us
        }

        use clobs2 = client2.Subscribe (handleClient)
        do! client2.Start()
        do! waitOrDie "clientRegistered" clientRegistered
        do! waitOrDie "addClientDone" addClientDone

        //  _____         _
        // |_   _|__  ___| |_ ___
        //   | |/ _ \/ __| __/ __|
        //   | |  __/\__ \ |_\__ \
        //   |_|\___||___/\__|___/

        // we need to wait twice more for the append commands that have been forwardecd issued
        do! waitOrDie "addClientDone" addClientDone
        do! waitOrDie "addClientDone" addClientDone

        expect "Service 1 and Service 2 should have both 2 Clients"
          service1.State.Clients
          id
          service2.State.Clients
      }
      |> noError