namespace TIKSN.smite.lib

module IndentationFeatures =
    type IndentedLine =
        { LineIndentCount : int
          LineContent : string }

    let convertIndentedLineToString(lines: seq<IndentedLine>) =
        ""
