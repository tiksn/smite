﻿namespace TIKSN.smite.lib

module CSharpTranspiler =
    open TIKSN.smite.lib

    let transpile(models: seq<NamespaceDefinition>) =
        let filespaceDefinition = CommonFeatures.getFilespaceDefinition(models)
        0