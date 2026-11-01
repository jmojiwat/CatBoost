/// <summary>
/// These tests were taken from Andrey Grazhdankov's catboost-cgo https://github.com/mirecl/catboost-cgo. Specifically
/// from https://github.com/mirecl/catboost-cgo/blob/master/catboost/catboost_test.go and translated to F#.
/// </summary>
module CatBoost.FSharp.Tests.Mirecl

open System
open System.IO
open Faqt
open FsToolkit.ErrorHandling
open CatBoost.FSharp
open CatBoost.FSharp.Api
open Xunit
open TestHelpers

let directory = Path.Combine(AppContext.BaseDirectory, "example")

[<Fact>]
let ``getCatFeatureIndices with classifier returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "classifier", "classifier.cbm")
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath

        let! catIndices = getCatFeatureIndices model
        
        catIndices.Should().Be([| 0UL; 1UL |]) |> ignore
        
        let! floatIndices = getFloatFeatureIndices model
        
        floatIndices.Should().Be[| 2UL; 3UL; 4UL; 5UL; |] |> ignore
    }
    |> assertOk

[<Fact>]
let ``getCatFeatureIndices with regression returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "regressor", "regressor.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
        
        let! catIndices = getCatFeatureIndices model
        
        catIndices.Should().Be([| |]) |> ignore
    }
    |> assertOk
    
[<Fact>]
let ``loadFullModelFromFile with fake returns expected result`` () =
    let filePath = Path.Combine(directory, "fake.cbm") 
    let model =
        modelCalcerCreate ()
        |> loadFullModelFromFile filePath
    
    model.Should().BeError() |> ignore
    
[<Fact>]
let ``calcModelPrediction with regressor model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "regressor", "regressor.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType RawFormulaVal model
        
        let floatFeatures = array2D [ [ 2f; 4f; 6f; 8f; ]; [ 1f; 4f; 50f; 60f; ] ]
        let catFeatures = Array2D.zeroCreate<string> 2 0
        
        let! prediction = calcModelPrediction floatFeatures catFeatures model
        
        prediction.Should().Be(array2D [| [| 15.625 |]; [| 18.125 |] |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPrediction with classifier model and class prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "classifier", "classifier.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Class model
        
        let floatFeatures = array2D [ [ 2f; 4f; 6f; 8f; 5f ]; [ 1f; 4f; 50f; 60f; 5f ] ]
        let catFeatures = array2D [ [ "a"; "b" ]; [ "a"; "d" ] ]
        
        let! prediction = calcModelPrediction floatFeatures catFeatures model
        
        prediction.Should().Be(array2D [| [| 1.0 |]; [| 1.0 |] |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPrediction with classifier model and probability prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "classifier", "classifier.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Probability model
        
        let floatFeatures = array2D [ [ 2f; 4f; 6f; 8f; 5f ]; [ 1f; 4f; 50f; 60f; 5f ] ]
        let catFeatures = array2D [ [ "a"; "b" ]; [ "a"; "d" ] ]
        
        let! prediction = calcModelPrediction floatFeatures catFeatures model
        
        prediction.Should().Be(array2D [| [| 0.629855013297618 |]; [| 0.5358421019868945 |] |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPrediction with multiclassification model and class prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "multiclassification", "multiclassification.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Class model
        
        let floatFeatures = array2D [ [ 1996f; 197f ]; [ 1968f; 37f ]; [ 2002f; 77f ]; [ 1948f; 59f ] ]
        let catFeatures = array2D [ [ "winter" ]; [ "winter" ]; [ "summer" ]; [ "summer" ] ]
        
        let! prediction = calcModelPrediction floatFeatures catFeatures model
        
        prediction.Should().Be(array2D [| [| 2.0 |]; [| 2.0 |]; [| 1.0 |]; [| 2.0 |] |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPrediction with multiclassification model and probability prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "multiclassification", "multiclassification.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Probability model
        
        let floatFeatures = array2D [ [ 1996f; 167f ]; [ 1968f; 37f ] ]
        let catFeatures = array2D [ [ "winter" ]; [ "winter" ] ]
        
        let! prediction = calcModelPrediction floatFeatures catFeatures model
        
        prediction.Should().Be(array2D [| [| 0.2006095939361826; 0.2862616005077138; 0.5131288055561035 |]; [| 0.07388963079437862; 0.060717262866699366; 0.8653931063389221 |] |]) |> ignore
        
        return prediction
    }
    |> assertOk

[<Fact>]
let ``calcModelPredictionSingle with regressor model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "regressor", "regressor.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType RawFormulaVal model
        
        let floatFeatures = [| 2f; 4f; 6f; 8f; |]
        let catFeatures = [| |]
        
        let! prediction = calcModelPredictionSingle floatFeatures catFeatures model
        
        prediction.Should().Be([| 15.625 |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPredictionSingle with classifier model and class prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "classifier", "classifier.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Class model
        
        let floatFeatures = [| 2f; 4f; 6f; 8f; 5f |]
        let catFeatures = [| "a"; "b" |]
        
        let! prediction = calcModelPredictionSingle floatFeatures catFeatures model
        
        prediction.Should().Be([| 1.0 |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPredictionSingle with classifier model and probability prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "classifier", "classifier.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Probability model
        
        let floatFeatures = [| 2f; 4f; 6f; 8f; 5f |]
        let catFeatures = [| "a"; "b" |]
        
        let! prediction = calcModelPredictionSingle floatFeatures catFeatures model
        
        prediction.Should().Be([| 0.629855013297618 |]) |> ignore
        
        return prediction
    }
    |> assertOk

[<Fact>]
let ``calcModelPredictionSingle with multiclassification model and class prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "multiclassification", "multiclassification.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Class model
        
        let floatFeatures = [| 1996f; 197f |]
        let catFeatures = [| "winter" |]
        
        let! prediction = calcModelPredictionSingle floatFeatures catFeatures model
        
        prediction.Should().Be([| 2.0 |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPredictionSingle with multiclassification model and probability prediction type returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "multiclassification", "multiclassification.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! _ = setPredictionType Probability model
        
        let floatFeatures = [| 1996f; 167f |]
        let catFeatures = [| "winter" |]
        
        let! prediction = calcModelPredictionSingle floatFeatures catFeatures model
        prediction.Should().Be([| 0.2006095939361826; 0.2862616005077138; 0.5131288055561035 |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelUsedFeaturesNames for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! featuresNames = getModelUsedFeaturesNames model
        
        featuresNames.Should().Be([| "Column=0"
                                     "Column=1"
                                     "Column=2"
                                     "Column=3"
                                     "Column=4"
                                     "Column=5"
                                     "Column=6"
                                     "Column=7"
                                     "Column=8"
                                     "Column=9"
                                     "CatColumn_1"
                                     "CatColumn_2" |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``getCatFeaturesCount for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let count = getCatFeaturesCount model
        
        count.Should().Be(2UL) |> ignore
    }
    |> assertOk

[<Fact>]
let ``getFloatFeaturesCount for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let count = getFloatFeaturesCount model
        
        count.Should().Be(10UL) |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelInfoValue modelguid for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let result = getModelInfoValue ModelGuid model
        
        result.Should().NotBeEmpty() |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelInfoValue outputoptions for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let result = getModelInfoValue OutputOptions model
        
        result.Should().NotBeEmpty() |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelInfoValue params for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let result = getModelInfoValue Params model
        
        result.Should().NotBeEmpty() |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelInfoValue train finish time for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let result = getModelInfoValue TrainFinishTime model
        
        result.Should().NotBeEmpty() |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelInfoValue training for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let result = getModelInfoValue Training model
        
        result.Should().NotBeEmpty() |> ignore
    }
    |> assertOk

[<Fact>]
let ``getModelInfoValue catboost version info for metadata model returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let result = getModelInfoValue CatboostVersionInfo model
        
        result.Should().NotBeEmpty() |> ignore
    }
    |> assertOk

[<Fact>]
let ``loadFullModelFromFile returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "metadata", "metadata.cbm") 
        use! _ =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
        return ()
    }
    |> assertOk

[<Fact>]
let ``getSupportedEvaluatorTypes returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "regressor", "regressor.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        let! evaluatorTypes = getSupportedEvaluatorTypes model
        
        evaluatorTypes.Should().Be([| CPU; GPU |]) |> ignore
    }
    |> assertOk

[<Fact>]
let ``enableGPUEvaluation returns expected result`` () =
    result {
        let filePath = Path.Combine(directory, "regressor", "regressor.cbm") 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile filePath
            
        return! enableGPUEvaluation 0 model
    }
    |> assertOk

