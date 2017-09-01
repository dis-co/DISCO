namespace Iris.Tests

open System
open System.IO
open Expecto
open Iris.Core
open Iris.Raft
open System.Net
open Iris.Core

[<AutoOpen>]
module AssetTests =
  type TestAsset = { Data: string }
    with
      member self.HasParent with get () = false

      member self.AssetPath
        with get () = filepath "test-asset.txt"

      member self.ToYaml(_) = self.Data

      member self.Save(basePath: FilePath) =
        either {
          let path = basePath </> Asset.path self
          let! info = IrisData.write path (Payload self.Data)
          return ()
        }

      static member Load(path: FilePath) : Either<IrisError, TestAsset> =
        either {
          let! data = IrisData.read path
          return { Data = data }
        }


  let test_write_read_asset_correctly =
    testCase "should write and read asset correctly" <| fun _ ->
      either {
        let path = tmpPath()
        let payload = string (Id.Create())
        let! info = IrisData.write path (Payload payload)
        let! data = IrisData.read path
        expect "Payload should be the same" payload id data
      }
      |> noError

  let test_save_load_asset_correctly =
    testCase "should save and load asset correctly" <| fun _ ->
      either {
        let path = tmpPath()
        let asset = { Data = string (Id.Create()) }
        do! Asset.save path asset
        let path = path </> Asset.path asset
        let! reasset = Asset.load path
        expect "Loaded asset should be the same" reasset id asset
      }
      |> noError

  let test_save_with_commit_adds_and_commits_an_asset =
    testCase "should save an asset with commit even if its new" <| fun _ ->
      either {
        let path = tmpPath()
        let! repo = Git.Repo.init path
        let signature = User.Admin.Signature
        let asset = { Data = (string (Id.Create())) }
        let! commit = IrisData.saveWithCommit path signature asset
        let path = path </> Asset.path asset
        let! reasset = Asset.load path
        expect "Loaded asset should be the same" reasset id asset
      }
      |> noError

  let assetTests =
    testList "Asset Tests" [
      test_write_read_asset_correctly
      test_save_load_asset_correctly
      test_save_with_commit_adds_and_commits_an_asset
    ] |> testSequenced
