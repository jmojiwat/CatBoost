namespace CatBoost.Core

open System
open System.Runtime.InteropServices
open CatBoost.Core.Handles
open CatBoost.Core.Interop

module Array2D =
    let isEmpty (array: 'a[,]) =
        Array2D.length1 array = 0 || Array2D.length2 array = 0

module Plumbing =

    /// <summary>
    /// If an error occurred, returns the stored exception message.
    /// If no error occurred, returns None.
    /// </summary>
    let getErrorString () : string option =
        let ptr = GetErrorString()
        if ptr <> 0n then
            Some(Marshal.PtrToStringUTF8(ptr))
        else
            None

    /// <summary>
    /// Formats an error Result containing the function name and native error message.
    /// </summary>
    let showError (fnName: string) : Result<'a, string> =
        match getErrorString() with
        | Some message -> Error $"Error - {fnName}: {message}"
        | None -> Error $"Error - {fnName}"

    /// <summary>
    /// Pins a 2D float or primitive feature array and creates a pinned array of native row pointers.
    /// </summary>
    let marshalFeatures (data: 'a array2d) : GCHandle * GCHandle =
        if Array2D.isEmpty data then
            (GCHandle(), GCHandle())
        else
            let rows = data.GetLongLength(0)
            let columns = data.GetLongLength(1)
            let dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned)
            let baseAddress = dataHandle.AddrOfPinnedObject()
            let rowPtrs =
                [| 0 .. (int rows) - 1 |]
                |> Array.map (fun i ->
                    let rowOffset = i * (int columns) * sizeof<'a>
                    IntPtr.Add(baseAddress, rowOffset))
            let ptrsHandle = GCHandle.Alloc(rowPtrs, GCHandleType.Pinned)
            (ptrsHandle, dataHandle)

    /// <summary>
    /// Converts a 2D string array into ANSI strings in global heap, pinning row pointer arrays and top-level pointers.
    /// </summary>
    let marshalStringFeatures (data: string array2d) : GCHandle * (GCHandle * nativeint[])[] =
        if Array2D.isEmpty data then
            (GCHandle(), [| GCHandle(), [| |] |])
        else
            let rows = data.GetLongLength(0)
            let columns = data.GetLongLength(1)
            let handlesAndStringPtrs = Array.zeroCreate<GCHandle * nativeint[]> (int rows)
            let docPointers = Array.zeroCreate<nativeint> (int rows)

            for i in 0 .. (int rows) - 1 do
                let stringPtrs = Array.zeroCreate<nativeint> (int columns)
                for j in 0 .. (int columns) - 1 do
                    stringPtrs[j] <- Marshal.StringToHGlobalAnsi(data[i, j])

                let docHandle = GCHandle.Alloc(stringPtrs, GCHandleType.Pinned)
                handlesAndStringPtrs[i] <- (docHandle, stringPtrs)
                docPointers[i] <- docHandle.AddrOfPinnedObject()

            let topLevelHandle = GCHandle.Alloc(docPointers, GCHandleType.Pinned)
            (topLevelHandle, handlesAndStringPtrs)

    /// <summary>
    /// Releases the GCHandles allocated by marshalFeatures.
    /// </summary>
    let freeFeatures (ptrsHandle: GCHandle) (dataHandle: GCHandle) =
        if ptrsHandle.IsAllocated then ptrsHandle.Free()
        if dataHandle.IsAllocated then dataHandle.Free()

    /// <summary>
    /// Releases the native memory and GCHandles allocated by marshalStringFeatures.
    /// </summary>
    let freeStringFeatures (topLevelHandle: GCHandle) (handlesAndStringPtrs: (GCHandle * nativeint[])[]) =
        for docHandle, stringPtrs in handlesAndStringPtrs do
            for ptr in stringPtrs do
                if ptr <> 0n then Marshal.FreeHGlobal(ptr)
            if docHandle.IsAllocated then docHandle.Free()
        if topLevelHandle.IsAllocated then topLevelHandle.Free()

    /// <summary>
    /// Safely executes an action with a ModelCalcerSafeHandle by adding and releasing references.
    /// </summary>
    let withModelHandle (modelHandle: ModelCalcerSafeHandle) (action: nativeint -> 'T) : 'T =
        let mutable success = false
        try
            modelHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            action (modelHandle.DangerousGetHandle())
        finally
            if success then
                modelHandle.DangerousRelease()

    /// <summary>
    /// Safely executes an action with a DataWrapperSafeHandle by adding and releasing references.
    /// </summary>
    let withDataWrapperHandle (wrapperHandle: DataWrapperSafeHandle) (action: nativeint -> 'T) : 'T =
        let mutable success = false
        try
            wrapperHandle.DangerousAddRef(&success)
            if not success then
                failwith "Failed to add ref to model handle."
            action (wrapperHandle.DangerousGetHandle())
        finally
            if success then
                wrapperHandle.DangerousRelease()
