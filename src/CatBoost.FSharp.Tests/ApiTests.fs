module ApiTests

open Faqt
open FsToolkit.ErrorHandling
open CatBoost.FSharp
open CatBoost.FSharp.Api
open Xunit
open TestHelpers

[<Fact>]
let ``getSupportedEvaluatorTypes returns expected result`` () =
    result {
        use model = modelCalcerCreate ()
        
        let! supportedTypes = getSupportedEvaluatorTypes model
        supportedTypes.Should().Be([| CPU; GPU |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelMetadata returns expected result on empty model`` () =
    result {
        use model = modelCalcerCreate ()
        
        let! metadata = getModelMetadata model
        metadata.TreeCount.Should().Be(0) |> ignore
        metadata.FloatFeaturesCount.Should().Be(0) |> ignore
        metadata.CatFeaturesCount.Should().Be(0) |> ignore
    }
    |> assertOk
