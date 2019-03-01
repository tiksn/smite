namespace TIKSN.smite.lib

module IndentationFeatures =
    open System.Text

    type IndentedLine =
        { LineIndentCount : int
          LineContent : string }

    let appendLineToBuilder (builder : StringBuilder, indentCount : int,
                             lineContent : string, spaces : string) =
        seq { 1..indentCount }
        |> Seq.iter (fun x -> builder.Append(spaces) |> ignore)
        builder.AppendLine lineContent |> ignore

    let convertIndentedLinesToString (lines : seq<IndentedLine>,
                                      indentSpaces : int) =
        let builder = StringBuilder()

        let spaces =
            seq {
                for i = 1 to indentSpaces do
                    yield " "
            }
            |> Seq.reduce (fun x y -> x + y)
        lines
        |> Seq.iter
               (fun x ->
               appendLineToBuilder
                   (builder, x.LineIndentCount, x.LineContent, spaces))
        builder.ToString()
