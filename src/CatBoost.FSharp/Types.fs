namespace CatBoost.FSharp

open CatBoost.Core.Interop

type FormulaEvaluatorType =
    | CPU
    | GPU

module internal FormulaEvaluatorType =
    let toNative = function
        | CPU -> ECatBoostApiFormulaEvaluatorType.CBA_FET_CPU
        | GPU -> ECatBoostApiFormulaEvaluatorType.CBA_FET_GPU

    let fromNative = function
        | ECatBoostApiFormulaEvaluatorType.CBA_FET_CPU -> CPU
        | ECatBoostApiFormulaEvaluatorType.CBA_FET_GPU -> GPU
        | _ -> failwith "Unknown ECatBoostApiFormulaEvaluatorType."

type PredictionType =
    | RawFormulaVal
    | Exponent
    | RMSEWithUncertainty
    | Probability
    | Class
    | MultiProbability

module internal PredictionType =
    let toNative = function
        | RawFormulaVal -> EApiPredictionType.APT_RAW_FORMULA_VAL
        | Exponent -> EApiPredictionType.APT_EXPONENT
        | RMSEWithUncertainty -> EApiPredictionType.APT_RMSE_WITH_UNCERTAINTY
        | Probability -> EApiPredictionType.APT_PROBABILITY
        | Class -> EApiPredictionType.APT_CLASS
        | MultiProbability -> EApiPredictionType.APT_MULTI_PROBABILITY

type ModelInfoKey =
    | CatboostVersionInfo
    | ModelGuid
    | Params
    | TrainFinishTime
    | Training
    | OutputOptions

module internal ModelInfoKey =
    let toStr = function
        | CatboostVersionInfo -> "catboost_version_info"
        | ModelGuid -> "model_guid"
        | Params -> "params"
        | TrainFinishTime -> "train_finish_time"
        | Training -> "training"
        | OutputOptions -> "output_options"

type ModelMetadata = {
    TreeCount: int
    FloatFeaturesCount: int
    CatFeaturesCount: int
    TextFeaturesCount: int
    EmbeddingFeaturesCount: int
    UsedFeatureNames: string[]
}
