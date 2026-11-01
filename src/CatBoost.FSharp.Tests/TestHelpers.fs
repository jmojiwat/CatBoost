module TestHelpers

open Xunit

/// Fails the test with the error message if the result is an Error.
let assertOk (result: Result<_, string>) =
    match result with
    | Ok _ -> ()
    | Error (e: string) -> Assert.Fail(e)

/// Writes a line to the current test's output. Does nothing outside a running test.
let writeOutput (message: string) =
    match TestContext.Current.TestOutputHelper with
    | null -> ()
    | output -> output.WriteLine(message)
