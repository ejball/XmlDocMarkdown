namespace ExtensionMethods

open System.Collections
open System.Runtime.CompilerServices

// F# extension style
// implicit `this` parameter has a null name
module Augment =
  type System.Object with
    member self.IsNotNull
      with get() =
        self |> isNull |> not

open Augment

// C# extension style
// no `this` parameter syntactic sugar available
// so explicitly decorate with `ExtensionAttribute`
[<Extension>]
module Utility =
  [<Extension>]
  let SafeLength (x : IEnumerable) =
    if x.IsNotNull
    then  x |> Seq.cast<obj> |> Seq.length
    else -1

type Fix<'T> = delegate of 'T -> Fix<'T>
