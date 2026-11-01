/// <summary>
/// These tests were taken from catboost https://github.com/catboost/catboost. Specifically
/// from https://github.com/catboost/catboost/blob/master/catboost/dotnet/CatBoostNetTests/CatBoostModelEvaluatorTest.cs
/// and translated to F#.
/// </summary>
module CatBoost.FSharp.Tests.CatBoostNet

open System
open System.Collections.Generic
open System.Globalization
open System.IO
open Deedle
open Faqt
open FsToolkit.ErrorHandling
open CatBoost.FSharp
open CatBoost.FSharp.Api
open Xunit
open TestHelpers

let directory = Path.Combine(AppContext.BaseDirectory, "testbed")

[<Fact>]
let ``calcModelPrediction for iris`` () =
    result {
        let dsPath = Path.Combine(directory, "iris", "iris.data")
        let modelPath = Path.Combine(directory, "iris", "iris_model.cbm")
        let df = Frame.ReadCsv(dsPath, hasHeaders = false)
        df.RenameColumns(seq [ "sepal length"; "sepal width"; "petal length"; "petal width"; "target" ])
        let target =
            df.Rows.Select(_.Value["target"]).Values
            |> Seq.map string
            |> Seq.toArray

        df.DropColumn("target")
        
        let floatFeatures = df.ToArray2D<float32>()
        let catFeatures = Array2D.zeroCreate (floatFeatures.GetLength(0)) 0 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        let! res = calcModelPrediction floatFeatures catFeatures model

        let targetLabelList = [| "Iris-setosa"; "Iris-versicolor"; "Iris-virginica" |]

        let errors =
            [ for i in 0 .. res.GetLength(0) - 1 do
                let argmax =
                    [0 .. res.GetLength(1) - 1]
                    |> List.map (fun j -> res[i, j], j)
                    |> List.maxBy fst
                    |> snd

                let predLabel = targetLabelList[argmax]
                let actual = target[i]

                if predLabel <> actual then
                    writeOutput predLabel
                    writeOutput actual
                    yield $"#{i + 1} " ]
            |> String.concat ""

        errors.Length.Should().Be(0, $"Iris test failed on samples: {errors}") |> ignore
    }
    |> assertOk
    
[<Fact>]
let ``calcModelPrediction for boston`` () =
    result {
        let dsPath = Path.Combine(directory, "boston", "housing.data")
        let modelPath = Path.Combine(directory, "boston", "boston_housing_model.cbm")

        let featureList = List<float32[]>()
        let targetList = List<float>()
        let pointCulture =
            let culture = CultureInfo("en")
            culture.NumberFormat.NumberDecimalSeparator <- "."
            culture

        use textReader = new StreamReader(dsPath)
        while textReader.Peek() <> -1 do
            let tokens =
                textReader.ReadLine().Split(' ')
                |> Array.filter (fun x -> x <> "")
                |> Array.toList
            let last = List.last tokens

            targetList.Add(Double.Parse(last, pointCulture))
            let features =
                tokens
                |> List.take (tokens.Length - 1)
                |> List.map (fun x -> Single.Parse(x, pointCulture))
                |> List.toArray
            featureList.Add(features)
           
        if featureList |> Seq.exists (fun x -> x.Length <> featureList[0].Length) then
            raise (InvalidDataException("Inconsistent column count in housing.data"))

        let target = targetList.ToArray()
        let features = Array2D.zeroCreate<float32> featureList.Count featureList[0].Length
        for i in 0 .. featureList.Count - 1 do
            for j in 0 .. featureList[0].Length - 1 do
                features[i, j] <- featureList[i][j]

        let catFeatures = Array2D.zeroCreate (features.GetLength(0)) 0 
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        let! res = calcModelPrediction features catFeatures model
        
        let deltas =
            [| 0 .. featureList.Count - 1 |]
            |> Array.map (fun i ->
                let pred = Math.Exp res[i, 0]
                let logDelta = abs (res[i, 0] - (log target[i]))
                {| Index = i + 1
                   LogDelta = logDelta
                   Pred = pred
                   Target = target[i] |})
        
        let totalErrors =
            deltas
            |> Seq.filter (fun x -> x.LogDelta >= 0.4)
            |> Seq.length

        let msg =
            let badSampleIds =
                deltas
                |> Seq.filter (fun x -> x.LogDelta >= 0.4)
                |> Seq.truncate 8
                |> Seq.map (fun x -> string (x.Index + 1))
                |> String.concat ", "

            $"Boston test crashed: expected <= 7 errors, got {totalErrors} error(s) on samples {{{badSampleIds}, ...}}"
                        
        totalErrors.Should().BeLessThanOrEqualTo(7, msg) |> ignore
    }
    |> assertOk
    
[<Fact>]
let ``calcModelPrediction for mushroom`` () =
    result {
        let dsPath = Path.Combine(directory, "mushrooms", "mushrooms.csv")
        let modelPath = Path.Combine(directory, "mushrooms", "mushroom_model.cbm")
        
        let df = Frame.ReadCsv(dsPath, hasHeaders = false)
        let target =
            df.Rows.Select(_.Value["Column1"]).Values
            |> Seq.map string
            |> Seq.toArray
            
        df.DropColumn("Column1")
        let data = df.ToArray2D<string>()
        let floatFeatures = Array2D.zeroCreate (data.GetLength(0)) 0 
        
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath
        
        let! res = calcModelPrediction floatFeatures data model
        
        let targetLabelList = [| "e"; "p" |]
        
        [| 0 .. res.GetLength(0) - 1 |]
        |> Array.iter (fun i ->
            let argmax = if res[i, 0] > 0 then 1 else 0
            let predLabel = targetLabelList[argmax]
            
            predLabel.Should().Be(target[i], $"Mushroom test crashed on sample {i + 1}") |> ignore)
    }
    |> assertOk

