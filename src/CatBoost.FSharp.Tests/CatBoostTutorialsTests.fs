/// <summary>
/// These tests were taken from CatBoost's Java model application tutorial
/// https://github.com/catboost/tutorials/blob/master/apply_model/java/src/main/java/CatBoost4jPredictionTutorial.java
/// and translated to F#. The model classifies the UCI Adult dataset: does a person make over 50K a year.
/// The tutorial only prints its predictions, so the expected raw values were recorded from CatBoost 1.2.10;
/// the over/under 50K outcomes match the dataset labels.
/// </summary>
module CatBoost.FSharp.Tests.Tutorials.Adult

open System
open System.IO
open Faqt
open FsToolkit.ErrorHandling
open CatBoost.FSharp
open CatBoost.FSharp.Api
open Xunit
open TestHelpers

let modelPath = Path.Combine(AppContext.BaseDirectory, "tutorials", "adult", "adult.cbm")

let sigmoid x = 1.0 / (1.0 + exp -x)

let classificationThreshold = 0.5

let tolerance = 1e-9

// Line 1 of adult.test:
// "25, Private, 226802, 11th, 7, Never-married, Machine-op-inspct, Own-child, Black, Male, 0, 0, 40, United-States, <=50K."
let personANumericFeatures = [| 25f; 226802f; 7f; 0f; 0f; 40f |]
let personACategoricFeatures = [| "Private"; "11th"; "Never-married"; "Machine-op-inspct"; "Own-child"; "Black"; "Male"; "United-States" |]
let personAExpected = -6.8901575282597012

// Line 40 of adult.test:
// "40, Private, 85019, Doctorate, 16, Married-civ-spouse, Prof-specialty, Husband, Asian-Pac-Islander, Male, 0, 0, 45, ?, >50K."
// The missing native-country ("?") is passed as "nan", as it was during training. "Married-civ-spouce" is the
// tutorial's own spelling, kept so the inputs match the tutorial.
let personBNumericFeatures = [| 40f; 85019f; 16f; 0f; 0f; 45f |]
let personBCategoricFeatures = [| "Private"; "Doctorate"; "Married-civ-spouce"; "Prof-specialty"; "Husband"; "Asian-Pac-Islander"; "Male"; "nan" |]
let personBExpected = 0.78330447631484756

[<Fact>]
let ``model metainformation for adult model returns expected result`` () =
    result {
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        (getTreeCount model).Should().Be(100UL) |> ignore
        (getDimensionsCount model).Should().Be(1UL) |> ignore
        (getFloatFeaturesCount model).Should().Be(6UL) |> ignore
        (getCatFeaturesCount model).Should().Be(8UL) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPredictionSingle for person A returns doesn't make over 50K`` () =
    result {
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        let! prediction = calcModelPredictionSingle personANumericFeatures personACategoricFeatures model

        prediction.Length.Should().Be(1) |> ignore
        prediction[0].Should().BeCloseTo(personAExpected, tolerance) |> ignore
        (sigmoid prediction[0]).Should().BeLessThan(classificationThreshold) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPredictionSingle for person B with missing feature returns makes over 50K`` () =
    result {
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        let! prediction = calcModelPredictionSingle personBNumericFeatures personBCategoricFeatures model

        prediction.Length.Should().Be(1) |> ignore
        prediction[0].Should().BeCloseTo(personBExpected, tolerance) |> ignore
        (sigmoid prediction[0]).Should().BeGreaterThan(classificationThreshold) |> ignore
    }
    |> assertOk

[<Fact>]
let ``calcModelPrediction for persons A and B returns same result as single predictions`` () =
    result {
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        let numericFeatures = array2D [ personANumericFeatures; personBNumericFeatures ]
        let categoricFeatures = array2D [ personACategoricFeatures; personBCategoricFeatures ]

        let! predictions = calcModelPrediction numericFeatures categoricFeatures model

        predictions.GetLength(0).Should().Be(2) |> ignore
        predictions.GetLength(1).Should().Be(1) |> ignore
        predictions[0, 0].Should().BeCloseTo(personAExpected, tolerance) |> ignore
        predictions[1, 0].Should().BeCloseTo(personBExpected, tolerance) |> ignore
    }
    |> assertOk

// The tutorial turns raw predictions into probabilities with its own sigmoid; the Probability prediction type
// should give the same values.
[<Fact>]
let ``calcModelPrediction with probability prediction type matches sigmoid of raw prediction`` () =
    result {
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        let! _ = setPredictionType Probability model

        let numericFeatures = array2D [ personANumericFeatures; personBNumericFeatures ]
        let categoricFeatures = array2D [ personACategoricFeatures; personBCategoricFeatures ]

        let! predictions = calcModelPrediction numericFeatures categoricFeatures model

        predictions[0, 0].Should().BeCloseTo(sigmoid personAExpected, tolerance) |> ignore
        predictions[1, 0].Should().BeCloseTo(sigmoid personBExpected, tolerance) |> ignore
    }
    |> assertOk

// calcModelPredictionWithHashedCatFeatures takes categorical features already hashed with getStringCatFeatureHash;
// it should give the same predictions as passing the strings.
[<Fact>]
let ``calcModelPredictionWithHashedCatFeatures for persons A and B returns same result as string features`` () =
    result {
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile modelPath

        let numericFeatures = array2D [ personANumericFeatures; personBNumericFeatures ]
        let hashedCategoricFeatures =
            array2D [ personACategoricFeatures; personBCategoricFeatures ]
            |> Array2D.map getStringCatFeatureHash

        let! predictions = calcModelPredictionWithHashedCatFeatures numericFeatures hashedCategoricFeatures model

        predictions[0, 0].Should().BeCloseTo(personAExpected, tolerance) |> ignore
        predictions[1, 0].Should().BeCloseTo(personBExpected, tolerance) |> ignore
    }
    |> assertOk

