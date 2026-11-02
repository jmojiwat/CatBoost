# CatBoost for F#

A thin F# wrapper around the [CatBoost](https://catboost.ai) model applier C API: load a trained `.cbm` model and make predictions from .NET.

This library only **applies** models. Train them with CatBoost's Python or R package or its command-line tool, then save them in CatBoost's binary format (`.cbm`).

## Requirements

- .NET 10
- One of `win-x64`, `linux-x64`, `linux-arm64` or macOS (`x64` and `arm64`). The CatBoost 1.2.10 native library for each platform is included in the `CatBoost.Core` package, so nothing else needs installing.

It's split into two projects:

- **`CatBoost.FSharp`** — the idiomatic F# API (`CatBoost.FSharp.Api`) and its types (`PredictionType`, `ModelInfoKey`, `ModelMetadata`, …). This is the one to reference.
- **`CatBoost.Core`** — the raw P/Invoke declarations, the `SafeHandle`s that own native objects, and the native libraries. `CatBoost.FSharp` depends on it.

Neither is published on NuGet yet. Reference `src/CatBoost.FSharp/CatBoost.FSharp.fsproj` from your project, or build packages with `dotnet pack` (see [Building from source](#building-from-source)).

## Example

Predict whether a person earns over 50K a year with the model from CatBoost's [model application tutorial](https://github.com/catboost/tutorials/tree/master/apply_model) (`adult.cbm`). The example uses `result { }` from [FsToolkit.ErrorHandling](https://github.com/demystifyfp/FsToolkit.ErrorHandling).

```fsharp
open FsToolkit.ErrorHandling
open CatBoost.FSharp
open CatBoost.FSharp.Api

let probabilityOver50K =
    result {
        use! model =
            modelCalcerCreate ()
            |> loadFullModelFromFile "adult.cbm"

        do! setPredictionType Probability model

        // One row per object; columns in the order the model was trained on.
        let numericFeatures = array2D [ [ 25f; 226802f; 7f; 0f; 0f; 40f ] ]
        let categoricalFeatures =
            array2D [ [ "Private"; "11th"; "Never-married"; "Machine-op-inspct"
                        "Own-child"; "Black"; "Male"; "United-States" ] ]

        let! predictions = calcModelPrediction numericFeatures categoricalFeatures model
        return predictions[0, 0]
    }

match probabilityOver50K with
| Ok p -> printfn $"Probability of earning over 50K: %.4f{p}"
| Error e -> eprintfn $"CatBoost error: {e}"
```

```text
Probability of earning over 50K: 0.0010
```

## Usage

- **Results:** every call that can fail returns `Result<_, string>`. The error carries CatBoost's own message.
- **Disposal:** `modelCalcerCreate` returns a `SafeHandle` that owns the native model. Bind it with `use` / `use!` so it's freed promptly.
- **Argument order:** the model handle is always the last argument, so calls pipe: `modelCalcerCreate () |> loadFullModelFromFile path`.
- **Features:** numeric features are `float32[,]` and categorical or text features are `string[,]`, one row per object. Columns must follow the order the model was trained on. Pass an empty array (`Array2D.zeroCreate rows 0`) for a feature type the model doesn't use. Predictions come back as `float[,]`: one row per object, one column per prediction dimension.
- **Model information:** `getModelMetadata` returns a `ModelMetadata` record with the tree count, the count of each feature type and the used feature names in one call. `getModelInfoValue` takes a `ModelInfoKey` (`CatboostVersionInfo`, `ModelGuid`, `Params`, `TrainFinishTime`, `Training`, `OutputOptions`).
- **Prediction type:** raw values (`RawFormulaVal`) are the default. Call `setPredictionType` with `Probability`, `Class`, `Exponent`, `MultiProbability` or `RMSEWithUncertainty` to get something else.

Functions map one-to-one onto the C API, named in camelCase (`CalcModelPrediction` → `calcModelPrediction`):

| Area | Functions |
|---|---|
| Loading | `modelCalcerCreate`, `loadFullModelFromFile`, `loadFullModelFromBuffer`, `loadFullModelZeroCopy` |
| Prediction | `calcModelPrediction`, `calcModelPredictionSingle`, `calcModelPredictionFlat`, `calcModelPredictionFlatTransposed`, `calcModelPredictionText`, `calcModelPredictionTextAndEmbeddings`, `calcModelPredictionWithHashedCatFeatures`, `*Staged` variants, `predictSpecificClass*` |
| Settings | `setPredictionType`, `setPredictionTypeString`, `enableGPUEvaluation`, `getSupportedEvaluatorTypes` |
| Model information | `getModelMetadata`, `getTreeCount`, `getDimensionsCount`, `getPredictionDimensionsCount`, `get{Float,Cat,Text,Embedding}FeaturesCount`, `get{Float,Cat,Text,Embedding}FeatureIndices`, `getModelUsedFeaturesNames`, `getModelInfoValue`, `getModelInfoValueSize`, `checkModelMetadataHasKey` |
| Hashing | `getStringCatFeatureHash`, `getIntegerCatFeatureHash` |

For what each function does, see the doc comments in CatBoost's [`c_api.h`](https://github.com/catboost/catboost/blob/v1.2.10/catboost/libs/model_interface/c_api.h).

## Building from source

```powershell
cd src
dotnet build CatBoost.slnx
dotnet test --solution CatBoost.slnx
dotnet pack CatBoost.Core\CatBoost.Core.fsproj -o <output folder>
dotnet pack CatBoost.FSharp\CatBoost.FSharp.fsproj -o <output folder>
```

The native libraries aren't committed to this repository. The first build downloads them from the [CatBoost 1.2.10 release](https://github.com/catboost/catboost/releases/tag/v1.2.10) and checks each against a SHA-256 stored in `CatBoost.Core.fsproj`, so it needs internet access. The native libraries are packed into the `CatBoost.Core` package under `runtimes/<rid>/native/`.

Run the tests in Debug: `CatBoost.slnx` doesn't build the test project (`CatBoost.FSharp.Tests`) in Release. They are ports of CatBoost's [.NET tests](https://github.com/catboost/catboost/blob/master/catboost/dotnet/CatBoostNetTests/CatBoostModelEvaluatorTest.cs), its [Java tutorial](https://github.com/catboost/tutorials/blob/master/apply_model/java/src/main/java/CatBoost4jPredictionTutorial.java) and [catboost-cgo](https://github.com/mirecl/catboost-cgo)'s tests.

## License

[Apache License 2.0](LICENSE). CatBoost is also licensed under Apache 2.0.
