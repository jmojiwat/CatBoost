namespace CatBoost.FSharp

open CatBoost.Core
open CatBoost.Core.Handles
open CatBoost.Core.Interop
open CatBoost.Core.Plumbing

module Api =

    open System    
    open System.Runtime.InteropServices
    open System.Text

    let internal formulaEvaluatorTypeToInt = FormulaEvaluatorType.toNative
    let internal formulaEvaluatorTypeFromEnum = FormulaEvaluatorType.fromNative
    let internal predictionTypeToInt = PredictionType.toNative
    let internal modelInfoKeyToStr = ModelInfoKey.toStr
    /// <summary>
    /// Create empty data wrapper.</summary>
    /// <returns>Empty data wrapper.</returns>
    ///
    /// <code>
    /// CATBOOST_API DataWrapperHandle* DataWrapperCreate(size_t docsCount);</code>
    let dataWrapperCreate docsCount =
        let handle = DataWrapperCreate(docsCount)
        new DataWrapperSafeHandle(handle)
        
    /// <code>
    /// CATBOOST_API void AddFloatFeatures(DataWrapperHandle* dataWrapperHandle, const float** floatFeatures, size_t floatFeaturesSize);</code>
    let addFloatFeatures (floatFeatures: float32 array2d) (dataWrapperHandle: DataWrapperSafeHandle) =
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let mutable success = false
        try
            dataWrapperHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = dataWrapperHandle.DangerousGetHandle()
                
                AddFloatFeatures(handle, floatFeaturesPtrsAddress, floatFeaturesSize)
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            if success then
                dataWrapperHandle.DangerousRelease()

    /// <code>
    /// CATBOOST_API void AddCatFeatures(DataWrapperHandle* dataWrapperHandle, const char*** catFeatures, size_t catFeaturesSize);</code>
    let addCatFeatures (catFeatures: string array2d) (dataWrapperHandle: DataWrapperSafeHandle) =
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalStringFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))
        
        let mutable success = false
        try
            dataWrapperHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = dataWrapperHandle.DangerousGetHandle()
                
                AddCatFeatures(handle, catFeaturesPtrsAddress, catFeaturesSize)
        finally
            freeStringFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            if success then
                dataWrapperHandle.DangerousRelease()

    /// <code>
    /// CATBOOST_API void AddTextFeatures(DataWrapperHandle* dataWrapperHandle, const char*** textFeatures, size_t textFeaturesSize);</code>
    let addTextFeatures (textFeatures: string array2d) (dataWrapperHandle: DataWrapperSafeHandle) =
        let textFeaturesPtrsHandle, textFeaturesDataHandle = marshalStringFeatures textFeatures
        let textFeaturesPtrsAddress = if textFeaturesPtrsHandle.IsAllocated then textFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let textFeaturesSize = uint64 (textFeatures.GetLength(1))
        
        let mutable success = false
        try
            dataWrapperHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = dataWrapperHandle.DangerousGetHandle()
                
                AddTextFeatures(handle, textFeaturesPtrsAddress, textFeaturesSize)
        finally
            freeStringFeatures textFeaturesPtrsHandle textFeaturesDataHandle
            if success then
                dataWrapperHandle.DangerousRelease()

    // CATBOOST_API void AddEmbeddingFeatures(
    //     DataWrapperHandle* dataWrapperHandle,
    //     const float*** embeddingFeatures,
    //     size_t* embeddingDimensions,
    //     size_t embeddingFeaturesSize);
    (*
    let addEmbeddingFeatures embeddingFeatures embeddingDimensions embeddingFeaturesSize (dataWrapperHandle: DataWrapperSafeHandle) =
        raise <| NotImplementedException()
        *)

    /// <code>
    /// CATBOOST_API DataProviderHandle* BuildDataProvider(DataWrapperHandle* dataWrapperHandle);</code>
    let buildDataProvider (dataWrapperHandle: DataWrapperSafeHandle) =
        let mutable success = false
        try
            dataWrapperHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = dataWrapperHandle.DangerousGetHandle()
            
                let newHandle = BuildDataProvider(handle)
                new DataWrapperSafeHandle(newHandle)
        finally
            if success then
                dataWrapperHandle.DangerousRelease()

    /// <summary>
    /// Create empty model handle.</summary>
    /// <returns>Empty model handle.</returns>
    ///
    /// <code>
    /// CATBOOST_API ModelCalcerHandle* ModelCalcerCreate();</code>
    let modelCalcerCreate () =
        let modelCalcerHandle = ModelCalcerCreate()
        new ModelCalcerSafeHandle(modelCalcerHandle)

    /// <create>
    /// Load model from file into given model handle.</create>
    /// <param name="filename">path to the file. Uses UTF-8 encoding.</param>
    /// <param name="modelHandle">Calcer.</param>
    /// <returns>Model.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool LoadFullModelFromFile(ModelCalcerHandle* modelHandle, const char* filename);</code>
    let loadFullModelFromFile filename (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match LoadFullModelFromFile(handle, filename) with
                | true -> Ok modelHandle
                | false -> showError "loadFullModelFromFile"
            finally
                if success = true then
                    modelHandle.DangerousRelease()

    /// <summary>
    /// Load model from memory buffer into given model handle.</summary>
    /// <param name="binaryBuffer">A memory buffer where model file is mapped.</param>
    /// <returns>Model.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool LoadFullModelFromBuffer(ModelCalcerHandle* modelHandle,
    ///                                           const void* binaryBuffer,
    ///                                           size_t binaryBufferSize);</code>
    let loadFullModelFromBuffer (binaryBuffer: byte array) =
        
        let binaryBufferSize = uint64 (Seq.length binaryBuffer)
        let binaryBufferPtr = GCHandle.Alloc(binaryBuffer, GCHandleType.Pinned)

        let modelHandle = ModelCalcerCreate()
        try
            if modelHandle = 0n then
                showError "loadFullModelFromBuffer"
            else
                match LoadFullModelFromBuffer(modelHandle, binaryBufferPtr.AddrOfPinnedObject(), binaryBufferSize) with
                | true -> Ok (new ModelCalcerSafeHandle(modelHandle))
                | false -> showError "loadFullModelFromBuffer"
        finally
            if binaryBufferPtr.IsAllocated then
                binaryBufferPtr.Free()

    /// <summary>Use model directly from given memory region with zero-copy method.</summary>
    /// 
    /// @param calcer
    /// @param binaryBuffer pointer to a memory buffer where model file is mapped
    /// @param binaryBufferSize size of the buffer in bytes
    /// @return false if error occured
    ///
    /// <code> 
    /// CATBOOST_API bool LoadFullModelZeroCopy(ModelCalcerHandle* modelHandle,
    ///                                         const void* binaryBuffer,
    ///                                         size_t binaryBufferSize);</code>
    let loadFullModelZeroCopy (binaryBuffer: byte array) =
        
        let binaryBufferSize = uint64 (Seq.length binaryBuffer)
        let binaryBufferPtr = GCHandle.Alloc(binaryBuffer, GCHandleType.Pinned)

        let modelHandle = ModelCalcerCreate()
        try
            if modelHandle = 0n then
                showError "loadFullModelZeroCopy"
            else
                match LoadFullModelZeroCopy(modelHandle, binaryBufferPtr.AddrOfPinnedObject(), binaryBufferSize) with
                | true -> Ok (new ModelCalcerSafeHandle(modelHandle))
                | false -> showError "loadFullModelZeroCopy"
        finally
            if binaryBufferPtr.IsAllocated then
                binaryBufferPtr.Free()

    /// <summary>
    /// Use CUDA GPU device for model evaluation.</summary>
    ///
    /// <code>
    /// CATBOOST_API bool EnableGPUEvaluation(ModelCalcerHandle* modelHandle, int deviceId);</code>
    let enableGPUEvaluation deviceId (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match EnableGPUEvaluation(handle, deviceId) with
                | true -> Ok ()
                | false -> showError "enableGPUEvaluation"
            finally
                if success then
                    modelHandle.DangerousRelease()

    /// <summary>
    /// Get supported formula evaluator types.</summary>
    /// <param name="modelHandle">Model handle.</param>
    /// <returns>List of supported evaluator types:
    /// <list type="bullet">
    /// <item><c>CPU</c></item>
    /// <item><c>GPU</c></item></list></returns>
    ///
    /// <code>
    /// CATBOOST_API bool GetSupportedEvaluatorTypes(
    ///     ModelCalcerHandle* modelHandle,
    ///     enum ECatBoostApiFormulaEvaluatorType** formulaEvaluatorTypes,
    ///     size_t* formulaEvaluatorTypesCount);</code>
    let getSupportedEvaluatorTypes (modelHandle: ModelCalcerSafeHandle) =
        let mutable formulaEvaluatorTypesPtr = 0n
        let mutable formulaEvaluatorTypesCount = 0UL
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match GetSupportedEvaluatorTypes(
                    handle,
                    &formulaEvaluatorTypesPtr,
                    &formulaEvaluatorTypesCount) with
                | true ->
                    let formulaEvaluatorTypesInt = Array.zeroCreate<int> (int formulaEvaluatorTypesCount)
                    Marshal.Copy(formulaEvaluatorTypesPtr, formulaEvaluatorTypesInt, 0, int formulaEvaluatorTypesCount)
                    
                    let formulaEvaluatorTypes =
                        Array.map enum<ECatBoostApiFormulaEvaluatorType> formulaEvaluatorTypesInt
                        |> Array.map formulaEvaluatorTypeFromEnum
                    
                    Ok formulaEvaluatorTypes
                | false -> showError "getSupportedEvaluatorTypes"
            finally
                if formulaEvaluatorTypesPtr <> 0n then
                    free(formulaEvaluatorTypesPtr)
                if success then
                    modelHandle.DangerousRelease()


    /// <summary>
    /// Set prediction type for model evaluation.</summary>
    /// <param name="predictionType">Prediction type can be one of:
    /// <list type="bullet">
    /// <item><c>RawFormulaVal</c></item>
    /// <item><c>Exponent</c></item>
    /// <item><c>RMSEWithUncertainty</c></item>
    /// <item><c>Probability</c></item>
    /// <item><c>Class</c></item>
    /// <item><c>MultiProbability</c></item>
    /// </list></param>
    /// <param name="modelHandle">Model handle.</param>
    ///
    /// <code>
    /// CATBOOST_API bool SetPredictionType(ModelCalcerHandle* modelHandle, enum EApiPredictionType predictionType);</code>
    let setPredictionType predictionType (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = modelHandle.DangerousGetHandle()
                
                match SetPredictionType(handle, predictionTypeToInt predictionType) with
                | true -> Ok()
                | false -> showError "setPredictionType"
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Set prediction type for model evaluation with string constant.</summary>
    /// <param name="predictionTypeStr">Prediction type string.</param>
    /// <param name="modelHandle">Model handle.</param>
    /// 
    /// <code>
    /// CATBOOST_API bool SetPredictionTypeString(ModelCalcerHandle* modelHandle, const char* predictionTypeStr);</code>
    let setPredictionTypeString predictionTypeStr (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = modelHandle.DangerousGetHandle()
                
                match SetPredictionTypeString(handle, predictionTypeStr) with
                | true -> Ok()
                | false -> showError "setPredictionTypeString"
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get number of dimensions in model.</summary>
    /// <param name="modelHandle">Calder model handle.</param>
    /// <returns>Number of dimensions in model.</returns>
    ///
    /// <code>
    /// CATBOOST_API size_t GetDimensionsCount(ModelCalcerHandle* modelHandle);</code>
    let getDimensionsCount (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                GetDimensionsCount(handle)
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary><para>
    /// Get number of dimensions for current prediction.</para>
    /// <para>
    /// For default <c>RawFormulaVal</c>, <c>Exponent</c>, <c>Probability</c>, <c>Class</c> prediction type
    /// GetPredictionDimensionsCount == GetDimensionsCount.</para>
    /// <para>
    /// For <c>RMSEWithUncertainty</c> - returns 2 (value prediction and predicted uncertainty).</para></summary>
    /// <param name="modelHandle">Calcer model handle.</param>
    ///
    /// <code>
    /// CATBOOST_API size_t GetPredictionDimensionsCount(ModelCalcerHandle* modelHandle);</code>
    let getPredictionDimensionsCount (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                GetPredictionDimensionsCount(handle)
        finally
            if success then
                modelHandle.DangerousRelease()

    
                    
    // **Use this method only if you really understand what you want.**
    // Calculate raw model predictions on flat feature vectors
    // Flat here means that float features and categorical feature are in the same float array.
    // @param calcer model handle
    // @param docCount number of objects
    // @param floatFeatures array of array of float (first dimension is object index, second is feature index)
    // @param floatFeaturesSize float values array size
    // @param result pointer to user allocated results vector
    // @param resultSize Result size should be equal to modelApproxDimension * docCount
    // (e.g. for non multiclass models should be equal to docCount)
    // @return false if error occured
    //
    // CATBOOST_API bool CalcModelPredictionFlat(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     double* result, size_t resultSize);
    (*
    let calcModelPredictionFlat docCount floatFeatures (modelHandle: ModelCalcerSafeHandle) =
        raise <| NotImplementedException()
        *)

    // **Use this method only if you really understand what you want.**
    // Calculate raw model predictions on flat feature vectors
    // taking into consideration only the trees in the range [treeStart; treeEnd)
    // Flat here means that float features and categorical feature are in the same float array.
    // @param calcer model handle
    // @param docCount number of objects
    // @param treeStart the index of the first tree to be used when applying the model (zero-based)
    // @param treeEnd the index of the last tree to be used when applying the model (non-inclusive, zero-based)
    // @param floatFeatures array of array of float (first dimension is object index, second is feature index)
    // @param floatFeaturesSize float values array size
    // @param result pointer to user allocated results vector
    // @param resultSize Result size should be equal to modelApproxDimension * docCount
    // (e.g. for non multiclass models should be equal to docCount)
    // @return false if error occured
    //
    // CATBOOST_API bool CalcModelPredictionFlatStaged(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     size_t treeStart, size_t treeEnd,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     double* result, size_t resultSize);
    (*
    let calcModelPredictionFlatStaged docCount treeStart treeEnd floatFeatures (modelHandle: ModelCalcerSafeHandle) =
        raise <| NotImplementedException()
        *)
        
    // **Use this method only if you really understand what you want.**
    // Calculate raw model predictions on transposed dataset layout
    // @param calcer model handle
    // @param docCount number of objects
    // @param floatFeatures array of array of float (first dimension is feature index, second is object index)
    // @param floatFeaturesSize float values array size
    // @param result pointer to user allocated results vector
    // @param resultSize Result size should be equal to modelApproxDimension * docCount
    // (e.g. for non multiclass models should be equal to docCount)
    // @return false if error occured
    //
    // CATBOOST_API bool CalcModelPredictionFlatTransposed(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     double* result, size_t resultSize);
    (*
    let calcModelPredictionFlatTransposed docCount floatFeatures (modelHandle: ModelCalcerSafeHandle) =
        raise <| NotImplementedException()
        *)

    // **Use this method only if you really understand what you want.**
    // Calculate raw model predictions on transposed dataset layout
    // taking into consideration only the trees in the range [treeStart; treeEnd)
    // @param calcer model handle
    // @param docCount number of objects
    // @param treeStart the index of the first tree to be used when applying the model (zero-based)
    // @param treeEnd the index of the last tree to be used when applying the model (non-inclusive, zero-based)
    // @param floatFeatures array of array of float (first dimension is feature index, second is object index)
    // @param floatFeaturesSize float values array size
    // @param result pointer to user allocated results vector
    // @param resultSize Result size should be equal to modelApproxDimension * docCount
    // (e.g. for non multiclass models should be equal to docCount)
    // @return false if error occured
    //
    // CATBOOST_API bool CalcModelPredictionFlatTransposedStaged(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     size_t treeStart, size_t treeEnd,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     double* result, size_t resultSize);
    (*
    let calcModelPredictionFlatTransposedStaged docCount treeStart treeEnd floatFeatures (modelHandle: ModelCalcerSafeHandle) =
        raise <| NotImplementedException()
        *)

    /// <summary>
    /// Calculate raw model predictions on float features and string categorical feature values.</summary>
    /// <param name="floatFeatures">Array of array of float (first dimension is object index, second is feature index).</param>
    /// <param name="catFeatures">Array of array of string categorical values.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Raw model predictions.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool CalcModelPrediction(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const char*** catFeatures, size_t catFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPrediction (floatFeatures: float32 array2d) (catFeatures: string array2d) (modelHandle: ModelCalcerSafeHandle) =
        let docCount = max (floatFeatures.GetLength(0)) (catFeatures.GetLength(0))
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalStringFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match CalcModelPrediction(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "calcModelPrediction"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeStringFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Calculate raw model predictions on float features and string categorical feature values taking into
    /// consideration only the trees in the range [treeStart; treeEnd)</summary>
    /// <param name="treeStart">The index of the first tree to be used when applying the model (zero-based).</param>
    /// <param name="treeEnd">The index of the last tree to be used when applying the model (non-inclusive, zero-based).</param>
    /// <param name="floatFeatures">Array of array of float (first dimension is object index, second is feature index).</param>
    /// <param name="catFeatures">array of array of string categorical values.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Raw model predictions.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool CalcModelPredictionStaged(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     size_t treeStart, size_t treeEnd,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const char*** catFeatures, size_t catFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPredictionStaged treeStart treeEnd (floatFeatures: float32 array2d) (catFeatures: string array2d) (modelHandle: ModelCalcerSafeHandle) =
        let docCount = max (floatFeatures.GetLength(0)) (catFeatures.GetLength(0))
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalStringFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match CalcModelPredictionStaged(
                    handle,
                    uint64 docCount,
                    treeStart,
                    treeEnd,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "calcModelPredictionStaged"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeStringFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Calculate raw model predictions on float features and string categorical feature values.</summary>
    /// <param name="floatFeatures">Array of array of float (first dimension is object index, second is feature index).</param>
    /// <param name="catFeatures">Array of array of string categorical values.</param>
    /// <param name="textFeatures">Array of array of string text values.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Raw model predictions.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool CalcModelPredictionText(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const char*** catFeatures, size_t catFeaturesSize,
    ///     const char*** textFeatures, size_t textFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPredictionText (floatFeatures: float32 array2d) (catFeatures: string array2d) (textFeatures: string array2d) (modelHandle: ModelCalcerSafeHandle) =
        let docCount = List.max [ floatFeatures.GetLength(0); catFeatures.GetLength(0); textFeatures.GetLength(0) ]
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalStringFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let textFeaturesPtrsHandle, textFeaturesDataHandle = marshalStringFeatures textFeatures
        let textFeaturesPtrsAddress = if textFeaturesPtrsHandle.IsAllocated then textFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let textFeaturesSize = uint64 (textFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match CalcModelPredictionText(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    textFeaturesPtrsAddress,
                    textFeaturesSize,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "calcModelPredictionText"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeStringFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            freeStringFeatures textFeaturesPtrsHandle textFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Calculate raw model predictions on float features and string categorical feature values, taking into
    /// consideration only the trees in the range [treeStart; treeEnd)</summary>
    /// <param name="treeStart">The index of the first tree to be used when applying the model (zero-based).</param>
    /// <param name="treeEnd">The index of the last tree to be used when applying the model (non-inclusive, zero-based).</param>
    /// <param name="floatFeatures">Array of array of float (first dimension is object index, second is feature index).</param>
    /// <param name="catFeatures">Array of array of string categorical values.</param>
    /// <param name="textFeatures">Array of array of string text values.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Raw model predictions.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool CalcModelPredictionTextStaged(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     size_t treeStart, size_t treeEnd,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const char*** catFeatures, size_t catFeaturesSize,
    ///     const char*** textFeatures, size_t textFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPredictionTextStaged treeStart treeEnd (floatFeatures: float32 array2d) (catFeatures: string array2d) (textFeatures: string array2d) (modelHandle: ModelCalcerSafeHandle) =
        let docCount = List.max [ floatFeatures.GetLength(0); catFeatures.GetLength(0); textFeatures.GetLength(0) ]
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalStringFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let textFeaturesPtrsHandle, textFeaturesDataHandle = marshalStringFeatures textFeatures
        let textFeaturesPtrsAddress = if textFeaturesPtrsHandle.IsAllocated then textFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let textFeaturesSize = uint64 (textFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match CalcModelPredictionTextStaged(
                    handle,
                    uint64 docCount,
                    treeStart,
                    treeEnd,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    textFeaturesPtrsAddress,
                    textFeaturesSize,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "calcModelPredictionTextStaged"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeStringFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            freeStringFeatures textFeaturesPtrsHandle textFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()

    // Calculate raw model predictions on float features and string categorical feature values
    // @param calcer model handle
    // @param docCount object count
    // @param floatFeatures array of array of float (first dimension is object index, second is feature index)
    // @param floatFeaturesSize float feature count
    // @param catFeatures array of array of char* categorical value pointers.
    // String pointer should point to zero terminated string.
    // @param catFeaturesSize categorical feature count
    // @param textFeatures array of array of char* text value pointers.
    // String pointer should point to zero terminated string.
    // @param textFeaturesSize text feature count
    // @param embeddingFeatures array of array of array of float (first dimension is object index, second is feature index, third is index in embedding array).
    // String pointer should point to zero terminated string.
    // @param embeddingFeaturesSize embedding feature count
    // @param result pointer to user allocated results vector
    // @param resultSize result size should be equal to modelApproxDimension * docCount
    // (e.g. for non multiclass models should be equal to docCount)
    // @return false if error occured
    //
    // CATBOOST_API bool CalcModelPredictionTextAndEmbeddings(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     const char*** catFeatures, size_t catFeaturesSize,
    //     const char*** textFeatures, size_t textFeaturesSize,
    //     const float*** embeddingFeatures, size_t* embeddingDimensions, size_t embeddingFeaturesSize,
    //     double* result, size_t resultSize);
    (*
    let calcModelPredictionTextAndEmbeddings
        docCount
        floatFeatures
        floatFeaturesSize
        catFeatures 
        catFeaturesSize
        textFeatures
        textFeaturesSize
        embeddingFeatures
         embeddingDimensions
        embeddingFeaturesSize
        (modelHandle: ModelCalcerSafeHandle) =
        raise <| NotImplementedException()
        *)

    // Calculate raw model predictions on float features and string categorical feature values
    // taking into consideration only the trees in the range [treeStart; treeEnd)
    // @param calcer model handle
    // @param docCount object count
    // @param treeStart the index of the first tree to be used when applying the model (zero-based)
    // @param treeEnd the index of the last tree to be used when applying the model (non-inclusive, zero-based)
    // @param floatFeatures array of array of float (first dimension is object index, second is feature index)
    // @param floatFeaturesSize float feature count
    // @param catFeatures array of array of char* categorical value pointers.
    // String pointer should point to zero terminated string.
    // @param catFeaturesSize categorical feature count
    // @param textFeatures array of array of char* text value pointers.
    // String pointer should point to zero terminated string.
    // @param textFeaturesSize text feature count
    // @param embeddingFeatures array of array of array of float (first dimension is object index, second is feature index, third is index in embedding array).
    // String pointer should point to zero terminated string.
    // @param embeddingFeaturesSize embedding feature count
    // @param result pointer to user allocated results vector
    // @param resultSize result size should be equal to modelApproxDimension * docCount
    // (e.g. for non multiclass models should be equal to docCount)
    // @return false if error occured
    //
    // CATBOOST_API bool CalcModelPredictionTextAndEmbeddingsStaged(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     size_t treeStart, size_t treeEnd,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     const char*** catFeatures, size_t catFeaturesSize,
    //     const char*** textFeatures, size_t textFeaturesSize,
    //     const float*** embeddingFeatures, size_t* embeddingDimensions, size_t embeddingFeaturesSize,
    //     double* result, size_t resultSize);
    (*
    let calcModelPredictionTextAndEmbeddingsStaged
        docCount
        treeStart
        treeEnd
        floatFeatures
        floatFeaturesSize
        catFeatures
        catFeaturesSize
        textFeatures
        textFeaturesSize
        embeddingFeatures
        embeddingDimensions
        embeddingFeaturesSize
        (modelHandle: ModelCalcerSafeHandle) =
        raise <| NotImplementedException()
        *)

    let internal marshalStringFeaturesSingle (data: string[]) =
        if Array.isEmpty data then
            (GCHandle(), [||])
        else
            let stringPtrs =
                [| 0 .. data.Length - 1 |]
                |> Array.map (fun i ->
                    Marshal.StringToHGlobalAnsi(data[i]))
            
            let ptrsHandle = GCHandle.Alloc(stringPtrs, GCHandleType.Pinned)
            
            (ptrsHandle, stringPtrs)
    
    let internal freeStringFeaturesSingle (ptrsHandle: GCHandle) (stringPtrs: nativeint array) =
        for ptr in stringPtrs do
            if ptr <> 0n then
                Marshal.FreeHGlobal(ptr)
        if ptrsHandle.IsAllocated then
            ptrsHandle.Free()

    /// <summary>
    /// Calculate raw model prediction on float features and string categorical feature values for single object.</summary>
    /// <param name="floatFeatures">Array of float features.</param>
    /// <param name="catFeatures">Array of string categorical feature.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Raw model prediction.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool CalcModelPredictionSingle(
    ///     ModelCalcerHandle* modelHandle,
    ///     const float* floatFeatures, size_t floatFeaturesSize,
    ///     const char** catFeatures, size_t catFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPredictionSingle (floatFeatures: float32 array) (catFeatures: string array) (modelHandle: ModelCalcerSafeHandle) =
        let floatFeaturesHandle = GCHandle.Alloc(floatFeatures, GCHandleType.Pinned)
        let floatFeaturesPtr = if floatFeaturesHandle.IsAllocated then floatFeaturesHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 floatFeatures.Length
        
        let catFeaturesHandle, catStringPtrs = marshalStringFeaturesSingle catFeatures
        let catFeaturesPtr = if catFeaturesHandle.IsAllocated then catFeaturesHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 catFeatures.Length

        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                let dimensions = GetPredictionDimensionsCount(handle)
                let result = Array.zeroCreate<float> (int dimensions)
                
                match CalcModelPredictionSingle(
                    handle,
                    floatFeaturesPtr,
                    floatFeaturesSize,
                    catFeaturesPtr,
                    catFeaturesSize,
                    result,
                    dimensions) with
                | true ->
                    Ok result
                | false -> showError "calcModelPredictionSingle"
        finally
            if floatFeaturesHandle.IsAllocated then
                floatFeaturesHandle.Free()
            
            freeStringFeaturesSingle catFeaturesHandle catStringPtrs
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Calculate raw model prediction on float features and string categorical feature values for single object taking
    /// into consideration only the trees in the range [treeStart; treeEnd).</summary>
    /// <param name="treeStart">The index of the first tree to be used when applying the model (zero-based).</param>
    /// <param name="treeEnd">The index of the last tree to be used when applying the model (non-inclusive, zero-based).</param>
    /// <param name="floatFeatures">Array of float features.</param>
    /// <param name="catFeatures">Array of string categorical feature.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Raw model prediction.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool CalcModelPredictionSingleStaged(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t treeStart, size_t treeEnd,
    ///     const float* floatFeatures, size_t floatFeaturesSize,
    ///     const char** catFeatures, size_t catFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPredictionSingleStaged treeStart treeEnd (floatFeatures: float32 array) (catFeatures: string array) (modelHandle: ModelCalcerSafeHandle) =
        let floatFeaturesHandle = GCHandle.Alloc(floatFeatures, GCHandleType.Pinned)
        let floatFeaturesPtr = if floatFeaturesHandle.IsAllocated then floatFeaturesHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 floatFeatures.Length
        
        let catFeaturesHandle, catStringPtrs = marshalStringFeaturesSingle catFeatures
        let catFeaturesPtr = if catFeaturesHandle.IsAllocated then catFeaturesHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 catFeatures.Length

        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                let dimensions = GetPredictionDimensionsCount(handle)
                let result = Array.zeroCreate<float> (int dimensions)
                
                match CalcModelPredictionSingleStaged(
                    handle,
                    treeStart,
                    treeEnd,
                    floatFeaturesPtr,
                    floatFeaturesSize,
                    catFeaturesPtr,
                    catFeaturesSize,
                    result,
                    dimensions) with
                | true ->
                    Ok result
                | false -> showError "calcModelPredictionSingleStaged"
        finally
            if floatFeaturesHandle.IsAllocated then
                floatFeaturesHandle.Free()
            
            freeStringFeaturesSingle catFeaturesHandle catStringPtrs
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Calculate raw model predictions on float features and hashed categorical feature values.</summary>
    /// <param name="floatFeatures">Array of array of float (first dimension is object index, second is feature index).</param>
    /// <param name="catFeatures">Array of array of integers - hashed categorical feature values.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Raw model predictions.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool CalcModelPredictionWithHashedCatFeatures(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const int** catFeatures, size_t catFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPredictionWithHashedCatFeatures (floatFeatures: float32 array2d) (catFeatures: int array2d) (modelHandle: ModelCalcerSafeHandle)=
        let docCount = max (floatFeatures.GetLength(0)) (catFeatures.GetLength(0))
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match CalcModelPredictionWithHashedCatFeatures(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "calcModelPredictionWithHashedCatFeatures"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()
        
    /// <code>
    /// CATBOOST_API bool CalcModelPredictionWithHashedCatFeaturesAndTextFeatures(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const int** catFeatures, size_t catFeaturesSize,
    ///     const char*** textFeatures, size_t textFeaturesSize,
    ///     double* result, size_t resultSize);</code>
    let calcModelPredictionWithHashedCatFeaturesAndTextFeatures (floatFeatures: float32 array2d) (catFeatures: int array2d) (textFeatures: string array2d) (modelHandle: ModelCalcerSafeHandle) =
        let docCount = List.max [ floatFeatures.GetLength(0); catFeatures.GetLength(0); textFeatures.GetLength(0) ]
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let textFeaturesPtrsHandle, textFeaturesDataHandle = marshalStringFeatures textFeatures
        let textFeaturesPtrsAddress = if textFeaturesPtrsHandle.IsAllocated then textFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let textFeaturesSize = uint64 (textFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match CalcModelPredictionWithHashedCatFeaturesAndTextFeatures(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    textFeaturesPtrsAddress,
                    textFeaturesSize,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "calcModelPredictionWithHashedCatFeaturesAndTextFeatures"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            freeStringFeatures textFeaturesPtrsHandle textFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()

    // CATBOOST_API bool CalcModelPredictionWithHashedCatFeaturesAndTextAndEmbeddingFeatures(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     const int** catFeatures, size_t catFeaturesSize,
    //     const char*** textFeatures, size_t textFeaturesSize,
    //     const float*** embeddingFeatures, size_t* embeddingDimensions, size_t embeddingFeaturesSize,
    //     double* result, size_t resultSize);
    (*
    [<DllImport(catboost, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)>]
    extern bool CalcModelPredictionWithHashedCatFeaturesAndTextAndEmbeddingFeatures(
        ModelCalcerHandle& modelHandle,
        uint64 docCount,
        nativeint floatFeatures,
        uint64 floatFeaturesSize,
        nativeint catFeatures,
        uint64 catFeaturesSize,
        nativeint textFeatures,
        uint64 textFeaturesSize,
        nativeint embeddingFeatures,
        unativeint& embeddingDimensions,
        uint64 embeddingFeaturesSize,
        nativeint result,
        uint64 resultSize);
        *)


    /// <summary>
    /// Methods equivalent to the methods above, only returning a prediction for the specific class.</summary>
    /// <param name="floatFeatures">Array of array of float (first dimension is object index, second is feature index).</param>
    /// <param name="classId">Number of the class should be in [0, modelApproxDimension - 1].</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    ///
    /// <code>
    /// CATBOOST_API bool PredictSpecificClassFlat(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     int classId,
    ///     double* result, size_t resultSize);</code>
    let predictSpecificClassFlat (floatFeatures: float32 array2d) classId (modelHandle: ModelCalcerSafeHandle) =
        let docCount = floatFeatures.GetLength(0)
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match PredictSpecificClassFlat(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    classId,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "predictSpecificClassFlat"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()
    
    /// <code>
    /// CATBOOST_API bool PredictSpecificClass(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const char*** catFeatures, size_t catFeaturesSize,
    ///     int classId,
    ///     double* result, size_t resultSize);</code>
    let predictSpecificClass (floatFeatures: float32 array2d) (catFeatures: string array2d) classId (modelHandle: ModelCalcerSafeHandle) =
        let docCount = max (floatFeatures.GetLength(0)) (catFeatures.GetLength(0))
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalStringFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize

        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match PredictSpecificClass(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    classId,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "predictSpecificClass"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeStringFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()
    
    /// <code>
    /// CATBOOST_API bool PredictSpecificClassText(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const char*** catFeatures, size_t catFeaturesSize,
    ///     const char*** textFeatures, size_t textFeaturesSize,
    ///     int classId,
    ///     double* result, size_t resultSize);</code>
    let predictSpecificClassText (floatFeatures: float32 array2d) (catFeatures: string array2d) (textFeatures: string array2d) classId (modelHandle: ModelCalcerSafeHandle) =
        let docCount = List.max [ floatFeatures.GetLength(0); catFeatures.GetLength(0); textFeatures.GetLength(0) ]
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalStringFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let textFeaturesPtrsHandle, textFeaturesDataHandle = marshalStringFeatures textFeatures
        let textFeaturesPtrsAddress = if textFeaturesPtrsHandle.IsAllocated then textFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let textFeaturesSize = uint64 (textFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize

        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match PredictSpecificClassText(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    textFeaturesPtrsAddress,
                    textFeaturesSize,
                    classId,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "predictSpecificClassText"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeStringFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            freeStringFeatures textFeaturesPtrsHandle textFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()

    // CATBOOST_API bool PredictSpecificClassTextAndEmbeddings(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     const char*** catFeatures, uint64 catFeaturesSize,
    //     const char*** textFeatures, size_t textFeaturesSize,
    //     const float*** embeddingFeatures, size_t* embeddingDimensions, size_t embeddingFeaturesSize,
    //     int classId,
    //     double* result, size_t resultSize);
    (*
    let predictSpecificClassTextAndEmbeddings floatFeatures catFeatures textFeatures embeddingFeatures classId =
        raise <| NotImplementedException ()
        *)

    /// <code>
    /// CATBOOST_API bool PredictSpecificClassSingle(
    ///     ModelCalcerHandle* modelHandle,
    ///     const float* floatFeatures, size_t floatFeaturesSize,
    ///     const char** catFeatures, size_t catFeaturesSize,
    ///     int classId,
    ///     double* result, size_t resultSize);</code>
    let predictSpecificClassSingle (floatFeatures: float32 array) (catFeatures: string array) classId (modelHandle: ModelCalcerSafeHandle) =
        let floatFeaturesHandle = GCHandle.Alloc(floatFeatures, GCHandleType.Pinned)
        let floatFeaturesPtr = if floatFeaturesHandle.IsAllocated then floatFeaturesHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 floatFeatures.Length
        
        let catFeaturesHandle, catStringPtrs = marshalStringFeaturesSingle catFeatures
        let catFeaturesPtr = if catFeaturesHandle.IsAllocated then catFeaturesHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 catFeatures.Length

        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                let dimensions = GetPredictionDimensionsCount(handle)
                let result = Array.zeroCreate<float> (int dimensions)
                
                match PredictSpecificClassSingle(
                    handle,
                    floatFeaturesPtr,
                    floatFeaturesSize,
                    catFeaturesPtr,
                    catFeaturesSize,
                    classId,
                    result,
                    dimensions) with
                | true ->
                    Ok result
                | false -> showError "predictSpecificClassSingle"
        finally
            if floatFeaturesHandle.IsAllocated then
                floatFeaturesHandle.Free()
            
            freeStringFeaturesSingle catFeaturesHandle catStringPtrs
            if success then
                modelHandle.DangerousRelease()

    /// <code>
    /// CATBOOST_API bool PredictSpecificClassWithHashedCatFeatures(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const int** catFeatures, size_t catFeaturesSize,
    ///     int classId,
    ///     double* result, size_t resultSize);</code>
    let predictSpecificClassWithHashedCatFeatures (floatFeatures: float32 array2d) (catFeatures: int array2d) classId (modelHandle: ModelCalcerSafeHandle) =
        let docCount = max (floatFeatures.GetLength(0)) (catFeatures.GetLength(0))
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize

        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match PredictSpecificClassWithHashedCatFeatures(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    classId,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "predictSpecificClassWithHashedCatFeatures"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()
    
    /// <code>
    /// CATBOOST_API bool PredictSpecificClassWithHashedCatFeaturesAndTextFeatures(
    ///     ModelCalcerHandle* modelHandle,
    ///     size_t docCount,
    ///     const float** floatFeatures, size_t floatFeaturesSize,
    ///     const int** catFeatures, size_t catFeaturesSize,
    ///     const char*** textFeatures, size_t textFeaturesSize,
    ///     int classId,
    ///     double* result, size_t resultSize);</code>
    let predictSpecificClassWithHashedCatFeaturesAndTextFeatures (floatFeatures: float32 array2d) (catFeatures: int array2d) (textFeatures: string array2d) classId (modelHandle: ModelCalcerSafeHandle) =
        let docCount = List.max [ floatFeatures.GetLength(0); catFeatures.GetLength(0); textFeatures.GetLength(0) ]
        
        let floatFeaturesPtrsHandle, floatFeaturesDataHandle = marshalFeatures floatFeatures
        let floatFeaturesPtrsAddress = if floatFeaturesPtrsHandle.IsAllocated then floatFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let floatFeaturesSize = uint64 (floatFeatures.GetLength(1))
        
        let catFeaturesPtrsHandle, catFeaturesDataHandle = marshalFeatures catFeatures
        let catFeaturesPtrsAddress = if catFeaturesPtrsHandle.IsAllocated then catFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let catFeaturesSize = uint64 (catFeatures.GetLength(1))

        let textFeaturesPtrsHandle, textFeaturesDataHandle = marshalStringFeatures textFeatures
        let textFeaturesPtrsAddress = if textFeaturesPtrsHandle.IsAllocated then textFeaturesPtrsHandle.AddrOfPinnedObject() else 0n
        let textFeaturesSize = uint64 (textFeatures.GetLength(1))

        let dimensions = getPredictionDimensionsCount modelHandle |> int
        let resultSize = docCount * dimensions
        let flatResult = Array.zeroCreate<float> resultSize

        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match PredictSpecificClassWithHashedCatFeaturesAndTextFeatures(
                    handle,
                    uint64 docCount,
                    floatFeaturesPtrsAddress,
                    floatFeaturesSize,
                    catFeaturesPtrsAddress,
                    catFeaturesSize,
                    textFeaturesPtrsAddress,
                    textFeaturesSize,
                    classId,
                    flatResult,
                    uint64 resultSize) with
                | true ->
                    let result = Array2D.init docCount dimensions (fun dc d -> flatResult[dimensions * dc + d])
                    Ok result
                | false -> showError "predictSpecificClassWithHashedCatFeaturesAndTextFeatures"
        finally
            freeFeatures floatFeaturesPtrsHandle floatFeaturesDataHandle
            freeFeatures catFeaturesPtrsHandle catFeaturesDataHandle
            freeStringFeatures textFeaturesPtrsHandle textFeaturesDataHandle
            if success then
                modelHandle.DangerousRelease()

    // CATBOOST_API bool PredictSpecificClassWithHashedCatFeaturesAndTextAndEmbeddingFeatures(
    //     ModelCalcerHandle* modelHandle,
    //     size_t docCount,
    //     const float** floatFeatures, size_t floatFeaturesSize,
    //     const int** catFeatures, size_t catFeaturesSize,
    //     const char*** textFeatures, size_t textFeaturesSize,
    //     const float*** embeddingFeatures, size_t* embeddingDimensions, size_t embeddingFeaturesSize,
    //     int classId,
    //     double* result, size_t resultSize);
    (*
    let predictSpecificClassWithHashedCatFeaturesAndTextAndEmbeddingFeatures floatFeatures catFeatures textFeatures embeddingFeatures classId modelHandle =
        raise <| NotImplementedException ()
        *)


    /// <summary>
    /// Get hash for given string value.</summary>
    /// <param name="data">A string value.</param>
    /// <returns>Hash value.</returns>
    ///
    /// <code>
    /// CATBOOST_API int GetStringCatFeatureHash(const char* data, size_t size);</code>
    let getStringCatFeatureHash (data: string) =
        let bytes = Encoding.UTF8.GetBytes(data)
        let size = bytes.Length |> uint64
        let handle = GCHandle.Alloc(bytes, GCHandleType.Pinned)

        try
            GetStringCatFeatureHash(handle.AddrOfPinnedObject(), size)
        finally
            if handle.IsAllocated then
                handle.Free()
        
    /// <summary><param>
    /// Special case for hash calculation - integer hash.</param>
    /// <param>
    /// Internally we cast value to string and then calulcate string hash function.</param>
    /// <param>
    /// Used in ClickHouse for catboost model evaluation on integer cat features.</param></summary>
    /// <param name="value">Integer cat feature value.</param>
    /// <returns>Hash value.</returns>
    ///
    /// <code>
    /// CATBOOST_API int GetIntegerCatFeatureHash(long long val);</code>
    let getIntegerCatFeatureHash value =
        GetIntegerCatFeatureHash(value)

    /// <summary>
    /// Get expected float feature count for model.</summary>
    /// <param name="modelHandle">Calcer model handle.</param>
    ///
    /// <code>
    /// CATBOOST_API size_t GetFloatFeaturesCount(ModelCalcerHandle* modelHandle);</code>
    let getFloatFeaturesCount (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                GetFloatFeaturesCount(handle)
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary><para>
    /// Get expected indices of float features used in the model.</para>
    /// <para>
    /// indices array must be deallocated using free() after use.</para></summary>
    /// <param name="modelHandle">Model handle.</param>
    /// <returns>Indices of float features used in the model.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool GetFloatFeatureIndices(ModelCalcerHandle* modelHandle, size_t** indices, size_t* count);</code>
    let getFloatFeatureIndices (modelHandle: ModelCalcerSafeHandle) =
        let mutable indicesPtr = 0n
        let mutable count = 0UL
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match GetFloatFeatureIndices(handle, &indicesPtr, &count) with
                | true ->
                    let indices = Array.zeroCreate<int64> (int count)
                    Marshal.Copy(indicesPtr, indices, 0, int count)
                    
                    Ok (Array.map uint64 indices)
                | false -> showError "getFloatFeatureIndices"
        finally
            free(indicesPtr)
            if success then
                modelHandle.DangerousRelease()
            
    /// <summary>
    /// Get expected categorical feature count for model.</summary>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Categorical feature count.</returns>
    /// 
    /// <code>
    /// CATBOOST_API size_t GetCatFeaturesCount(ModelCalcerHandle* modelHandle);</code>
    let getCatFeaturesCount (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                GetCatFeaturesCount(handle)
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get expected indices of category features used in the model.</summary>
    /// <param name="modelHandle">Model handle.</param>
    /// <returns>Indices of category features.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool GetCatFeatureIndices(ModelCalcerHandle* modelHandle, size_t** indices, size_t* count);</code>
    let getCatFeatureIndices (modelHandle: ModelCalcerSafeHandle) =
        let mutable indicesPtr = 0n
        let mutable count = 0UL
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match GetCatFeatureIndices(handle, &indicesPtr, &count) with
                | true when count > 0UL ->
                    let indices = Array.zeroCreate<int64> (int count)
                    Marshal.Copy(indicesPtr, indices, 0, int count)
                    Ok (Array.map uint64 indices)
                | true ->
                    Ok [||]
                | false -> showError "getCatFeatureIndices"
        finally
            free(indicesPtr)                
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get expected text feature count for model.</summary>
    /// <param name="modelHandle">Model handle.</param>
    /// <returns>Text feature count.</returns>
    ///
    /// <code>
    /// CATBOOST_API size_t GetTextFeaturesCount(ModelCalcerHandle* modelHandle);</code>
    let getTextFeaturesCount (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                GetTextFeaturesCount(handle)
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get expected indices of text features used in the model.</summary>
    /// <param name="modelHandle">Model handle.</param>
    /// <returns>Indices of text features.</returns>
    /// 
    /// <code>
    /// CATBOOST_API bool GetTextFeatureIndices(ModelCalcerHandle* modelHandle, size_t** indices, size_t* count);</code>
    let getTextFeatureIndices (modelHandle: ModelCalcerSafeHandle) =
        let mutable indicesPtr = 0n
        let mutable count = 0UL
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match GetTextFeatureIndices(handle, &indicesPtr, &count) with
                | true ->
                    let indices = Array.zeroCreate<float32> (int count)
                    Marshal.Copy(indicesPtr, indices, 0, int count)
                    Ok(indices, count)
                | false -> showError "getTextFeatureIndices"
        finally
            free(indicesPtr)
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get expected embedding feature count for model.</summary>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Embedding features count.</returns>
    /// 
    /// <code>
    /// CATBOOST_API size_t GetEmbeddingFeaturesCount(ModelCalcerHandle* modelHandle);</code>
    let getEmbeddingFeaturesCount (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                GetEmbeddingFeaturesCount(handle)
        finally
            if success then
                modelHandle.DangerousRelease()


    /// <summary>
    /// Get expected indices of embedding features used in the model.</summary>
    /// <param name="modelHandle">Model handle.</param>
    /// <returns>Indices of embedding features.</returns>
    /// 
    /// <code>
    /// CATBOOST_API bool GetEmbeddingFeatureIndices(ModelCalcerHandle* modelHandle, size_t** indices, size_t* count);</code>
    let getEmbeddingFeatureIndices (modelHandle: ModelCalcerSafeHandle) =
        let mutable indicesPtr = 0n
        let mutable count = 0UL
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                match GetEmbeddingFeatureIndices(handle, &indicesPtr, &count) with
                | true ->
                    let indices = Array.zeroCreate<float32> (int count)
                    Marshal.Copy(indicesPtr, indices, 0, int count)
                    Ok(indices, count)
                | false -> showError "getEmbeddingFeatureIndices"
        finally
            free(indicesPtr)
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get number of trees in model.</summary>
    /// <param name="modelHandle">Calcer model handle.</param>
    /// <returns>Number of trees.</returns>
    /// 
    /// <code>
    /// CATBOOST_API size_t GetTreeCount(ModelCalcerHandle* modelHandle);</code>
    let getTreeCount (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else                
                let handle = modelHandle.DangerousGetHandle()
                
                GetTreeCount(handle)
        finally
            if success then
                modelHandle.DangerousRelease()


    /// <summary>
    /// Check if model metadata holds some value for provided key.</summary>
    /// <param name="key">Key for metainfo.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    ///
    /// <code>
    /// CATBOOST_API bool CheckModelMetadataHasKey(ModelCalcerHandle* modelHandle, const char* keyPtr, size_t keySize);</code>
    let checkModelMetadataHasKey key (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = modelHandle.DangerousGetHandle()

                CheckModelMetadataHasKey(handle, key, uint64 key.Length)
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get model metainfo value size for some key. Returns 0 both if key is missing in model metadata and if it is
    /// really missing.</summary>
    /// <param name="key">Key for metainfo.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    ///
    /// <code>
    /// CATBOOST_API size_t GetModelInfoValueSize(ModelCalcerHandle* modelHandle, const char* keyPtr, size_t keySize);</code>
    let getModelInfoValueSize key (modelHandle: ModelCalcerSafeHandle) =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = modelHandle.DangerousGetHandle()

                GetModelInfoValueSize(handle, key, uint64 key.Length)
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get model metainfo for some key. Returns string. If key is missing in model metainfo storage this method will
    /// return empty string.</summary>
    /// <param name="modelInfoKey">Key for metainfo.</param>
    /// <param name="modelHandle">Calcer model handle.</param>
    ///
    /// <code>
    /// CATBOOST_API const char* GetModelInfoValue(ModelCalcerHandle* modelHandle, const char* keyPtr, size_t keySize);</code>
    let getModelInfoValue modelInfoKey (modelHandle: ModelCalcerSafeHandle) =
        let key = modelInfoKeyToStr modelInfoKey
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = modelHandle.DangerousGetHandle()

                let ptr = GetModelInfoValue(handle, key, uint64 key.Length)
                if ptr <> 0n then
                    Marshal.PtrToStringUTF8(ptr)
                else
                    ""
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Get names of features used in the model.</summary>
    /// <param name="modelHandle">Model handle.</param>
    /// <returns>Features used.</returns>
    ///
    /// <code>
    /// CATBOOST_API bool GetModelUsedFeaturesNames(ModelCalcerHandle* modelHandle, char*** featureNames, size_t* featureCount);</code>
    let getModelUsedFeaturesNames (modelHandle: ModelCalcerSafeHandle) =
        let mutable featureNamesPtr = 0n
        let mutable featureCount = 0UL
        
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            else
                let handle = modelHandle.DangerousGetHandle()

                match GetModelUsedFeaturesNames(handle, &featureNamesPtr, &featureCount) with
                | true when featureNamesPtr <> 0n ->
                    let count = int featureCount
                    let stringPtrs = Array.zeroCreate<nativeint> count
                    Marshal.Copy(featureNamesPtr, stringPtrs, 0, count)

                    let featureNames =
                        stringPtrs
                        |> Array.map (fun ptr ->
                            if ptr <> 0n then
                                let name = Marshal.PtrToStringUTF8(ptr)
                                free(ptr)
                                name
                            else
                                "")
                    Ok featureNames
                | true -> Ok [||]
                | false -> showError "getModelUsedFeaturesNames"
            finally
                if featureNamesPtr <> 0n then
                    free(featureNamesPtr)
                if success then
                    modelHandle.DangerousRelease()
    /// <summary>
    /// Returns aggregated model metadata as a ModelMetadata record.
    /// </summary>
    let getModelMetadata (modelHandle: ModelCalcerSafeHandle) : Result<ModelMetadata, string> =
        let treeCount = getTreeCount modelHandle
        let floatCount = getFloatFeaturesCount modelHandle
        let catCount = getCatFeaturesCount modelHandle
        let textCount = getTextFeaturesCount modelHandle
        let embeddingCount = getEmbeddingFeaturesCount modelHandle
        match getModelUsedFeaturesNames modelHandle with
        | Error err -> Error err
        | Ok usedNames ->
            Ok {
                TreeCount = int treeCount
                FloatFeaturesCount = int floatCount
                CatFeaturesCount = int catCount
                TextFeaturesCount = int textCount
                EmbeddingFeaturesCount = int embeddingCount
                UsedFeatureNames = usedNames
            }
