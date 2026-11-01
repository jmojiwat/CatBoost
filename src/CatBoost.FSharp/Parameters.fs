namespace CatBoost.FSharp

module Re =
    
    type RegressionFunction =
        | MAE
        | MAEWithBoostFromAverage of bool
        | MAPE
        | MAPEWithBoostFromAverage of bool
        | Poisson
        | Quantile
        | QuantileWithBoosFromAverage of bool
        | MultiQuantile
        | RMSE
        | RMSEWithBoostFromAverage of bool
        | RMSEWithUncertainty
        | LogLineQuantile
        | Lq
        | Huber
        | Expectile
        | Tweedie
        | LogCosh
        | Cox
        | SurvivalAft
    
    type RegressionMetric =
        | FairLoss
        | NumErrors
        | SMAPE
        | R2
        | MSLE
        | MedianAbsoluteError

    type Regression =
        | LossFunction of RegressionFunction
        | CustomMetric of RegressionMetric
        | EvalMetric of RegressionMetric

module MuRe =
    
    type MultiregressionFunction =
        | MultiRMSE
        | MultiRMSEWithMissingValues
        
    type MultiregressionMetric =
        | MultiRMSE
        | MultiRMSEWithMissingValues

    type Multiregression =
        | LossFunction of MultiregressionFunction
        | CustomMetric of MultiregressionMetric
        | EvalMetric of MultiregressionMetric

module Cl =

    type ClassificationFunction =
        | Logloss
        | CrossEntropy
        
    type ClassificationMetric =        
        | Logloss
        | CrossEntropy
        | Precision
        | Recall
        | F
        | F1
        | BalancedAccuracy
        | BalancedErrorRate
        | MCC
        | Accuracy
        | AUC
        | QueryAUC
        | PRAUC
        | NormalizedGini
        | BrierScore
        | HingeLoss
        | HammingLoss
        | ZeroOneLoss
        | Kappa
        | WKappa
        | LogLikelihoodOfPrediction
            
    type Classification =
        | LossFunction of ClassificationFunction
        | CustomMetric of ClassificationMetric
        | EvalMetric of ClassificationMetric

module MuCl =
    
    type MulticlassificationFunction =
        | MultiClass
        | MultiClassOneVsAll
        
    type MulticlassificationMetric =
        | MultiClass
        | MultiClassOneVsAll
        | Precision
        | Recall
        | F
        | F1
        | TotalF1
        | MCC
        | Accuracy
        | HingeLoss
        | HammingLoss
        | ZeroOneLoss
        | Kappa
        | WKappa
        | AUC
        | PRAUC

    type Multiclassification =
        | LossFunction of MulticlassificationFunction
        | CustomMetric of MulticlassificationMetric
        | EvalMetric of MulticlassificationMetric

module MuLaCl =
    
    type MultilabelClassificationFunction =
        | MultiLogloss
        | MultiCrossEntropy
    
    type MultilabelClassificationMetric =
        | MultiLogloss
        | MultiCrossEntropy
        | Precision
        | Recall
        | F
        | F1
        | Accuracy
        | HammingLoss

    type MultilabelClassification =
        | LossFunction of MultilabelClassificationFunction
        | CustomMetric of MultilabelClassificationMetric
        | EvalMetric of MultilabelClassificationMetric
        
module Ra =
    
    type RankingFunction =
        | PairLogit
        | PairLogitPairwise
        | YetiRank
        | YetiRankPairwise
        | LambdaMart
        | StochasticFilter
        | StochasticRank
        | QueryCrossEntropy
        | QueryRMSE
        | QuerySoftMax
        | GroupQuantile
        
    type RankingMetric =
        | PairLogit
        | PairLogitPairwise
        | PairAccuracy
        | YetiRank
        | YetiRankPairwise
        | LambdaMart
        | StochasticFilter
        | StochasticRank
        | QueryCrossEntropy
        | QueryRMSE
        | QuerySoftMax
        | GroupQuantile
        | PFound
        | NDCG
        | DCG
        | FilteredDCG
        | AverageGain
        | PrecisionAt
        | RecallAt
        | MAP
        | ERR
        | MRR
        | AUC
        | QueryAUC

    type Ranking =
        | LossFunction of RankingFunction
        | CustomMetric of RankingMetric
        | EvalMetric of RankingMetric
    
module Parameters =
    
    open Re
    open MuRe
    open Cl
    open MuCl
    open MuLaCl
    open Ra
    
    type BootstrapType =
        | Bayesian
        | Bernoulli
        | BernoulliWithSubsample of float
        | MVS
        | MVSWithSubsample of float
        // supported for GPU only
        | Poisson
        | PoissonWithSubsample of float
        | No
        
    type SamplingFrequency =
        | PerTree
        | PerTreeLevel
        
    type SamplingUnit =
        | Object
        | Group

    type GrowPolicy =
        | SymmetricTree
        | Depthwise
        
    type NanMode =
        | Forbidden
        | Min
        | Max

    type LeafEstimationMethod =
        | Newton
        | Gradient
        | Exact

    type LeafEstimationBacktracking =
        | NoLeafEstimationBacktracking
        | AnyImprovement
        | Armijo

    type AutoClassWeights =
        | NoAutoClassWeights
        | Balanced
        | SqrtBalanced

    type BoostingType =
        | Ordered
        | Plain

    type ScoreFunction =
        /// Do not use this score type with the Lossguide tree growing policy.
        | Cosine
        | L2
        /// Do not use this score type with the Lossguide tree growing policy.
        | NewtonCosine
        | NewtonL2

    type ModelShrinkMode =
        | Constant
        | Decreasing

    type Metric =
        | Regression of Regression
        | Multiregression of Multiregression
        | Classification of Classification
        | Multiclassification of Multiclassification
        | MultilabelClassification of MultilabelClassification
        | Ranking of Ranking
                                
    type CommonParameter =
        /// The metric to use in training. The specified value also determines the machine learning problem to solve.
        /// Some metrics support optional parameters.
        | Metric of Metric
        | Iterations of int
        | LearningRate of float
        | RandomSeed of int
        | L2LeafReg of float
        | BoostrapType of BootstrapType
        | BaggingTemperature of float
        /// Sample rate for bagging. This parameter can be used if one of the following bootstrap types is slected
        /// Poisson
        /// Bernoulli
        /// MCVS
//        | Subsample of float
        | SamplingFrequency of SamplingFrequency
        | SamplingUnit of SamplingUnit
        | MvsReg of float
        | RandomStrength of float
        | UseBestModel of bool
        | BestModelMinTrees of int
        | Depth of int
        | GrowPolicy of GrowPolicy
        | MinDataInLeaf of int
        | MaxLeaves of int
        | IgnoredFeatures of int array
        | OneHotMaxSize of int
        | HasTime of bool
        | Rsm of float
        | NanMode of NanMode
        | InputBorders of string
        | OutputBorders of string
        | FoldPermutationBlock of int
        | LeafEstimationMethod of LeafEstimationMethod
        | LeafEstimationIterations of int
        | LeafEstimationBacktracking of LeafEstimationBacktracking
        | FoldLenMultiplier of float
        | ApproxOnFullHistory of bool
        | ClassWeights of int array
        | ClassNames of string array
        | AutoClassWeights of AutoClassWeights
        | ScalePosWeight of float
        | BoostingType of BoostingType
        /// Initialize approximate values by best constant value for the specified loss function. Sets the value of bias
        /// to the initial best constant value.
        /// Available for the following loss functions:
        /// <list type="bullet">
        /// <item>RMSE</item>
        /// <item>Logloss</item>
        /// <item>CrossEntropy</item>
        /// <item>Quantile</item>
        /// <item>MAE</item>
        /// <item>MAPE</item></list>
//        | BoostFromAverage
        | Langevin of bool
        | DiffusionTemperature of float
        | PosteriorSampling of bool
        | AllowConstLabel of bool
        | ScoreFunction of ScoreFunction
        | MonotoneConstraints // todo:
        | FeatureWeights // todo:
        | FirstFeatureUsePenalties // todo:
        | FixedBinarySplits of float array
        | PenaltiesCoefficient of float
        | PerObjectFeaturePenalties of float list
        | ModelShrinkRate of float
        | ModelShrinkMode of ModelShrinkMode

    type Ctr =        
        | SimpleCtr
        | CombinationsCtr
        | PerFeatureCtr
        | CtrTargetBorderCount
        | CounterCalcMethod
        | MaxCtrComplexity
        | CtrLeafCountLimit
        | StoreAllSimpleCtr
        | FindCtrComputationMode
        
    type Multiclassification =
        | ClassesCount
        
    type Output =        
        | LoggingLevel
        | MetricPeriod
        | Verbose
        | TrainDir
        | ModelSizeReg
        | AllowWritingFiles
        | SaveSnaptshot
        | SnapshotFile
        | SnapshotInterval
        | RocFile

    type OverfittingDetection =        
        | EarlyStoppingRounds
        | OdType
        | OdPval
        | OdWait

    type UseRamLimit =
        | MB of int
        | KB of int
        | GB of int
        
    type PinnedMemorySize =
        | MB of int
        | KB of int
        | GB of int

    type GpuCatFeaturesStorage =
        | CpuPinnedMemory
        | GpuRam

    type DataPartition =
        | FeatureParallel
        | DocParallel
        
    type Performance =
        | ThreadCount of int
        | UsedRamLimit of UseRamLimit
        | GpuRamPart of float32
        | PinnedMemorySize of PinnedMemorySize
        | GpuCatFeaturesStorage of GpuCatFeaturesStorage
        | DataPartition of DataPartition

    type TaskType =
        | CPU
        | GPU

    type Devices =
        | Id of int array
        | IdRange of int * int
        
    type ProcessingUnit =
        | TaskType of TaskType
        | Devices of Devices

    type FeatureBorderType =
        | Median
        | Uniform
        | UniformAndQuantiles
        | MaxLogSum
        | MinEntropy
        | GreedyLogSum
        
    type Quantization =
        | TargetBorder of float32
        | BorderCount of int
        | FeatureBorderType of FeatureBorderType
        | PerFloatFeatureQuantization of Map<int, int>

    type FeatureCalcers =
        | FeatureCalcerName
        | OptionName
        
    type TextProcessing =
        /// Tokenizers used to preprocess Text type feature columns before creating the dictionary.
        | Tokenizers of string array
        /// Dictionaries used to preprocess Text type feature columns.
        | Dictionaries of Map<string, string>
        /// Feature calcers used to calculate new features based on preprocessed Text type feature columns.
        | FeatureCalcers of FeatureCalcers
        /// A JSON specification of tokenizers, dictionaries and feature calcers, which determine how text features are 
        // converted into a list of float features.
        | TextProcessing of string
        
    /// The experiment name to display in visualization tools.
    type Visualization = string

(*
module ClassifierParameters =

    type MetricForClassification =
        | Logless
        | CrossEntropy
        | Precision
        | Recall
        | F
        | F1
        | BalancedAccuracy
        | BalancedErrorRate
        | MCC
        | Accuracy
        | CtrFactor
        | AUC
        | QueryAUC
        | PRAUC
        | NormalizedGini
        | BrierScore
        | HingeLoss
        | HammingLoss
        | ZeroOneLoss
        | Kappa
        | WKappa
        | LogLikelihoodOfPrediction
        
    type Metric =
        | RMSE
        | RMSEWithBoostFromAverage of bool
        | Logloss
        | LoglossWithBoostFromAverage of bool
        | MAE
        | MAEWithBoostFromAverage of bool
        | CrossEntropy
        | CrossEntropyWithBoostFromAverage of bool
        | Quantile
        | QuantileWithBoosFromAverage of bool
        | LogLineQuantile
        | Lq
        | MultiRMSE
        | MultiClass
        | MultiClassOneVsAll
        | MultiLogloss
        | MultiCrossEntropy
        | MAPE
        | MAPEWithBoostFromAverage of bool
        | Poisson
        | PairLogit
        | PairLogitPairwise
        | QueryRMSE
        | QuerySoftMax
        | GroupQuantile
        | Tweedie
        | YetiRank
        | YetiRankPairwise
        | StochasticFilter
        | StochasticRank
    
    
    type BootstrapType =
        | Bayesian
        | Bernoulli
        | BernoulliWithSubsample of float
        | MVS
        | MVSWithSubsample of float
        // supported for GPU only
        | Poisson
        | PoissonWithSubsample of float
        | No
    
    type TaskType =
        | CPU
        | GPU

    type Devices =
        | Id of int array
        | IdRange of int * int
        
    type ProcessingUnit =
        | TaskType of TaskType  // classifier
        | Devices of Devices // classifier

    type Multiclassification =
        | ClassesCount // classifier

    type Performance =
        | ThreadCount of int  // classifier
        | UsedRamLimit of UseRamLimit // classifier
        | GpuRamPart of float32 // classifier
        | PinnedMemorySize of PinnedMemorySize // classifier
        | GpuCatFeaturesStorage of GpuCatFeaturesStorage
        | DataPartition of DataPartition

    type Ctr =        
        | SimpleCtr  // classifier
        | CombinationsCtr // classifier
        | PerFeatureCtr // classifier
        | CtrTargetBorderCount // classifier
        | CounterCalcMethod  // classifier
        | MaxCtrComplexity // classifier
        | CtrLeafCountLimit // classifier
        | StoreAllSimpleCtr // classifier
        | FindCtrComputationMode

    type OverfittingDetection =        
        | EarlyStoppingRounds // classifier
        | OdType  // classifier
        | OdPval // classifier
        | OdWait // classifier
        
    type FeatureBorderType =
        | Median
        | Uniform
        | UniformAndQuantiles
        | MaxLogSum
        | MinEntropy
        | GreedyLogSum

    type Quantization =
        | TargetBorder of float32  // classifier
        | BorderCount of int  // classifier
        | FeatureBorderType of FeatureBorderType  // classifier
        | PerFloatFeatureQuantization of Map<int, int>  // classifier
    
    type CommonParameter =
        /// The metric to use in training. The specified value also determines the machine learning problem to solve.
        /// Some metrics support optional parameters.
        | LossFunction of Metric  // classifier
        | CustomMetric of CustomMetric  // classifier
        | EvalMetric of EvalMetric // classifier
        | Iterations of int // classifier
        | LearningRate of float // classifier
        | RandomSeed of int // classifier
        | L2LeafReg of float // classifier
        | BoostrapType of BootstrapType // classifier
        | BaggingTemperature of float // classifier
        /// Sample rate for bagging. This parameter can be used if one of the following bootstrap types is slected
        /// Poisson
        /// Bernoulli
        /// MCVS
//        | Subsample of float
        | SamplingFrequency of SamplingFrequency // classifier
        | SamplingUnit of SamplingUnit // classifier
        | MvsReg of float // classifier
        | RandomStrength of float // classifier
        | UseBestModel of bool  // classifier
        | BestModelMinTrees of int // classifier
        | Depth of int // classifier
        | GrowPolicy of GrowPolicy // classifier
        | MinDataInLeaf of int // classifier
        | MaxLeaves of int // classifier
        | IgnoredFeatures of int array // classifier
        | OneHotMaxSize of int // classifier
        | HasTime of bool  // classifier
        | Rsm of float // classifier
        | NanMode of NanMode  // classifier
        | InputBorders of string  // classifier
        | OutputBorders of string  // classifier
        | FoldPermutationBlock of int  // classifier
        | LeafEstimationMethod of LeafEstimationMethod  // classifier
        | LeafEstimationIterations of int // classifier
        | LeafEstimationBacktracking of LeafEstimationBacktracking // classifier
        | FoldLenMultiplier of float // classifier
        | ApproxOnFullHistory of bool  // classifier
        | ClassWeights of int array // classifier
        | ClassNames of string array // classifier
        | AutoClassWeights of AutoClassWeights // classifier
        | ScalePosWeight of float // classifier
        | BoostingType of BoostingType // classifier
        /// Initialize approximate values by best constant value for the specified loss function. Sets the value of bias
        /// to the initial best constant value.
        /// Available for the following loss functions:
        /// <list type="bullet">
        /// <item>RMSE</item>
        /// <item>Logloss</item>
        /// <item>CrossEntropy</item>
        /// <item>Quantile</item>
        /// <item>MAE</item>
        /// <item>MAPE</item></list>
        | BoostFromAverage of bool
        | Langevin of bool // classifier
        | DiffusionTemperature of float // classifier
        | PosteriorSampling of bool // classifier
        | AllowConstLabel of bool  // classifier
        | ScoreFunction of ScoreFunction // classifier
        | MonotoneConstraints // todo: // classifier
        | FeatureWeights // todo: // classifier
        | FirstFeatureUsePenalties // todo: // classifier
        | FixedBinarySplits of float array
        | PenaltiesCoefficient of float // classifier
        | PerObjectFeaturePenalties of float list // classifier
        | ModelShrinkRate of float // classifier
        | ModelShrinkMode of ModelShrinkMode // classifier
    
    type LoggingLevel =
        | Silent
        | Verbose
        | Info
        | Debug
    
    type Output =        
        | LoggingLevel of LoggingLevel // classifier
        | MetricPeriod // classifier
        | Verbose  // classifier
        | TrainDir  // classifier
        | ModelSizeReg  // classifier
        | AllowWritingFiles // classifier
        | SaveSnaptshot // classifier
        | SnapshotFile // classifier
        | SnapshotInterval // classifier
        | RocFile

    type RandomScoreType =
        | Gumbel
        | NormalWithModelSizeDecrease
        
    type Unknow =
        | RandomScoreType of RandomScoreType

    type FinalCtrComputationMode =
        | Default
        | Skip
        
    type Unknown2 =
        | FinalCtrComputationMode of FinalCtrComputationMode // classifier
        
    type Unknown3 =
        | DevScoreCalcObjBlockSize of int // classifier
        
    type Unknown4 =
        | DevEfbMaxBuckets of int // classifier
        
    type Unknown5 =
        | SparseFeaturesConflictFraction of int // classifier
        
    type Unknown6 =
        | CatFeaturesConflictFraction of int array // classifier
        
    type Unknown7 =
        | TextFeatures of int array // classifier
        
    type Unknown8 =
        | EmbeddingFeatures of int array // classifier
        
    type Visualization = string // classifier
        
    type TextProcessing =
        /// Tokenizers used to preprocess Text type feature columns before creating the dictionary.
        | Tokenizers of string array // classifier
        /// Dictionaries used to preprocess Text type feature columns.
        | Dictionaries of Map<string, string> // classifier
        /// Feature calcers used to calculate new features based on preprocessed Text type feature columns.
        | FeatureCalcers of FeatureCalcers // classifier
        /// A JSON specification of tokenizers, dictionaries and feature calcers, which determine how text features are 
        // converted into a list of float features.
        | TextProcessing of string // classifier
        
    type Unknown9 =
        | EvalFraction of float
        *)
        

