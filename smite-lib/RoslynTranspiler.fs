namespace TIKSN.smite.lib

module RoslynTranspiler =
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Editing

    let generateFieldCode(syntaxGenerator: SyntaxGenerator, fieldDefinition: FieldDefinition, getTypeName) =
        let tn = getTypeName(fieldDefinition.Type)
        let ts = syntaxGenerator.IdentifierName(tn)
        let fd = syntaxGenerator.FieldDeclaration(fieldDefinition.Name, ts, Accessibility.Public)
        fd