namespace CatBoost.Core

open Microsoft.Win32.SafeHandles
open System.Runtime.InteropServices

[<AutoOpen>]
module Handles =
    
    type DataWrapperHandle = nativeint
    
    type DataProviderHandle = nativeint

    type ModelCalcerHandle = nativeint

    // No extension, so the runtime probes the platform-specific file name:
    // catboostmodel.dll (Windows), libcatboostmodel.so (Linux), libcatboostmodel.dylib (macOS).
    [<Literal>]
    let catboost = "catboostmodel"

    // CATBOOST_API void DataWrapperDelete(DataWrapperHandle* dataWrapperHandle);
    [<DllImport(catboost, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)>]
    extern unit DataWrapperDelete(DataWrapperHandle dataWrapperHandle)
    
    type DataWrapperSafeHandle (handle: DataWrapperHandle) =
        inherit SafeHandleZeroOrMinusOneIsInvalid(true)
        
        do base.SetHandle(handle)
        
        override this.ReleaseHandle() =
            DataWrapperDelete(this.handle)
            true

    // Delete model handle
    // @param calcer
    // 
    // CATBOOST_API void ModelCalcerDelete(ModelCalcerHandle* modelHandle);
    [<DllImport(catboost, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)>]
    extern unit ModelCalcerDelete(ModelCalcerHandle modelHandle)

    type ModelCalcerSafeHandle (handle: ModelCalcerHandle) =
        inherit SafeHandleZeroOrMinusOneIsInvalid(true)
        
        do base.SetHandle(handle)
        
        override this.ReleaseHandle() =
            ModelCalcerDelete(this.handle)
            true

    // C runtime `free`, used to release memory that the native library allocates with malloc()
    // (GetSupportedEvaluatorTypes, Get*FeatureIndices and GetModelUsedFeaturesNames return
    // malloc'd buffers that callers must free). NativeMemory.Free calls the platform C `free`
    // (ucrtbase on Windows, libc elsewhere).
    let free (ptr: nativeint) = NativeMemory.Free(ptr.ToPointer())
